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

        // Witcher xenotypes (linear progression ladder).
        public static XenotypeDef WitcherInitiate;
        public static XenotypeDef Witcher;
        public static XenotypeDef WitcherMaster;
        public static XenotypeDef WitcherMutated;

        // Custom genes.
        public static GeneDef Witcher_ContractGene;

        // Research and quest defs.
        public static ResearchProjectDef Witcher_Traditions;
        public static ResearchProjectDef Witcher_TrialOfGrasses;
        public static ResearchProjectDef Witcher_TrialOfMountains;
        public static QuestScriptDef Witcher_TrialOfMountainsQuest;

        static WitcherDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WitcherDefOf));
        }
    }
}
