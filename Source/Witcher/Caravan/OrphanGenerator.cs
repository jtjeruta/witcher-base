using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WitcherBase
{
    public static class OrphanGenerator
    {
        private static readonly string[] UnwantedTraitNames =
        {
            "SlowLearner",
            "Wimp",
            "AnnoyingVoice",
            "CreepyBreathing"
        };

        private static readonly string[] PremiumTraitNames =
        {
            "FastLearner",
            "Nimble",
            "Tough",
            "GreatMemory"
        };

        public static Pawn Generate(Settlement settlement, OrphanTier tier)
        {
            Faction faction = settlement.Faction;
            PawnKindDef kind = faction?.def?.basicMemberKind ?? PawnKindDefOf.Slave;
            float ageYears = Rand.RangeInclusive(OrphanStockUtility.MinAgeYears, OrphanStockUtility.MaxAgeYears);
            Gender gender = Rand.Value < OrphanStockUtility.MaleChance ? Gender.Male : Gender.Female;

            List<TraitDef> forcedTraits = null;
            List<TraitDef> prohibitedTraits = null;
            switch (tier)
            {
                case OrphanTier.Unwanted:
                {
                    TraitDef unwanted = PickTrait(UnwantedTraitNames);
                    forcedTraits = unwanted != null ? new List<TraitDef> { unwanted } : null;
                    prohibitedTraits = BuildTraitList(PremiumTraitNames);
                    break;
                }
                case OrphanTier.Premium:
                {
                    TraitDef premium = PickTrait(PremiumTraitNames);
                    forcedTraits = premium != null ? new List<TraitDef> { premium } : null;
                    prohibitedTraits = BuildTraitList(UnwantedTraitNames);
                    break;
                }
                default:
                    prohibitedTraits = new List<TraitDef>();
                    prohibitedTraits.AddRange(BuildTraitList(UnwantedTraitNames));
                    prohibitedTraits.AddRange(BuildTraitList(PremiumTraitNames));
                    break;
            }

            PawnGenerationRequest request = new PawnGenerationRequest(
                kind,
                faction,
                PawnGenerationContext.NonPlayer,
                settlement.Tile,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f,
                forceAddFreeWarmLayerIfNeeded: false,
                allowGay: true,
                allowFood: true,
                allowAddictions: true,
                inhabitant: false,
                certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false,
                worldPawnFactionDoesntMatter: false,
                biocodeWeaponChance: 0f,
                biocodeApparelChance: 0f,
                extraPawnForExtraRelationChance: null,
                relationWithExtraPawnChanceFactor: 0f,
                validatorPreGear: null,
                validatorPostGear: null,
                forcedTraits: forcedTraits,
                prohibitedTraits: prohibitedTraits,
                minChanceToRedressWorldPawn: null,
                fixedBiologicalAge: ageYears,
                fixedChronologicalAge: ageYears,
                fixedGender: gender,
                fixedLastName: null,
                fixedBirthName: null,
                fixedTitle: null,
                fixedIdeo: null,
                forceNoIdeo: false,
                forceNoBackstory: false,
                forbidAnyTitle: true,
                forceDead: false,
                forcedXenogenes: null,
                forcedEndogenes: null,
                forcedXenotype: null,
                forcedCustomXenotype: null,
                allowedXenotypes: null,
                forceBaselinerChance: 1f,
                developmentalStages: DevelopmentalStage.Child,
                pawnKindDefGetter: null,
                excludeBiologicalAgeRange: null,
                biologicalAgeRange: null,
                forceRecruitable: false,
                dontGiveWeapon: true,
                onlyUseForcedBackstories: false,
                maximumAgeTraits: 1,
                minimumAgeTraits: 0,
                forceNoGear: true);

            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (pawn == null)
            {
                return null;
            }

            ApplyTierEffects(pawn, tier);
            pawn.SetFaction(faction, null);
            return pawn;
        }

        private static void ApplyTierEffects(Pawn pawn, OrphanTier tier)
        {
            if (tier != OrphanTier.Unwanted)
            {
                return;
            }

            TraitDef nerves = DefDatabase<TraitDef>.GetNamedSilentFail("Nerves");
            if (nerves != null && pawn.story?.traits != null && !pawn.story.traits.HasTrait(nerves))
            {
                pawn.story.traits.GainTrait(new Trait(nerves, -2));
            }

            if (Rand.Chance(0.5f))
            {
                BodyPartRecord part = pawn.RaceProps.body.AllParts.RandomElementWithFallback();
                if (part != null)
                {
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Cut, 8f, 999f, -1f, null, part);
                    pawn.TakeDamage(damageInfo);
                }
            }

            if (Rand.Chance(0.25f) && pawn.story?.traits != null)
            {
                TraitDef extra = PickTrait(UnwantedTraitNames);
                if (extra != null && !pawn.story.traits.HasTrait(extra))
                {
                    pawn.story.traits.GainTrait(new Trait(extra));
                }
            }
        }

        private static List<TraitDef> BuildTraitList(IEnumerable<string> names)
        {
            List<TraitDef> traits = new List<TraitDef>();
            foreach (string name in names)
            {
                TraitDef trait = DefDatabase<TraitDef>.GetNamedSilentFail(name);
                if (trait != null)
                {
                    traits.Add(trait);
                }
            }

            return traits;
        }

        private static TraitDef PickTrait(IEnumerable<string> names)
        {
            List<TraitDef> traits = BuildTraitList(names);
            return traits.Count > 0 ? traits.RandomElement() : null;
        }
    }
}
