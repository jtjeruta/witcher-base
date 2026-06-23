using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompAbilityEffect_Yrden : CompAbilityEffect
    {
        public new CompProperties_AbilityYrden Props => (CompProperties_AbilityYrden)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent.pawn;
            Map map = caster?.Map;
            if (map == null || !target.IsValid || Props.hediffDef == null)
            {
                return;
            }

            float radius = parent.def.EffectRadius;
            int duration = GetDurationTicks();

            foreach (Pawn victim in GetHostilePawnsInRadius(target.Cell, map, caster, radius))
            {
                ApplySnare(victim, duration);
            }
        }

        private int GetDurationTicks()
        {
            return Props.durationTicks;
        }

        private void ApplySnare(Pawn victim, int duration)
        {
            BodyPartRecord part = Props.onlyBrain ? victim.health.hediffSet.GetBrain() : null;
            Hediff existing = victim.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (existing != null)
            {
                victim.health.RemoveHediff(existing);
            }

            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, victim, part);
            hediff.Severity = 1f;
            victim.health.AddHediff(hediff, part);

            HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.ticksToDisappear = duration;
            }
        }

        private static IEnumerable<Pawn> GetHostilePawnsInRadius(IntVec3 center, Map map, Pawn caster, float radius)
        {
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(center, map, radius, true))
            {
                if (thing is Pawn pawn
                    && pawn != caster
                    && pawn.Spawned
                    && !pawn.Dead
                    && pawn.HostileTo(caster))
                {
                    yield return pawn;
                }
            }
        }
    }
}
