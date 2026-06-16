using RimWorld;
using Verse;

namespace WitcherBase
{
    // Graduated witcher toxin metabolism. On top of the vanilla -0.08/day decay of
    // ToxicBuildup, a witcher's accelerated metabolism actively burns the poison
    // down faster. The recovery rate is derived from the gene's own ToxicResistance
    // offset, so all four tolerance tiers (I-IV) share this one class and scale
    // automatically with their resistance value.
    public class Gene_WitcherToxMetabolism : Gene
    {
        // Extra ToxicBuildup cleared per day, per point of ToxicResistance offset.
        private const float RecoveryPerResistancePerDay = 0.5f;
        private const float TicksPerDay = 60000f;

        private float cachedRatePerDay = -1f;

        private float RecoveryPerDay
        {
            get
            {
                if (cachedRatePerDay < 0f)
                {
                    float toxResistance = 0f;
                    if (def.statOffsets != null)
                    {
                        foreach (StatModifier mod in def.statOffsets)
                        {
                            if (mod.stat == StatDefOf.ToxicResistance)
                            {
                                toxResistance = mod.value;
                                break;
                            }
                        }
                    }

                    cachedRatePerDay = toxResistance * RecoveryPerResistancePerDay;
                }

                return cachedRatePerDay;
            }
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            if (!Active || pawn == null || pawn.Dead || RecoveryPerDay <= 0f)
            {
                return;
            }

            HediffSet hediffSet = pawn.health?.hediffSet;
            if (hediffSet == null)
            {
                return;
            }

            Hediff toxicBuildup = hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
            if (toxicBuildup == null || toxicBuildup.Severity <= 0f)
            {
                return;
            }

            float reduction = RecoveryPerDay * (delta / TicksPerDay);
            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, -reduction);
        }
    }
}
