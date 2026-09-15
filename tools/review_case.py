#!/usr/bin/env python3
"""Restricted read-only review harness. Process exit 0 is not APPROVE; parse the verdict."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
import tempfile
from pathlib import Path

VERDICT_RE = re.compile(r"\b(APPROVE|BLOCK)\b")
DEFAULT_CONTEXT = (
    "AGENTS.md",
    "docs/catalog/SLICE.md",
    "docs/catalog/SCHEMA.md",
    "docs/catalog/INVARIANTS.md",
)


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Restricted read-only review harness. --case is required for a real review; it does not default to slice-1-playable."
    )
    parser.add_argument("name", nargs="?")
    parser.add_argument("base", nargs="?")
    parser.add_argument("head", nargs="?")
    parser.add_argument("--lane")
    parser.add_argument("--case", default=None, help="Case id under docs/cases/<id>/")
    parser.add_argument("--proof", action="append", default=[], help="Proof file; must exist and be non-empty")
    parser.add_argument("--context", action="append", default=[])
    parser.add_argument("--self-check", action="store_true", help="Dry fail-closed fixtures; does not call Codex")
    return parser.parse_args(argv)


def case_dir(root: Path, case_id: str) -> Path:
    if not case_id or case_id.strip() != case_id:
        raise SystemExit("missing --case <id> (no default; not slice-1-playable)")
    if "/" in case_id or "\\" in case_id or case_id in (".", ".."):
        raise SystemExit(f"invalid --case {case_id!r}")
    return root / "docs" / "cases" / case_id


def sha256_file(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def collect_proofs(root: Path, proofs: list[str]) -> tuple[list[dict], list[str]]:
    records: list[dict] = []
    errors: list[str] = []
    if not proofs:
        errors.append("missing proof: at least one --proof is required")
        return records, errors
    for name in proofs:
        path = Path(name)
        if not path.is_absolute():
            path = root / path
        if not path.is_file():
            errors.append(f"missing proof: {name}")
            continue
        size = path.stat().st_size
        if size == 0:
            errors.append(f"empty proof: {name}")
            continue
        records.append({"path": str(name), "sha256": sha256_file(path), "bytes": size})
    return records, errors


def parse_verdict(text: str) -> str:
    tokens = VERDICT_RE.findall(text or "")
    if not tokens:
        return "MISSING"
    if "BLOCK" in tokens:
        return "BLOCK"
    return "APPROVE"


def review_exit(proof_errors: list[str], verdict: str, proc_code: int, timed_out: bool = False) -> int:
    if proof_errors:
        return 2
    if timed_out:
        return 124
    if proc_code != 0:
        return proc_code if proc_code else 1
    if verdict == "BLOCK":
        return 3
    if verdict != "APPROVE":
        return 4
    return 0


def identity_payload(case_id: str, base: str, head: str, lane: str, proofs: list[dict]) -> dict:
    return {
        "case": case_id,
        "base": base,
        "head": head,
        "range": f"{base}..{head}",
        "lane": lane,
        "proofs": proofs,
    }


def strip_codex_write_tools(work: Path) -> Path:
    cache = Path.home() / ".codex" / "models_cache.json"
    if not cache.is_file():
        raise FileNotFoundError("missing ~/.codex/models_cache.json")
    payload = json.loads(cache.read_text())
    model = next(m for m in payload["models"] if m["slug"] == "gpt-6-astra")
    model["apply_patch_tool_type"] = None
    model["experimental_supported_tools"] = []
    model["multi_agent_version"] = "v1"
    catalog = work / "models.json"
    catalog.write_text(json.dumps({"models": [model]}))
    return catalog


def build_prompt(
    root: Path,
    case_id: str,
    name: str,
    base: str,
    head: str,
    lane: str,
    proofs: list[dict],
    extra_context: list[str],
) -> tuple[str, list[str]]:
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", base, head], cwd=root, text=True
    ).splitlines()
    diff = subprocess.check_output(
        ["git", "diff", "--no-ext-diff", "--no-color", base, head], cwd=root, text=True
    )
    parts = [
        f"You are an independent read-only reviewer for {case_id}. You authored none of this code. "
        f"Review the range {base}..{head}; authorized lane: {lane}. Do not spawn another agent. "
        "You have no write or shell tools by harness. The lead supplies raw source and proof outputs below. "
        "Do not accept author narratives. Review correctness and accepted requirements, report actionable blockers "
        "with file:line and priority, and identify proof gaps. Return APPROVE or BLOCK and concise findings. No fixes. "
        "Scope is this range; supplied unchanged files are dependency context. Provisional gameplay tuning is permitted "
        "only in the single data file. Structural constants and art geometry are not gameplay balance. For a final case "
        "review focus on integration/spec drift. Missing proof is a gap; do not invent test results. "
        "Process success is not approval.\n"
        f"Reviewed range: {base}..{head}\nChanged files:\n" + "\n".join(changed)
    ]
    context = [
        *DEFAULT_CONTEXT,
        f"docs/cases/{case_id}/intent.md",
        f"docs/cases/{case_id}/spec.md",
        f"docs/cases/{case_id}/plan.md",
        f"docs/cases/{case_id}/contract.md",
        *extra_context,
    ]
    paths = list(dict.fromkeys(context + changed))
    for name in paths:
        path = root / name
        if path.suffix in {".glb", ".blend", ".png", ".jpg", ".bin"}:
            continue
        try:
            content = subprocess.check_output(
                ["git", "show", f"{head}:{name}"], cwd=root, stderr=subprocess.PIPE
            ).decode("utf-8")
        except (subprocess.CalledProcessError, UnicodeDecodeError):
            continue
        parts.append("\nSOURCE " + name + "\n" + "\n".join(f"{i}: {line}" for i, line in enumerate(content.splitlines(), 1)))
    parts.append("\nRAW GIT DIFF\n" + diff)
    parts.append("\nPROOF IDENTITY\n" + json.dumps(identity_payload(case_id, base, head, lane, proofs), indent=2))
    for rec in proofs:
        rel = rec["path"]
        path = Path(rel)
        if not path.is_absolute():
            path = root / path
        parts.append(f"\nRAW PROOF {rel} sha256={rec['sha256']} bytes={rec['bytes']}\n" + path.read_text())
    return "\n".join(parts), paths


def run_self_check() -> int:
    root = Path(__file__).resolve().parents[1]
    failures: list[str] = []

    def check(name: str, ok: bool, detail: str) -> None:
        status = "ok" if ok else "BROKEN"
        print(f"SELF-CHECK {name}: {status} {detail}")
        if not ok:
            failures.append(name)

    _, missing_errs = collect_proofs(root, ["no/such/proof.txt"])
    missing_code = review_exit(missing_errs, "APPROVE", 0)
    check("missing-proof", missing_code != 0 and missing_code == 2, f"exit={missing_code} (must not pass)")

    empty_errs: list[str]
    with tempfile.NamedTemporaryFile(prefix="iron-empty-proof-", delete=False) as handle:
        empty_path = Path(handle.name)
    try:
        _, empty_errs = collect_proofs(root, [str(empty_path)])
        empty_code = review_exit(empty_errs, "APPROVE", 0)
        check("empty-proof", empty_code != 0, f"exit={empty_code} (must not pass)")
    finally:
        empty_path.unlink(missing_ok=True)

    none_code = review_exit(collect_proofs(root, [])[1], "APPROVE", 0)
    check("no-proof-args", none_code != 0, f"exit={none_code} (must not pass)")

    block_verdict = parse_verdict("findings\nBLOCK\nseverity high")
    block_code = review_exit([], block_verdict, 0)
    check("BLOCK", block_code != 0 and block_verdict == "BLOCK", f"verdict={block_verdict} exit={block_code}")

    both = parse_verdict("APPROVE\nBLOCK")
    both_code = review_exit([], both, 0)
    check("BLOCK-wins", both == "BLOCK" and both_code != 0, f"verdict={both} exit={both_code}")

    missing_verdict = parse_verdict("looks fine, process succeeded")
    missing_code = review_exit([], missing_verdict, 0)
    check(
        "process-exit-0-is-not-APPROVE",
        missing_verdict == "MISSING" and missing_code != 0,
        f"verdict={missing_verdict} exit={missing_code}",
    )

    proc_fail = review_exit([], "APPROVE", 1)
    check("nonzero-process-not-APPROVE", proc_fail != 0, f"exit={proc_fail}")

    with tempfile.NamedTemporaryFile(prefix="iron-proof-", mode="w", delete=False) as handle:
        handle.write("PASS proof fixture\n")
        good = Path(handle.name)
    try:
        recs, errs = collect_proofs(root, [str(good)])
        approve = parse_verdict("APPROVE\nno blockers")
        approve_code = review_exit(errs, approve, 0)
        check(
            "APPROVE-with-proof",
            approve_code == 0 and not errs and recs and recs[0]["sha256"],
            f"exit={approve_code} sha256={recs[0]['sha256'] if recs else ''}",
        )
    finally:
        good.unlink(missing_ok=True)

    ns = parse_args(["--self-check"])
    check("default-case-is-not-slice-1-playable", ns.case not in ("slice-1-playable",), f"case={ns.case!r}")

    if failures:
        print("SELF-CHECK failed: " + ", ".join(failures))
        return 1
    print("SELF-CHECK ok: missing proof ≠ pass; BLOCK ≠ pass; process exit 0 is not APPROVE")
    return 0


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    if args.self_check:
        return run_self_check()
    if not args.name or not args.base or not args.head:
        print("usage: review_case.py <name> <base> <head> --lane <glob> --case <id> --proof <file>", file=sys.stderr)
        return 2
    if not args.lane:
        print("missing --lane", file=sys.stderr)
        return 2
    if not args.case:
        print("missing --case <id> (no default; not slice-1-playable)", file=sys.stderr)
        return 2
    root = Path(__file__).resolve().parents[1]
    case = case_dir(root, args.case)
    proofs, proof_errors = collect_proofs(root, args.proof)
    identity = identity_payload(args.case, args.base, args.head, args.lane, proofs)
    print(
        f"Review {args.name}: case={args.case} {args.base}..{args.head} proofs={len(proofs)}",
        flush=True,
    )
    for rec in proofs:
        print(f"PROOF sha256={rec['sha256']} bytes={rec['bytes']} {rec['path']}", flush=True)
    print(f"REVIEWED_RANGE {args.case} {args.base}..{args.head}", flush=True)
    if proof_errors:
        for err in proof_errors:
            print(err, file=sys.stderr)
        return review_exit(proof_errors, "MISSING", 0)

    work = Path(tempfile.mkdtemp(prefix="iron-review-"))
    reviews = case / "reviews"
    reviews.mkdir(parents=True, exist_ok=True)
    output = reviews / (args.name + ".md")
    identity_path = reviews / (args.name + ".identity.json")
    identity_path.write_text(json.dumps(identity, indent=2) + "\n")

    try:
        catalog = strip_codex_write_tools(work)
    except Exception as exc:
        print(f"REVIEW_TOOLS: {exc}", file=sys.stderr)
        return 1

    prompt, paths = build_prompt(root, args.case, args.name, args.base, args.head, args.lane, proofs, args.context)
    command = [
        "codex",
        "exec",
        "--ignore-user-config",
        "--ephemeral",
        "--skip-git-repo-check",
        "-C",
        str(work),
        "-s",
        "read-only",
        "-m",
        "gpt-6-astra",
        "-c",
        'model_reasoning_effort="xhigh"',
        "-c",
        f'model_catalog_json="{catalog}"',
        "-c",
        'web_search="disabled"',
        "-o",
        str(output),
    ]
    for feature in ["shell_tool", "multi_agent", "multi_agent_v2", "js_repl", "apps", "plugins", "image_generation", "goals"]:
        command += ["-c", f"features.{feature}=false"]
    command.append("-")
    print(f"Review {args.name}: {len(paths)} source paths, no write tools", flush=True)
    try:
        completed = subprocess.run(
            command, input=prompt, text=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, timeout=600
        )
        (work / "runner.txt").write_text(completed.stdout + completed.stderr)
        text = output.read_text() if output.exists() else completed.stdout + completed.stderr
        print(text)
        verdict = parse_verdict(text)
        print(f"REVIEW_VERDICT {verdict} proc={completed.returncode} range={args.base}..{args.head}", flush=True)
        identity_path.write_text(
            json.dumps({**identity, "verdict": verdict, "proc": completed.returncode}, indent=2) + "\n"
        )
        return review_exit([], verdict, completed.returncode)
    except subprocess.TimeoutExpired:
        print("REVIEW_TIMEOUT: 10 minute cap reached; spawn one fresh reviewer.")
        return review_exit([], "MISSING", 0, timed_out=True)


if __name__ == "__main__":
    raise SystemExit(main())
