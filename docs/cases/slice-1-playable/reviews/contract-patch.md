**BLOCK**

- **P1 — `src/Bootstrap/Main.cs:16`:** `FileAccess` is ambiguous between `Godot.FileAccess` and `System.IO.FileAccess`, because `IronDoctrine.csproj:6` enables implicit usings. This causes CS0104 independently of the expected missing implementation types.

- **P2 — `tests/Program.cs:2`:** The executable invokes only `SeamConformance.Run`. Nothing calls `SubmissionTiming.Run`, so `tools/proof.sh` cannot exercise the supplemental timing and input-detachment assertions when implementation arrives.

- **Proof gap — `tests/SubmissionTiming.cs:18`:** The supplement tests only Move. Immediate construction/debit inside `Submit(Build)` could still pass both tests; the original construction timing gap at `tests/SeamConformance.cs:33` remains uncovered.

The supplied missing-factory CS0234 is expected at this gate. It establishes no runtime results; simulation, complete-match, Godot build/import/boot, and visible-play proofs remain pending.