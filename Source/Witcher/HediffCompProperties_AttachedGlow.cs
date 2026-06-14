using Verse;

namespace WitcherBase
{
    public class HediffCompProperties_AttachedGlow : HediffCompProperties
    {
        public ThingDef moteDef;
        public int emitInterval = 20;
        public float scale = 1f;
        public float solidTimeOverride = 25f;

        public HediffCompProperties_AttachedGlow()
        {
            compClass = typeof(HediffComp_AttachedGlow);
        }
    }
}
