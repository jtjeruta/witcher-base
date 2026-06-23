using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompProperties_AbilityYrden : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public bool onlyBrain = true;
        public int durationTicks = 2400;

        public CompProperties_AbilityYrden()
        {
            compClass = typeof(CompAbilityEffect_Yrden);
        }
    }
}
