using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompAbilityEffect_TameAnimal : CompAbilityEffect
    {
        public new CompProperties_AbilityTameAnimal Props => (CompProperties_AbilityTameAnimal)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent.pawn;
            Pawn tameTarget = target.Pawn;
            if (!CanTameTarget(tameTarget))
            {
                return;
            }

            if (tameTarget.InMentalState)
            {
                tameTarget.mindState.mentalStateHandler.Reset();
            }

            if (WildManUtility.IsWildMan(tameTarget) || tameTarget.RaceProps.Animal)
            {
                InteractionWorker_RecruitAttempt.DoRecruit(caster, tameTarget, useAudiovisualEffects: true);
            }
        }

        private bool CanTameTarget(Pawn tameTarget)
        {
            if (tameTarget == null || !tameTarget.Spawned || tameTarget.Dead)
            {
                return false;
            }

            if (tameTarget.Faction == Faction.OfPlayer)
            {
                return false;
            }

            if (WildManUtility.IsWildMan(tameTarget))
            {
                return true;
            }

            if (tameTarget.RaceProps == null || !tameTarget.RaceProps.Animal)
            {
                return false;
            }

            if (Props.maxWildness < 1f)
            {
                float wildness = tameTarget.GetStatValue(StatDefOf.Wildness);
                if (wildness > Props.maxWildness)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return CanTameTarget(target.Pawn);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!CanTameTarget(target.Pawn))
            {
                if (throwMessages)
                {
                    Messages.Message(
                        "Axii can only tame wild animals or wild people.",
                        MessageTypeDefOf.RejectInput,
                        historical: false);
                }

                return false;
            }

            return true;
        }
    }
}
