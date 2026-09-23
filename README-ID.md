<div align="center">

**🌐 Bahasa:** [English](README.md) · [Bahasa Indonesia](README-ID.md)

</div>

<div align="center">

# 🍩 BATTLE CHEATS

**Mount & Blade II: Bannerlord — Kumpulan Cheat Singleplayer**

`oleh @donutcoffe`

[![Version](https://img.shields.io/badge/versi-2.1.0-crimson)]()
[![Game](https://img.shields.io/badge/Bannerlord-1.2.x%20%2F%20War%20Sails-blue)]()
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Proton%20%2F%20Linux-green)]()
[![Build](https://img.shields.io/badge/build-netstandard2.1-brightgreen)]()
[![Mod Type](https://img.shields.io/badge/type-Community%20Mod-orange)]()

*Tanpa Harmony. Tanpa injeksi. MissionBehavior native + pembungkusan Campaign Model.*

</div>

---

## 📜 Daftar Isi

- [Filosofi](#-filosofi)
- [Fitur](#-fitur)
- [Install](#-install)
- [Cara Pakai](#-cara-pakai)
- [Build dari Source](#-build-dari-source)
- [Catatan Teknis](#-catatan-teknis)
- [Kompatibilitas Save](#-kompatibilitas-save)
- [Changelog](#-changelog)
- [Kredit](#-kredit)

---

## 🧠 Filosofi

> *"Menu cheat yang bagus itu panel kontrol, bukan tumpukan hotkey."*

Battle Cheats dibangun di atas tiga prinsip:

1. **Berbasis menu** — semua fitur jadi baris toggle yang keliatan. Gak ada yang disembunyiin, gak ada yang harus ditebak.
2. **Non-destruktif** — fitur jalan lewat API publik TaleWorlds (roster, model, slot gubernur). Engine game sendiri yang nulis state-nya. Gak ada vektor korupsi save.
3. **Per-session** — setting reset tiap keluar game. Lo gak akan balik main dan kaget god mode masih nyala.

---

## ⚔️ Fitur

### Halaman 1 — TEMPUR
| Fitur | Keterangan |
|---|---|
| God Mode | Player kebal semua damage |
| One / Two Hit Kill | Musuh mati dalam 1 / 2 hit |
| Damage 3x / 10x / 100x | Pengali damage player |
| Panah Tak Terbatas | Quiver auto-refill tiap detik |
| Kill Aura | Musuh radius 10m mati otomatis |
| Bunuh Semua | Sapu medan perang instan |

### Halaman 2 — PARTY & EKONOMI
| Fitur | Keterangan |
|---|---|
| Unlimited Troops | Batas party +5000 |
| Unlimited Tahanan / Inventory | Roster & kapasitas tanpa batas |
| Anti Berat | Speed tetap max bawa barang apapun |
| Rekrut Gratis | Biaya rekrut 0 denar |
| Party Tak Terlihat | Party AI musuh ngabaikan lo total |

### Halaman 3 — OTOMATIS & KERAJAAN
| Fitur | Keterangan |
|---|---|
| Auto-Gubernur Pintar | Companion paling cocok per settlement: profil militer (Tactics/Leadership/Scouting) buat castle, profil ekonomi (Trade/Steward/Medicine/Engineering) buat town |
| Auto-Bekal | Makanan party auto-refill — kelaparan mustahil |
| Anti-Pembelot | Moral terkunci di max; nol event desersi |
| Tahanan Nyata | Tiap town/castle milik lo otomatis ada tahanan perang beneran di penjara |
| Auto Rekanan | Notable & guildmaster di settlement lo pindah pihak ke clan lo |

### Halaman 4 — TROOP & TAHANAN
| Fitur | Keterangan |
|---|---|
| Rekrut Semua Tahanan | Instan, tahanan langsung sehat dan ikut perang berikutnya |
| Auto-Rekrut | Tahanan jadi prajurit begitu masuk party |
| Auto-Heal | Troop luka sembuh instan |
| Upgrade Instan / MAX | Chain-upgrade semua troop sampai tier akhir; level 0 naik, level 2/3 naik lagi |
| Troop Kebal / One-Hit | Pasukan lo gak bisa mati / one-shot musuh |
| Auto-Loot | Perlengkapan musuh masuk inventory lo |
| Spawn Warga Sipil | +50 villager per klik — `Occupation.Villager` asli, permanen bukan militer, gak bisa upgrade |

### Global
| Kontrol | Keterangan |
|---|---|
| `>> MATIKAN SEMUA CHEAT <<` | Panic button — semua off sekali klik |
| Ganti Hotkey | Default **B**, bisa diubah in-game |
| Counter Live | Judul tiap halaman nunjukin jumlah cheat aktif |

---

## 📦 Install

```bash
# 1. Copy folder mod ke direktori Modules/ Bannerlord
cp -r BattleCheats "<install Bannerlord>/Modules/"

# 2. Aktifkan di launcher: tab Mods > centang "Battle Cheats"

# 3. Di dalam game, tekan B
```

**Path Steam (Linux/Proton):** `~/.local/share/Steam/steamapps/common/Mount & Blade II Bannerlord/Modules/`

---

## 🎮 Cara Pakai

1. Masuk campaign, tekan **B** — menu otomatis pause game.
2. Navigasi 4 halaman lewat tombol panah di bawah.
3. Toggle fitur — status `[ON]` / `[off]` keliatan tiap baris.
4. Fitur otomatis (halaman 3) jalan tiap 2 detik di peta campaign.
5. Sebelum streaming atau sesi "jujur": pencet panic button di halaman 1.

---

## 🔧 Build dari Source

```bash
git clone https://github.com/MasXzoom/bannerlord-battle-cheats
cd bannerlord-battle-cheats
dotnet build -c Release
cp bin/Release/netstandard2.1/BattleCheats.dll "<Bannerlord>/Modules/BattleCheats/bin/Win64_Shipping_Client/"
```

Log runtime: `ProgramData/Mount and Blade II Bannerlord/logs/BattleCheats_runtime.txt`

---

## 🧪 Catatan Teknis

- **Arsitektur**: `MissionBehavior` (battle) + pembungkusan `CampaignModel` (campaign). Nol Harmony patch, nol injeksi IL.
- **Model yang dibungkus**: PartySizeLimit, InventoryCapacity, PartySpeed, Wage, Healing, TroopUpgrade, Morale.
- **Catatan ceiling upgrade instan**: biaya XP upgrade dibatesin minimal 1 (bukan 0) — biaya 0 bikin crash division native di party screen.
- **Null-guard missile**: handler hit ngabaikan victim null dari proyektil nyasar; log error dibatasi 1x/30 detik biar gak stutter I/O.

---

## 💾 Kompatibilitas Save

⚠️ **Save mencatat ID module aktif di headernya.** Campaign yang dibuat dengan Battle Cheats harus selalu di-load dengan mod aktif. Cabut mod = game nolak save ("module which has been removed"). Mod ini sendiri gak pernah korupsi save — semua perubahan state lewat API engine vanilla.

Setting per-session dan gak pernah tersimpan antar main.

---

## 📋 Changelog

Lihat [CHANGELOG.md](CHANGELOG.md).

---

## 🤝 Kredit

- **Author / Maintainer** — [@donutcoffe](https://github.com/MasXzoom)
- Dibangun untuk **Mount & Blade II: Bannerlord** v1.2.x (War Sails)
- Hargain singleplayer. Jangan bawa ini ke multiplayer.

<div align="center">

*🍩 diseduh dengan kopi dingin*

</div>
