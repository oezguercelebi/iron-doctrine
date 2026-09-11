"""Compose the original mesh previews and labels into a Blender-rendered sheet."""
from pathlib import Path
import bpy

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/"assets/previews"
NAMES=[("command","Command"),("fusion","Fusion"),("barracks","Barracks"),
       ("factory","Factory"),("dropoff","Drop-off"),("dozer","Dozer"),
       ("tank","Tank"),("scout","Scout"),("gatherer","Gatherer"),
       ("aa_turret","AA turret"),("rifle","Rifle"),("rocket","Rocket"),
       ("garrison","Garrison"),("dock","Supply dock"),("rocks","Scrub rocks")]


def flat(name,color):
    mat=bpy.data.materials.new(name)
    mat.use_nodes=True
    nodes=mat.node_tree.nodes
    nodes.clear()
    shader=nodes.new("ShaderNodeEmission")
    shader.inputs[0].default_value=(*color,1)
    output=nodes.new("ShaderNodeOutputMaterial")
    mat.node_tree.links.new(shader.outputs[0],output.inputs[0])
    return mat


def text(body,x,y,size,mat):
    curve=bpy.data.curves.new("Sheet label","FONT")
    curve.body=body
    curve.size=size
    curve.align_x="LEFT"
    obj=bpy.data.objects.new("Sheet label",curve)
    bpy.context.collection.objects.link(obj)
    obj.location=(x,y,.10)
    curve.materials.append(mat)


def main():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.samples=1
    scene.view_settings.view_transform="Standard"
    scene.render.resolution_x=1820
    scene.render.resolution_y=1460
    scene.render.resolution_percentage=100
    background=flat("Sheet background",(.028,.047,.054))
    white=flat("Sheet white",(.84,.87,.80))
    muted=flat("Sheet muted",(.43,.54,.52))
    accent=flat("Sheet accent",(.39,.62,.70))
    bpy.ops.mesh.primitive_plane_add(size=200)
    bpy.context.object.data.materials.append(background)
    text("IRON DOCTRINE",-8.45,6.34,.41,white)
    text("SLICE 01  /  ORIGINAL ASSET STUDY",-8.43,5.92,.18,accent)
    text("BLENDER  /  glTF 2.0",5.87,6.43,.15,muted)
    text("GROUND ORIGINS  +  TEAM TINT",4.99,6.12,.15,muted)
    for i,(name,label) in enumerate(NAMES):
        path=OUT/(name+".png")
        if not path.exists():
            raise FileNotFoundError(path)
        x=(i%5-2)*3.42
        y=3.75-(i//5)*3.82
        mat=bpy.data.materials.new("Preview "+name)
        mat.use_nodes=True
        nodes=mat.node_tree.nodes
        nodes.clear()
        texture=nodes.new("ShaderNodeTexImage")
        texture.image=bpy.data.images.load(str(path),check_existing=True)
        shader=nodes.new("ShaderNodeEmission")
        output=nodes.new("ShaderNodeOutputMaterial")
        mat.node_tree.links.new(texture.outputs["Color"],shader.inputs[0])
        mat.node_tree.links.new(shader.outputs[0],output.inputs[0])
        bpy.ops.mesh.primitive_plane_add(size=3.16,location=(x,y,.02))
        bpy.context.object.data.materials.append(mat)
        text(f"{i+1:02}",x-1.50,y-1.86,.16,accent)
        text(label,x-1.13,y-1.88,.22,white)
    text("ORIGINAL PROCEDURAL GEOMETRY  /  MIT",-8.43,-6.47,.15,muted)
    text("15 MODELS   ·   EDITABLE BLENDER SOURCES",3.84,-6.47,.15,muted)
    camdata=bpy.data.cameras.new("Sheet camera")
    cam=bpy.data.objects.new("Sheet camera",camdata)
    scene.collection.objects.link(cam)
    cam.location=(0,0,10)
    camdata.type="ORTHO"
    camdata.ortho_scale=18.2
    scene.camera=cam
    scene.render.image_settings.file_format="PNG"
    scene.render.filepath=str(OUT/"contact_sheet.png")
    bpy.ops.render.render(write_still=True)
    print("CONTACT_SHEET",scene.render.filepath,flush=True)


if __name__=="__main__":
    main()
