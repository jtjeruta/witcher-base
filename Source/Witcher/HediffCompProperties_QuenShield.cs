using Verse;

namespace WitcherBase
{
    public class HediffCompProperties_QuenShield : HediffCompProperties
    {
        public float maxEnergy = 50f;

        public HediffCompProperties_QuenShield()
        {
            compClass = typeof(HediffComp_QuenShield);
        }
    }
}
