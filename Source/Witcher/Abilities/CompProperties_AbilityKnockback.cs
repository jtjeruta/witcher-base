using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompProperties_AbilityKnockback : CompProperties_AbilityEffect
    {
        public float radius = 1.9f;
        public int damageAmount = 10;
        public float armorPenetration = 0.2f;
        public int pushDistance = 1;
        public int stunTicks = 120;
        public float stunChance;
        public bool alwaysStun;
        public bool useEffectRadius;
        public bool targetHostilesOnly = true;

        // Cone mode: affect every pawn inside a wedge aimed at the target cell.
        public bool cone;
        public float coneAngle = 90f;

        public CompProperties_AbilityKnockback()
        {
            compClass = typeof(CompAbilityEffect_Knockback);
        }
    }
}
