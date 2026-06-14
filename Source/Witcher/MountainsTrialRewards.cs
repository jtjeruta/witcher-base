using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public static class MountainsTrialRewards
    {
        private static List<GeneDef> grantGenes;
        private static List<GeneUpgradeEntry> geneUpgrades;

        public static TrialRewardConfig GetConfig()
        {
            EnsureInitialized();
            return new TrialRewardConfig
            {
                grantGenes = grantGenes,
                geneUpgrades = geneUpgrades,
                upgradeGenes = true,
                markerHediff = WitcherDefOf.Witcher_Master,
                removeMarkerHediff = WitcherDefOf.Witcher_FullyTrained
            };
        }

        public static void Apply(Pawn pawn)
        {
            TrialRewards.Apply(pawn, GetConfig());
        }

        private static void EnsureInitialized()
        {
            if (grantGenes != null) return;

            grantGenes = new List<GeneDef>
            {
                DefDatabase<GeneDef>.GetNamed("ToxResist_Total"),
                DefDatabase<GeneDef>.GetNamed("StrongStomach")
            };

            geneUpgrades = new List<GeneUpgradeEntry>
            {
                new GeneUpgradeEntry
                {
                    from = DefDatabase<GeneDef>.GetNamed("AptitudeStrong_Melee"),
                    to = DefDatabase<GeneDef>.GetNamed("AptitudeRemarkable_Melee")
                },
                new GeneUpgradeEntry
                {
                    from = DefDatabase<GeneDef>.GetNamed("AptitudeStrong_Shooting"),
                    to = DefDatabase<GeneDef>.GetNamed("AptitudeRemarkable_Shooting")
                }
            };
        }
    }
}
