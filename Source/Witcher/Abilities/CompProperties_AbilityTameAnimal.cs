using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompProperties_AbilityTameAnimal : CompProperties_AbilityEffect
    {
        public float maxWildness = 1f;

        public CompProperties_AbilityTameAnimal()
        {
            compClass = typeof(CompAbilityEffect_TameAnimal);
        }
    }
}
