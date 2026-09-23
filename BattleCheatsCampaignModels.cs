using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BattleCheats;

public sealed class BattleCheatsWageModel : PartyWageModel
{
    private readonly PartyWageModel _wrapped;

    public BattleCheatsWageModel(PartyWageModel wrapped) { _wrapped = wrapped; }

    public override int MaxWagePaymentLimit => _wrapped.MaxWagePaymentLimit;
    public override int GetCharacterWage(CharacterObject character) => _wrapped.GetCharacterWage(character);
    public override ExplainedNumber GetTotalWage(MobileParty mobileParty, TroopRoster troopRoster, bool includeDescriptions = false)
        => _wrapped.GetTotalWage(mobileParty, troopRoster, includeDescriptions);

    public override ExplainedNumber GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
    {
        if (BattleCheatsSettings.FreeRecruit)
            return new ExplainedNumber(0f);
        return _wrapped.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost);
    }
}

public sealed class BattleCheatsHealingModel : PartyHealingModel
{
    private readonly PartyHealingModel _wrapped;

    public BattleCheatsHealingModel(PartyHealingModel wrapped) { _wrapped = wrapped; }

    public override float GetSurgeryChance(PartyBase party) => _wrapped.GetSurgeryChance(party);
    public override float GetSiegeBombardmentHitSurgeryChance(PartyBase party) => _wrapped.GetSiegeBombardmentHitSurgeryChance(party);
    public override int GetSkillXpFromHealingTroop(PartyBase party) => _wrapped.GetSkillXpFromHealingTroop(party);

    public override float GetSurvivalChance(PartyBase party, CharacterObject agentCharacter, DamageTypes damageType, bool canDamageKillEvenIfBlunt, PartyBase enemyParty = null)
        => _wrapped.GetSurvivalChance(party, agentCharacter, damageType, canDamageKillEvenIfBlunt, enemyParty);

    public override ExplainedNumber GetDailyHealingForRegulars(PartyBase partyBase, bool isPrisoner, bool includeDescriptions = false)
    {
        if (BattleCheatsSettings.AutoHealTroops && !isPrisoner && partyBase == PartyBase.MainParty)
            return new ExplainedNumber(999f);
        return _wrapped.GetDailyHealingForRegulars(partyBase, isPrisoner, includeDescriptions);
    }

    public override ExplainedNumber GetDailyHealingHpForHeroes(PartyBase partyBase, bool isPrisoners, bool includeDescriptions = false)
    {
        if (BattleCheatsSettings.AutoHealTroops && !isPrisoners && partyBase == PartyBase.MainParty)
            return new ExplainedNumber(999f);
        return _wrapped.GetDailyHealingHpForHeroes(partyBase, isPrisoners, includeDescriptions);
    }

    public override int GetHeroesEffectedHealingAmount(Hero hero, float healingRate)
        => _wrapped.GetHeroesEffectedHealingAmount(hero, healingRate);

    public override ExplainedNumber GetBattleEndHealingAmount(PartyBase partyBase, Hero hero)
        => _wrapped.GetBattleEndHealingAmount(partyBase, hero);
}

public sealed class BattleCheatsUpgradeModel : PartyTroopUpgradeModel
{
    private readonly PartyTroopUpgradeModel _wrapped;

    public BattleCheatsUpgradeModel(PartyTroopUpgradeModel wrapped) { _wrapped = wrapped; }

    public override bool CanPartyUpgradeTroopToTarget(PartyBase party, CharacterObject character, CharacterObject target)
    {
        if (BattleCheatsSettings.InstantUpgrade) return true;
        return _wrapped.CanPartyUpgradeTroopToTarget(party, character, target);
    }

    public override bool IsTroopUpgradeable(PartyBase party, CharacterObject character)
    {
        if (BattleCheatsSettings.InstantUpgrade) return true;
        return _wrapped.IsTroopUpgradeable(party, character);
    }

    public override bool DoesPartyHaveRequiredItemsForUpgrade(PartyBase party, CharacterObject upgradeTarget)
    {
        if (BattleCheatsSettings.InstantUpgrade) return true;
        return _wrapped.DoesPartyHaveRequiredItemsForUpgrade(party, upgradeTarget);
    }

    public override bool DoesPartyHaveRequiredPerksForUpgrade(PartyBase party, CharacterObject character, CharacterObject upgradeTarget, out PerkObject requiredPerk)
    {
        if (BattleCheatsSettings.InstantUpgrade) { requiredPerk = null; return true; }
        return _wrapped.DoesPartyHaveRequiredPerksForUpgrade(party, character, upgradeTarget, out requiredPerk);
    }

    public override ExplainedNumber GetGoldCostForUpgrade(PartyBase party, CharacterObject characterObject, CharacterObject upgradeTarget)
    {
        if (BattleCheatsSettings.InstantUpgrade) return new ExplainedNumber(0f);
        return _wrapped.GetGoldCostForUpgrade(party, characterObject, upgradeTarget);
    }

    public override int GetXpCostForUpgrade(PartyBase party, CharacterObject characterObject, CharacterObject upgradeTarget)
    {
        if (BattleCheatsSettings.InstantUpgrade) return 1;
        return _wrapped.GetXpCostForUpgrade(party, characterObject, upgradeTarget);
    }

    public override int GetSkillXpFromUpgradingTroops(PartyBase party, CharacterObject troop, int numberOfTroops)
        => _wrapped.GetSkillXpFromUpgradingTroops(party, troop, numberOfTroops);

    public override float GetUpgradeChanceForTroopUpgrade(PartyBase party, CharacterObject troop, int upgradeTargetIndex)
    {
        if (BattleCheatsSettings.InstantUpgrade) return 1f;
        return _wrapped.GetUpgradeChanceForTroopUpgrade(party, troop, upgradeTargetIndex);
    }
}


public sealed class BattleCheatsMoraleModel : PartyMoraleModel
{
    private readonly PartyMoraleModel _wrapped;

    public BattleCheatsMoraleModel(PartyMoraleModel wrapped) { _wrapped = wrapped; }

    public override float HighMoraleValue => _wrapped.HighMoraleValue;

    public override int GetDailyStarvationMoralePenalty(PartyBase party)
        => BattleCheatsSettings.NoDesertion ? 0 : _wrapped.GetDailyStarvationMoralePenalty(party);

    public override int GetDailyNoWageMoralePenalty(MobileParty party)
        => BattleCheatsSettings.NoDesertion ? 0 : _wrapped.GetDailyNoWageMoralePenalty(party);

    public override float GetStandardBaseMorale(PartyBase party)
        => BattleCheatsSettings.NoDesertion ? _wrapped.HighMoraleValue : _wrapped.GetStandardBaseMorale(party);

    public override float GetVictoryMoraleChange(PartyBase party)
        => _wrapped.GetVictoryMoraleChange(party);

    public override float GetDefeatMoraleChange(PartyBase party)
        => _wrapped.GetDefeatMoraleChange(party);

    public override ExplainedNumber GetEffectivePartyMorale(MobileParty party, bool includeDescription = false)
    {
        if (BattleCheatsSettings.NoDesertion)
            return new ExplainedNumber(_wrapped.HighMoraleValue);
        return _wrapped.GetEffectivePartyMorale(party, includeDescription);
    }
}