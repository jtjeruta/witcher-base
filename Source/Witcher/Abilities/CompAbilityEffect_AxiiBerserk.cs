using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace WitcherBase
{
    public class CompAbilityEffect_AxiiBerserk : CompAbilityEffect
    {
        public new CompProperties_AbilityAxiiBerserk Props => (CompProperties_AbilityAxiiBerserk)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent.pawn;
            Pawn victim = target.Pawn;
            if (!CanBerserkTarget(victim) || Props.mentalStateDef == null)
            {
                return;
            }

            victim.GetLord()?.RemovePawn(victim);

            CompAbilityEffect_GiveMentalState.TryGiveMentalState(
                Props.mentalStateDef,
                victim,
                parent.def,
                StatDefOf.Ability_Duration,
                caster,
                forced: true);

            Messages.Message(
                caster.LabelShort + " sent " + victim.LabelShort + " into a berserk rage with Axii.",
                victim,
                MessageTypeDefOf.PositiveEvent);
        }

        private bool CanBerserkTarget(Pawn victim)
        {
            if (victim == null || !victim.Spawned || victim.Dead || victim.Downed)
            {
                return false;
            }

            if (victim.Faction == Faction.OfPlayer)
            {
                return false;
            }

            if (!victim.HostileTo(Faction.OfPlayer))
            {
                return false;
            }

            if (victim.RaceProps == null || !victim.RaceProps.Humanlike)
            {
                return false;
            }

            if (WildManUtility.IsWildMan(victim))
            {
                return false;
            }

            if (victim.RaceProps.IsMechanoid)
            {
                return false;
            }

            return true;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return CanBerserkTarget(target.Pawn);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!CanBerserkTarget(target.Pawn))
            {
                if (throwMessages)
                {
                    Messages.Message(
                        "Axii berserk only works on hostile humanlikes.",
                        MessageTypeDefOf.RejectInput,
                        historical: false);
                }

                return false;
            }

            return true;
        }
    }
}
