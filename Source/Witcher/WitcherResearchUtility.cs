using RimWorld;
using Verse;

namespace WitcherBase
{
    public static class WitcherResearchUtility
    {
        public static bool IsTraditionsResearched =>
            WitcherDefOf.Witcher_Traditions != null && WitcherDefOf.Witcher_Traditions.IsFinished;
    }
}
