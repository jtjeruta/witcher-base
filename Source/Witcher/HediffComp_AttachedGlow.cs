using RimWorld;
using Verse;

namespace WitcherBase
{
    public class HediffComp_AttachedGlow : HediffComp
    {
        public HediffCompProperties_AttachedGlow Props => (HediffCompProperties_AttachedGlow)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            Pawn pawn = parent.pawn;
            if (!pawn.Spawned || Props.moteDef == null)
            {
                return;
            }

            if (!pawn.IsHashIntervalTick(Props.emitInterval))
            {
                return;
            }

            MoteAttached mote = (MoteAttached)ThingMaker.MakeThing(Props.moteDef);
            if (Props.solidTimeOverride > 0f)
            {
                mote.solidTimeOverride = Props.solidTimeOverride;
            }

            mote.Attach(pawn);
            GenSpawn.Spawn(mote, pawn.Position, pawn.Map);
        }
    }
}
