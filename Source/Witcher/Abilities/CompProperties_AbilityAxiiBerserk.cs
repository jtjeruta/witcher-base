using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompProperties_AbilityAxiiBerserk : CompProperties_AbilityEffect
    {
        public MentalStateDef mentalStateDef;

        public CompProperties_AbilityAxiiBerserk()
        {
            compClass = typeof(CompAbilityEffect_AxiiBerserk);
        }
    }
}
