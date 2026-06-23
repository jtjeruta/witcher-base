using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace WitcherBase
{
    public class WorldObjectComp_OrphanStock : WorldObjectComp
    {
        private List<OrphanOffer> offers = new List<OrphanOffer>();
        private int nextRestockTick = -1;
        private bool everGeneratedStock;

        public Settlement Settlement => parent as Settlement;

        public IReadOnlyList<OrphanOffer> Offers => offers;

        public int TicksUntilRestock
        {
            get
            {
                if (nextRestockTick < 0)
                {
                    return 0;
                }

                return System.Math.Max(0, nextRestockTick - Find.TickManager.TicksGame);
            }
        }

        public bool HasStock => offers.Any(o => o.pawn != null && !o.pawn.Destroyed);

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);

            Settlement settlement = Settlement;
            if (settlement == null || !OrphanStockUtility.CanOfferOrphans(settlement))
            {
                return;
            }

            if (!everGeneratedStock)
            {
                return;
            }

            if (Find.TickManager.TicksGame >= nextRestockTick)
            {
                RegenerateStock();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref offers, "offers", LookMode.Deep);
            Scribe_Values.Look(ref nextRestockTick, "nextRestockTick", -1);
            Scribe_Values.Look(ref everGeneratedStock, "everGeneratedStock", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                offers = offers ?? new List<OrphanOffer>();
                offers.RemoveAll(o => o.pawn == null || o.pawn.Destroyed);
                foreach (OrphanOffer offer in offers)
                {
                    if (offer.circumstanceLabel.NullOrEmpty())
                    {
                        offer.circumstanceLabel = OrphanBackstoryUtility.ResolveCircumstanceLabel(offer);
                    }
                }
            }
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            Settlement settlement = Settlement;
            if (settlement == null || caravan == null || caravan.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            if (!OrphanStockUtility.CanOfferOrphans(settlement))
            {
                yield break;
            }

            EnsureInitialized();

            string label = "Buy children";
            string desc = "Purchase trial-age children from this settlement for the Trial of Grasses.";
            if (HasStock)
            {
                desc += "\n\n" + offers.Count(o => o.pawn != null && !o.pawn.Destroyed) + " available.";
            }
            else
            {
                desc += "\n\nNothing for sale.";
            }

            yield return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = desc,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Trade", true),
                action = () => Find.WindowStack.Add(new Dialog_BuyOrphans(caravan, settlement, this))
            };
        }

        public void EnsureInitialized()
        {
            if (everGeneratedStock)
            {
                return;
            }

            RegenerateStock();
        }

        public void RegenerateStock()
        {
            Settlement settlement = Settlement;
            if (settlement == null)
            {
                return;
            }

            ClearOffers();

            OrphanSettlementCategory category = OrphanStockUtility.GetCategory(settlement);
            int count = OrphanStockUtility.GetStockRange(category).RandomInRange;
            for (int i = 0; i < count; i++)
            {
                OrphanTier tier = OrphanStockUtility.RollTier(category);
                Pawn pawn = OrphanGenerator.Generate(settlement, tier);
                if (pawn == null)
                {
                    continue;
                }

                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
                int price = OrphanStockUtility.GetFinalPrice(tier, category);
                string circumstance = OrphanBackstoryUtility.ApplyChildhoodAndGetCircumstance(pawn, tier);
                offers.Add(new OrphanOffer(tier, price, pawn, circumstance));
            }

            everGeneratedStock = true;
            nextRestockTick = Find.TickManager.TicksGame
                + OrphanStockUtility.GetRestockDays(category) * GenDate.TicksPerDay;
        }

        public bool TryPurchase(Caravan caravan, OrphanOffer offer, out string failReason)
        {
            failReason = null;
            if (caravan == null || offer == null || offer.pawn == null || offer.pawn.Destroyed)
            {
                failReason = "That child is no longer available.";
                return false;
            }

            if (!offers.Contains(offer))
            {
                failReason = "That child is no longer available.";
                return false;
            }

            if (caravan.ContainsPawn(offer.pawn))
            {
                failReason = "That child is already in your caravan.";
                return false;
            }

            if (OrphanStockUtility.GetCaravanSilver(caravan) < offer.price)
            {
                failReason = "Not enough silver in the caravan.";
                return false;
            }

            if (!OrphanStockUtility.TryPaySilver(caravan, offer.price))
            {
                failReason = "Could not pay for the child.";
                return false;
            }

            offers.Remove(offer);
            offer.pawn.SetFaction(Faction.OfPlayer, null);
            caravan.AddPawn(offer.pawn, addCarriedPawnToWorldPawnsIfAny: true);

            Messages.Message(
                caravan.Name + " purchased " + offer.pawn.LabelShort + " for " + offer.price + " silver.",
                caravan,
                MessageTypeDefOf.NeutralEvent);

            return true;
        }

        public string GetOfferSummary(OrphanOffer offer)
        {
            if (offer?.pawn == null)
            {
                return "Unavailable";
            }

            Pawn pawn = offer.pawn;
            StringBuilder sb = new StringBuilder();
            sb.Append(pawn.LabelCap);
            sb.Append(" (");
            sb.Append(GenderLabel(pawn.gender));
            sb.Append(", ");
            sb.Append(pawn.ageTracker.AgeBiologicalYears);
            sb.Append(")");
            sb.Append(" — ");
            sb.Append(OrphanBackstoryUtility.ResolveCircumstanceLabel(offer));
            sb.Append(", ");
            sb.Append(offer.price);
            sb.Append(" silver");

            if (pawn.story?.traits?.allTraits != null)
            {
                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    sb.Append("\n  • ");
                    sb.Append(trait.LabelCap);
                }
            }

            return sb.ToString();
        }

        private void ClearOffers()
        {
            foreach (OrphanOffer offer in offers)
            {
                if (offer.pawn != null && !offer.pawn.Destroyed && !offer.pawn.IsCaravanMember())
                {
                    offer.pawn.Destroy(DestroyMode.Vanish);
                }
            }

            offers.Clear();
        }

        private static string GenderLabel(Gender gender)
        {
            if (gender == Gender.Male)
            {
                return "male";
            }

            if (gender == Gender.Female)
            {
                return "female";
            }

            return "unknown";
        }
    }
}
