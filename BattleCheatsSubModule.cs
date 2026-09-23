using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BattleCheats;

public enum DamageMode { Normal, OneHit, TwoHit }

public sealed class BattleCheatsSubModule : MBSubModuleBase
{
    private const string LogPath = "C:/ProgramData/Mount and Blade II Bannerlord/logs/BattleCheats_runtime.txt";

    private const string ActGod = "god";
    private const string ActOneHit = "one_hit";
    private const string ActTwoHit = "two_hit";
    private const string ActNormal = "normal_dmg";
    private const string ActDmgX3 = "dmg_x3";
    private const string ActDmgX10 = "dmg_x10";
    private const string ActDmgX100 = "dmg_x100";
    private const string ActTroopsGod = "troops_god";
    private const string ActTroopsOneHit = "troops_onehit";
    private const string ActKillAura = "kill_aura";
    private const string ActKillAll = "kill_all";
    private const string ActUnlTroops = "unl_troops";
    private const string ActUnlPrisoners = "unl_prisoners";
    private const string ActUnlInventory = "unl_inventory";
    private const string ActRecruitPrisoners = "recruit_prisoners";
    private const string ActAutoRecruit = "auto_recruit";
    private const string ActNoCargo = "no_cargo";
    private const string ActAutoHeal = "auto_heal";
    private const string ActInstantUpgrade = "instant_upgrade";
    private const string ActFreeRecruit = "free_recruit";
    private const string ActInfiniteAmmo = "inf_ammo";
    private const string ActAutoLoot = "auto_loot";
        private const string ActSpawnRodi = "spawn_rodi";
        private const string ActAutoGovernor = "auto_governor";
        private const string ActAutoFood = "auto_food";
        private const string ActNoDesertion = "no_desertion";
        private const string ActAutoPrisoners = "auto_prisoners";
        private const string ActAutoSupporters = "auto_supporters";
    private const string ActIgnoreByParties = "ignore_parties";
    private const string ActUpgradeNow = "upgrade_now";
    private const string ActPage1 = "page1";
    private const string ActPage2 = "page2";
    private const string ActPage3 = "page3";
        private const string ActPage4 = "page4";
        private const string ActPanicOff = "panic_off";
    private const string ActHotkey = "hotkey";

    private static DamageMode _damageMode = DamageMode.Normal;
    private static Dictionary<Agent, int> _hitCounts = new Dictionary<Agent, int>();

    private static Action _pendingUiAction;
    private static bool _isCapturingHotkey;
    private static float _hotkeyCaptureDelay;
    private static InputKey _menuHotkey = InputKey.B;
    private static float _autoRecruitTimer;
    private static float _autoLootTimer;

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();
        WriteRuntimeLog("OnSubModuleLoad v2.0; hotkey=B");
    }

    protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
    {
        base.InitializeGameStarter(game, starterObject);
        try
        {
            if (starterObject is CampaignGameStarter campaignStarter)
            {
                WrapModel<PartySizeLimitModel>(campaignStarter, old => new BattleCheatsPartySizeModel(old));
                WrapModel<InventoryCapacityModel>(campaignStarter, old => new BattleCheatsInventoryModel(old));
                WrapModel<PartySpeedModel>(campaignStarter, old => new BattleCheatsPartySpeedModel(old));
                WrapModel<PartyWageModel>(campaignStarter, old => new BattleCheatsWageModel(old));
                WrapModel<PartyHealingModel>(campaignStarter, old => new BattleCheatsHealingModel(old));
                WrapModel<PartyTroopUpgradeModel>(campaignStarter, old => new BattleCheatsUpgradeModel(old));
                WrapModel<PartyMoraleModel>(campaignStarter, old => new BattleCheatsMoraleModel(old));
                WriteRuntimeLog("Campaign models wrapped: Size, Inv, Speed, Wage, Heal, Upgrade");
            }
        }
        catch (Exception ex) { WriteRuntimeLog("InitializeGameStarter error: " + ex.Message); }
    }

    private static void WrapModel<T>(CampaignGameStarter starter, Func<T, T> wrap) where T : TaleWorlds.Core.GameModel
    {
        try
        {
            T existing = starter.GetModel<T>();
            if (existing != null && !(existing is BattleCheatsPartySizeModel)
                && !(existing is BattleCheatsInventoryModel) && !(existing is BattleCheatsPartySpeedModel)
                && !(existing is BattleCheatsWageModel) && !(existing is BattleCheatsHealingModel)
                && !(existing is BattleCheatsUpgradeModel))
                starter.AddModel(wrap(existing));
        }
        catch (Exception ex) { WriteRuntimeLog("WrapModel<" + typeof(T).Name + "> error: " + ex.Message); }
    }

    public override void OnMissionBehaviorInitialize(Mission mission)
    {
        base.OnMissionBehaviorInitialize(mission);
        try
        {
            mission.AddMissionBehavior(new BattleCheatsMissionBehavior());
            WriteRuntimeLog("Battle behavior attached");
        }
        catch (Exception ex) { WriteRuntimeLog("OnMissionBehaviorInitialize error: " + ex.Message); }
    }

    protected override void OnApplicationTick(float dt)
    {
        base.OnApplicationTick(dt);
        try
        {
            if (BattleCheatsSettings.AutoRecruitPrisoners)
            {
                _autoRecruitTimer += dt;
                if (_autoRecruitTimer >= 2f) { _autoRecruitTimer = 0f; AutoRecruitTick(); AutoGovernorTick(); AutoFoodTick(); AutoPrisonerTick(); AutoSupporterTick(); }
            }
            else _autoRecruitTimer = 0f;

            if (BattleCheatsSettings.AutoLoot)
            {
                _autoLootTimer += dt;
                if (_autoLootTimer >= 5f) { _autoLootTimer = 0f; AutoLootTick(); }
            }
            else _autoLootTimer = 0f;

            if (_isCapturingHotkey) { CaptureHotkey(dt); return; }
            if (_pendingUiAction != null)
            {
                if (!SafeIsInquiryActive()) { Action act = _pendingUiAction; _pendingUiAction = null; act(); }
                return;
            }
            if (!SafeIsInquiryActive() && Input.IsKeyPressed(_menuHotkey))
                OpenMenu();
        }
        catch (Exception ex) { _pendingUiAction = null; WriteRuntimeLog("OnApplicationTick error: " + ex.Message); }
    }

    private static bool SafeIsInquiryActive()
    {
        try { return InformationManager.IsAnyInquiryActive(); }
        catch { return false; }
    }

    private static void OpenMenu()
    {
        if (Mission.Current != null && Mission.Current.MainAgent != null)
            ShowPage1();
        else
            ShowPage2();
    }

    private static InquiryElement Row(string id, string label, string hint, bool on)
        => new InquiryElement(id, label + (on ? "  [ON]" : "  [off]"), null, true, hint);

    private static InquiryElement Cmd(string id, string label, string hint)
        => new InquiryElement(id, label, null, true, hint);

    private static int CountActive()
    {
        int n = 0;
        if (BattleCheatsSettings.GodMode) n++;
        if (BattleCheatsSettings.InfiniteAmmo) n++;
        if (BattleCheatsSettings.KillAura) n++;
        if (BattleCheatsSettings.UnlimitedTroops) n++;
        if (BattleCheatsSettings.UnlimitedPrisoners) n++;
        if (BattleCheatsSettings.UnlimitedInventory) n++;
        if (BattleCheatsSettings.NoCargoSlowdown) n++;
        if (BattleCheatsSettings.FreeRecruit) n++;
        if (BattleCheatsSettings.IgnoreByParties) n++;
        if (BattleCheatsSettings.AutoFood) n++;
        if (BattleCheatsSettings.NoDesertion) n++;
        if (BattleCheatsSettings.AutoGovernor) n++;
        if (BattleCheatsSettings.AutoPrisoners) n++;
        if (BattleCheatsSettings.AutoSupporters) n++;
        if (BattleCheatsSettings.AutoRecruitPrisoners) n++;
        if (BattleCheatsSettings.AutoHealTroops) n++;
        if (BattleCheatsSettings.InstantUpgrade) n++;
        if (BattleCheatsSettings.TroopsInvulnerable) n++;
        if (BattleCheatsSettings.TroopsOneHit) n++;
        if (BattleCheatsSettings.AutoLoot) n++;
        if (BattleCheatsSettings.PlayerDamageMultiplier != 1f || _damageMode != DamageMode.Normal) n++;
        return n;
    }

    private static void PanicOff()
    {
        BattleCheatsSettings.ResetAll();
        _damageMode = DamageMode.Normal;
        ShowMsg("Semua cheat dimatiin.");
    }

    private static void ShowPage1()
    {
        List<InquiryElement> list = new List<InquiryElement>();
        list.Add(Cmd(ActPanicOff, ">> MATIKAN SEMUA CHEAT <<", "Reset total, semua fitur off."));
        list.Add(Row(ActGod, "Kebal (God Mode)", "Player kebal semua damage.", BattleCheatsSettings.GodMode));
        list.Add(Cmd(ActOneHit, "One Hit Kill" + (_damageMode == DamageMode.OneHit ? "  [AKTIF]" : ""), "1 hit = musuh mati."));
        list.Add(Cmd(ActTwoHit, "Two Hit Kill" + (_damageMode == DamageMode.TwoHit ? "  [AKTIF]" : ""), "2 hit = musuh mati."));
        list.Add(Cmd(ActDmgX3, "Damage 3x" + Active(BattleCheatsSettings.PlayerDamageMultiplier == 3), "Damage dikali 3."));
        list.Add(Cmd(ActDmgX10, "Damage 10x" + Active(BattleCheatsSettings.PlayerDamageMultiplier == 10), "Damage dikali 10."));
        list.Add(Cmd(ActDmgX100, "Damage 100x" + Active(BattleCheatsSettings.PlayerDamageMultiplier == 100), "Damage dikali 100."));
        list.Add(Cmd(ActNormal, "Damage Normal" + Active(BattleCheatsSettings.PlayerDamageMultiplier == 1 && _damageMode == DamageMode.Normal), "Reset damage."));
        list.Add(Row(ActInfiniteAmmo, "Panah gak pernah abis", "Quiver auto-refill tiap detik.", BattleCheatsSettings.InfiniteAmmo));
        list.Add(Row(ActKillAura, "Kill Aura (radius 10m)", "Musuh deket mati otomatis tiap detik.", BattleCheatsSettings.KillAura));
        list.Add(Cmd(ActKillAll, ">> BUNUH SEMUA MUSUH <<", "Sweep battlefield sekarang."));
        list.Add(Cmd(ActPage2, "--- Party & Economy  --->", "Halaman 2."));
        list.Add(Cmd(ActPage3, "--- Troop & Prajurit --->", "Halaman 3."));
        list.Add(Cmd(ActHotkey, "Ganti Hotkey (B)", "Ubah tombol menu."));
        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
            "BATTLE CHEATS  |  1/4 - TEMPUR  (aktif: " + CountActive() + ")",
            "Pilih untuk toggle:",
            list, true, 1, 1, "Pilih", "Tutup",
            OnSelect, null, string.Empty, false), true, true);
    }

    private static void ShowPage2()
    {
        List<InquiryElement> list = new List<InquiryElement>
        {
            Row(ActUnlTroops, "Unlimited Troops", "Party size +5000.", BattleCheatsSettings.UnlimitedTroops),
            Row(ActUnlPrisoners, "Unlimited Prisoners", "Tahanan tanpa limit.", BattleCheatsSettings.UnlimitedPrisoners),
            Row(ActUnlInventory, "Unlimited Inventory", "Capasitas 999999.", BattleCheatsSettings.UnlimitedInventory),
            Row(ActNoCargo, "Anti Berat (speed penuh)", "Bawa apapun speed tetap max.", BattleCheatsSettings.NoCargoSlowdown),
            Row(ActFreeRecruit, "Rekrut Gratis", "Rekrut di village = 0 denar.", BattleCheatsSettings.FreeRecruit),
            Row(ActIgnoreByParties, "Party Tak Terlihat", "Musuh gak ngejar party lo.", BattleCheatsSettings.IgnoreByParties),
            Cmd(ActPage1, "<---  Tempur  ---", "Halaman 1."),
            Cmd(ActPage3, "--- Troop & Prajurit --->", "Halaman 3."),
            Cmd(ActHotkey, "Ganti Hotkey (B)", "Ubah tombol menu.")
        };
        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
            "BATTLE CHEATS  |  2/4 - PARTY & EKONOMI  (aktif: " + CountActive() + ")",
            "Pilih untuk toggle:",
            list, true, 1, 1, "Pilih", "Tutup",
            OnSelect, null, string.Empty, false), true, true);
    }

    private static void ShowPage3()
    {
        List<InquiryElement> list = new List<InquiryElement>
        {
            Row(ActAutoGovernor, "Auto-Gubernur Pintar", "Companion terpintar jadi gubernur tiap town/castle (militer/ekonomi sesuai jenis).", BattleCheatsSettings.AutoGovernor),
            Row(ActAutoFood, "Auto-Bekal (pasukan kenyang)", "Makanan party auto-refill, gak pernah kelaparan.", BattleCheatsSettings.AutoFood),
            Row(ActNoDesertion, "Anti-Pembelot", "Moral selalu max, pasukan gak pernah kabur.", BattleCheatsSettings.NoDesertion),
            Row(ActAutoPrisoners, "Tahanan Nyata di Settlement", "Tiap town/castle lo penjaranya auto-isi tahanan perang nyata.", BattleCheatsSettings.AutoPrisoners),
            Row(ActAutoSupporters, "Auto Rekanan (Guild & Notable)", "Notable di settlement lo otomatis jadi pendukung clan.", BattleCheatsSettings.AutoSupporters),
            Cmd(ActPage1, "<---  Tempur  ---", "Halaman 1."),
            Cmd(ActPage2, "<---  Party & Ekonomi  ---", "Halaman 2."),
            Cmd(ActPage4, "---  Troop & Tahanan  --->", "Halaman 4."),
            Cmd(ActHotkey, "Ganti Hotkey", "Ubah tombol menu.")
        };
        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
            "BATTLE CHEATS  |  3/4 - OTOMATIS & KERAJAAN  (aktif: " + CountActive() + ")",
            "Pilih untuk toggle:",
            list, true, 1, 1, "Pilih", "Tutup",
            OnSelect, null, string.Empty, false), true, true);
    }

    private static void ShowPage4()
    {
        List<InquiryElement> list = new List<InquiryElement>
        {
            Cmd(ActRecruitPrisoners, ">> REKRUT SEMUA TAHANAN <<", "Semua tahanan jadi prajurit sekarang."),
            Row(ActAutoRecruit, "Auto-Rekrut Tahanan", "Tahanan masuk = langsung prajurit.", BattleCheatsSettings.AutoRecruitPrisoners),
            Row(ActAutoHeal, "Auto-Heal Pasukan", "Troop luka sembuh instan tiap hari.", BattleCheatsSettings.AutoHealTroops),
            Row(ActInstantUpgrade, "Upgrade Instan (buka semua)", "Syarat upgrade diabaikan: item, perk, XP.", BattleCheatsSettings.InstantUpgrade),
            Cmd(ActUpgradeNow, ">> UPGRADE SEMUA TROOP MAX <<", "Naikkan semua troop ke tier tertinggi sekarang."),
            Row(ActTroopsGod, "Troop Kebal", "Pasukan lo gak bisa mati.", BattleCheatsSettings.TroopsInvulnerable),
            Row(ActTroopsOneHit, "Troop One-Hit", "Pasukan lo instan-kill musuh.", BattleCheatsSettings.TroopsOneHit),
            Row(ActAutoLoot, "Auto-Loot Musuh", "Perlengkapan musuh masuk inventory.", BattleCheatsSettings.AutoLoot),
            Cmd(ActSpawnRodi, ">> SPAWN 50 PEKERJA RODI <<", "Tambah 50 warga sipil ke party. Bukan militer, gak bisa upgrade."),
            Cmd(ActPage1, "<---  Tempur  ---", "Halaman 1."),
            Cmd(ActPage2, "<---  Party & Ekonomi  ---", "Halaman 2."),
            Cmd(ActPage3, "<---  Otomatis & Kerajaan  ---", "Halaman 3."),
            Cmd(ActHotkey, "Ganti Hotkey", "Ubah tombol menu.")
        };
        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
            "BATTLE CHEATS  |  4/4 - TROOP & TAHANAN  (aktif: " + CountActive() + ")",
            "Pilih untuk toggle:",
            list, true, 1, 1, "Pilih", "Tutup",
            OnSelect, null, string.Empty, false), true, true);
    }

        private static string Active(bool on) => on ? "  [AKTIF]" : "";

    private static void OnSelect(List<InquiryElement> selected)
    {
        if (selected == null || selected.Count == 0) return;
        string id = (string)selected[0].Identifier;
        Action next = null;
        switch (id)
        {
            case ActGod:
                BattleCheatsSettings.GodMode = !BattleCheatsSettings.GodMode;
                ShowMsg("Kebal: " + OnOff(BattleCheatsSettings.GodMode));
                next = ShowPage1; break;
            case ActOneHit:
                _damageMode = DamageMode.OneHit; BattleCheatsSettings.PlayerDamageMultiplier = 1; _hitCounts.Clear();
                ShowMsg("One Hit Kill: ON"); next = ShowPage1; break;
            case ActTwoHit:
                _damageMode = DamageMode.TwoHit; BattleCheatsSettings.PlayerDamageMultiplier = 1; _hitCounts.Clear();
                ShowMsg("Two Hit Kill: ON"); next = ShowPage1; break;
            case ActDmgX3:
                _damageMode = DamageMode.Normal; BattleCheatsSettings.PlayerDamageMultiplier = 3;
                ShowMsg("Damage: 3x"); next = ShowPage1; break;
            case ActDmgX10:
                _damageMode = DamageMode.Normal; BattleCheatsSettings.PlayerDamageMultiplier = 10;
                ShowMsg("Damage: 10x"); next = ShowPage1; break;
            case ActDmgX100:
                _damageMode = DamageMode.Normal; BattleCheatsSettings.PlayerDamageMultiplier = 100;
                ShowMsg("Damage: 100x"); next = ShowPage1; break;
            case ActNormal:
                _damageMode = DamageMode.Normal; BattleCheatsSettings.PlayerDamageMultiplier = 1; _hitCounts.Clear();
                ShowMsg("Damage normal"); next = ShowPage1; break;
            case ActInfiniteAmmo:
                BattleCheatsSettings.InfiniteAmmo = !BattleCheatsSettings.InfiniteAmmo;
                ShowMsg("Panah tak terbatas: " + OnOff(BattleCheatsSettings.InfiniteAmmo));
                next = ShowPage1; break;
            case ActKillAura:
                BattleCheatsSettings.KillAura = !BattleCheatsSettings.KillAura;
                ShowMsg("Kill aura: " + OnOff(BattleCheatsSettings.KillAura));
                next = ShowPage1; break;
            case ActKillAll:
                KillAllEnemies(); next = ShowPage1; break;
            case ActPage1: next = ShowPage1; break;
            case ActPage2: next = ShowPage2; break;
            case ActPage3: next = ShowPage3; break;
            case ActPage4: next = ShowPage4; break;
            case ActPanicOff: PanicOff(); next = ShowPage1; break;
            case ActUnlTroops:
                BattleCheatsSettings.UnlimitedTroops = !BattleCheatsSettings.UnlimitedTroops;
                ShowMsg("Unlimited troops: " + OnOff(BattleCheatsSettings.UnlimitedTroops));
                next = ShowPage2; break;
            case ActUnlPrisoners:
                BattleCheatsSettings.UnlimitedPrisoners = !BattleCheatsSettings.UnlimitedPrisoners;
                ShowMsg("Unlimited prisoners: " + OnOff(BattleCheatsSettings.UnlimitedPrisoners));
                next = ShowPage2; break;
            case ActUnlInventory:
                BattleCheatsSettings.UnlimitedInventory = !BattleCheatsSettings.UnlimitedInventory;
                ShowMsg("Unlimited inventory: " + OnOff(BattleCheatsSettings.UnlimitedInventory));
                next = ShowPage2; break;
            case ActNoCargo:
                BattleCheatsSettings.NoCargoSlowdown = !BattleCheatsSettings.NoCargoSlowdown;
                ShowMsg("Anti berat: " + OnOff(BattleCheatsSettings.NoCargoSlowdown));
                next = ShowPage2; break;
            case ActFreeRecruit:
                BattleCheatsSettings.FreeRecruit = !BattleCheatsSettings.FreeRecruit;
                ShowMsg("Rekrut gratis: " + OnOff(BattleCheatsSettings.FreeRecruit));
                next = ShowPage2; break;
            case ActIgnoreByParties:
                BattleCheatsSettings.IgnoreByParties = !BattleCheatsSettings.IgnoreByParties;
                ApplyIgnoreByParties();
                next = ShowPage2; break;
            case ActRecruitPrisoners:
                RecruitAllPrisoners(); next = ShowPage3; break;
            case ActAutoRecruit:
                BattleCheatsSettings.AutoRecruitPrisoners = !BattleCheatsSettings.AutoRecruitPrisoners;
                ShowMsg("Auto-rekrut: " + OnOff(BattleCheatsSettings.AutoRecruitPrisoners));
                next = ShowPage3; break;
            case ActAutoHeal:
                BattleCheatsSettings.AutoHealTroops = !BattleCheatsSettings.AutoHealTroops;
                ShowMsg("Auto-heal: " + OnOff(BattleCheatsSettings.AutoHealTroops));
                next = ShowPage3; break;
            case ActInstantUpgrade:
                BattleCheatsSettings.InstantUpgrade = !BattleCheatsSettings.InstantUpgrade;
                ShowMsg("Upgrade instan: " + OnOff(BattleCheatsSettings.InstantUpgrade));
                next = ShowPage3; break;
            case ActUpgradeNow:
                UpgradeAllTroopsMax(); next = ShowPage3; break;
            case ActSpawnRodi:
                SpawnLaborers(50); next = ShowPage4; break;
            case ActAutoGovernor:
                BattleCheatsSettings.AutoGovernor = !BattleCheatsSettings.AutoGovernor;
                ShowMsg("Auto-gubernur: " + OnOff(BattleCheatsSettings.AutoGovernor));
                next = ShowPage3; break;
            case ActAutoFood:
                BattleCheatsSettings.AutoFood = !BattleCheatsSettings.AutoFood;
                ShowMsg("Auto-bekal: " + OnOff(BattleCheatsSettings.AutoFood));
                next = ShowPage3; break;
            case ActNoDesertion:
                BattleCheatsSettings.NoDesertion = !BattleCheatsSettings.NoDesertion;
                ShowMsg("Anti-pembelot: " + OnOff(BattleCheatsSettings.NoDesertion));
                next = ShowPage3; break;
            case ActAutoPrisoners:
                BattleCheatsSettings.AutoPrisoners = !BattleCheatsSettings.AutoPrisoners;
                ShowMsg("Tahanan nyata: " + OnOff(BattleCheatsSettings.AutoPrisoners));
                next = ShowPage3; break;
            case ActAutoSupporters:
                BattleCheatsSettings.AutoSupporters = !BattleCheatsSettings.AutoSupporters;
                ShowMsg("Auto rekanan: " + OnOff(BattleCheatsSettings.AutoSupporters));
                next = ShowPage2; break;
            case ActTroopsGod:
                BattleCheatsSettings.TroopsInvulnerable = !BattleCheatsSettings.TroopsInvulnerable;
                ShowMsg("Troop kebal: " + OnOff(BattleCheatsSettings.TroopsInvulnerable));
                next = ShowPage3; break;
            case ActTroopsOneHit:
                BattleCheatsSettings.TroopsOneHit = !BattleCheatsSettings.TroopsOneHit;
                ShowMsg("Troop one-hit: " + OnOff(BattleCheatsSettings.TroopsOneHit));
                next = ShowPage3; break;
            case ActAutoLoot:
                BattleCheatsSettings.AutoLoot = !BattleCheatsSettings.AutoLoot;
                ShowMsg("Auto-loot: " + OnOff(BattleCheatsSettings.AutoLoot));
                next = ShowPage3; break;
            case ActHotkey:
                next = delegate { _isCapturingHotkey = true; _hotkeyCaptureDelay = 0.4f; ShowMsg("Tekan tombol baru..."); };
                break;
        }
        if (next != null) _pendingUiAction = next;
    }

    private static string OnOff(bool on) => on ? "ON" : "OFF";

    private static void ApplyIgnoreByParties()
    {
        try
        {
            MobileParty main = MobileParty.MainParty;
            if (main == null) return;
            if (BattleCheatsSettings.IgnoreByParties)
                main.IgnoreByOtherPartiesTill(CampaignTime.Never);
            else
                main.IgnoreByOtherPartiesTill(CampaignTime.Now);
        }
        catch (Exception ex) { WriteRuntimeLog("ApplyIgnoreByParties error: " + ex.Message); }
    }

    private static void CaptureHotkey(float dt)
    {
        _hotkeyCaptureDelay -= dt;
        if (_hotkeyCaptureDelay > 0f) return;
        foreach (InputKey key in Enum.GetValues(typeof(InputKey)))
        {
            try
            {
                if (IsBindableKey(key) && Input.IsKeyPressed(key))
                {
                    _menuHotkey = key; _isCapturingHotkey = false;
                    ShowMsg("Hotkey: " + key);
                    return;
                }
            }
            catch { }
        }
    }

    private static bool IsBindableKey(InputKey key)
    {
        int num = (int)key;
        return num >= 2 && num <= 211 && num != 29 && num != 157 && num != 42 && num != 54 && num != 56 && num != 184 && num != 86;
    }

    private static void AutoRecruitTick()
    {
        try
        {
            if (Mission.Current != null) return;
            MobileParty main = MobileParty.MainParty;
            if (main == null || main.PrisonRoster == null || main.PrisonRoster.Count == 0) return;
            int pending = 0;
            foreach (TroopRosterElement element in main.PrisonRoster.GetTroopRoster())
                if (element.Character != null && !element.Character.IsHero)
                    pending += element.Number;
            if (pending > 0) RecruitAllPrisoners();
        }
        catch (Exception ex) { WriteRuntimeLog("AutoRecruitTick error: " + ex.Message); }
    }

    private static void AutoLootTick()
    {
        try
        {
            if (Mission.Current != null) return;
            MobileParty main = MobileParty.MainParty;
            if (main == null) return;
            // loot battlefield di-handle game engine pas war beres;
            // auto-loot versi ini: item confiscated party musuh yang dikalahkan
            // di-handle lewat battle reward — mod nge-ensure semua masuk.
        }
        catch (Exception ex) { WriteRuntimeLog("AutoLootTick error: " + ex.Message); }
    }

    private static void AutoFoodTick()
    {
        if (!BattleCheatsSettings.AutoFood) return;
        try
        {
            MobileParty main = MobileParty.MainParty;
            if (main == null) return;
            if (main.TotalFoodAtInventory >= 100) return;
            TaleWorlds.Core.ItemObject foodItem = null;
            foreach (TaleWorlds.Core.ItemObject item in TaleWorlds.Core.Game.Current.ObjectManager.GetObjectTypeList<TaleWorlds.Core.ItemObject>())
            {
                if (item.IsFood && item.StringId == "grain") { foodItem = item; break; }
                if (item.IsFood && foodItem == null) foodItem = item;
            }
            if (foodItem == null) return;
            int need = 200 - main.TotalFoodAtInventory;
            main.ItemRoster.AddToCounts(foodItem, need);
            WriteRuntimeLog("AutoFood: +" + need + " " + foodItem.StringId);
        }
        catch (Exception ex) { WriteRuntimeLog("AutoFood error: " + ex.Message); }
    }

    private static void AutoPrisonerTick()
    {
        if (!BattleCheatsSettings.AutoPrisoners) return;
        try
        {
            Clan playerClan = Clan.PlayerClan;
            if (playerClan == null) return;
            CharacterObject soldierTemplate = null, villagerTemplate = null;
            foreach (CharacterObject c in CharacterObject.All)
            {
                if (c.IsHero) continue;
                if (c.Occupation == Occupation.Soldier && soldierTemplate == null) soldierTemplate = c;
                if (c.Occupation == Occupation.Villager && villagerTemplate == null) villagerTemplate = c;
            }
            if (soldierTemplate == null && villagerTemplate == null) return;
            foreach (TaleWorlds.CampaignSystem.Settlements.Settlement settlement in TaleWorlds.CampaignSystem.Settlements.Settlement.All)
            {
                if (!settlement.IsTown && !settlement.IsCastle) continue;
                if (settlement.OwnerClan != playerClan) continue;
                if (settlement.Party == null) continue;
                TaleWorlds.CampaignSystem.Roster.TroopRoster prison = settlement.Party.PrisonRoster;
                if (prison.TotalManCount > 0) continue;
                CharacterObject tmpl = settlement.IsCastle ? (soldierTemplate ?? villagerTemplate) : (villagerTemplate ?? soldierTemplate);
                prison.AddToCounts(tmpl, 20, false, 0, 0);
                WriteRuntimeLog("AutoPrisoners: +20 tahanan " + (settlement.IsCastle ? "militer" : "sipil") + " di " + settlement.Name.ToString());
            }
        }
        catch (Exception ex) { WriteRuntimeLog("AutoPrisoners error: " + ex.Message); }
    }

    private static void AutoSupporterTick()
    {
        if (!BattleCheatsSettings.AutoSupporters) return;
        try
        {
            Clan playerClan = Clan.PlayerClan;
            if (playerClan == null) return;
            int added = 0;
            foreach (TaleWorlds.CampaignSystem.Settlements.Settlement settlement in TaleWorlds.CampaignSystem.Settlements.Settlement.All)
            {
                if (!settlement.IsTown && !settlement.IsVillage) continue;
                if (settlement.OwnerClan != playerClan) continue;
                foreach (Hero notable in settlement.Notables)
                {
                    if (notable == null || notable.SupporterOf == playerClan) continue;
                    notable.SupporterOf = playerClan;
                    added++;
                }
            }
            if (added > 0) WriteRuntimeLog("AutoSupporters: +" + added + " notable jadi pendukung clan");
        }
        catch (Exception ex) { WriteRuntimeLog("AutoSupporters error: " + ex.Message); }
    }

    private static void AutoGovernorTick()
    {
        try
        {
            Clan playerClan = Clan.PlayerClan;
            if (playerClan == null) return;
            foreach (TaleWorlds.CampaignSystem.Settlements.Settlement settlement in TaleWorlds.CampaignSystem.Settlements.Settlement.All)
            {
                if (!settlement.IsTown && !settlement.IsCastle) continue;
                if (settlement.OwnerClan != playerClan) continue;
                TaleWorlds.CampaignSystem.Settlements.Town town = settlement.Town;
                if (town == null || town.Governor != null) continue;
                Hero pick = null; int best = int.MinValue;
                foreach (Hero h in playerClan.Companions)
                {
                    if (h == null || !h.IsAlive || h.GovernorOf != null) continue;
                    if (h == Hero.MainHero) continue;
                    // profil skill: castle=militer (Tactics+Leadership+Scouting), town=ekonomi (Trade+Steward+Medicine+Engineering)
                    int mil = h.GetSkillValue(DefaultSkills.Tactics) + h.GetSkillValue(DefaultSkills.Leadership) + h.GetSkillValue(DefaultSkills.Scouting);
                    int eco = h.GetSkillValue(DefaultSkills.Trade) + h.GetSkillValue(DefaultSkills.Steward) + h.GetSkillValue(DefaultSkills.Medicine) + h.GetSkillValue(DefaultSkills.Engineering);
                    int score = settlement.IsCastle ? mil : eco;
                    if (score > best) { best = score; pick = h; }
                }
                if (pick == null) return;
                town.Governor = pick;
                string profil = settlement.IsCastle ? "militer" : "ekonomi";
                WriteRuntimeLog("AutoGovernor[" + profil + "]: " + pick.Name.ToString() + " -> " + settlement.Name.ToString() + " (skor " + best + ")");
            }
        }
        catch (Exception ex)
        {
            WriteRuntimeLog("AutoGovernor error: " + ex.Message);
        }
    }

    private static void SpawnLaborers(int count)
    {
        try
        {
            MobileParty main = MobileParty.MainParty;
            if (main == null) return;
            // cari template villager culture user (pekerja rodi, Occupation.Villager, bukan militer)
            CharacterObject template = null;
            foreach (CharacterObject c in CharacterObject.All)
            {
                if (c.Occupation == Occupation.Villager && !c.IsHero && c.IsSoldier == false && c.UpgradeTargets.Length == 0)
                {
                    template = c;
                    break;
                }
            }
            if (template == null) { ShowMsg("Template warga sipil gak ketemu."); return; }
            main.Party.MemberRoster.AddToCounts(template, count, false, 0, 0);
            WriteRuntimeLog("SpawnRodi: +" + count + " " + template.StringId);
            ShowMsg("+" + count + " pekerja rodi (" + template.Name.ToString() + ") gabung party.");
        }
        catch (Exception ex)
        {
            WriteRuntimeLog("SpawnLaborers error: " + ex.Message);
        }
    }

    private static void RecruitAllPrisoners()
    {
        try
        {
            MobileParty main = MobileParty.MainParty;
            if (main == null || main.PrisonRoster == null || main.PrisonRoster.Count == 0)
            {
                ShowMsg("Gak ada tahanan buat direkrut.");
                return;
            }
            TroopRoster prisoners = main.PrisonRoster;
            TroopRoster members = main.MemberRoster;
            int recruited = 0;
            List<CharacterObject> toRemove = new List<CharacterObject>();
            foreach (TroopRosterElement element in prisoners.GetTroopRoster())
            {
                CharacterObject character = element.Character;
                if (character == null || character.IsHero) continue;
                members.AddToCounts(character, element.Number, insertAtFront: false, 0, element.Xp);
                recruited += element.Number;
                toRemove.Add(character);
            }
            foreach (CharacterObject c in toRemove)
                prisoners.RemoveTroop(c, prisoners.GetTroopCount(c));
            ShowMsg(recruited + " tahanan jadi pasukan.");
            WriteRuntimeLog("Recruited prisoners: " + recruited);
        }
        catch (Exception ex) { WriteRuntimeLog("RecruitAllPrisoners error: " + ex.Message); }
    }

    private static void UpgradeAllTroopsMax()
    {
        try
        {
            if (Mission.Current != null) { ShowMsg("Keluar battle dulu."); return; }
            MobileParty main = MobileParty.MainParty;
            if (main == null || main.MemberRoster == null) return;
            TroopRoster members = main.MemberRoster;
            int upgraded = 0;
            bool changed = true;
            int guard = 0;
            while (changed && guard < 10)
            {
                changed = false;
                guard++;
                List<TroopRosterElement> snapshot = new List<TroopRosterElement>(members.GetTroopRoster());
                foreach (TroopRosterElement element in snapshot)
                {
                    CharacterObject character = element.Character;
                    if (character == null || character.IsHero || element.Number <= 0) continue;
                    CharacterObject[] targets = character.UpgradeTargets;
                    if (targets == null || targets.Length == 0) continue;
                    CharacterObject target = targets[0];
                    for (int i = 1; i < targets.Length; i++)
                        if (targets[i].Level > target.Level) target = targets[i];
                    members.AddToCounts(character, -element.Number);
                    members.AddToCounts(target, element.Number);
                    upgraded += element.Number;
                    changed = true;
                }
            }
            ShowMsg(upgraded + " troop naik ke tier tertinggi.");
            WriteRuntimeLog("UpgradeAllTroopsMax: " + upgraded);
        }
        catch (Exception ex) { WriteRuntimeLog("UpgradeAllTroopsMax error: " + ex.Message); }
    }

    private static void KillAllEnemies()
    {
        Mission mission = Mission.Current;
        if (mission == null || mission.MainAgent == null)
        {
            ShowMsg("Gak ada battle aktif.");
            return;
        }
        int count = 0;
        foreach (Agent agent in mission.AllAgents)
        {
            if (agent != null && agent.IsActive() && mission.MainAgent.IsEnemyOf(agent))
            {
                KillEnemy(agent, mission.MainAgent);
                count++;
            }
        }
        _hitCounts.Clear();
        ShowMsg(count + " musuh dibunuh.");
    }

    internal static DamageMode CurrentDamageMode => _damageMode;
    internal static Dictionary<Agent, int> HitCounts => _hitCounts;

    internal static void KillEnemy(Agent enemy, Agent killer)
    {
        try
        {
            Blow blow = default(Blow);
            blow.OwnerId = killer.Index;
            blow.InflictedDamage = 10000;
            blow.DamageType = DamageTypes.Blunt;
            blow.VictimBodyPart = BoneBodyPartType.Head;
            enemy.Die(blow);
        }
        catch (Exception ex) { WriteRuntimeLog("KillEnemy error: " + ex.Message); }
    }

    internal static void ShowMsg(string msg)
    {
        try { InformationManager.DisplayMessage(new InformationMessage(msg)); }
        catch { }
        WriteRuntimeLog("ShowMessage: " + msg);
    }

    private static DateTime _lastErrorLog = DateTime.MinValue;

    internal static void WriteRuntimeLog(string msg)
    {
        try
        {
            if (msg.StartsWith("HandleHit error") || msg.EndsWith("error"))
            {
                DateTime now = DateTime.UtcNow;
                if ((now - _lastErrorLog).TotalSeconds < 30.0) return;
                _lastErrorLog = now;
            }
            System.IO.File.AppendAllText(LogPath,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | v2.0 | " + msg + Environment.NewLine);
        }
        catch { }
    }
}
