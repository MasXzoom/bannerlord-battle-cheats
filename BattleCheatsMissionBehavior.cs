using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleCheats;

public sealed class BattleCheatsMissionBehavior : MissionBehavior
{
    private float _killAuraTimer;
    private float _ammoTimer;
    private const float KillAuraRadius = 10f;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        try
        {
            Mission mission = Mission.Current;
            if (mission == null || mission.MainAgent == null)
                return;

            if (BattleCheatsSettings.KillAura)
            {
                _killAuraTimer += dt;
                if (_killAuraTimer >= 1f)
                {
                    _killAuraTimer = 0f;
                    RunKillAura(mission);
                }
            }

            if (BattleCheatsSettings.InfiniteAmmo)
            {
                _ammoTimer += dt;
                if (_ammoTimer >= 1f)
                {
                    _ammoTimer = 0f;
                    RefillAmmo(mission.MainAgent);
                }
            }

            if (BattleCheatsSettings.GodMode && mission.MainAgent.Health < mission.MainAgent.HealthLimit)
                mission.MainAgent.Health = mission.MainAgent.HealthLimit;

            if (BattleCheatsSettings.TroopsInvulnerable)
            {
                foreach (Agent agent in mission.AllAgents)
                {
                    if (agent != null && agent.IsActive() && agent != mission.MainAgent
                        && !mission.MainAgent.IsEnemyOf(agent) && agent.Health < agent.HealthLimit)
                        agent.Health = agent.HealthLimit;
                }
            }
        }
        catch (Exception ex)
        {
            BattleCheatsSubModule.WriteRuntimeLog("OnMissionTick error: " + ex.Message);
        }
    }

    private static void RefillAmmo(Agent agent)
    {
        try
        {
            for (int i = 0; i < 12; i++)
            {
                EquipmentIndex slot = (EquipmentIndex)i;
                MissionWeapon weapon = agent.Equipment[slot];
                if (weapon.IsEmpty) continue;
                if (!weapon.IsAnyAmmo()) continue;
                short max = weapon.ModifiedMaxAmount;
                if (max > 0 && weapon.Amount < max)
                    agent.SetWeaponAmountInSlot(slot, max, true);
            }
        }
        catch { }
    }

    public override void OnMeleeHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
    {
        base.OnMeleeHit(attacker, victim, isCanceled, collisionData);
        HandleHit(attacker, victim);
    }

    public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
    {
        base.OnMissileHit(attacker, victim, isCanceled, collisionData);
        HandleHit(attacker, victim);
    }

    private void HandleHit(Agent attacker, Agent victim)
    {
        try
        {
            if (attacker == null || victim == null)
                return;
            Mission mission = Mission.Current;
            if (mission == null)
                return;
            Agent player = mission.MainAgent;
            if (player == null)
                return;

            bool victimIsPlayer = victim == player;
            bool attackerIsPlayer = attacker == player;

            if (BattleCheatsSettings.GodMode && victimIsPlayer)
            {
                player.Health = player.HealthLimit;
                return;
            }

            if (BattleCheatsSettings.TroopsInvulnerable && !victimIsPlayer && !attackerIsPlayer
                && victim.IsActive() && !player.IsEnemyOf(victim))
            {
                victim.Health = victim.HealthLimit;
                return;
            }

            if (attackerIsPlayer && victim.IsActive() && player.IsEnemyOf(victim))
                HandlePlayerHitEnemy(player, victim);

            if (BattleCheatsSettings.TroopsOneHit && !attackerIsPlayer && !victimIsPlayer
                && attacker.IsActive() && victim.IsActive()
                && !player.IsEnemyOf(attacker) && player.IsEnemyOf(victim))
            {
                BattleCheatsSubModule.KillEnemy(victim, attacker);
            }
        }
        catch (Exception ex)
        {
            BattleCheatsSubModule.WriteRuntimeLog("HandleHit error: " + ex.Message);
        }
    }

    private static void HandlePlayerHitEnemy(Agent player, Agent victim)
    {
        switch (BattleCheatsSubModule.CurrentDamageMode)
        {
            case DamageMode.OneHit:
                BattleCheatsSubModule.KillEnemy(victim, player);
                break;
            case DamageMode.TwoHit:
                int hits = BattleCheatsSubModule.HitCounts.TryGetValue(victim, out int c) ? c : 0;
                hits++;
                if (hits >= 2)
                {
                    BattleCheatsSubModule.HitCounts.Remove(victim);
                    BattleCheatsSubModule.KillEnemy(victim, player);
                }
                else
                    BattleCheatsSubModule.HitCounts[victim] = hits;
                break;
        }
    }

    private static void RunKillAura(Mission mission)
    {
        Agent player = mission.MainAgent;
        int count = 0;
        foreach (Agent agent in mission.AllAgents)
        {
            if (agent != null && agent.IsActive() && player.IsEnemyOf(agent))
            {
                float distSq = agent.Position.DistanceSquared(player.Position);
                if (distSq <= KillAuraRadius * KillAuraRadius)
                {
                    BattleCheatsSubModule.KillEnemy(agent, player);
                    count++;
                }
            }
        }
        if (count > 0)
            BattleCheatsSubModule.WriteRuntimeLog("Kill aura: " + count);
    }
}
