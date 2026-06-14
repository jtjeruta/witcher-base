using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    // Applies the surviving-the-trial rewards: gene grants, gene upgrades, and the marker hediff.
    public static class TrialRewards
    {
        public static void Apply(Pawn pawn, HediffCompProperties_TrialOutcome props)
        {
            if (pawn == null || props == null) return;
            if (pawn.genes == null)
            {
                Log.Warning($"[Witcher] Pawn {pawn.LabelShort} has no gene tracker; trial rewards skipped.");
                return;
            }

            if (props.upgradeGenes && props.geneUpgrades != null)
            {
                foreach (var entry in props.geneUpgrades)
                {
                    if (entry?.from == null || entry.to == null) continue;
                    UpgradeGene(pawn, entry.from, entry.to);
                }
            }

            if (props.grantGenes != null)
            {
                foreach (var gene in props.grantGenes)
                {
                    if (gene == null) continue;
                    if (pawn.genes.HasActiveGene(gene)) continue;
                    pawn.genes.AddGene(gene, xenogene: true);
                }
            }

            if (props.removeMarkerHediff != null)
            {
                var oldMarker = pawn.health.hediffSet.GetFirstHediffOfDef(props.removeMarkerHediff);
                if (oldMarker != null)
                {
                    pawn.health.RemoveHediff(oldMarker);
                }
            }

            if (props.markerHediff != null && !pawn.health.hediffSet.HasHediff(props.markerHediff))
            {
                pawn.health.AddHediff(props.markerHediff, null, null, null);
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
