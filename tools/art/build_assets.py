"""Original slice-1 meshes, authored procedurally in Blender.

Run: blender --background --factory-startup --python tools/art/build_assets.py
Optional arguments after --: --only tank gatherer --render
Geometry measurements below are artistic dimensions, never gameplay balance.
Blender +Y is forward; the glTF exporter converts this to Godot -Z.

No armatures. export_animations stays False unless an allowlisted gatherer
rotor rotation clip is added. Runtime spins rotor_* and root-bobs infantry
and vehicles. Structures stay static.
"""
from __future__ import annotations

import argparse
import json
import math
import random
import struct
import sys
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
MODELS = ROOT / "assets/models"
SOURCES = ROOT / "assets/sources"
PREVIEWS = ROOT / "assets/previews"
TAU = math.tau
MATERIALS = {}


def material(name, rgb, metallic=0.0, roughness=0.65, emission=0.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*rgb, 1)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*rgb, 1)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    if emission:
        shader.inputs["Emission Color"].default_value = (*rgb, 1)
        shader.inputs["Emission Strength"].default_value = emission
    MATERIALS[name] = mat
    return mat


def palette():
    material("sand_armor", (0.49, 0.44, 0.30), 0.16)
    material("sand_edge", (0.69, 0.63, 0.45), 0.12)
    material("olive_armor", (0.21, 0.27, 0.20), 0.14)
    material("olive_dark", (0.105, 0.14, 0.115), 0.1)
    material("steel", (0.25, 0.29, 0.285), 0.6, 0.4)
    material("gunmetal", (0.07, 0.10, 0.105), 0.55, 0.45)
    material("rubber", (0.035, 0.042, 0.039), 0.0, 0.85)
    material("glass", (0.07, 0.18, 0.21), 0.55, 0.25)
    material("concrete", (0.39, 0.40, 0.345), 0.0, 0.9)
    material("plaster", (0.64, 0.54, 0.38), 0.0, 0.85)
    material("roof_clay", (0.34, 0.25, 0.18), 0.0, 0.9)
    material("wood", (0.31, 0.235, 0.13), 0.0, 0.85)
    material("skin", (0.41, 0.28, 0.18), 0.0, 0.85)
    material("warning", (0.9, 0.52, 0.115), 0.15)
    material("lamp", (0.9, 0.8, 0.47), 0.05, 0.3, 0.4)
    material("energy", (0.16, 0.62, 0.66), 0.35, 0.3, 0.6)
    material("team_color", (0.18, 0.43, 0.56), 0.14, 0.55)
    material("stone", (0.41, 0.35, 0.25), 0.0, 1)
    material("stone_light", (0.54, 0.46, 0.33), 0.0, 1)
    material("scrub", (0.28, 0.31, 0.16), 0.0, 1)


def finish(obj, name, mat, bevel=0, smooth=False):
    obj.name = name
    obj.data.materials.clear()
    obj.data.materials.append(MATERIALS[mat])
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        mod = obj.modifiers.new("Soft manufactured edges", "BEVEL")
        mod.width = bevel
        mod.segments = 2
        bpy.ops.object.modifier_apply(modifier=mod.name)
        mod = obj.modifiers.new("Weighted face normals", "WEIGHTED_NORMAL")
        mod.keep_sharp = True
        bpy.ops.object.modifier_apply(modifier=mod.name)
    if smooth:
        for poly in obj.data.polygons:
            poly.use_smooth = True
    obj.select_set(False)
    return obj


def box(name, loc, size, mat="sand_armor", bevel=0.06, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.scale = size
    return finish(obj, name, mat, bevel)


def cylinder(name, loc, radius, depth, mat="steel", vertices=12,
             rot=(0, 0, 0), radius2=None, bevel=0.025):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius,
                                  radius2=radius if radius2 is None else radius2,
                                  depth=depth, location=loc, rotation=rot)
    return finish(bpy.context.object, name, mat, bevel)


def beam(name, start, end, radius=0.07, mat="steel", vertices=8):
    a, b = Vector(start), Vector(end)
    obj = cylinder(name, (a+b)/2, radius, (b-a).length, mat, vertices, bevel=0.01)
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = (b-a).to_track_quat("Z", "Y")
    return obj


def ico(name, loc, scale, mat="olive_armor", subdivisions=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdivisions, radius=1, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    return finish(obj, name, mat)


def mesh(name, verts, faces, mat, bevel=0.025):
    data = bpy.data.meshes.new(name)
    data.from_pydata(verts, [], faces)
    data.update()
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    # Recalculate normals for original solid section geometry.
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(obj, name, mat, bevel)


def hull(name, sections, mat="sand_armor", bevel=0.04):
    """Longitudinal octagonal rings: y, half-width, bottom, top, chamfer."""
    verts = []
    for y, w, bottom, top, c in sections:
        verts += [(-w+c,y,bottom), (w-c,y,bottom), (w,y,bottom+c),
                  (w,y,top-c), (w-c,y,top), (-w+c,y,top),
                  (-w,y,top-c), (-w,y,bottom+c)]
    faces = [tuple(range(7,-1,-1))]
    for i in range(len(sections)-1):
        for j in range(8):
            faces.append((i*8+j, i*8+(j+1)%8, (i+1)*8+(j+1)%8, (i+1)*8+j))
    faces.append(tuple(range((len(sections)-1)*8,len(sections)*8)))
    return mesh(name, verts, faces, mat, bevel)


def arch_roof(name, center, width, length, height, mat="olive_armor", count=8):
    x,y,z = center
    verts=[]
    for yy in (y-length/2,y+length/2):
        for i in range(count+1):
            theta=math.pi*i/count
            verts.append((x+width/2*math.cos(theta), yy, z+height*math.sin(theta)))
    n=count+1
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]
    for i in range(count):
        faces.append((i,i+1,n+i+1,n+i))
    faces.append((0,n,n*2-1,n-1))
    return mesh(name,verts,faces,mat,0.03)


def slats(center, count, step, size, mat="gunmetal", axis="y"):
    for i in range(count):
        loc=list(center)
        loc[0 if axis=="x" else 1] += (i-(count-1)/2)*step
        box("Vent fin",loc,size,mat,0.01)


def track(x, length=3.5, width=0.58, z=0.56):
    hull("Continuous tread", [(-length/2+0.3,width/2,0.05,z*2,0.18),
                              (length/2-0.3,width/2,0.05,z*2,0.18)], "rubber", 0.035).location.x=x
    for side in (-1,1):
        xx=x+side*(width/2+0.015)
        for y in (-length*.32,-length*.12,length*.12,length*.32):
            cylinder("Road wheel",(xx,y,z),z*.64,0.055,"steel",12,(0,math.pi/2,0),bevel=.01)
            cylinder("Wheel hub",(xx+side*.035,y,z),z*.25,.04,"olive_dark",8,(0,math.pi/2,0),bevel=.01)
    for i in range(12):
        y=(i/11-.5)*(length-.55)
        box("Track shoe top",(x,y,z*2+.015),(width+.035,.145,.07),"gunmetal",.009)
        box("Track shoe ground",(x,y,.055),(width+.035,.145,.11),"gunmetal",.009)


def wheel(x,y,z=.55,r=.55,width=.35):
    cylinder("All terrain tire",(x,y,z),r,width,"rubber",12,(0,math.pi/2,0),bevel=.045)
    side=1 if x>0 else -1
    cylinder("Armored wheel",(x+side*width*.51,y,z),r*.57,.06,"steel",8,(0,math.pi/2,0),bevel=.015)
    cylinder("Hub cap",(x+side*width*.62,y,z),r*.24,.08,"olive_dark",8,(0,math.pi/2,0),bevel=.012)


def light_pair(y,z,width):
    for x in (-width/2,width/2):
        box("Light housing",(x,y,z),(.29,.17,.20),"gunmetal",.04)
        box("Headlamp",(x,y+.09,z),(.21,.025,.12),"lamp",.02)


def dozer():
    for x in (-1.06,1.06):
        track(x,3.35,.62,.53)
    hull("Builder chassis",[(-1.48,.78,.55,1.22,.22),(1.18,.86,.5,1.24,.22)],"olive_armor")
    hull("Engine cowl",[(-1.25,.68,1.14,1.61,.1),(-.13,.68,1.14,1.63,.12)],"sand_armor")
    slats((0,-.7,1.642),6,.13,(1.0,.065,.04))
    hull("Operator cabin",[(-.15,.60,1.16,2.34,.20),(.88,.54,1.16,2.2,.16)],"sand_edge")
    hull("Cab glazing",[(-.13,.604,1.57,2.15,.14),(.82,.53,1.55,2.04,.12)],"glass",.015)
    box("Front cab window",(0,.895,1.79),(.86,.04,.47),"glass",.035)
    for side in (-1,1):
        box("Side cab window",(side*.612,.30,1.83),(.04,.67,.47),"glass",.035)
    box("Cab roof",(.0,.31,2.29),(1.39,1.26,.18),"team_color",.09)
    for x in (-.64,.64):
        box("Cab pillar",(x,.08,1.89),(.09,.11,.76),"sand_edge",.015)
        beam("Blade lift arm",(x,0,.91),(x,2.0,.62),.12)
        beam("Hydraulic ram",(x,0,1.15),(x,1.54,.79),.065,"steel")
    hull("Angled grading blade",[(1.80,1.66,.12,1.29,.16),(2.07,1.69,.12,1.20,.12)],"steel",.07)
    box("Blade cutting edge",(0,2.1,.16),(3.5,.13,.18),"sand_edge",.025)
    for x in (-1.35,-.9,.9,1.35):
        box("Blade wear plate",(x,2.11,.67),(.20,.025,.72),"warning",.01,rot=(0,.18,0))
    beam("Exhaust stack",(-.60,-.98,1.45),(-.60,-.98,2.17),.10,"gunmetal")
    cylinder("Work beacon",(.42,.35,2.49),.105,.20,"warning",8)
    light_pair(.94,1.50,1.35)


def tank():
    for x in (-1.29,1.29):
        track(x,4.55,.66,.58)
    hull("Sloped tank hull",[(-2.13,.97,.59,1.34,.28),(-1.6,1.11,.5,1.51,.23),
                            (1.56,1.11,.55,1.40,.23),(2.05,.80,.78,1.20,.13)],"sand_armor")
    for x in (-1.3,1.3):
        box("Track shoulder",(x,0,1.19),(.65,4.22,.21),"olive_armor",.09)
        for y in (-1.50,-.60,.3,1.2):
            box("Side armor skirt",(x*1.24,y,.98),(.12,.77,.46),"sand_armor",.04)
    cylinder("Turret race",(0,-.1,1.48),.89,.22,"gunmetal",16)
    hull("Faceted turret",[(-1.24,.59,1.52,2.14,.15),(-.8,.85,1.5,2.28,.23),
                           (.77,.72,1.5,2.19,.19),(1.10,.43,1.55,1.97,.13)],"olive_armor")
    box("Turret recognition panel",(0,-.39,2.30),(.88,.70,.045),"team_color",.06)
    for x in (-.73,.73):
        box("Turret side panel",(x,-.25,1.95),(.11,.88,.25),"team_color",.02)
    cylinder("Cannon collar",(0,1.00,1.84),.23,.51,"sand_armor",10,(math.pi/2,0,0))
    beam("Main cannon",(0,1.05,1.84),(0,3.42,1.84),.12,"steel",12)
    cylinder("Muzzle brake",(0,3.44,1.84),.18,.4,"gunmetal",8,(math.pi/2,0,0))
    cylinder("Muzzle opening",(0,3.645,1.84),.10,.01,"rubber",10,(math.pi/2,0,0),bevel=0)
    cylinder("Commander hatch",(-.38,-.55,2.3),.27,.10,"sand_edge",10)
    beam("Antenna",(.52,-.89,2.05),(.52,-.94,2.98),.025,"gunmetal",6)
    slats((0,-1.61,1.505),5,.10,(1.1,.045,.04))
    light_pair(1.91,1.09,1.25)


def scout():
    for x in (-1.11,1.11):
        for y in (-1.31,1.35):
            wheel(x,y,.57,.57,.42)
    hull("Transport underbody",[(-1.94,.8,.54,1.23,.21),(1.8,.8,.52,1.13,.19)],"olive_dark")
    hull("Troop compartment",[(-1.75,.89,.93,1.97,.24),(.82,.87,.97,1.95,.21),
                              (1.21,.81,.91,1.50,.13)],"sand_armor")
    hull("Engine wedge",[(1.1,.83,.89,1.44,.16),(1.99,.70,.81,1.29,.1)],"olive_armor")
    box("Wide windshield",(0,1.015,1.68),(1.25,.055,.40),"glass",.025,rot=(.48,0,0))
    for side in (-1,1):
        for y in (-1.05,-.35,.38):
            box("Passenger window",(side*.878,y,1.67),(.03,.46,.33),"glass",.03)
        box("Side recognition stripe",(side*.907,-.34,1.19),(.035,2.6,.17),"team_color",.025)
        box("Step rail",(side*1.0,-.12,.71),(.22,2.1,.12),"steel",.03)
        box("Front fender",(side*1.04,1.37,1.08),(.53,1.13,.15),"sand_edge",.04)
        box("Rear fender",(side*1.04,-1.31,1.09),(.53,1.13,.15),"sand_edge",.04)
    cylinder("Roof ring",(0,-.33,2.02),.41,.15,"gunmetal",12)
    box("Gunner shield",(0,-.02,2.28),(.73,.18,.42),"olive_armor",.045)
    beam("Mounted machine gun",(.10,-.17,2.34),(.10,1.06,2.34),.055,"gunmetal")
    box("Rear bumper",(0,-1.96,.73),(2.1,.20,.20),"steel",.035)
    box("Front bumper",(0,2.03,.74),(2.09,.19,.23),"steel",.045)
    light_pair(2.03,1.09,1.34)
    beam("Whip antenna",(-.7,-1.48,1.87),(-.7,-1.53,2.88),.018,"gunmetal",5)


def infantry(rocket=False):
    for side in (-1,1):
        x=side*.16
        y=side*.06
        box("Combat boot",(x,y+.065,.12),(.25,.41,.24),"rubber",.06)
        beam("Lower leg",(x,y,.22),(x,y,.57),.105,"olive_dark")
        beam("Upper leg",(x,y,.55),(x*.80,0,.95),.125,"olive_armor")
        box("Knee armor",(x,y+.105,.52),(.19,.10,.17),"sand_armor",.035)
    hull("Armored torso",[(-.16,.27,.89,1.40,.08),(.17,.27,.91,1.39,.07)],"olive_armor",.025)
    box("Chest plate",(0,.183,1.18),(.43,.12,.37),"sand_armor",.055)
    box("Chest identification",(0,.255,1.26),(.29,.025,.09),"team_color",.015)
    box("Backpack",(0,-.24,1.15),(.40,.23,.47),"olive_dark",.07)
    for x in (-.12,.12):
        box("Belt pouch",(x,.20,.98),(.16,.12,.15),"sand_edge",.03)
    cylinder("Neck",(0,0,1.44),.085,.15,"skin",8)
    ico("Face",(0,.025,1.59),(.15,.145,.19),"skin",2)
    ico("Helmet",(0,-.02,1.70),(.20,.19,.145),"sand_armor",2)
    box("Visor",(0,.153,1.635),(.25,.035,.08),"glass",.02)
    box("Helmet top stripe",(0,-.015,1.841),(.075,.15,.02),"team_color",.005)
    for side in (-1,1):
        shoulder=(side*.31,.02,1.36)
        elbow=(side*.37,.20,1.14 if not rocket else 1.31)
        hand=(side*.16,.38,1.20 if not rocket else 1.47)
        beam("Upper arm",shoulder,elbow,.10,"olive_armor")
        beam("Forearm",elbow,hand,.085,"olive_armor")
        ico("Glove",hand,(.09,.09,.095),"olive_dark",1)
        ico("Shoulder plate",shoulder,(.14,.13,.12),"team_color",1)
    if rocket:
        beam("Rocket launcher",(.25,-.42,1.52),(.25,.78,1.52),.13,"olive_dark",10)
        cylinder("Launcher front rim",(.25,.76,1.52),.16,.10,"sand_edge",10,(math.pi/2,0,0),bevel=.014)
        cylinder("Launcher bore",(.25,.816,1.52),.115,.012,"rubber",10,(math.pi/2,0,0),bevel=0)
        cylinder("Launcher rear rim",(.25,-.44,1.52),.16,.11,"warning",10,(math.pi/2,0,0),bevel=.01)
        box("Launcher sight",(.26,.19,1.70),(.055,.15,.12),"gunmetal",.01)
    else:
        box("Rifle body",(.03,.34,1.20),(.11,.48,.15),"gunmetal",.02)
        beam("Rifle barrel",(.03,.55,1.23),(.03,.88,1.23),.035,"steel")
        box("Rifle magazine",(.03,.36,1.08),(.08,.10,.16),"olive_dark",.015,rot=(.15,0,0))
        box("Rifle stock",(.03,.02,1.22),(.11,.22,.12),"wood",.02)


def gatherer():
    hull("Cargo fuselage",[(-2.72,.46,.61,1.57,.18),(-1.99,1.0,.42,2.11,.34),
                           (1.23,1.02,.48,2.03,.30),(2.5,.64,.74,1.72,.22),
                           (2.90,.24,.98,1.37,.1)],"sand_armor",.10)
    hull("Cockpit canopy",[(1.48,.90,1.25,2.02,.17),(2.42,.59,1.17,1.73,.13)],"glass",.025)
    box("Cockpit central brace",(0,2.07,1.85),(.10,1.1,.07),"sand_edge",.02,rot=(-.26,0,0))
    box("Cargo roof armor",(0,-.33,2.13),(1.39,2.57,.17),"olive_armor",.12)
    box("Dorsal recognition",(0,-.22,2.227),(.62,1.28,.035),"team_color",.025)
    box("Rear cargo ramp",(0,-2.42,.89),(.95,.19,.49),"steel",.05,rot=(-.35,0,0))
    for side in (-1,1):
        box("Cargo access door",(side*1.012,-.39,1.28),(.05,1.42,.94),"olive_armor",.035)
        box("Door stripe",(side*1.042,-.39,1.20),(.028,1.21,.18),"team_color",.015)
        for y in (-1.11,-.41,.29):
            box("Cabin porthole",(side*1.036,y,1.74),(.025,.30,.23),"glass",.055)
        beam("Landing front strut",(side*.68,1.22,.82),(side*.95,1.31,.18),.075)
        beam("Landing rear strut",(side*.71,-1.71,.81),(side*.95,-1.73,.18),.075)
        beam("Landing skid",(side*.95,-2.10,.10),(side*.95,1.83,.10),.10,"gunmetal")
        wing=box("Lift boom",(side*1.86,-.08,1.69),(2.35,.73,.27),"olive_armor",.12)
        wing.rotation_euler.y=side*-.045
        hull("Rotor nacelle",[(-.93,.40,1.57,2.26,.15),(.83,.40,1.56,2.16,.15)],"olive_armor",.075).location.x=side*2.83
        cylinder("Rotor mast",(side*2.83,-.08,2.46),.11,.42,"steel",10)
        cylinder("Rotor hub",(side*2.83,-.08,2.67),.22,.15,"gunmetal",10)
        before=set(bpy.context.scene.objects)
        for i in range(4):
            angle=i*math.pi/2+.22
            # Each blade is tapered and swept, distinct from a rectangular slab.
            verts=[(.16,-.11,0),(2.36,-.06,.015),(2.52,.11,.015),(.52,.21,0),
                   (.16,-.11,.055),(2.36,-.06,.055),(2.52,.11,.055),(.52,.21,.055)]
            blade=mesh("Rotor blade",verts,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],"gunmetal",.009)
            blade.location=(side*2.83,-.08,2.69)
            blade.rotation_euler.z=angle
            tip=box("Rotor tip",(0,0,0),(.24,.18,.064),"sand_edge",.008)
            tip.location=(side*2.83+2.29*math.cos(angle),-.08+2.29*math.sin(angle),2.722)
            tip.rotation_euler.z=angle
        group="rotor_left" if side<0 else "rotor_right"
        for obj in set(bpy.context.scene.objects)-before:
            obj["assembly"]=group
        box("Tail stabilizer",(side*.89,-2.12,1.76),(1.01,.67,.10),"team_color",.05)
    light_pair(2.49,1.08,.85)


def foundation(w,l):
    box("Foundation slab",(0,0,.14),(w,l,.28),"concrete",.14)
    for x in (-w/2+.18,w/2-.18):
        box("Foundation curb",(x,0,.28),(.17,l-.28,.13),"sand_edge",.025)


def door(x,y,z=1.14,w=1.25,h=1.80):
    box("Door frame",(x,y,z),(w+.23,.20,h+.19),"sand_edge",.045)
    box("Recessed entrance",(x,y+.12,z),(w,.04,h),"gunmetal",.025)
    box("Door inset",(x,y+.15,z-.03),(w-.15,.04,h-.12),"olive_dark",.02)
    box("Entrance lintel color",(x,y+.16,z+h/2+.10),(w+.23,.045,.15),"team_color",.02)


def bollards(w,y):
    for x in (-w/2,w/2):
        cylinder("Safety bollard",(x,y,.58),.105,.85,"warning",8)
        cylinder("Bollard stripe",(x,y,.73),.11,.17,"gunmetal",8,bevel=.01)


def command():
    foundation(8.4,7.5)
    hull("Angled operations block",[(-2.89,3.43,.28,2.8,.50),(2.19,3.43,.28,2.8,.50)],"sand_armor",.10)
    box("Command roof rim",(0,-.35,2.88),(7.13,5.39,.27),"sand_edge",.12)
    box("Upper operations tower",(-1.40,-.73,3.6),(3.0,3.02,1.38),"olive_armor",.15)
    box("Tower roof",(-1.4,-.73,4.34),(3.23,3.23,.19),"sand_edge",.07)
    for x in (-2.4,-1.65,-.90,1.22,1.97,2.72):
        box("Operations window",(x,2.23,1.75),(.57,.06,.53),"glass",.04)
    for x in (-2.24,-1.45,-.66):
        box("Upper window",(x,.801,3.64),(.62,.035,.61),"glass",.035)
    door(0,2.28,1.11,1.28,1.58)
    box("Entry canopy",(0,2.75,2.20),(2.1,1.19,.20),"team_color",.075)
    for x in (-.85,.85):
        beam("Canopy support",(x,3.20,.29),(x,3.20,2.12),.055)
    cylinder("Radar pedestal",(1.73,-.56,3.49),.39,1.08,"steel",12)
    # Shallow concave, octagonal dish facing forwards and tilted skyward.
    verts=[(0,0,0)]
    for r,depth in ((.64,-.13),(1.12,-.37)):
        for i in range(16):
            a=i*TAU/16
            verts.append((r*math.cos(a),depth,r*math.sin(a)))
    faces=[(0,1+i,1+(i+1)%16) for i in range(16)]
    faces += [(1+i,17+i,17+(i+1)%16,1+(i+1)%16) for i in range(16)]
    dish=mesh("Radar reflector",verts,faces,"sand_edge",0)
    dish.location=(1.73,-.56,4.29)
    dish.rotation_euler.x=.48
    # Double-sided only for this thin dish surface.
    sol=dish.modifiers.new("Reflector thickness","SOLIDIFY")
    sol.thickness=.04
    bpy.context.view_layer.objects.active=dish
    bpy.ops.object.modifier_apply(modifier=sol.name)
    beam("Radar feed",(1.73,-.56,4.29),(1.73,.06,4.63),.065,"gunmetal")
    beam("Comms mast",(-2.17,-1.46,4.45),(-2.17,-1.46,6.13),.045)
    for z in (5.08,5.61):
        beam("Comms element",(-2.63,-1.46,z),(-1.71,-1.46,z),.035)
    box("Recognition roof panel",(-1.4,-.73,4.452),(1.59,1.16,.04),"team_color",.03)
    box("Service cabinet",(3.29,-1.46,1.18),(1.22,1.66,1.81),"olive_dark",.07)
    slats((3.917,-1.46,1.28),5,.24,(.025,.13,.77),"steel")
    bollards(2.8,3.2)


def fusion():
    foundation(6.5,6.2)
    box("Power control house",(-1.85,.4,1.19),(2.15,3.88,1.85),"sand_armor",.11)
    box("Control house roof",(-1.85,.4,2.16),(2.4,4.13,.20),"olive_armor",.08)
    door(-1.85,2.35,1.16,1.01,1.45)
    for y in (-1.39,1.07):
        cylinder("Reactor containment",(.8,y,1.56),1.04,2.51,"olive_armor",12,radius2=.90,bevel=.06)
        for z in (.49,1.45,2.51):
            cylinder("Containment band",(.8,y,z),1.105,.23,"sand_edge",12,bevel=.035)
        cylinder("Core crown",(.8,y,2.96),.65,.55,"steel",12,radius2=.49)
        cylinder("Core indicator",(.8,y,3.19),.48,.11,"energy",12)
        for i in range(6):
            a=i*TAU/6
            x=.8+1.057*math.cos(a)
            yy=y+1.057*math.sin(a)
            box("Vertical cooling fin",(x,yy,1.62),(.13,.14,1.57),"steel",.02,rot=(0,0,a))
        box("Plant team panel",(.8,y,2.71),(.54,.58,.06),"team_color",.025)
    beam("Coolant manifold",(2.16,-1.4,.69),(2.16,1.33,.69),.19,"steel",10)
    for y in (-1.4,1.33):
        beam("Coolant return",(1.18,y,.69),(2.16,y,.69),.19,"steel",10)
    slats((-1.85,-.32,2.279),7,.30,(1.65,.095,.04))
    box("Electrical cabinet",(2.46,.1,1.1),(.85,1.0,1.64),"sand_armor",.065)
    box("Power status panel",(2.46,.619,1.43),(.52,.045,.43),"energy",.025)


def barracks():
    foundation(6.7,7.3)
    box("Barracks walls",(0,-.28,1.28),(4.85,5.94,2.02),"sand_armor",.10)
    arch_roof("Barrel roof",(0,-.28,2.24),5.3,6.3,1.11,"olive_armor",10)
    for y in (-2.80,-1.75,-.7,.35,1.4,2.45):
        arch_roof("Roof rib",(0,y,2.24),5.38,.09,1.16,"sand_edge",10)
    door(0,2.77,1.28,1.28,1.80)
    box("Entry shade",(0,3.00,2.39),(2.73,.86,.18),"team_color",.065)
    for side in (-1,1):
        for y in (-2,-.6,.8):
            box("Barracks window",(side*2.433,y,1.57),(.05,.68,.59),"glass",.03)
        box("Front side vent",(side*1.65,2.704,1.64),(.64,.06,.38),"olive_dark",.025)
    box("Roof team panel",(0,.1,3.381),(1.01,2.8,.025),"team_color",.02)
    for x in (-2.7,2.7):
        box("Utility storage",(x,-2.4,.7),(.53,1.10,.83),"olive_dark",.04)
    beam("Pennant pole",(-2.95,2.92,.30),(-2.95,2.92,3.68),.035)
    mesh("Recognition pennant",[(-2.95,2.92,3.64),(-2.05,2.92,3.50),(-2.95,2.92,3.16)],[(0,1,2),(2,1,0)],"team_color",0)
    bollards(2.42,3.13)


def factory():
    foundation(10.1,9.7)
    hull("Vehicle hall",[(-3.73,3.80,.28,4.10,.35),(3.15,3.80,.28,4.10,.35)],"sand_armor",.12)
    arch_roof("Factory vault",(0,-.29,4.0),8.03,7.29,1.02,"olive_armor",8)
    for x in (-2.38,0,2.38):
        box("Skylight frame",(x,-.5,4.93 if x==0 else 4.72),(1.10,4.94,.17),"sand_edge",.05)
        box("Smoked skylight",(x,-.5,5.025 if x==0 else 4.815),(.84,4.64,.035),"glass",.02)
    box("Bay aperture",(0,3.17,1.97),(5.26,.07,3.33),"gunmetal",.04)
    for x in (-2.86,2.86):
        box("Bay jamb",(x,3.28,2.0),(.50,.36,3.53),"sand_edge",.07)
        for z in (.75,1.70,2.65):
            box("Bay caution inlay",(x,3.474,z),(.33,.024,.30),"warning",.01)
    box("Bay recognition lintel",(0,3.38,3.83),(6.28,.45,.39),"team_color",.05)
    for z in (2.56,2.88,3.2):
        box("Raised shutter segments",(0,3.24,z),(5.0,.12,.21),"steel",.025)
    box("Service apron",(0,3.98,.31),(6.73,1.66,.12),"olive_dark",.045)
    for x in (-2.05,2.05):
        box("Vehicle guide stripe",(x,3.99,.38),(.10,1.49,.014),"warning",.001)
    box("Service annex",(-4.02,-.62,1.48),(1.63,5.37,2.36),"olive_armor",.10)
    slats((-4.855,-.62,1.6),10,.36,(.04,.16,1.26),"steel")
    for x in (-2.0,2.0):
        cylinder("Vent turbine",(x,-2.87,4.77),.40,.62,"steel",12)
        cylinder("Vent cowl",(x,-2.87,5.10),.54,.16,"sand_edge",12)
    box("Roof recognition panel",(0,1.30,5.04),(1.04,1.24,.04),"team_color",.025)


def crate(loc,size=(1.0,.85,.8),mat="wood"):
    x,y,z=loc
    w,l,h=size
    box("Cargo case",loc,size,mat,.055)
    for sx in (-1,1):
        box("Case edge brace",(x+sx*w*.42,y,z),(.09,l+.035,h+.04),"sand_edge",.015)
    for sy in (-1,1):
        box("Case band",(x,y+sy*l*.43,z), (w+.035,.075,h+.04),"steel",.012)


def dropoff():
    foundation(8.9,8.5)
    box("Receiving warehouse",(-1.83,-.60,1.66),(4.42,5.42,2.74),"sand_armor",.11)
    arch_roof("Receiving roof",(-1.83,-.60,2.99),4.68,5.63,.63,"olive_armor",6)
    box("Warehouse opening",(-1.84,2.137,1.62),(2.91,.05,2.31),"gunmetal",.045)
    box("Receiving stripe",(-1.84,2.20,2.98),(3.39,.17,.28),"team_color",.035)
    box("Loading platform",(1.50,.08,.47),(4.52,6.47,.39),"olive_dark",.07)
    for y in (-2.39,2.48):
        box("Hoist pillar",(3.12,y,2.22),(.23,.25,3.50),"steel",.035)
        box("Hoist column marking",(3.12,y,1.10),(.27,.29,.49),"warning",.025)
    beam("Overhead cargo rail",(3.12,-2.63,3.92),(3.12,2.68,3.92),.13,"steel")
    box("Hoist head",(2.69,.0,3.91),(1.1,.56,.33),"warning",.05)
    beam("Lifting cable",(2.25,0,3.76),(2.25,0,2.45),.025,"gunmetal",6)
    beam("Hook neck",(2.25,0,2.45),(2.25,0,2.29),.075,"steel")
    for pos in ((1.45,-1.77,1.09),(2.68,-1.65,1.09),(1.47,-1.72,1.94),(1.38,1.80,1.09)):
        crate(pos,(1.03,.97,.85),"olive_armor")
    for x in (.65,3.33):
        box("Loading guide",(x,.48,.681),(.10,1.7,.018),"warning",.003)
    box("Roof recognition panel",(-1.83,-.2,3.641),(1.12,2.45,.035),"team_color",.025)
    bollards(7.52,3.52)


def aa_turret():
    cylinder("Turret foundation",(0,0,.16),2.04,.32,"concrete",8,bevel=.065)
    cylinder("Octagonal plinth",(0,0,.54),1.33,.63,"sand_armor",8,radius2=1.05,bevel=.045)
    cylinder("Rotation race",(0,0,.93),.74,.18,"gunmetal",12)
    box("Launcher core",(0,-.18,1.50),(1.33,1.40,.94),"olive_armor",.11)
    for side in (-1,1):
        beam("Elevation trunnion",(side*.42,0,1.48),(side*.96,0,1.48),.23,"steel",10)
        # Two compact launch cells per side; raised front gives a true AA silhouette.
        for x in (side*.92,side*1.38):
            tube=box("Elevated launch cell",(x,0,1.97),(.39,2.34,.42),"sand_armor",.065,rot=(.45,0,0))
            box("Launch cell front",(x,1.079,2.49),(.30,.06,.32),"gunmetal",.035,rot=(.45,0,0))
            box("Launch cell back",(x,-1.08,1.45),(.30,.055,.30),"warning",.025,rot=(.45,0,0))
        box("Team armor panel",(side*.679,-.24,1.59),(.05,.73,.34),"team_color",.025)
    beam("Targeting mast",(0,-.70,1.69),(0,-.70,2.71),.065)
    box("Targeting array",(0,-.69,2.71),(.96,.21,.54),"steel",.045,rot=(.12,0,0))
    box("Array face",(0,-.566,2.73),(.78,.026,.38),"glass",.025,rot=(.12,0,0))
    box("Top team panel",(0,-.11,1.997),(.61,.64,.035),"team_color",.025)


def dock():
    foundation(7.75,6.95)
    box("Supply loading deck",(0,0,.39),(7.18,6.32,.30),"olive_dark",.08)
    for x in (-2.35,0,2.35):
        for y in (-1.89,-.51):
            crate((x,y,1.16),(1.95,1.13,1.22),"sand_armor" if x==0 else "olive_armor")
        if x!=0:
            crate((x,-1.58,2.36),(1.87,1.10,1.12),"wood")
    for y in (.66,1.9):
        crate((-2.24,y,1.05),(1.90,.99,.99),"wood")
    for x in (.22,1.01,1.80):
        cylinder("Sealed supply drum",(x,-2.38,1.10),.30,1.04,"steel",10)
        cylinder("Drum closure",(x,-2.38,1.63),.28,.045,"sand_edge",10,bevel=.01)
    for x in (-.27,2.59):
        box("Receiving lane stripe",(x,1.43,.557),(.13,2.27,.014),"warning",.001)
    for x in (-3.35,3.35):
        cylinder("Stack marker post",(x,-2.83,1.33),.055,2.01,"steel",8)
        box("Stack caution flag",(x,-2.83,2.27),(.32,.13,.33),"warning",.03)


def garrison():
    foundation(6.5,5.7)
    box("Civil dwelling",(0,-.11,2.34),(5.28,4.4,4.13),"plaster",.12)
    box("Lower story course",(0,-.11,.56),(5.41,4.50,.31),"roof_clay",.055)
    box("Middle story course",(0,-.11,2.48),(5.39,4.50,.19),"sand_edge",.035)
    box("Flat roof",(0,-.11,4.43),(5.5,4.6,.23),"roof_clay",.06)
    for x in (-2.65,2.65):
        box("Roof parapet",(x,-.11,4.72),(.18,4.53,.48),"plaster",.04)
    for y in (-2.27,2.05):
        box("Roof parapet",(0,y,4.72),(5.43,.19,.48),"plaster",.04)
    for z in (1.48,3.42):
        for x in (-1.61,1.61):
            box("Window surround",(x,2.135,z),(.91,.11,1.04),"sand_edge",.04)
            box("Dark window",(x,2.206,z),(.66,.03,.80),"glass",.02)
            box("Window mullion",(x,2.233,z),(.06,.035,.79),"roof_clay",.008)
            box("Window sill",(x,2.27,z-.45),(1.04,.24,.12),"roof_clay",.025)
    for side in (-1,1):
        for y in (-1.34,.53):
            for z in (1.48,3.42):
                box("Side window",(side*2.654,y,z),(.035,.72,.80),"glass",.025)
                box("Side sill",(side*2.70,y,z-.45),(.19,.91,.12),"roof_clay",.025)
    # A neutral identity stripe becomes ownership color when occupied.
    box("Occupancy panel",(0,2.213,2.67),(1.49,.055,.28),"team_color",.025)
    door(0,2.21,1.13,1.05,1.71)
    box("Front stoop",(0,2.64,.41),(1.64,.90,.27),"concrete",.045)
    box("Door shade",(0,2.58,2.21),(1.94,.99,.15),"wood",.045)
    cylinder("Rooftop cistern",(-1.38,-.71,5.03),.56,1.02,"olive_dark",12,bevel=.06)
    cylinder("Cistern lid",(-1.38,-.71,5.59),.62,.13,"sand_edge",12)
    box("Roof utility hut",(1.29,-.92,4.89),(1.50,1.55,.86),"plaster",.075)
    box("Utility hut roof",(1.29,-.92,5.36),(1.66,1.70,.14),"roof_clay",.045)


def rocks():
    rng=random.Random(217)
    for i,(loc,scale) in enumerate([((-.86,.1,.76),(1.43,1.0,.91)),
                                   ((.83,.49,.57),(.92,1.08,.66)),
                                   ((.61,-.85,.28),(.76,.51,.33))]):
        obj=ico("Weathered boulder",loc,scale,"stone",2)
        for vert in obj.data.vertices:
            vert.co *= rng.uniform(.82,1.13)
        obj.rotation_euler=(.04*i,.18*i,.47*i)
        obj.data.materials.append(MATERIALS["stone_light"])
        for poly in obj.data.polygons:
            poly.material_index=1 if poly.normal.z>.32 else 0
    for x,y in ((-1.70,-.49),(1.69,-.59),(.22,1.53)):
        for i in range(5):
            a=i*TAU/5
            beam("Dry scrub stem",(x,y,.03),(x+.27*math.cos(a),y+.27*math.sin(a),.47),.035,"scrub",5)


BUILDERS = {
    "dozer":("build.dozer",dozer),
    "gatherer":("eco.chinook",gatherer),
    "fusion":("power.fusion",fusion),
    "command":("prod.command",command),
    "barracks":("prod.barracks",barracks),
    "factory":("prod.factory",factory),
    "dropoff":("eco.dropoff",dropoff),
    "rifle":("inf.rifle",lambda:infantry(False)),
    "rocket":("inf.rocket",lambda:infantry(True)),
    "tank":("armor.basic",tank),
    "scout":("veh.scout_gun",scout),
    "aa_turret":("def.patriot",aa_turret),
    "dock":("map.dock",dock),
    "garrison":("map.garrison",garrison),
    "rocks":("ter.unbuildable",rocks),
}


def reset():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for mesh_data in list(bpy.data.meshes):
        if not mesh_data.users:
            bpy.data.meshes.remove(mesh_data)


def object_bounds(objects):
    pts=[obj.matrix_world@vertex.co for obj in objects for vertex in obj.data.vertices]
    lo=Vector(tuple(min(v[i] for v in pts) for i in range(3)))
    hi=Vector(tuple(max(v[i] for v in pts) for i in range(3)))
    return lo,hi


def combine(name):
    """Keep one body mesh and optional named rotor meshes; bake static transforms."""
    objects=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    bpy.context.view_layer.update()
    lo,_=object_bounds(objects)
    for obj in objects:
        obj.location.z-=lo.z
    bpy.context.view_layer.update()
    groups={}
    for obj in objects:
        groups.setdefault(obj.get("assembly","body"),[]).append(obj)
    combined=[]
    for key,items in groups.items():
        bpy.ops.object.select_all(action="DESELECT")
        for obj in items:
            obj.select_set(True)
        bpy.context.view_layer.objects.active=items[0]
        bpy.ops.object.join()
        obj=bpy.context.object
        obj.name=key
        if key.startswith("rotor"):
            side=-1 if key=="rotor_left" else 1
            bpy.context.scene.cursor.location=(side*2.83,-.08,2.69-lo.z)
        else:
            bpy.context.scene.cursor.location=(0,0,0)
        bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
        # Bake rotation and scale so named rotor-local Y is glTF vertical Y.
        bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
        obj.data.name=name+"_"+key
        combined.append(obj)
    bpy.context.scene.cursor.location=(0,0,0)
    root=bpy.data.objects.new(name,None)
    bpy.context.collection.objects.link(root)
    for obj in combined:
        obj.parent=root
    bpy.context.view_layer.update()
    return root,combined


def render_preview(name,objects):
    lo,hi=object_bounds(objects)
    center=(lo+hi)/2
    span=max(hi.x-lo.x,hi.y-lo.y,hi.z-lo.z)
    scene=bpy.context.scene
    scene.render.engine="CYCLES"
    scene.cycles.samples=24
    scene.cycles.use_denoising=True
    scene.render.resolution_x=640
    scene.render.resolution_y=640
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format="PNG"
    scene.render.film_transparent=False
    if scene.world is None:
        scene.world=bpy.data.worlds.new("Preview environment")
    scene.world.use_nodes=True
    scene.world.node_tree.nodes["Background"].inputs[0].default_value=(.115,.15,.165,1)
    scene.world.node_tree.nodes["Background"].inputs[1].default_value=.6
    scene.view_settings.view_transform="AgX"
    stage=box("Preview ground",(0,0,-.07),(200,200,.1),"olive_dark",0)
    camera_data=bpy.data.cameras.new("Preview camera")
    camera=bpy.data.objects.new("Preview camera",camera_data)
    scene.collection.objects.link(camera)
    camera.location=center+Vector((1.1,1.6,1.25))*span
    camera.rotation_euler=(center-camera.location).to_track_quat("-Z","Y").to_euler()
    camera_data.type="ORTHO"
    view=camera.rotation_euler.to_matrix().transposed()
    projected=[view@(obj.matrix_world@v.co-center) for obj in objects for v in obj.data.vertices]
    xmin,xmax=min(v.x for v in projected),max(v.x for v in projected)
    ymin,ymax=min(v.y for v in projected),max(v.y for v in projected)
    camera_data.ortho_scale=max(xmax-xmin,ymax-ymin)*1.16
    camera.location+=camera.rotation_euler.to_matrix()@Vector(((xmin+xmax)/2,(ymin+ymax)/2,0))
    scene.camera=camera
    for loc,energy,size,color in [((1,2,5),1500,5,(1,.86,.65)),((-3,-1,3),950,4,(.63,.80,1))]:
        light_data=bpy.data.lights.new("Preview softbox","AREA")
        light_data.energy=energy*(span/5)**2
        light_data.shape="DISK"
        light_data.size=size*span/5
        light_data.color=color
        light=bpy.data.objects.new("Preview softbox",light_data)
        scene.collection.objects.link(light)
        light.location=center+Vector(loc)*span/3
        light.rotation_euler=(center-light.location).to_track_quat("-Z","Y").to_euler()
    scene.render.filepath=str(PREVIEWS/(name+".png"))
    bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(stage,do_unlink=True)
    for obj in list(scene.objects):
        if obj.type in {"CAMERA","LIGHT"}:
            bpy.data.objects.remove(obj,do_unlink=True)


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--only",nargs="*",choices=list(BUILDERS))
    parser.add_argument("--render",action="store_true")
    args=parser.parse_args(sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else [])
    for folder in (MODELS,SOURCES,PREVIEWS):
        folder.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.unit_settings.system="METRIC"
    bpy.context.scene.unit_settings.scale_length=1
    bpy.context.preferences.filepaths.save_version=0
    palette()
    records={}
    manifest=MODELS/"manifest.json"
    if args.only and manifest.exists():
        records=json.loads(manifest.read_text())["assets"]
    for name,(role,builder) in BUILDERS.items():
        if args.only and name not in args.only:
            continue
        reset()
        builder()
        root,objects=combine(name)
        lo,hi=object_bounds(objects)
        # Blender XYZ -> glTF X, Z, -Y. Extents are purely visual.
        bounds_min=[round(lo.x,4),round(lo.z,4),round(-hi.y,4)]
        bounds_max=[round(hi.x,4),round(hi.z,4),round(-lo.y,4)]
        tris=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects)
        bpy.ops.object.select_all(action="SELECT")
        # Keep False unless exporting an allowlisted gatherer rotor clip.
        bpy.ops.export_scene.gltf(filepath=str(MODELS/(name+".glb")),export_format="GLB",
            use_selection=True,export_yup=True,export_apply=True,export_materials="EXPORT",
            export_cameras=False,export_lights=False,export_animations=False,export_extras=False)
        exported=(MODELS/(name+".glb")).read_bytes()
        json_length=struct.unpack_from("<I",exported,12)[0]
        exported_json=json.loads(exported[20:20+json_length])
        tris=sum(exported_json["accessors"][primitive["indices"]]["count"]//3
                 for mesh_data in exported_json["meshes"] for primitive in mesh_data["primitives"])
        # Unjoined procedural parts remain reproducible in this script. The native
        # Blender source is the exact exported mesh with editable materials.
        bpy.ops.wm.save_as_mainfile(filepath=str(SOURCES/(name+".blend")),compress=True)
        records[name]={"catalog_id":role,"resource":"res://assets/models/"+name+".glb",
                       "bounds_min":bounds_min,"bounds_max":bounds_max,
                       "dimensions":[round(bounds_max[i]-bounds_min[i],4) for i in range(3)],
                       "triangles":tris,"team_material":"team_color" if name not in {"dock","rocks"} else None,
                       "assemblies":[o.name for o in objects]}
        if args.render:
            render_preview(name,objects)
        print("ASSET",name,records[name],flush=True)
    manifest.write_text(json.dumps({"schema_version":1,"units":"metres","up":"+Y",
        "forward":"-Z","origin":"ground center","purpose":"visual geometry only; no gameplay numbers",
        "assets":records},indent=2)+"\n")


if __name__=="__main__":
    main()
