using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WitcherBase
{
    public enum OrphanSettlementCategory
    {
        Tribal,
        Outlander,
        Advanced
    }

    public static class OrphanStockUtility
    {
        public const int MinAgeYears = 8;
        public const int MaxAgeYears = 12;
        public const float MaleChance = 0.8f;

        public const int UnwantedPrice = 1;
        public static readonly IntRange StandardPriceRange = new IntRange(200, 450);
        public static readonly IntRange PremiumPriceRange = new IntRange(600, 1200);

        public static OrphanSettlementCategory GetCategory(Settlement settlement)
        {
            Faction faction = settlement?.Faction;
            if (faction?.def == null)
            {
                return OrphanSettlementCategory.Outlander;
            }

            TechLevel tech = faction.def.techLevel;
            if (tech <= TechLevel.Neolithic)
            {
                return OrphanSettlementCategory.Tribal;
            }

            if (tech >= TechLevel.Spacer)
            {
                return OrphanSettlementCategory.Advanced;
            }

            return OrphanSettlementCategory.Outlander;
        }

        public static IntRange GetStockRange(OrphanSettlementCategory category)
        {
            switch (category)
            {
                case OrphanSettlementCategory.Tribal:
                    return new IntRange(2, 4);
                case OrphanSettlementCategory.Advanced:
                    return new IntRange(0, 1);
                default:
                    return new IntRange(1, 2);
            }
        }

        public static int GetRestockDays(OrphanSettlementCategory category)
        {
            switch (category)
            {
                case OrphanSettlementCategory.Tribal:
                    return 45;
                case OrphanSettlementCategory.Advanced:
                    return 90;
                default:
                    return 60;
            }
        }

        public static float GetPriceMultiplier(OrphanSettlementCategory category)
        {
            switch (category)
            {
                case OrphanSettlementCategory.Tribal:
                    return 0.5f;
                case OrphanSettlementCategory.Advanced:
                    return 1.5f;
                default:
                    return 1f;
            }
        }

        public static OrphanTier RollTier(OrphanSettlementCategory category)
        {
            float roll = Rand.Value;
            switch (category)
            {
                case OrphanSettlementCategory.Tribal:
                    if (roll < 0.40f) return OrphanTier.Unwanted;
                    if (roll < 0.85f) return OrphanTier.Standard;
                    return OrphanTier.Premium;
                case OrphanSettlementCategory.Advanced:
                    if (roll < 0.10f) return OrphanTier.Unwanted;
                    if (roll < 0.50f) return OrphanTier.Standard;
                    return OrphanTier.Premium;
                default:
                    if (roll < 0.25f) return OrphanTier.Unwanted;
                    if (roll < 0.75f) return OrphanTier.Standard;
                    return OrphanTier.Premium;
            }
        }

        public static int GetBasePrice(OrphanTier tier)
        {
            switch (tier)
            {
                case OrphanTier.Unwanted:
                    return UnwantedPrice;
                case OrphanTier.Premium:
                    return PremiumPriceRange.RandomInRange;
                default:
                    return StandardPriceRange.RandomInRange;
            }
        }

        public static int GetFinalPrice(OrphanTier tier, OrphanSettlementCategory category)
        {
            float price = GetBasePrice(tier) * GetPriceMultiplier(category);
            return System.Math.Max(1, GenMath.RoundRandom(price));
        }

        public static bool CanOfferOrphans(Settlement settlement)
        {
            if (settlement == null || settlement.Faction == null)
            {
                return false;
            }

            if (settlement.Faction.IsPlayer)
            {
                return false;
            }

            if (settlement.Faction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }

            if (!WitcherResearchUtility.IsTraditionsResearched)
            {
                return false;
            }

            return true;
        }

        public static int GetCaravanSilver(Caravan caravan)
        {
            int silver = 0;
            foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
            {
                if (item.def == ThingDefOf.Silver)
                {
                    silver += item.stackCount;
                }
            }

            return silver;
        }

        public static bool TryPaySilver(Caravan caravan, int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (GetCaravanSilver(caravan) < amount)
            {
                return false;
            }

            int remaining = amount;
            CaravanInventoryUtility.TakeThings(caravan, thing =>
            {
                if (remaining <= 0 || thing.def != ThingDefOf.Silver)
                {
                    return 0;
                }

                int take = System.Math.Min(remaining, thing.stackCount);
                remaining -= take;
                return take;
            });

            return remaining <= 0;
        }

        public static string RestockLabel(int ticksUntilRestock)
        {
            if (ticksUntilRestock <= 0)
            {
                return "Orphan stock available now.";
            }

            return "Next orphan stock in " + ticksUntilRestock.ToStringTicksToPeriod();
        }
    }
}
