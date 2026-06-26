using LudeonTK;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public static class DebugActionsWitcher
    {
        [DebugAction("Witcher", "Spawn witcher initiate", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnWitcherInitiate()
        {
            SpawnWitcherAtClick(WitcherDefOf.WitcherInitiate, "witcher initiate");
        }

        [DebugAction("Witcher", "Spawn witcher", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnWitcher()
        {
            SpawnWitcherAtClick(WitcherDefOf.Witcher, "witcher");
        }

        [DebugAction("Witcher", "Spawn master witcher", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnMasterWitcher()
        {
            SpawnWitcherAtClick(WitcherDefOf.WitcherMaster, "master witcher");
        }

        [DebugAction("Witcher", "Spawn mutated witcher", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnMutatedWitcher()
        {
            SpawnWitcherAtClick(WitcherDefOf.WitcherMutated, "mutated witcher");
        }

        private static void SpawnWitcherAtClick(XenotypeDef xenotype, string label)
        {
            Map map = Find.CurrentMap;
            if (map == null || xenotype == null)
            {
                return;
            }

            IntVec3 cell = UI.MouseCell();
            if (!cell.InBounds(map) || !cell.Standable(map))
            {
                cell = CellFinder.RandomClosewalkCellNear(cell, map, 3);
            }

            Pawn pawn = WitcherSpawnUtility.SpawnWitcher(xenotype, cell, map);
            if (pawn != null)
            {
                Messages.Message(
                    $"Spawned {label}.",
                    pawn,
                    MessageTypeDefOf.NeutralEvent,
                    historical: false);
            }
        }
    }
}
