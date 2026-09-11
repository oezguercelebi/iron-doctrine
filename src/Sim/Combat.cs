using System;
using System.Linq;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

internal sealed partial class Match
{
    private int DamagePercent(string damageId, Body victim)
    {
        var damage = C.Damage.FirstOrDefault(d => d.Id == damageId);
        var armor = victim.RoleId == "map.garrison" && victim.Occupants.Count != 0 ? "arm.garrison" : Role(victim).ArmorId;
        return damage != null && damage.ArmorPercent.TryGetValue(armor, out int percent) ? percent : 0;
    }
    private Body? Acquire(Body unit, int range, WorldPoint? origin = null)
    {
        if (unit.Owner < 0 || Role(unit).Damage <= 0) return null;
        var pos = origin ?? unit.Pos;
        return Bodies.Values.Where(b => b.ContainerId == 0 && Enemy(unit.Owner, b.Owner) && !Role(b).Indestructible && Visible(unit.Owner, b.Pos) && DamagePercent(Role(unit).DamageId, b) > 0 && Near(pos, b.Pos, range + Role(b).Radius))
            .OrderBy(b => Distance2(pos, b.Pos)).ThenBy(b => b.Id).FirstOrDefault();
    }
    private void AutoFire(Body unit, WorldPoint origin)
    {
        var enemy = Acquire(unit, Role(unit).AttackRange, origin);
        if (enemy != null) Fire(unit, enemy, origin);
    }
    private void Engage(Body attacker, Body victim)
    {
        attacker.TargetId = victim.Id; attacker.Activity = EntityActivity.Attacking;
        if (!Near(attacker.Pos, victim.Pos, Role(attacker).AttackRange + Role(victim).Radius)) MoveToward(attacker, victim.Pos, Role(attacker).AttackRange + Role(victim).Radius);
        else Fire(attacker, victim, attacker.Pos);
    }
    private void AttackAction(Body unit, MatchOrder action)
    {
        if (action.TargetId == 0)
        {
            unit.Activity = EntityActivity.Attacking;
            if (!Near(unit.Pos, action.Position, Role(unit).AttackRange)) { MoveToward(unit, action.Position, Role(unit).AttackRange); return; }
            if (unit.Cooldown != 0) return;
            unit.Cooldown = Math.Max(1, Role(unit).AttackCooldownTicks * C.Ranks[unit.Rank].CooldownPercent / 100);
            if (Role(unit).DeliveryId == "del.missile")
                shots.Add(new Shot { Id = nextShotId++, Owner = unit.Owner, Source = unit.Id, Target = 0, Pos = unit.Pos, TargetPos = action.Position,
                    Damage = Role(unit).Damage * C.Ranks[unit.Rank].DamagePercent / 100, DamageId = Role(unit).DamageId, Speed = Role(unit).ProjectileSpeedPerTick });
            else HitGround(action.Position, Role(unit).DamageId, Role(unit).Damage * C.Ranks[unit.Rank].DamagePercent / 100, unit);
            return;
        }
        if (Bodies.TryGetValue(action.TargetId, out var target) && (target.Owner == unit.Owner || Visible(unit.Owner, target.Pos)) && target.ContainerId == 0 && (action.Kind == OrderKind.ForceAttack || Enemy(unit.Owner, target.Owner)))
        {
            unit.Actions[0] = action with { Position = target.Pos };
            Engage(unit, target);
        }
        else if (MoveToward(unit, action.Position, C.Rules.InteractionRange) != TravelResult.Moving) FinishAction(unit);
    }
    private void Fire(Body attacker, Body victim, WorldPoint origin)
    {
        if (attacker.Cooldown != 0 || !attacker.Powered || DamagePercent(Role(attacker).DamageId, victim) == 0) return;
        attacker.TargetId = victim.Id;
        if (attacker.ContainerId == 0) attacker.Activity = EntityActivity.Attacking;
        attacker.Cooldown = Math.Max(1, Role(attacker).AttackCooldownTicks * C.Ranks[attacker.Rank].CooldownPercent / 100);
        Launch(attacker, victim, origin);
    }
    private void Launch(Body source, Body target, WorldPoint origin)
    {
        var role = Role(source);
        var damage = role.Damage * C.Ranks[source.Rank].DamagePercent / 100;
        if (role.DeliveryId == "del.missile") shots.Add(new Shot { Id = nextShotId++, Owner = source.Owner, Source = source.Id, Target = target.Id, Pos = origin, TargetPos = target.Pos, Damage = damage, DamageId = role.DamageId, Speed = role.ProjectileSpeedPerTick });
        else Hit(target, damage * DamagePercent(role.DamageId, target) / 100, source);
    }
    private void StepShots()
    {
        foreach (var shot in shots.ToArray())
        {
            if (shot.Target == 0)
            {
                shot.Pos = Toward(shot.Pos, shot.TargetPos, shot.Speed);
                if (Near(shot.Pos, shot.TargetPos, C.Rules.MissileHitRadius))
                {
                    Bodies.TryGetValue(shot.Source, out var shooter);
                    HitGround(shot.TargetPos, shot.DamageId, shot.Damage, shooter); shots.Remove(shot);
                }
                continue;
            }
            if (!Bodies.TryGetValue(shot.Target, out var victim) || victim.ContainerId != 0) { shots.Remove(shot); continue; }
            shot.TargetPos = victim.Pos;
            shot.Pos = Toward(shot.Pos, shot.TargetPos, shot.Speed);
            if (!Near(shot.Pos, shot.TargetPos, C.Rules.MissileHitRadius + Role(victim).Radius)) continue;
            Bodies.TryGetValue(shot.Source, out var source);
            Hit(victim, shot.Damage * DamagePercent(shot.DamageId, victim) / 100, source);
            shots.Remove(shot);
        }
    }
    private void HitGround(WorldPoint position, string damageId, int damage, Body? source)
    {
        foreach (var victim in Bodies.Values.Where(b => b.ContainerId == 0 && !Role(b).Indestructible && Near(b.Pos, position, C.Rules.GroundForceAttackRadius + Role(b).Radius)).ToArray())
            Hit(victim, damage * DamagePercent(damageId, victim) / 100, source);
    }
    internal void Hit(Body victim, int damage, Body? killer)
    {
        if (Role(victim).Indestructible || damage <= 0 || !Bodies.ContainsKey(victim.Id)) return;
        victim.Hp -= damage; victim.LastHit = Tick;
        if (victim.CaptureLeft > 0) FinishAction(victim);
        if (victim.Owner >= 0)
        {
            Event("fx.under_attack", victim.Owner, victim);
            if (Role(victim).IsBuilding) Event("vo.under_attack", victim.Owner, victim);
        }
        if (victim.Hp <= 0) Destroy(victim, killer);
    }
    internal void Destroy(Body victim, Body? killer)
    {
        if (!Bodies.Remove(victim.Id)) return;
        if (Obstacle(victim)) topology++;
        ReleaseDock(victim);
        if (victim.BuilderId != 0 && Bodies.TryGetValue(victim.BuilderId, out var builder)) { builder.ConstructionId = 0; FinishAction(builder); }
        if (victim.ConstructionId != 0 && Bodies.TryGetValue(victim.ConstructionId, out var site)) Destroy(site, null);
        if (victim.ContainerId != 0 && Bodies.TryGetValue(victim.ContainerId, out var parent))
        {
            parent.Occupants.Remove(victim.Id);
            if (parent.RoleId == "map.garrison" && parent.Occupants.Count == 0) { Abort(parent); parent.Owner = -1; }
        }
        foreach (var id in victim.Occupants.ToArray())
        {
            if (!Bodies.TryGetValue(id, out var passenger)) continue;
            if (victim.RoleId != "map.garrison") { Destroy(passenger, killer); continue; }
            passenger.ContainerId = 0;
            passenger.Hp = Math.Max(1, passenger.Hp * C.Rules.GarrisonSpillHpPercent / 100);
            var point = FindFree(victim.Pos, Role(passenger), passenger.Id);
            if (point == null) { Destroy(passenger, null); continue; }
            passenger.Pos = point.Value; passenger.Destination = point.Value; passenger.Activity = EntityActivity.Idle;
        }
        if (killer != null && Bodies.ContainsKey(killer.Id) && Enemy(killer.Owner, victim.Owner))
        {
            int oldMax = MaxHp(killer);
            killer.Experience += Role(victim).KillExperience;
            while (killer.Rank + 1 < C.Ranks.Length && killer.Experience >= C.Ranks[killer.Rank + 1].Experience) killer.Rank++;
            if (MaxHp(killer) > oldMax) { killer.Hp += MaxHp(killer) - oldMax; Event("fx.promote", killer.Owner, killer); }
        }
    }
}
