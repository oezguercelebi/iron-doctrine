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

- [ ] final-findings · cancelled · superseded by feel-sim / feel-hud / feel-input
- [x] feel-sim · done · lane src/Sim/**, tests/** · after — · Clamp unload FindFree to local reach; CanPlace matches GroundFits(units); regressions. R approve.
- [x] feel-hud · done · lane src/Client/Battlefield.cs, src/Client/FieldHud.cs, src/Client/FieldAudio.cs · after — · Persistent rally flag on re-select; Rally only on producers; pick infantry over buildings; command bar uses owned selection; sfx.rally/sfx.place tones. R approve.
- [x] feel-input · done · lane src/Client/MatchClient.cs, src/Client/CommandIntent.cs, src/Client/Proof/**, src/Client/README.md · after — · Guard keeps unit TargetId; sell mode; edge pan; box-select radius; play sfx.rally/sfx.place; upgrade toast. R approve after sell-incomplete patch b7a5417.
- [x] feel-integration · done · lane hubs on main · after feel-sim, feel-hud, feel-input · Proofs on main, Godot build, case R approve.
