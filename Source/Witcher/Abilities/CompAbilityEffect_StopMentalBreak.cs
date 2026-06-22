using RimWorld;
using Verse;
using Verse.AI;

namespace WitcherBase
{
    public class CompAbilityEffect_StopMentalBreak : CompAbilityEffect
    {
        public new CompProperties_AbilityStopMentalBreak Props => (CompProperties_AbilityStopMentalBreak)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = target.Pawn;
            if (pawn?.InMentalState != true)
            {
                return;
            }

            MentalState mentalState = pawn.MentalState;
            if (mentalState == null)
            {
                pawn.mindState.mentalStateHandler.Reset();
                return;
            }

            ThoughtDef recoveryThought = mentalState.def?.moodRecoveryThought ?? ThoughtDefOf.Catharsis;
            mentalState.RecoverFromState();
            TryGrantRecoveryThought(pawn, recoveryThought);
            if (Props.dazeHediff != null)
            {
                Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(Props.dazeHediff);
                if (existing != null)
                {
                    pawn.health.RemoveHediff(existing);
                }

                Hediff hediff = HediffMaker.MakeHediff(Props.dazeHediff, pawn);
                HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (disappears != null)
                {
                    disappears.ticksToDisappear = Props.dazeDurationTicks;
                }

                pawn.health.AddHediff(hediff);
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return target.Pawn?.InMentalState == true;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return target.Pawn?.RaceProps.Humanlike == true;
        }

        private static void TryGrantRecoveryThought(Pawn pawn, ThoughtDef recoveryThought)
        {
            if (recoveryThought == null || pawn.needs?.mood == null)
            {
                return;
            }

            MemoryThoughtHandler memories = pawn.needs.mood.thoughts.memories;
            if (memories == null || memories.GetFirstMemoryOfDef(recoveryThought) != null)
            {
                return;
            }

            memories.TryGainMemory(recoveryThought, pawn);
        }
    }
}
