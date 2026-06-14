using RimWorld;
using Verse;

namespace WitcherBase
{
    public static class WitcherXenotypes
    {
        public static void SetStage(Pawn pawn, XenotypeDef xenotype)
        {
            if (pawn == null || xenotype == null)
            {
                return;
            }

            if (pawn.genes == null)
            {
                Log.Warning($"[Witcher] Pawn {pawn.LabelShort} has no gene tracker; xenotype {xenotype.defName} not applied.");
                return;
            }

            pawn.genes.SetXenotype(xenotype);
            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }

        public static bool IsAnyWitcher(Pawn pawn)
        {
            return pawn?.genes?.Xenotype != null && IsWitcherXenotype(pawn.genes.Xenotype);
        }

        public static bool IsWitcherXenotype(XenotypeDef xenotype)
        {
            if (xenotype == null)
            {
                return false;
            }

            return xenotype == WitcherDefOf.WitcherInitiate
                || xenotype == WitcherDefOf.Witcher
                || xenotype == WitcherDefOf.WitcherMaster
                || xenotype == WitcherDefOf.WitcherMutated;
        }

        public static bool HasActiveMountainsTrial(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            foreach (Quest quest in Find.QuestManager.QuestsListForReading)
            {
                if (quest.State != QuestState.Ongoing)
                {
                    continue;
                }

                foreach (QuestPart part in quest.PartsListForReading)
                {
                    if (part is QuestPart_WitcherMountainsWatcher watcher && watcher.trialPawn == pawn)
                    {
                        return true;
                    }

                    if (part is QuestPart_WitcherMountainsReward reward && reward.trialPawn == pawn)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
