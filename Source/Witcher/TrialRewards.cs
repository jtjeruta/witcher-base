using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    // Applies trial rewards: gene grants, gene upgrades, and marker hediffs.
    public static class TrialRewards
    {
        public static void Apply(Pawn pawn, HediffCompProperties_TrialOutcome props)
        {
            if (pawn == null || props == null) return;

            Apply(pawn, new TrialRewardConfig
            {
                grantGenes = props.grantGenes,
                geneUpgrades = props.geneUpgrades,
                upgradeGenes = props.upgradeGenes,
                markerHediff = props.markerHediff,
                removeMarkerHediff = props.removeMarkerHediff
            });
        }

        public static void Apply(Pawn pawn, TrialRewardConfig config)
        {
            if (pawn == null || config == null) return;
            if (pawn.genes == null)
            {
                Log.Warning($"[Witcher] Pawn {pawn.LabelShort} has no gene tracker; trial rewards skipped.");
                return;
            }

            if (config.upgradeGenes && config.geneUpgrades != null)
            {
                foreach (var entry in config.geneUpgrades)
                {
                    if (entry?.from == null || entry.to == null) continue;
                    UpgradeGene(pawn, entry.from, entry.to);
                }
            }

            if (config.grantGenes != null)
            {
                foreach (var gene in config.grantGenes)
                {
                    if (gene == null) continue;
                    if (pawn.genes.HasActiveGene(gene)) continue;
                    pawn.genes.AddGene(gene, xenogene: true);
                }
            }

            if (config.removeMarkerHediff != null)
            {
                var oldMarker = pawn.health.hediffSet.GetFirstHediffOfDef(config.removeMarkerHediff);
                if (oldMarker != null)
                {
                    pawn.health.RemoveHediff(oldMarker);
                }
            }

            if (config.markerHediff != null && !pawn.health.hediffSet.HasHediff(config.markerHediff))
            {
                pawn.health.AddHediff(config.markerHediff, null, null, null);
            }
        }

        private static void UpgradeGene(Pawn pawn, GeneDef from, GeneDef to)
        {
            var existing = pawn.genes.GetGene(from);
            if (existing != null)
            {
                pawn.genes.RemoveGene(existing);
            }

            if (!pawn.genes.HasActiveGene(to))
            {
                pawn.genes.AddGene(to, xenogene: true);
            }
        }
    }
}
