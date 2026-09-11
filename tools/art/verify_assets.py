"""GLB structure and actual import proof. Runs in Python or Blender, no extras."""
from __future__ import annotations
import json
import math
import struct
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
MODELS=ROOT/"assets/models"
EXPECTED={"dozer":"build.dozer","gatherer":"eco.chinook","fusion":"power.fusion",
          "command":"prod.command","barracks":"prod.barracks","factory":"prod.factory",
          "dropoff":"eco.dropoff","rifle":"inf.rifle","rocket":"inf.rocket",
          "tank":"armor.basic","scout":"veh.scout_gun","aa_turret":"def.patriot",
          "dock":"map.dock","garrison":"map.garrison","rocks":"ter.unbuildable"}


def read_glb(path):
    data=path.read_bytes()
    magic,version,size=struct.unpack_from("<III",data)
    assert magic==0x46546C67 and version==2 and size==len(data),path
    offset=12
    chunks=[]
    while offset<len(data):
        length,kind=struct.unpack_from("<II",data,offset)
        offset+=8
        assert length%4==0 and offset+length<=len(data),path
        chunks.append((kind,data[offset:offset+length]))
        offset+=length
    assert len(chunks)==2 and chunks[0][0]==0x4E4F534A and chunks[1][0]==0x004E4942,path
    doc=json.loads(chunks[0][1])
    assert doc["asset"]["version"]=="2.0"
    assert len(doc["buffers"])==1 and "uri" not in doc["buffers"][0]
    assert doc["buffers"][0]["byteLength"]<=len(chunks[1][1])
    assert not doc.get("images") and not doc.get("cameras") and not doc.get("animations")
    return doc,chunks[1][1]


def accessor(doc,blob,index):
    a=doc["accessors"][index]
    view=doc["bufferViews"][a["bufferView"]]
    formats={5121:("B",1),5123:("H",2),5125:("I",4),5126:("f",4)}
    components={"SCALAR":1,"VEC2":2,"VEC3":3,"VEC4":4}
    fmt,width=formats[a["componentType"]]
    count=components[a["type"]]
    start=view.get("byteOffset",0)+a.get("byteOffset",0)
    stride=view.get("byteStride",count*width)
    assert start+(a["count"]-1)*stride+count*width<=view.get("byteOffset",0)+view["byteLength"]
    return [struct.unpack_from("<"+fmt*count,blob,start+i*stride) for i in range(a["count"])]


def validate_file(name,record):
    path=MODELS/(name+".glb")
    doc,blob=read_glb(path)
    assert record["catalog_id"]==EXPECTED[name]
    assert record["resource"]=="res://assets/models/"+name+".glb"
    assert (ROOT/"assets/sources"/(name+".blend")).exists()
    assert all(v>0 for v in record["dimensions"])
    materials={m["name"] for m in doc["materials"]}
    assert ("team_color" in materials)==(name not in {"dock","rocks"}),(name,materials)
    rootnodes=doc["scenes"][doc.get("scene",0)]["nodes"]
    assert len(rootnodes)==1 and doc["nodes"][rootnodes[0]]["name"]==name
    root=doc["nodes"][rootnodes[0]]
    assert root.get("translation",[0,0,0])==[0,0,0]
    assert root.get("scale",[1,1,1])==[1,1,1]
    assert root.get("rotation",[0,0,0,1])==[0,0,0,1]
    triangles=0
    for mesh in doc["meshes"]:
        for primitive in mesh["primitives"]:
            assert primitive.get("mode",4)==4
            pos=accessor(doc,blob,primitive["attributes"]["POSITION"])
            assert all(math.isfinite(v) for p in pos for v in p)
            normals=accessor(doc,blob,primitive["attributes"]["NORMAL"])
            assert len(pos)==len(normals)
            assert all(.8<sum(v*v for v in n)<1.2 for n in normals)
            indices=[v[0] for v in accessor(doc,blob,primitive["indices"])]
            assert len(indices)%3==0 and all(0<=i<len(pos) for i in indices)
            triangles+=len(indices)//3
            assert 0<=primitive["material"]<len(materials)
    assert triangles==record["triangles"] and triangles>100,(name,triangles,record["triangles"])
    if name=="gatherer":
        for rotor in ("rotor_left","rotor_right"):
            node=next(n for n in doc["nodes"] if n["name"]==rotor)
            assert "mesh" in node
            assert node["translation"][1]>2
    return triangles


def validate_blender(name,record):
    import bpy
    from mathutils import Vector
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.gltf(filepath=str(MODELS/(name+".glb")))
    bpy.context.view_layer.update()
    objects=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    assert objects
    points=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
    lo=[min(v[i] for v in points) for i in range(3)]
    hi=[max(v[i] for v in points) for i in range(3)]
    actual=[hi[0]-lo[0],hi[2]-lo[2],hi[1]-lo[1]]
    assert abs(lo[2])<.002,(name,"floor",lo)
    assert all(abs(a-b)<.002 for a,b in zip(actual,record["dimensions"])),(name,actual,record)
    assert all(not o.modifiers for o in objects)


def main():
    manifest=json.loads((MODELS/"manifest.json").read_text())
    assert manifest["up"]=="+Y" and manifest["forward"]=="-Z" and manifest["units"]=="metres"
    records=manifest["assets"]
    assert set(records)==set(EXPECTED)
    assert {p.stem for p in MODELS.glob("*.glb")}==set(EXPECTED)
    try:
        import bpy
        blender=True
    except ImportError:
        blender=False
    total=0
    for name in EXPECTED:
        tris=validate_file(name,records[name])
        if blender:
            validate_blender(name,records[name])
        total+=tris
        print(f"PASS {name}: {tris} triangles; glTF structure, materials, normals, indices"+
              ("; Blender import and bounds" if blender else ""),flush=True)
    print(f"PASS {len(EXPECTED)} original assets; {total} triangles; "+
          ("15/15 Blender imports" if blender else "file validation (run in Blender for import proof)"),flush=True)


if __name__=="__main__":
    main()
