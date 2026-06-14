using RimWorld;
using Verse;

namespace WitcherBase
{
    [DefOf]
    public static class WitcherDefOf
    {
        // Trial fever hediffs (this mod).
        public static HediffDef Witcher_GrassesFever;
        public static HediffDef Witcher_DreamsFever;
        public static HediffDef Witcher_MutagensFever;

        // Rank and enhancement marker hediffs.
        public static HediffDef Witcher_Initiate;
        public static HediffDef Witcher_FullyTrained;
        public static HediffDef Witcher_Master;
        public static HediffDef Witcher_Mutated;

        // Research and quest defs.
        public static ResearchProjectDef Witcher_TrialOfMountains;
        public static QuestScriptDef Witcher_TrialOfMountainsQuest;

        [MayRequire("Ludeon.RimWorld.Biotech")]
        public static GeneDef MoveSpeed_VeryQuick;

        static WitcherDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WitcherDefOf));
        }
    }
}
