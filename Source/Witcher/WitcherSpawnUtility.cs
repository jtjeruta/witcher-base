using RimWorld;
using Verse;

namespace WitcherBase
{
    public static class WitcherSpawnUtility
    {
        public static Pawn SpawnWitcher(XenotypeDef xenotype, IntVec3 cell, Map map)
        {
            if (xenotype == null || map == null || !cell.InBounds(map))
            {
                return null;
            }

            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                PawnKindDefOf.Colonist,
                Faction.OfPlayer,
                PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                fixedBiologicalAge: 25f,
                fixedChronologicalAge: 25f,
                forcedXenotype: xenotype,
                forceNoGear: true,
                dontGiveWeapon: true));

            if (pawn == null)
            {
                return null;
            }

            EquipDevGear(pawn);
            GenSpawn.Spawn(pawn, cell, map);
            return pawn;
        }

        private static void EquipDevGear(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            TryWear(pawn, "Apparel_Pants");
            TryWear(pawn, "Apparel_ShirtBasic");
            TryWear(pawn, "Apparel_PlateArmor");
            TryEquipWeapon(pawn, "MeleeWeapon_LongSword");
        }

        private static void TryWear(Pawn pawn, string apparelDefName)
        {
            ThingDef apparelDef = DefDatabase<ThingDef>.GetNamedSilentFail(apparelDefName);
            if (apparelDef == null || pawn.apparel == null)
            {
                return;
            }

            Thing thing = MakeThing(apparelDef);
            if (thing is Apparel apparel)
            {
                pawn.apparel.Wear(apparel, false);
            }
        }

        private static void TryEquipWeapon(Pawn pawn, string weaponDefName)
        {
            ThingDef weaponDef = DefDatabase<ThingDef>.GetNamedSilentFail(weaponDefName);
            if (weaponDef == null || pawn.equipment == null)
            {
                return;
            }

            Thing thing = MakeThing(weaponDef);
            if (thing is ThingWithComps weapon)
            {
                pawn.equipment.AddEquipment(weapon);
            }
        }

        private static Thing MakeThing(ThingDef def)
        {
            if (def == null)
            {
                return null;
            }

            if (def.MadeFromStuff)
            {
                ThingDef stuff = GenStuff.DefaultStuffFor(def);
                return ThingMaker.MakeThing(def, stuff);
            }

            return ThingMaker.MakeThing(def);
        }
    }
}
