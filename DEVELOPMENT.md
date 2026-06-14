# Witcher Base — Developer Notes

Internal documentation for contributors. **Spoils gameplay mechanics** — keep this out of the player-facing README and the Steam description.

## Core design

The trials are research-gated surgery recipes (Health tab) that apply a "fever" hediff. The outcome is **predetermined**: it is rolled the instant the hediff is added, not driven by tending or the immunity race. The fever severity is purely cosmetic — it climbs toward 100% over a randomized duration, then either removes itself (survivor) or kills the pawn (doomed).

This means tending, medicine, and pawn health have **no** effect on the result. The visible fever is theater.

The Trial of Mountains is the exception: a player-initiated quest where success/failure depends on whether the trial pawn survives until the spawned beast(s) are dead.

## Survival rolls

Rolled in `HediffComp_TrialOutcome.RollOutcome()`:

| Trial | `surviveChance` | `idealSurviveChance` | Ideal subject |
|-------|-----------------|----------------------|---------------|
| Grasses | 0.05 | 0.30 | Male, biological age 8–12 |
| Dreams | 0.70 | — | n/a (initiates only) |
| Additional Mutagens | 0.10 | — | n/a (full witchers only) |

## Marker hediffs

Two axes: **rank** (one at a time) and **enhancement** (parallel).

| Hediff | Label | Applied by |
|--------|-------|------------|
| `Witcher_Initiate` | witcher initiate | Grasses survival |
| `Witcher_FullyTrained` | witcher | Dreams survival (removes Initiate) |
| `Witcher_Master` | master witcher | Mountains quest success (removes FullyTrained) |
| `Witcher_Mutated` | mutated | Additional Mutagens survival (parallel, does not replace rank) |

Mountains and Mutagens both branch off Dreams and can be done in either order.

## Eligibility gating

- **Grasses**: any humanlike pawn, not already initiate, not currently undergoing.
- **Dreams**: has `Witcher_Initiate`, not `Witcher_FullyTrained`, not currently undergoing.
- **Additional Mutagens**: has `Witcher_FullyTrained` or `Witcher_Master`, not `Witcher_Mutated`, not at peak (`MoveSpeed_VeryQuick`), not currently undergoing.
- **Mountains**: gizmo on `Witcher_FullyTrained` when `Witcher_TrialOfMountains` research is finished; blocked if already `Witcher_Master` or a mountains trial is in progress.

## Rewards

Applied in `TrialRewards.Apply` (fever trials on hediff removal; Mountains via `MountainsTrialRewards` on quest success).

**Grasses — grants:** `Immunity_Strong`, `Robust`, `WoundHealing_Fast`, `DarkVision`, `Sterile`, `MoveSpeed_Quick`, `Beauty_Ugly`, `PsychicAbility_Dull`.

**Dreams — grants:** `LowSleep`, `Pain_Reduced`, `Aggression_DeadCalm`, `Learning_Fast`, `AptitudePoor_Social`, `Ageless`, `DiseaseFree`, `ArchiteMetabolism`, `MeleeDamage_Strong`, `AptitudeStrong_Melee`, `AptitudeStrong_Shooting`.

**Dreams — upgrades:** `PsychicAbility_Dull` → `PsychicAbility_Deaf`, `Beauty_Ugly` → `Beauty_VeryUgly`.

**Additional Mutagens — grants:** `Hair_SnowWhite`. **upgrades:** `MoveSpeed_Quick` → `MoveSpeed_VeryQuick`, `WoundHealing_Fast` → `WoundHealing_SuperFast`, `Immunity_Strong` → `Immunity_SuperStrong`. Applies parallel marker `Witcher_Mutated`.

**Mountains — grants:** `ToxResist_Total`, `StrongStomach`.

**Mountains — upgrades:** `AptitudeStrong_Melee` → `AptitudeRemarkable_Melee`, `AptitudeStrong_Shooting` → `AptitudeRemarkable_Shooting`. Swaps `Witcher_FullyTrained` → `Witcher_Master`.

## Trial of Mountains quest flow

1. Player selects a witcher (`Witcher_FullyTrained`) and clicks **Begin Trial of Mountains** (gizmo from `HediffComp_MountainsTrigger`).
2. `QuestUtility.GenerateQuestAndMakeAvailable(Witcher_TrialOfMountainsQuest, slate)` with `trialPawn`, `map`, and fixed site threat `points` in the slate.
3. The quest generates a nearby world-map site using RimWorld's vanilla `Manhunters` site part.
4. The player sends a caravan to the site. RimWorld generates the site map and fires `site.AllEnemiesDefeated` when the hostile animals are dead.
5. `QuestPart_WitcherMountainsReward` listens for `site.AllEnemiesDefeated`, applies `MountainsTrialRewards`, and the quest ends successfully. If the site expires or is abandoned before completion, the quest fails.

**Beast pool (v1):** vanilla `Manhunters` site generation. This is intentionally using RimWorld's built-in travel-site behavior first; a custom curated beast-lair `SitePartDef`/`GenStepDef` can replace it later.

**Simplifications:** any colonist may help kill the beasts; solo participation is not enforced.

## Repository layout

```
About/                 Mod metadata (About.xml)
Assemblies/            Compiled Witcher.dll
Defs/
  HediffDefs/          Trial fevers + rank/enhancement markers
  QuestScriptDefs/     Trial of Mountains quest
  RecipeDefs/          Trial operations
  ResearchProjectDefs/ Trial research
Source/
  Witcher/             Recipe workers, hediff comps, rewards, quest nodes
  Witcher/Quest/       Mountains quest spawn, watcher, rewards
  build.sh             Offline build script
LoadFolders.xml        Version loading rules
```

## Building

```bash
./Source/build.sh
```

Outputs `Assemblies/Witcher.dll`. Override paths with `RIMWORLD_MANAGED`, `DOTNET`, and `CSC_DLL` env vars if auto-detection fails.

## Dev workflow

The repo is symlinked into RimWorld's `Mods/` folder (`Mods/WitcherBase`), so XML and DLL changes are picked up on the next game restart without copying files.

## Conventions / gotchas

- C# namespace is `WitcherBase`; XML `Class=`/`workerClass=` references must use the `WitcherBase.` prefix.
- Use `System.Math` rather than `UnityEngine.Mathf` to avoid the `netstandard` reference error under the game's Mono build.
- `QuestPart` in the target RimWorld build does not expose overridable tick/kill hooks; mountains trial watching uses `MapComponent_WitcherTrials` instead.
- Combat aptitude ramps: Grasses (none) → Dreams (strong) → Mountains (great).
- Changing `packageId` or the mod folder name breaks existing saves that referenced the old identity.
