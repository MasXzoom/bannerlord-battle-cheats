# Battle Cheats - Mount & Blade II Bannerlord

Mod cheat custom untuk Bannerlord (build 1.2.x / War Sails). Menu in-game 4 halaman, hotkey **B** (bisa diganti dari menu).

## Fitur

### Halaman 1 - TEMPUR
- Kebal (God Mode)
- One Hit / Two Hit Kill
- Damage 3x / 10x / 100x / Normal
- Panah gak pernah abis (quiver auto-refill)
- Kill Aura (radius 10m)
- BUNUH SEMUA MUSUH

### Halaman 2 - PARTY & EKONOMI
- Unlimited Troops (+5000 party size)
- Unlimited Prisoners / Inventory
- Anti Berat (speed tetap max bawa apapun)
- Rekrut Gratis
- Party Tak Terlihat

### Halaman 3 - OTOMATIS & KERAJAAN
- Auto-Gubernur Pintar (companion terpintar sesuai jenis: militer utk castle, ekonomi utk town)
- Auto-Bekal (makanan party auto-refill)
- Anti-Pembelot (moral selalu max, tanpa desersi)
- Tahanan Nyata di Settlement (penjara tiap town/castle auto-isi)
- Auto Rekanan (notable/guild otomatis jadi pendukung clan)

### Halaman 4 - TROOP & TAHANAN
- Rekrut Semua Tahanan (instan, sehat langsung ikut war)
- Auto-Rekrut Tahanan (masuk = jadi prajurit)
- Auto-Heal Pasukan
- Upgrade Instan + UPGRADE SEMUA TROOP MAX (tier 0 -> max)
- Troop Kebal / One-Hit
- Auto-Loot Musuh
- SPAWN 50 PEKERJA RODI (villager sipil, bukan militer, gak bisa upgrade)

### Semua halaman
- >> MATIKAN SEMUA CHEAT << (panic button)
- Ganti Hotkey

## Install
1. Copy folder `BattleCheats` ke `Modules/`
2. Launcher > Mods > centang **Battle Cheats**
3. In-game tekan **B**

## Build
```
dotnet build -c Release
cp bin/Release/netstandard2.1/BattleCheats.dll <game>/Modules/BattleCheats/bin/Win64_Shipping_Client/
```

## Catatan Teknis
- Campaign model wrap (PartySizeLimit, InventoryCapacity, Speed, Wage, Healing, Upgrade, Morale) - tanpa Harmony patch
- MissionBehavior native untuk battle cheat
- Setting gak persist antar session (anti lupa matiin)
- Log runtime: `ProgramData/Mount and Blade II Bannerlord/logs/BattleCheats_runtime.txt`

Save yang dibuat saat mod aktif mencatat mod di header - load harus dengan mod enabled.
