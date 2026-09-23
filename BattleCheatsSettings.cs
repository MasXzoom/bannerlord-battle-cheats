namespace BattleCheats;

public static class BattleCheatsSettings
{
    public static bool GodMode;
    public static bool TroopsInvulnerable;
    public static bool TroopsOneHit;
    public static int PlayerDamageMultiplier = 1;
    public static bool KillAura;
    public static bool UnlimitedTroops;
    public static bool UnlimitedPrisoners;
    public static bool UnlimitedInventory;
    public static bool AutoRecruitPrisoners;
    public static bool NoCargoSlowdown;
    public static bool AutoHealTroops;
    public static bool InstantUpgrade;
    public static bool FreeRecruit;
    public static bool InfiniteAmmo;
    public static bool AutoLoot;
    public static bool AutoGovernor;
    public static bool AutoFood;
    public static bool NoDesertion;
    public static bool AutoPrisoners;
    public static bool AutoSupporters;
    public static bool IgnoreByParties;

    public static void ResetAll()
    {
        GodMode = false;
        TroopsInvulnerable = false;
        TroopsOneHit = false;
        PlayerDamageMultiplier = 1;
        KillAura = false;
        UnlimitedTroops = false;
        UnlimitedPrisoners = false;
        UnlimitedInventory = false;
        AutoRecruitPrisoners = false;
        NoCargoSlowdown = false;
        AutoHealTroops = false;
        InstantUpgrade = false;
        FreeRecruit = false;
        InfiniteAmmo = false;
        AutoLoot = false;
        IgnoreByParties = false;
    }
}
