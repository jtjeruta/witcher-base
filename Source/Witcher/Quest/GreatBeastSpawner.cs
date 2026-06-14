using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public class GreatBeastEntry
    {
        public string pawnKindDefName;
        public IntRange countRange = new IntRange(1, 1);
        public float weight = 1f;
        public bool spawnAsHostileHumanlike;
    }

    public static class GreatBeastSpawner
    {
        private static readonly List<GreatBeastEntry> DefaultEntries = new List<GreatBeastEntry>
        {
            new GreatBeastEntry { pawnKindDefName = "Thrumbo", countRange = new IntRange(1, 1), weight = 1f },
            new GreatBeastEntry { pawnKindDefName = "Megaspider", countRange = new IntRange(1, 2), weight = 2f },
            new GreatBeastEntry { pawnKindDefName = "Sanguophage", countRange = new IntRange(1, 1), weight = 1.5f, spawnAsHostileHumanlike = true },
            new GreatBeastEntry { pawnKindDefName = "Wolf_Timber", countRange = new IntRange(3, 4), weight = 3f },
            new GreatBeastEntry { pawnKindDefName = "Wolf_Arctic", countRange = new IntRange(3, 4), weight = 3f },
            new GreatBeastEntry { pawnKindDefName = "Muffalo", countRange = new IntRange(3, 4), weight = 3f }
        };

        public static List<Pawn> SpawnRandomBeasts(Map map, out string beastDescription)
        {
            var validEntries = new List<GreatBeastEntry>();
            foreach (var entry in DefaultEntries)
            {
                if (DefDatabase<PawnKindDef>.GetNamedSilentFail(entry.pawnKindDefName) != null)
                {
                    validEntries.Add(entry);
                }
            }

            if (validEntries.Count == 0)
            {
                beastDescription = "a great beast";
                return new List<Pawn>();
            }

            GreatBeastEntry chosen = validEntries.RandomElementByWeight(e => e.weight);
            PawnKindDef kindDef = DefDatabase<PawnKindDef>.GetNamed(chosen.pawnKindDefName);
            int count = chosen.countRange.RandomInRange;
            var spawned = new List<Pawn>();

            for (int i = 0; i < count; i++)
            {
                IntVec3 cell = FindSpawnCell(map, i);
                Pawn beast = GenerateBeast(kindDef, chosen.spawnAsHostileHumanlike);
                if (beast == null) continue;

                GenSpawn.Spawn(beast, cell, map);
                if (chosen.spawnAsHostileHumanlike)
                {
                    MakeHostileHumanlike(beast);
                }
                else
                {
                    beast.mindState.mentalStateHandler.TryStartMentalState(
                        MentalStateDefOf.ManhunterPermanent,
                        null,
                        true,
                        false,
                        false);
                }

                spawned.Add(beast);
            }

            beastDescription = count == 1
                ? kindDef.label
                : $"{count} {kindDef.GetLabelPlural()}";

            return spawned;
        }

        private static Pawn GenerateBeast(PawnKindDef kindDef, bool hostileHumanlike)
        {
            if (hostileHumanlike)
            {
                Faction faction = Find.FactionManager.AllFactionsListForReading
                    .Find(f => f != null && f.HostileTo(Faction.OfPlayer));
                if (faction == null)
                {
                    faction = Faction.OfMechanoids;
                }

                return PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    kindDef,
                    faction,
                    PawnGenerationContext.NonPlayer,
                    forceGenerateNewPawn: true));
            }

            return PawnGenerator.GeneratePawn(kindDef);
        }

        private static void MakeHostileHumanlike(Pawn beast)
        {
            Faction faction = beast.Faction;
            if (faction == null || !faction.HostileTo(Faction.OfPlayer))
            {
                faction = Find.FactionManager.AllFactionsListForReading
                    .Find(f => f != null && f.HostileTo(Faction.OfPlayer));
                if (faction != null)
                {
                    beast.SetFaction(faction);
                }
            }
        }

        private static IntVec3 FindSpawnCell(Map map, int index)
        {
            if (RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entryCell, map, CellFinder.EdgeRoadChance_Hostile))
            {
                return CellFinder.RandomClosewalkCellNear(entryCell, map, 8);
            }

            return CellFinder.RandomEdgeCell(map) + new IntVec3(index, 0, 0);
        }
    }
}
