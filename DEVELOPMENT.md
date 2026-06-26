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
- **Dreams**: xenotype == `WitcherInitiate`, not currently undergoing. Vanilla Health operations omit the recipe when psychite tea (a drug ingredient) is not on-map; stock tea + smokeleaf for it to appear.
- **Additional Mutagens**: xenotype == `WitcherMaster`, not currently undergoing.
- **Mountains contract**: `Witcher_ContractGene` on the **`Witcher` xenotype only** (not Master/Mutated) + research finished; gizmo from `Gene_WitcherContract.GetGizmos()` (xenotype guard). Completing Mountains replaces xenotype with `WitcherMaster`, dropping the contract gene. `GameComponent_WitcherContractCleanup` strips stale contract genes from Master/Mutated pawns on load and periodically.

## Xenotype gene sets

All four xenotypes share **`Witcher_Immunity`**, **`Witcher_Stoicism`**, and **`Witcher_Reflexes`** (see below). Per-stage genes follow.

**WitcherInitiate:** `Witcher_Immunity`, `Witcher_Stoicism`, `Witcher_Reflexes`, `Robust`, `WoundHealing_Fast`, `Witcher_ToxTolerance_I`, `DarkVision`, `Sterile`, `MoveSpeed_Quick`, `Beauty_Ugly`, `PsychicAbility_Dull`, `AptitudeStrong_Melee`, `AptitudeStrong_Shooting`, `Witcher_CatEyes`, `Skin_SheerWhite`, `Witcher_WeakAard`.

**Witcher:** initiate upgrades resolved (`PsychicAbility_Deaf`, `Beauty_VeryUgly`, tox tolerance upgraded to `Witcher_ToxTolerance_II`) plus `LowSleep`, `Pain_Reduced`, `Aggression_DeadCalm`, `Learning_Fast`, `AptitudePoor_Social`, `ArchiteMetabolism`, **`Witcher_ContractGene`**, `Witcher_WeakAard`, `Witcher_StrongAard`, `Witcher_Igni`, `Witcher_Quen`.

**WitcherMaster:** witcher sign set retained (both Aard tiers, Igni, Quen), with `AptitudeRemarkable_Melee`/`AptitudeRemarkable_Shooting` instead of strong; plus `Witcher_ToxTolerance_III`, `StrongStomach`, `MeleeDamage_Strong`, `Witcher_Axii`, `Witcher_Yrden`. No contract gene.

**WitcherMutated:** master set with `MoveSpeed_VeryQuick`, `WoundHealing_SuperFast`, tox tolerance upgraded to `Witcher_ToxTolerance_IV`; plus `Hair_SnowWhite`, `Ageless`, `DiseaseFree`. No contract gene.

### Custom witcher genes (`Defs/GeneDefs/Witcher_Genes.xml`)

| Gene | Stat | Value | Notes |
|------|------|-------|-------|
| `Witcher_Immunity` | `ImmunityGainSpeed` factor | ×3.0 | Replaces vanilla `Immunity_SuperStrong` (×1.5). `exclusionTags`: `Immunity`. |
| `Witcher_Stoicism` | `MentalBreakThreshold` offset | −0.34 | Base 35% → **1%** floor (vanilla stat minimum). Mood-based comfort breaks effectively never trigger; does not use `blocksMentalBreaks` (that would block pain/forced breaks too). |
| `Witcher_Reflexes` | `MeleeDodgeChance` offset | +0.18 | +18 percentage points; subject to vanilla 80% cap. |
| `Witcher_ContractGene` | — | — | Mountains contract gizmo; **`Witcher` xenotype only**. |

`Aggression_DeadCalm` (Witcher+) still governs non-violent break *type*; stoicism governs break *threshold* on all stages.

### Toxicity progression (companion: Witcher Potions)

Witcher toxin handling models the two canonical witcher traits: **higher tolerance** (hold more toxicity safely) and **faster clearance** (metabolize it away quicker, like the draining Toxicity meter in the games).

**Tolerance** is a single graduated stat — `ToxicResistance` — granted by a custom gene tier per stage (`Witcher_ToxTolerance_I`..`_IV` in `Defs/GeneDefs/Witcher_ToxGenes.xml`, all sharing the `WitcherToxTolerance` exclusion tag). `ToxicResistance` covers toxic buildup from **all** sources (venom, fallout, pollution, tox gas, and the Witcher Potions companion mod, which scales `ToxicBuildup` by `(1 - ToxicResistance)`), so one dial serves both the potion mechanic and environmental flavor — no separate antitoxic-lungs gene needed.

The tiers are intentionally capped **below 100%**: a fully tox-immune (`ToxResist_Total`) pawn would drink potions with zero risk, defeating the mechanic. Capping at 85% means even a mutated witcher can overdose by chugging brews in quick succession. A normal human (0% resistance) dies from a single undiluted dose.

**Clearance** is the custom gene class `Gene_WitcherToxMetabolism` (`Source/Witcher/Gene_WitcherToxMetabolism.cs`), set as the `geneClass` on all four tiers. In `TickInterval(int delta)` it reduces any existing `ToxicBuildup` via `HealthUtility.AdjustSeverity`, on top of the universal vanilla decay (`-0.08/day` from `HediffComp_ImmunizableToxic`). The extra rate is derived from the gene's own `ToxicResistance` offset (`offset * 0.5`/day), so the one class auto-scales across tiers with no per-tier fields. (`Pawn_GeneTracker.GeneTrackerTickInterval` drives `Gene.TickInterval`, which is public in 1.6.)

| Stage | Gene | `ToxicResistance` | Extra clearance/day | Total `ToxicBuildup` decay/day | Potions to lethal cap (undiluted, ignoring decay) |
|-------|------|-------------------|---------------------|-------------------------------|---------------------------------------------------|
| Initiate (Grasses) | `Witcher_ToxTolerance_I` | +0.40 | -0.20 | ~-0.28 | 2nd dose |
| Witcher (Dreams) | `Witcher_ToxTolerance_II` | +0.60 | -0.30 | ~-0.38 | ~3rd dose |
| Master (Mountains) | `Witcher_ToxTolerance_III` | +0.75 | -0.375 | ~-0.455 | ~4th dose |
| Mutated (Mutagens) | `Witcher_ToxTolerance_IV` | +0.85 | -0.425 | ~-0.505 | ~7th dose |

Tuning knob: `RecoveryPerResistancePerDay` (currently `0.5`) in `Gene_WitcherToxMetabolism`. Gene icons reuse vanilla paths (`Gene_PartialToxicityResistance` / `Gene_TotalToxicityResistance`), which resolve from base resources.

## Witcher signs

Signs are gene-granted `AbilityDef`s (see `Defs/AbilityDefs/Witcher_SignAbilities.xml` and `Defs/GeneDefs/Witcher_SignGenes.xml`). They appear on the pawn Abilities gizmo row when drafted.

| Gene | Abilities | Custom C# |
|------|-----------|-----------|
| `Witcher_WeakAard` | `Witcher_WeakAard` | `CompAbilityEffect_Knockback` |
| `Witcher_StrongAard` | `Witcher_StrongAard` | `CompAbilityEffect_Knockback` (aimed cone) |
| `Witcher_Igni` | `Witcher_Igni` | vanilla `CompProperties_AbilityFireSpew` |
| `Witcher_Quen` | `Witcher_Quen` | `HediffComp_QuenShield` + `CompProperties_AbilityGiveHediff` |
| `Witcher_Axii` | Serenity, Trust, CalmBeast, Tame, Berserk | `CompAbilityEffect_StopMentalBreak`; vanilla prisoner/manhunter comps; `CompAbilityEffect_TameAnimal`; `CompAbilityEffect_AxiiBerserk` |
| `Witcher_Yrden` | `Witcher_Yrden` | `CompAbilityEffect_Yrden` (hostiles only) → `Witcher_YrdenDebuff` + `HediffComp_YrdenMark` |

Cooldowns use in-game time (2500 ticks = 1 hour). Tuned for **2–3 casts per ~45–60 in-game minute fight**: 750 ticks (~18m) for lighter signs, 900 (~22m) for AoE/control, 1200 (~29m) for Quen/Berserk, 1500 (~36m) for Tame. Weak Aard stays at 120 (~3m). Quen shield lasts 2 in-game hours (`Ability_Duration` 83 seconds → 5000 ticks via vanilla `GiveHediff`; `disappearsAfterTicks` fallback 5000). Weak Aard and Strong Aard coexist on Witcher, Master, and Mutated xenotypes — Weak Aard is not replaced at Dreams.

**Combat sign warmup (`verbProperties.warmupTime`):** Weak Aard, Strong Aard, Igni, Quen, Yrden, and Axii Berserk all use **0.2s**. Axii touch abilities (serenity, trust, calm beast, tame) stay at 1.5–2s.

**Strong Aard:** `targetHostilesOnly` true — cone knockback never hits colonists or allies.

**Igni:** `lineWidthEnd` **4** on `CompProperties_AbilityFireSpew` (vanilla Fire Spew uses 3).

Strong Aard is an **aimed cone**: the player targets a cell, and `CompAbilityEffect_Knockback.CollectConeVictims` selects pawns within `radius` whose angle from the caster→target direction is inside `coneAngle`. While aiming, `DrawEffectPreview` outlines the wedge with `GenDraw.DrawFieldEdges` (falls back to `GenDraw.DrawRadiusRing` if the cone cannot be resolved). Knockback pushes pawns through cells checked with `GenGrid.Walkable` (terrain/buildings only) rather than `Standable`, and calls `pather.StopDead()` + `Notify_Teleported` so moving foes don't immediately walk back. Axii: Tame uses `InteractionWorker_RecruitAttempt.DoRecruit` for wild animals and wild men (`WildManUtility.IsWildMan`). Berserk forces `Witcher_AxiiBerserk` mental state (`MentalState_AxiiBerserk` extends berserk but only targets pawns/factions hostile to the player); duration from `Ability_Duration`.

**Yrden debuff (`Witcher_YrdenDebuff`):** `MoveSpeed` ×0.1 (walk speed only — do not debuff `Moving` capacity or pawns go down), `AimingDelayFactor` ×1.6, `ShootingAccuracyPawn` −3, `MeleeHitChance` −2, `Consciousness` −0.15. Duration: `CompProperties_AbilityYrden.durationTicks` (default 2400 → ~40 seconds in the health tooltip; keep under 2500 ticks so RimWorld displays seconds, not hours). Set `ticksToDisappear` after `AddHediff`; force severity 1 on apply.

**Phase 2 (not implemented):** persistent Yrden ground field, passive trade-price influence, sign sound effects.

**Sign sounds (deferred):** sign abilities ship silent. Do not use `PsycastCastLoop` (or any `<sustain>True</sustain>` SoundDef) for `warmupStartSound`/`soundCast` — those expect one-shot sounds and the loop variant throws "Tried to play subSound ... as a one-shot sound". When adding audio, use one-shot SoundDefs (e.g. Biotech `FireSpew_Warmup`/`FireSpew_Resolve`, or Royalty psycast one-shots like `Psycast_Skip_Exit` if Royalty becomes a dependency).

## Trial of Mountains quest flow

1. Player selects a witcher (`Witcher` xenotype) and clicks **Take witcher contract** (gizmo from `Gene_WitcherContract`).
2. `QuestUtility.GenerateQuestAndMakeAvailable(Witcher_TrialOfMountainsQuest, slate)` with `trialPawn`, `map`, and fixed site threat `points`. A quest-linked letter confirms the contract.
3. The quest generates a nearby world-map site using RimWorld's vanilla `Manhunters` site part.
4. The player sends a caravan to the site. RimWorld fires `site.AllEnemiesDefeated` when hostile animals are dead.
5. `QuestPart_WitcherMountainsReward` sets `WitcherMaster` xenotype and ends the quest. Site expiry/abandonment fails the quest.

**Beast pool (v1):** vanilla `Manhunters` site generation.

**Simplifications:** any colonist may help kill the beasts; solo participation is not enforced.

## Caravan orphan recruitment

Settlement action **Buy children** (caravan gizmo when visiting a friendly NPC base). Implemented via `WorldObjectComp_OrphanStock` on `Settlement` — same hook pattern as vanilla `TradeRequestComp.GetCaravanGizmos`, no Harmony patch.

**Gating:** `Witcher_TrialOfGrasses` research finished; settlement faction non-hostile and not player-owned.

**Stock / restock** (per settlement, saved on the world object):

| Category | Detection | Stock / restock | Price mult |
|----------|-----------|-----------------|------------|
| Tribal | `techLevel <= Neolithic` | 2–4 / 45 days | 0.5× |
| Outlander | Industrial | 1–2 / 60 days | 1.0× |
| Advanced | Spacer+ | 0–1 / 90 days | 1.5× |

Restock timer starts on first generation; buying does not reset it. When the timer expires, stock is fully replaced.

**Quality tiers** (rolled on restock):

| Tier | Price (before mult) | Notes |
|------|---------------------|-------|
| Unwanted | 1 silver | Bad traits, optional Nerves (−2), cut scar |
| Standard | 200–450 silver | No forced extremes |
| Premium | 600–1200 silver | FastLearner / Nimble / Tough / GreatMemory |

**Pawn gen:** 80% male, age 8–12, `DevelopmentalStage.Child`, settlement faction culture, no witcher xenotypes. Purchased pawns join the caravan; silver is taken from caravan inventory. Final price floors at 1 silver (tribal unwanted no longer shows 0).

**List UI:** childhood backstory title as the circumstance line (e.g. "fire-scarred child", "orphan of war"); premium tier appends " — family sale". Internal tier names are not shown. No restock countdown in the UI — empty stock shows "Nothing for sale." Dialog reserves space for the Close button and uses dynamic row heights so trait lists scroll cleanly.

**Deferred:** event-based orphan collection, comms-console access, colony mood on purchase.

## Repository layout

```
About/                 Mod metadata (About.xml)
Assemblies/            Compiled Witcher.dll
Defs/
  GeneDefs/            Custom witcher genes (eyes, immunity, stoicism, reflexes, contract, signs)
  AbilityDefs/         Witcher sign abilities
  XenotypeDefs/        Four witcher xenotypes
  HediffDefs/          Trial fevers and sign hediffs
  QuestScriptDefs/     Trial of Mountains quest
  RecipeDefs/          Trial operations
  ResearchProjectDefs/ Trial research + Witcher research tab
Patches/               Settlement orphan-stock comp injection (XML patches, not under Defs/)
Source/
  Witcher/             Recipe workers, hediff comps, xenotype helper, contract gene, sign comps
  Witcher/Caravan/     Settlement orphan stock, purchase dialog, pawn generation
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
- All witcher research (trials + companion Potions alchemy) uses the **`Witcher` research tab** (`ResearchTabDef` in `Witcher_ResearchTab.xml`; Potions references the same tab via `loadAfter` witcher.base).
- Changing `packageId` or the mod folder name breaks existing saves that referenced the old identity.
- Marker hediffs were removed in the xenotype conversion; in-progress saves with old markers need re-testing.
