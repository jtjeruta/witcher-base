using RimWorld;
using Verse;

namespace WitcherBase
{
    // Removes stale Witcher_ContractGene from pawns promoted past the Witcher stage.
    public class GameComponent_WitcherContractCleanup : GameComponent
    {
        private const int CleanupIntervalTicks = 250;

        private int ticksUntilCleanup;

        public GameComponent_WitcherContractCleanup()
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RunCleanup();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (--ticksUntilCleanup > 0)
            {
                return;
            }

            ticksUntilCleanup = CleanupIntervalTicks;
            RunCleanup();
        }

        private static void RunCleanup()
        {
            if (WitcherDefOf.Witcher_ContractGene == null)
            {
                return;
            }

            foreach (Map map in Find.Maps)
            {
                if (map == null)
                {
                    continue;
                }

                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    TryStripStaleContractGene(pawn);
                }
            }
        }

        private static void TryStripStaleContractGene(Pawn pawn)
        {
            if (pawn?.genes == null || pawn.Dead)
            {
                return;
            }

            XenotypeDef xenotype = pawn.genes.Xenotype;
            if (xenotype != WitcherDefOf.WitcherMaster && xenotype != WitcherDefOf.WitcherMutated)
            {
                return;
            }

            Gene contractGene = pawn.genes.GetGene(WitcherDefOf.Witcher_ContractGene);
            if (contractGene == null)
            {
                return;
            }

            pawn.genes.RemoveGene(contractGene);
        }
    }
}
