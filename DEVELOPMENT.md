# Witcher Base — Developer Notes

Internal documentation for contributors. **Spoils gameplay mechanics** — keep this out of the player-facing README and the Steam description.

## Core design

The trials are research-gated surgery recipes (Health tab) that apply a "fever" hediff. The outcome is **predetermined**: it is rolled the instant the hediff is added, not driven by tending or the immunity race. The fever severity is purely cosmetic — it climbs toward 100% over a randomized duration, then either removes itself (survivor) or kills the pawn (doomed).

This means tending, medicine, and pawn health have **no** effect on the result. The visible fever is theater.

## Survival rolls

Rolled in `HediffComp_TrialOutcome.RollOutcome()`:

| Trial | `surviveChance` | `idealSurviveChance` | Ideal subject |
|-------|-----------------|----------------------|---------------|
| Grasses | 0.05 | 0.30 | Male, biological age 8–12 |
| Dreams | 0.70 | — | n/a (initiates only) |
| Additional Mutagens | 0.10 | — | n/a (initiates only) |

- "Ideal subject" logic lives in `HediffComp_TrialOutcome.IsIdealSubject` (`IdealMinAge = 8`, `IdealMaxAge = 12`, `Gender.Male`).
- `idealSurviveChance` only applies when `>= 0`; Dreams/Mutagens leave it unset so all subjects use the flat `surviveChance`.
- Fever duration is `feverDurationTicks.RandomInRange` (default 1–2 days). Severity = `progress * peak`, where `peak` is `survivorPeakSeverity` for survivors and `1f` for the doomed.

## Eligibility gating

- **Trial of Grasses** (`Recipe_TrialOfGrasses`): any humanlike pawn that is not already an initiate and not currently undergoing a trial.
- **Trial of Dreams** (`Recipe_TrialOfDreams`): carries `Witcher_Initiate`, not yet `Witcher_FullyTrained`, not currently undergoing.
- **Additional Mutagens** (`Recipe_AdditionalMutagens`): carries `Witcher_FullyTrained` (i.e. survived Dreams), not at peak, not currently undergoing.

Two invisible marker hediffs drive the gating:
- `Witcher_Initiate` (label "witcher initiate") — applied on surviving Grasses.
- `Witcher_FullyTrained` (label "witcher") — applied on surviving Dreams; Dreams also removes `Witcher_Initiate` via the comp's `removeMarkerHediff`, so a pawn shows only one marker at a time.

## Rewards

Applied in `TrialRewards.Apply` on `CompPostPostRemoved` (only when `willSurvive`).

**Grasses — grants genes:** `Immunity_Strong`, `Robust`, `WoundHealing_Fast`, `DarkVision`, `Sterile`, `MoveSpeed_Quick`, `Beauty_Ugly`, `PsychicAbility_Dull`, `AptitudeStrong_Melee`, `AptitudeStrong_Shooting`.

**Dreams — grants genes:** `LowSleep`, `Pain_Reduced`, `Aggression_DeadCalm`, `Learning_Fast`, `AptitudePoor_Social`, `Ageless`, `DiseaseFree`, `ArchiteMetabolism`, `MeleeDamage_Strong`.

**Dreams — upgrades existing genes** (`upgradeGenes` true):
- `PsychicAbility_Dull` → `PsychicAbility_Deaf`
- `Beauty_Ugly` → `Beauty_VeryUgly`
- `AptitudeStrong_Melee` → `AptitudeRemarkable_Melee`
- `AptitudeStrong_Shooting` → `AptitudeRemarkable_Shooting`

**Additional Mutagens — upgrades existing genes:**
- `MoveSpeed_Quick` → `MoveSpeed_VeryQuick`
- `WoundHealing_Fast` → `WoundHealing_SuperFast`
- `Immunity_Strong` → `Immunity_SuperStrong`

All trial genes are vanilla Biotech defs (no custom genes ship with the mod). The "strong/great melee/shooting" and "poor social" genes are template-generated aptitude genes (`AptitudeStrong_*`, `AptitudeRemarkable_*`, `AptitudePoor_*`). All granted genes are added as xenogenes; upgrades remove the lower-tier gene before adding the higher tier. Letters are sent via `LetterStack` using `{PAWN_...}` templates resolved with `template.Formatted(pawn.Named("PAWN"))`.

## Repository layout

```
About/                 Mod metadata (About.xml)
Assemblies/            Compiled Witcher.dll (assembly name stays "Witcher")
Defs/
  GeneDefs/            (empty — all genes are vanilla Biotech defs)
  HediffDefs/          Trial fevers + Witcher_Initiate / Witcher_FullyTrained markers
  RecipeDefs/          Trial operations
  ResearchProjectDefs/ Trial research
Source/
  Witcher/             Recipe workers, hediff comp, gene rewards (namespace WitcherBase)
  Witcher.csproj       Project file
  build.sh             Offline build script (invokes Roslyn csc directly)
LoadFolders.xml        Version loading rules
```

## Building

Builds offline without NuGet by invoking the game's own Mono DLLs via Roslyn:

```bash
./Source/build.sh
```

Outputs `Assemblies/Witcher.dll`. Override paths with `RIMWORLD_MANAGED`, `DOTNET`, and `CSC_DLL` env vars if auto-detection fails.

Note: `RootNamespace` in `Witcher.csproj` is informational only — `build.sh` calls `csc` directly with explicit `namespace WitcherBase` declarations in the source.

## Dev workflow

The repo is symlinked into RimWorld's `Mods/` folder (`Mods/WitcherBase`), so XML and DLL changes are picked up on the next game restart without copying files.

## Conventions / gotchas

- C# namespace is `WitcherBase`; XML `Class=`/`workerClass=` references must use the `WitcherBase.` prefix.
- Use `System.Math` rather than `UnityEngine.Mathf` to avoid the `netstandard` reference error under the game's Mono build.
- All granted/upgraded genes must reference real def names. Aptitude/skill genes are template-generated as `Aptitude<Tier>_<Skill>` (e.g. `AptitudeStrong_Shooting`); if you add a custom gene later, its `displayCategory` must be a valid vanilla `GeneCategoryDef` (e.g. `Mood`), not an arbitrary string.
- Changing `packageId` (now `witcher.base`) or the mod folder name breaks existing saves that referenced the old identity.

## Roadmap

- **Trial of Mountains**: planned as a quest-based final trial (research exists as a placeholder).
- Future add-on mods in the suite ship as separate assemblies/namespaces, depending on this base.
