using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompProperties_AbilityStopMentalBreak : CompProperties_AbilityEffect
    {
        public HediffDef dazeHediff;
        public int dazeDurationTicks = 180;

        public CompProperties_AbilityStopMentalBreak()
        {
            compClass = typeof(CompAbilityEffect_StopMentalBreak);
        }
    }
}
