using RimWorld;
using Verse;

namespace WitcherBase
{
    public class HediffComp_YrdenMark : HediffComp
    {
        private MoteAttached attachedMote;

        public HediffCompProperties_YrdenMark Props => (HediffCompProperties_YrdenMark)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            SpawnMote();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            Pawn pawn = parent.pawn;
            if (!pawn.Spawned)
            {
                DestroyMote();
                return;
            }

            if (attachedMote == null || attachedMote.Destroyed || attachedMote.Position != pawn.Position)
            {
                SpawnMote();
            }
        }

        public override void CompPostPostRemoved()
        {
            DestroyMote();
            base.CompPostPostRemoved();
        }

        private void SpawnMote()
        {
            Pawn pawn = parent.pawn;
            if (Props.moteDef == null || !pawn.Spawned)
            {
                return;
            }

            DestroyMote();
            attachedMote = (MoteAttached)ThingMaker.MakeThing(Props.moteDef);
            attachedMote.Attach(pawn);
            GenSpawn.Spawn(attachedMote, pawn.Position, pawn.Map);
        }

        private void DestroyMote()
        {
            if (attachedMote != null && !attachedMote.Destroyed)
            {
                attachedMote.Destroy();
            }

            attachedMote = null;
        }
    }
}
