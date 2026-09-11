# Manual input proof

Source: integrated main bb5c572; Godot4.7.2 .NET actual visible window, no proof pilot. Computer Use mouse/keyboard on the rendered game.

- Escape: Local Pause visible at00:19, clock remained stopped between calls; Resume worked. Screenshot manual-pause.jpg.
- Selected Dozer, clicked Fusion card and open ground: construction completed, funds6000→5400, power20/0, original Fusion mesh visible.
- Ctrl+1 saved Dozer; selected Command; key1 recalled Dozer.
- Command production accepted two Dozers, then a later six-item batch. manual-queue.jpg.
- Clicked waiting queue item3: queue shortened and funds2500→3000 on next tick. Screenshot manual-cancel.jpg. Initial single-item attempts completed before the slow inspection returned, so those attempts are not cancellation proof.
- Drag box selected7Dozers; rightclick issued Move (Moving visible), wheel changed zoom.
- H displayed field guide and paused; explicit Attack Z shown in guide. manual-controls.jpg.
- Resign from local pause: DEFEAT result at01:12. manual-resign.jpg.
- Rematch: fresh00:00,6000funds, Command+Dozer; immediate pause. manual-rematch.jpg.
- Quit to desktop closed the game. Runtime log manual-input-runtime.txt has no errors.

Specialized combat/capture/transport/repair orders are covered by simulation regressions and reviewed client mappings, not claimed as separate manual GUI executions here. Final visible complete-match proof is recorded separately.
