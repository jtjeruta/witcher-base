using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompProperties_AbilityWitcherJumpSlash : CompProperties_AbilityEffect
    {
        public float damageAmount = 20f;
        public float armorPenetration = 0.25f;
        public float slashRadius = 1.5f;
        public int stunTicks = 60;

        public CompProperties_AbilityWitcherJumpSlash()
        {
            compClass = typeof(CompAbilityEffect_WitcherJumpSlash);
        }
    }
}
