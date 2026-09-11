# Original slice-1 art

Original low-poly meshes for the accepted `slice-1-playable` case. All geometry,
materials, and preview compositions were authored for this repository with the
procedural Blender source in [build_assets.py](../tools/art/build_assets.py).
No downloaded models, reference meshes, textures, branded markings, or external
asset packs are used. Blender is only the authoring tool.

These source files, meshes, materials, and previews are provided under the
repository's [MIT license](../LICENSE). The native `.blend` files are editable
copies of the exported geometry. The Python source retains the procedural parts
and construction logic. Preview lamps, cameras, and floors are excluded from
the `.blend` sources and `.glb` models.

## Integration

- Resources: `res://assets/models/<filename>.glb`.
- Units: one metre. Identity transform: Godot **+Y up**, **−Z forward**.
- Origins: on the ground at the model's central body; long barrels, blades,
  rotors, and other overhangs can extend past the body center.
- Use the authored dimensions directly or apply a uniform visual scale. Exact
  visual bounds and triangle counts are in [manifest.json](models/manifest.json).
  They are geometry measurements, not collision, targeting, or balance data.
- Every owned model and the garrison contain the separately named material
  `team_color`. Duplicate that material per instance and tint only that surface.
  Other materials give sand, olive, steel, rubber, glass, and status accents.
  The source preview color is a provisional blue. Use a neutral gray for an
  unoccupied garrison. The neutral supply dock and rocks have no team material.
- Gatherers are exported resting on their landing skids. The client supplies
  airborne elevation. Their `rotor_left` and `rotor_right` mesh nodes have pivot
  origins at the rotor hubs; rotate them about local Y in Godot. The static
  `body` remains separate. Rotors have modeled blades without alpha textures.
- Meshes supply appearance only. They contain no collision bodies, scripts,
  gameplay numbers, cameras, lights, animations, or outside file references.

| File | Catalog id | Visual role |
| --- | --- | --- |
| `dozer.glb` | `build.dozer` | Tracked builder, blade, glazed cabin, lift arms |
| `gatherer.glb` | `eco.chinook` | Wide twin-rotor cargo aircraft, skids, cargo door |
| `fusion.glb` | `power.fusion` | Paired containment vessels, cooling fins, control house |
| `command.glb` | `prod.command` | Stepped operations building, dish, communications mast |
| `barracks.glb` | `prod.barracks` | Ribbed barrel roof, entrance canopy, pennant |
| `factory.glb` | `prod.factory` | Large vehicle bay, skylights, service annex |
| `dropoff.glb` | `eco.dropoff` | Receiving warehouse, loading platform, cargo hoist |
| `rifle.glb` | `inf.rifle` | Helmeted infantry, armor, backpack, rifle |
| `rocket.glb` | `inf.rocket` | Helmeted infantry with shoulder launcher |
| `tank.glb` | `armor.basic` | Sloped armor, skirted tracks, turret and long cannon |
| `scout.glb` | `veh.scout_gun` | Wheeled infantry carrier, passenger windows, roof gun |
| `aa_turret.glb` | `def.patriot` | Elevated launch cells, rotating base, targeting array |
| `dock.glb` | `map.dock` | Neutral stacked supply cases and marked receiving lane |
| `garrison.glb` | `map.garrison` | Two-storey plaster dwelling, parapets, windows, cistern |
| `rocks.glb` | `ter.unbuildable` | Low weathered boulders and dry scrub; terrain decoration |

Only the existing stable catalog ids above are used. Filenames and visible
preview labels are semantic working labels. The rock cluster is terrain art,
not a new unit or map-object row, and does not introduce a cover mechanic.

## Reproduce and inspect

Authored and verified with Blender **4.5.13 LTS**, downloaded from Blender's
[official release directory](https://download.blender.org/release/Blender4.5/).
No Python packages are needed outside Blender's bundled runtime.

```sh
blender --background --factory-startup --python-exit-code 1 --python tools/art/build_assets.py -- --render
blender --background --factory-startup --python-exit-code 1 --python tools/art/render_sheet.py
blender --background --factory-startup --python-exit-code 1 --python tools/art/verify_assets.py
```

The first command rebuilds all `.glb` and `.blend` files and individual previews.
For an iteration, append `--only tank gatherer --render` after the separator.
The second produces [the contact sheet](previews/contact_sheet.png).
The third checks every GLB's structure and geometry, imports every GLB through
Blender, and checks the authored dimensions, floor position, and material names.
For the file-only part of validation, `python3 tools/art/verify_assets.py` works
without Blender. This is an asset check; Godot import remains the client gate.
