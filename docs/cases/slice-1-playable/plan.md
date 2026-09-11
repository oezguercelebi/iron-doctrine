# Tasks
- [x] contract · done · lane proposal only (isolated worktree) · after — · Freeze engine-free sim/view/data API and meaningful failing seam proof; lead adopts hub.
- [x] art · done · lane assets/**, tools/art/** · after — · Original Blender glTF meshes for all slice bodies and asset provenance.
- [ ] sim · idle-user · lane src/Sim/**, tests/** · after contract · All tick systems, fair AI, regression and finished match proof.
- [x] client · done · lane src/Client/**, scenes/** · after contract · 3D view, HUD, inputs, sound, fog, pause/results.
- [ ] integration · idle-user · lane hubs on main · after art,sim,client · Runtime, manifests, shared contracts/data, launch/build, visible match and gates.
- [ ] review · open · lane none · after each merge · Fresh read-only R on lane and final range.
- [x] contract-proof-patch · done · lane tests/SubmissionTiming.cs · after contract · Assert Submit leaves visible gameplay unchanged until Step; supplement frozen test.

- [x] sim-r1 · done · lane src/Sim/**, tests/** · after sim · Five independent R edge cases with regressions.
- [x] client-r1 · done · lane src/Client/** · after client · Attack, producer cancellation, public combat feedback and radar gating fixes.

- [x] client-r2 · done · lane src/Client/** · after client-r1 · Preserve legal build point; distinguish unknown enemy passenger/activity fields.

- [ ] sim-r2 · idle-user · lane src/Sim/**, tests/** · after sim-r1 · Sanitize point orders, queue unload correctly, clear control on capture.

- [ ] final-findings · idle-user · lanes src/Sim/**, tests/** and src/Client/** · after user authorizes patch-budget extension · Keep unload passengers within actual local reach on default terrain; restore unit-targeted Guard through UI and simulation without exposing hidden targets. Preserve frozen contract/test. Add regressions for blocked unload destinations and moving-Dozer Guard, repeat visible proof and independent reviews.
