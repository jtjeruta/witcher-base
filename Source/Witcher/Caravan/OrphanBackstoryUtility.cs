using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public static class OrphanBackstoryUtility
    {
        private const string FoundlingFallback = "foundling";
        private const string FamilySaleSuffix = " — family sale";

        private static readonly string[] UnwantedBackstoryNames =
        {
            "FireScarredChild1",
            "OrphanOfWar60",
            "Orphan15",
            "Orphan25",
            "AbandonedOrphan61",
            "LaborCampOrphan91",
            "AbandonedChild30",
            "AbandonedChild23"
        };

        private static readonly string[] StandardBackstoryNames =
        {
            "Orphan11",
            "SpacerOrphan77",
            "IndustrialOrphan13",
            "WildChild5",
            "OrphanedAcrobat84",
            "Orphan25",
            "Orphan15"
        };

        private static readonly string[] PremiumBackstoryNames =
        {
            "Student65",
            "Bookworm3",
            "Bookworm19",
            "StudentEngineer34",
            "StudentSocialite89"
        };

        public static string ApplyChildhoodAndGetCircumstance(Pawn pawn, OrphanTier tier)
        {
            if (pawn?.story == null)
            {
                return GetCircumstanceForTier(tier, FoundlingFallback);
            }

            BackstoryDef backstory = null;
            switch (tier)
            {
                case OrphanTier.Unwanted:
                    backstory = PickBackstory(UnwantedBackstoryNames);
                    break;
                case OrphanTier.Premium:
                    backstory = PickBackstory(PremiumBackstoryNames);
                    break;
                default:
                    backstory = PickBackstory(StandardBackstoryNames);
                    break;
            }

            if (backstory != null)
            {
                pawn.story.Childhood = backstory;
            }

            string title = GetBackstoryTitle(pawn.story.Childhood);
            return GetCircumstanceForTier(tier, title);
        }

        public static string ResolveCircumstanceLabel(OrphanOffer offer)
        {
            if (offer == null)
            {
                return FoundlingFallback;
            }

            if (!offer.circumstanceLabel.NullOrEmpty())
            {
                return offer.circumstanceLabel;
            }

            string title = GetBackstoryTitle(offer.pawn?.story?.Childhood);
            if (title.NullOrEmpty())
            {
                title = FoundlingFallback;
            }

            return GetCircumstanceForTier(offer.tier, title);
        }

        private static string GetCircumstanceForTier(OrphanTier tier, string backstoryTitle)
        {
            if (backstoryTitle.NullOrEmpty())
            {
                backstoryTitle = FoundlingFallback;
            }

            if (tier == OrphanTier.Premium)
            {
                return backstoryTitle + FamilySaleSuffix;
            }

            return backstoryTitle;
        }

        private static string GetBackstoryTitle(BackstoryDef backstory)
        {
            if (backstory == null)
            {
                return null;
            }

            if (!backstory.title.NullOrEmpty())
            {
                return backstory.title;
            }

            return backstory.titleShort;
        }

        private static BackstoryDef PickBackstory(IReadOnlyList<string> names)
        {
            if (names == null || names.Count == 0)
            {
                return null;
            }

            List<BackstoryDef> candidates = new List<BackstoryDef>();
            foreach (string name in names)
            {
                BackstoryDef backstory = DefDatabase<BackstoryDef>.GetNamedSilentFail(name);
                if (backstory != null && backstory.slot == BackstorySlot.Childhood)
                {
                    candidates.Add(backstory);
                }
            }

            return candidates.Count > 0 ? candidates.RandomElement() : null;
        }
    }
}
