#!/usr/bin/env python3
"""Crew read-only review harness. The model cannot edit or execute shell commands."""
import argparse
import json
from pathlib import Path
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('name')
parser.add_argument('base')
parser.add_argument('head')
parser.add_argument('--lane', required=True)
parser.add_argument('--proof', action='append', default=[])
parser.add_argument('--context', action='append', default=[])
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
case = root / 'docs/cases/slice-1-playable'
work = Path(tempfile.mkdtemp(prefix='iron-review-'))
cache = json.loads((Path.home() / '.codex/models_cache.json').read_text())
model = next(m for m in cache['models'] if m['slug'] == 'gpt-6-astra')
model['apply_patch_tool_type'] = None
model['experimental_supported_tools'] = []
model['multi_agent_version'] = 'v1'
catalog = work / 'models.json'
catalog.write_text(json.dumps({'models': [model]}))
changed = subprocess.check_output(['git','diff','--name-only',args.base,args.head], cwd=root, text=True).splitlines()
diff = subprocess.check_output(['git','diff','--no-ext-diff','--no-color',args.base,args.head], cwd=root, text=True)
parts = [f'''You are Crew R, independent read-only reviewer for slice-1-playable. You authored none of this code. Review the range {args.base}..{args.head}; authorized lane: {args.lane}. Do not spawn another agent. You have no write or shell tools by harness. The lead supplies raw source and proof outputs below. Do not accept author narratives. Review correctness and accepted requirements, report actionable blockers with file:line and priority, and identify proof gaps. Return APPROVE or BLOCK and concise findings. No fixes. Scope is this range; supplied unchanged files are dependency context. Provisional gameplay tuning is permitted only in the single data file. Structural constants and art geometry are not gameplay balance. For a final case review focus on integration/spec drift. Missing proof is a gap; do not invent test results.\nChanged files:\n''' + '\n'.join(changed)]
context = ['AGENTS.md','docs/catalog/SLICE.md','docs/catalog/SCHEMA.md','docs/catalog/INVARIANTS.md','docs/cases/slice-1-playable/intent.md','docs/cases/slice-1-playable/spec.md','docs/cases/slice-1-playable/plan.md']
context += args.context
paths = list(dict.fromkeys(context + changed))
for name in paths:
    path = root / name
    if path.suffix in {'.glb','.blend','.png','.jpg','.bin'}:
        continue
    try:
        content = subprocess.check_output(['git', 'show', f'{args.head}:{name}'], cwd=root, stderr=subprocess.PIPE).decode('utf-8')
    except (subprocess.CalledProcessError, UnicodeDecodeError):
        continue
    parts.append(f'\nSOURCE {name}\n' + '\n'.join(f'{i}: {line}' for i,line in enumerate(content.splitlines(), 1)))
parts.append('\nRAW GIT DIFF\n' + diff)
for name in args.proof:
    path = Path(name)
    if not path.is_absolute(): path = root / path
    parts.append(f'\nRAW PROOF {name}\n' + path.read_text())
prompt = '\n'.join(parts)
(case / 'reviews').mkdir(exist_ok=True)
output = case / 'reviews' / (args.name + '.md')
command = ['codex','exec','--ignore-user-config','--ephemeral','--skip-git-repo-check','-C',str(work),'-s','read-only','-m','gpt-6-astra','-c','model_reasoning_effort="xhigh"','-c',f'model_catalog_json="{catalog}"','-c','web_search="disabled"','-o',str(output)]
for feature in ['shell_tool','multi_agent','multi_agent_v2','js_repl','apps','plugins','image_generation','goals']:
    command += ['-c', f'features.{feature}=false']
command.append('-')
print(f'Crew R {args.name}: {args.base}..{args.head}, {len(paths)} source paths, no write tools', flush=True)
try:
    completed = subprocess.run(command, input=prompt, text=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, timeout=600)
    (work / 'runner.txt').write_text(completed.stdout + completed.stderr)
    print(output.read_text() if output.exists() else completed.stdout + completed.stderr)
    raise SystemExit(completed.returncode)
except subprocess.TimeoutExpired:
    print('REVIEW_TIMEOUT: 10 minute cap reached; spawn one fresh R per Crew.')
    raise SystemExit(124)
