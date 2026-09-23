<div align="center">

**🌐 Languages:** [English](README.md) · [Bahasa Indonesia](README-ID.md)

</div>

<div align="center">

# 🍩 BATTLE CHEATS

**Mount & Blade II: Bannerlord — Singleplayer Cheat Suite**

`by @donutcoffe`

[![Version](https://img.shields.io/badge/version-2.1.0-crimson)]()
[![Game](https://img.shields.io/badge/Bannerlord-1.2.x%20%2F%20War%20Sails-blue)]()
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Proton%20%2F%20Linux-green)]()
[![Build](https://img.shields.io/badge/build-netstandard2.1-brightgreen)]()
[![Mod Type](https://img.shields.io/badge/type-Community%20Mod-orange)]()

*No Harmony. No injection. Native MissionBehavior + Campaign Model wrapping.*

</div>

---

## 📜 Table of Contents

- [Philosophy](#-philosophy)
- [Features](#-features)
- [Install](#-install)
- [Usage](#-usage)
- [Build from Source](#-build-from-source)
- [Technical Notes](#-technical-notes)
- [Save Compatibility](#-save-compatibility)
- [Changelog](#-changelog)
- [Credits](#-credits)

---

## 🧠 Philosophy

> *"A good cheat menu is a control panel, not a chaos of hotkeys."*

Battle Cheats is built on three principles:

1. **Menu-driven** — every feature is a visible, toggleable row. Nothing hidden, nothing guessed.
2. **Non-destructive** — features ride public TaleWorlds APIs (rosters, models, governor slots). The game engine itself writes the state. No save corruption vectors.
3. **Session-scoped** — settings reset on game exit. You never log back in to discover god mode was still on.

---

## ⚔️ Features

### Page 1 — TEMPUR (Combat)
| Feature | Description |
|---|---|
| God Mode | Player takes zero damage |
| One / Two Hit Kill | Fixed hit-count kill switch |
| Damage 3x / 10x / 100x | Player damage multiplier |
| Infinite Arrows | Quiver auto-refill every tick |
| Kill Aura | Enemies within 10m die automatically |
| Kill All | Instant battlefield sweep |

### Page 2 — PARTY & EKONOMI
| Feature | Description |
|---|---|
| Unlimited Troops | Party size limit +5000 |
| Unlimited Prisoners / Inventory | Roster & capacity uncapped |
| Anti Berat | Speed ignores cargo/herd/footman penalty |
| Free Recruit | Recruitment cost 0 denar |
| Ghost Party | AI parties ignore you entirely |

### Page 3 — OTOMATIS & KERAJAAN (Automation & Kingdom)
| Feature | Description |
|---|---|
| Smart Auto-Governor | Assigns best idle companion per settlement: military profile (Tactics/Leadership/Scouting) for castles, economy profile (Trade/Steward/Medicine/Engineering) for towns |
| Auto Rations | Party food auto-refills — starvation impossible |
| Anti-Desertion | Morale locked to maximum; zero desertion events |
| Real Prisoners | Every owned town/castle auto-stocked with genuine war prisoners |
| Auto Supporters | Notables & guildmasters in your settlements flip to your clan |

### Page 4 — TROOP & TAHANAN
| Feature | Description |
|---|---|
| Recruit All Prisoners | Instant, prisoners spawn healthy and fight in the very next battle |
| Auto-Recruit | Prisoners become troops the moment they enter your party |
| Auto-Heal | Wounded troops recover instantly |
| Instant Upgrade / MAX | Chain-upgrades every troop to its final tier; level 0 climbs, level 2/3 climbs again |
| Troop God / One-Hit | Your troops cannot die / one-shot everything |
| Auto-Loot | Enemy equipment flows to your inventory |
| Spawn Civilians | +50 villagers per click — real `Occupation.Villager`, permanently non-military, cannot upgrade |

### Global
| Control | Description |
|---|---|
| `>> KILL ALL CHEATS <<` | Panic button — everything off in one click |
| Rebind Hotkey | Default **B**, remappable in-game |
| Live Counter | Every page header shows active cheat count |

---

## 📦 Install

```bash
# 1. Copy module folder into Bannerlord's Modules/ directory
cp -r BattleCheats "<Bannerlord install>/Modules/"

# 2. Enable in launcher: Mods tab > check "Battle Cheats"

# 3. In-game, press B
```

**Steam (Linux/Proton) path:** `~/.local/share/Steam/steamapps/common/Mount & Blade II Bannerlord/Modules/`

---

## 🎮 Usage

1. Launch campaign, press **B** — the menu pauses the game.
2. Navigate the 4 pages with the arrow entries at the bottom.
3. Toggle features — `[ON]` / `[off]` status is shown per row.
4. Automation features (page 3) act every 2 seconds on the campaign map.
5. Before streaming or "honest" sessions: press the panic button on page 1.

---

## 🔧 Build from Source

```bash
git clone https://github.com/MasXzoom/bannerlord-battle-cheats
cd bannerlord-battle-cheats
dotnet build -c Release
cp bin/Release/netstandard2.1/BattleCheats.dll "<Bannerlord>/Modules/BattleCheats/bin/Win64_Shipping_Client/"
```

Runtime log: `ProgramData/Mount and Blade II Bannerlord/logs/BattleCheats_runtime.txt`

---

## 🧪 Technical Notes

- **Architecture**: `MissionBehavior` (battle) + `CampaignModel` wrapping (campaign). Zero Harmony patches, zero IL injection.
- **Wrapped models**: PartySizeLimit, InventoryCapacity, PartySpeed, Wage, Healing, TroopUpgrade, Morale.
- **Instant upgrade ceiling note**: upgrade XP cost floors at 1 (not 0) — a 0 cost trips a native division crash in the party screen.
- **Missile null-guard**: hit handler ignores null victims from missed projectiles; error logging rate-limited to 1/30s to prevent I/O stutter.

---

## 💾 Save Compatibility

⚠️ **Saves record enabled module IDs in their header.** A campaign started with Battle Cheats must always be loaded with the mod enabled. Removing the mod makes the game refuse the save ("module which has been removed"). The mod itself never corrupts saves — all state changes pass through vanilla engine APIs.

Settings are session-scoped and never persist between runs.

---

## 📋 Changelog

See [CHANGELOG.md](CHANGELOG.md).

---

## 🤝 Credits

- **Author / Maintainer** — [@donutcoffe](https://github.com/MasXzoom)
- Built for **Mount & Blade II: Bannerlord** v1.2.x (War Sails)
- Respect singleplayer. Never bring this to multiplayer.

<div align="center">

*🍩 brewed with cold coffee*

</div>
