# Witcher Base — Developer Notes

Internal documentation for contributors. **Spoils gameplay mechanics** — keep this out of the player-facing README and the Steam description.

## Core design

The trials are research-gated surgery recipes (Health tab) that apply a "fever" hediff. The outcome is **predetermined**: it is rolled the instant the hediff is added, not driven by tending or the immunity race. The fever severity is purely cosmetic — it climbs toward 100% over a randomized duration, then either removes itself (survivor) or kills the pawn (doomed).

This means tending, medicine, and pawn health have **no** effect on the result. The visible fever is theater.

On survival, the pawn's **xenotype is replaced** via `pawn.genes.SetXenotype(...)` (`WitcherXenotypes.SetStage`). Each stage carries a fixed cumulative gene set defined in `Defs/XenotypeDefs/Witcher_Xenotypes.xml`. Genes are overridden, not incrementally granted or upgraded.

The Trial of Mountains is the exception: a player-initiated quest where success/failure depends on combat at a nearby site.

## Survival rolls

Rolled in `HediffComp_TrialOutcome.RollOutcome()`:

| Trial | `surviveChance` | `idealSurviveChance` | Ideal subject |
|-------|-----------------|----------------------|---------------|
| Grasses | 0.05 | 0.30 | Male, biological age 8–12 |
| Dreams | 0.70 | — | n/a (initiates only) |
| Additional Mutagens | 0.10 | — | n/a (master witchers only) |

Player-facing text advertises the optimistic Grasses rate (~3 in 10) without mentioning age/gender variance.

## Xenotype ladder

Linear progression. Each trial sets `resultXenotype` on the fever comp.

| XenotypeDef | Label | Set by |
|-------------|-------|--------|
| `WitcherInitiate` | witcher initiate | Grasses survival |
| `Witcher` | witcher | Dreams survival |
| `WitcherMaster` | master witcher | Mountains contract success |
| `WitcherMutated` | mutated witcher | Additional Mutagens survival |

All four xenotypes have `inheritable=false`, no `factionlessGenerationWeight`, and are not added to any faction `xenotypeSet` — they never spawn on random pawns but are browsable/selectable for starting colonists.

## Eligibility gating

Gating reads `pawn.genes.Xenotype`:

- **Grasses**: any humanlike pawn with genes, not any witcher xenotype, not currently undergoing.
- **Dreams**: xenotype == `WitcherInitiate`, not currently undergoing.
- **Additional Mutagens**: xenotype == `WitcherMaster`, not currently undergoing.
- **Mountains contract**: `Witcher_ContractGene` on the `Witcher` xenotype + research finished; gizmo from `Gene_WitcherContract.GetGizmos()`.

## Xenotype gene sets

**WitcherInitiate:** `Immunity_Strong`, `Robust`, `WoundHealing_Fast`, `DarkVision`, `Sterile`, `MoveSpeed_Quick`, `Beauty_Ugly`, `PsychicAbility_Dull`, `AptitudeStrong_Melee`, `AptitudeStrong_Shooting`, `Witcher_CatEyes`, `Skin_SheerWhite`.

**Witcher:** initiate upgrades resolved (`PsychicAbility_Deaf`, `Beauty_VeryUgly`) plus `LowSleep`, `Pain_Reduced`, `Aggression_DeadCalm`, `Learning_Fast`, `AptitudePoor_Social`, `ArchiteMetabolism`, `Witcher_ContractGene`.

**WitcherMaster:** witcher set minus `Witcher_ContractGene`, with `AptitudeRemarkable_Melee`/`AptitudeRemarkable_Shooting` instead of strong; plus `ToxResist_Total`, `StrongStomach`, `MeleeDamage_Strong`.

**WitcherMutated:** master set with `MoveSpeed_VeryQuick`, `WoundHealing_SuperFast`, `Immunity_SuperStrong`; plus `Hair_SnowWhite`, `Ageless`, `DiseaseFree`.

## Trial of Mountains quest flow

1. Player selects a witcher (`Witcher` xenotype) and clicks **Take witcher contract** (gizmo from `Gene_WitcherContract`).
2. `QuestUtility.GenerateQuestAndMakeAvailable(Witcher_TrialOfMountainsQuest, slate)` with `trialPawn`, `map`, and fixed site threat `points`. A quest-linked letter confirms the contract.
3. The quest generates a nearby world-map site using RimWorld's vanilla `Manhunters` site part.
4. The player sends a caravan to the site. RimWorld fires `site.AllEnemiesDefeated` when hostile animals are dead.
5. `QuestPart_WitcherMountainsReward` sets `WitcherMaster` xenotype and ends the quest. Site expiry/abandonment fails the quest.

**Beast pool (v1):** vanilla `Manhunters` site generation.

**Simplifications:** any colonist may help kill the beasts; solo participation is not enforced.

## Repository layout

```
About/                 Mod metadata (About.xml)
Assemblies/            Compiled Witcher.dll
Defs/
  GeneDefs/            Custom witcher genes (Witcher_CatEyes, Witcher_ContractGene)
  XenotypeDefs/        Four witcher xenotypes
  HediffDefs/          Trial fevers
  QuestScriptDefs/     Trial of Mountains quest
  RecipeDefs/          Trial operations
  ResearchProjectDefs/ Trial research
Source/
  Witcher/             Recipe workers, hediff comps, xenotype helper, contract gene
  Witcher/Quest/       Mountains quest nodes and watcher
  build.sh             Offline build script
LoadFolders.xml        Version loading rules
Textures/              Witcher eye overlays and gene icon
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
- `QuestPart` in the target RimWorld build does not expose overridable tick/kill hooks; mountains trial watching uses `MapComponent_WitcherTrials` for registration (legacy; site signals handle completion).
- Combat aptitude ramps: Grasses (strong) → Mountains (great + strong melee damage).
- Additional Mutagens is linear: requires `WitcherMaster`, not parallel with Mountains.
- Changing `packageId` or the mod folder name breaks existing saves that referenced the old identity.
- Marker hediffs were removed in the xenotype conversion; in-progress saves with old markers need re-testing.
