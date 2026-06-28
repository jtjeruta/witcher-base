using RimWorld;
using Verse;
using Verse.AI;

namespace WitcherBase
{
    public class CompAbilityEffect_WitcherJumpSlash : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
    {
        public new CompProperties_AbilityWitcherJumpSlash Props => (CompProperties_AbilityWitcherJumpSlash)props;

        public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target)
        {
            Pawn caster = parent.pawn;
            Map map = caster?.Map;
            if (map == null || caster.Dead)
            {
                return;
            }

            foreach (Pawn victim in AffectedPawns(caster, map))
            {
                ApplySlash(caster, victim);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Pawn == null || !target.Pawn.Spawned || target.Pawn.Dead)
            {
                return false;
            }

            if (!target.Pawn.HostileTo(parent.pawn))
            {
                return false;
            }

            return parent.pawn.CanReach(target.Pawn, PathEndMode.Touch, Danger.Deadly);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null && target.Pawn.HostileTo(parent.pawn);
        }

        private System.Collections.Generic.IEnumerable<Pawn> AffectedPawns(Pawn caster, Map map)
        {
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(caster.Position, map, Props.slashRadius, true))
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

        private void ApplySlash(Pawn caster, Pawn victim)
        {
            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.Cut,
                Props.damageAmount,
                Props.armorPenetration,
                -1f,
                caster);
            victim.TakeDamage(dinfo);

            if (victim.Dead || !victim.Spawned || Props.stunTicks <= 0)
            {
                return;
            }

            if (victim.stances?.stunner != null)
            {
                victim.stances.stunner.StunFor(Props.stunTicks, caster);
            }
        }
    }
}
