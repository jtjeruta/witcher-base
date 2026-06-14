using RimWorld;
using Verse;

namespace WitcherBase
{
    // Statically-resolved def references. Decorated with [DefOf] so the game wires these up at load.
    // The MayRequireBiotech() attribute lets the class load even if Biotech is missing, though the
    // mod's hard dependency means in practice Biotech will always be present.
    [DefOf]
    public static class WitcherDefOf
    {
        // Trial fever hediffs (this mod).
        public static HediffDef Witcher_GrassesFever;
        public static HediffDef Witcher_DreamsFever;
        public static HediffDef Witcher_MutagensFever;

        // Marker hediff applied to surviving initiates (Grasses), and to fully trained witchers (Dreams).
        public static HediffDef Witcher_Initiate;
        public static HediffDef Witcher_FullyTrained;

        // Biotech genes used in eligibility checks. These exist in the Biotech DLC; if the DLC ever
        // becomes optional we'd guard these with [MayRequire("Ludeon.RimWorld.Biotech")].
        [MayRequire("Ludeon.RimWorld.Biotech")]
        public static GeneDef MoveSpeed_VeryQuick;

        static WitcherDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WitcherDefOf));
        }
    }
}
