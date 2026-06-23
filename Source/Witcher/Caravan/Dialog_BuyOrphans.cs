using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace WitcherBase
{
    public class Dialog_BuyOrphans : Window
    {
        private const float RowPadding = 8f;
        private const float RowSpacing = 6f;
        private const float BuyButtonWidth = 90f;
        private const float BuyButtonMargin = 10f;

        private readonly Caravan caravan;
        private readonly Settlement settlement;
        private readonly WorldObjectComp_OrphanStock stock;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(620f, 580f);

        public Dialog_BuyOrphans(Caravan caravan, Settlement settlement, WorldObjectComp_OrphanStock stock)
        {
            this.caravan = caravan;
            this.settlement = settlement;
            this.stock = stock;
            forcePause = true;
            absorbInputAroundWindow = true;
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Buy children");
            Text.Font = GameFont.Small;

            float y = 40f;
            Widgets.Label(new Rect(0f, y, inRect.width, 22f), settlement.LabelCap);
            y += 24f;
            Widgets.Label(new Rect(0f, y, inRect.width, 22f), "Caravan silver: " + OrphanStockUtility.GetCaravanSilver(caravan));
            y += 30f;

            float bottomMargin = CloseButSize.y + 10f;
            float labelWidth = inRect.width - BuyButtonWidth - BuyButtonMargin - 24f;
            Rect scrollRect = new Rect(0f, y, inRect.width, inRect.height - y - bottomMargin);
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - 16f, GetContentHeight(labelWidth));

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0f;

            List<OrphanOffer> offers = new List<OrphanOffer>(stock.Offers);
            if (!stock.HasStock)
            {
                Widgets.Label(new Rect(0f, curY, viewRect.width, 30f), "Nothing for sale.");
            }
            else
            {
                foreach (OrphanOffer offer in offers)
                {
                    if (offer.pawn == null || offer.pawn.Destroyed)
                    {
                        continue;
                    }

                    string summary = stock.GetOfferSummary(offer);
                    float rowHeight = GetRowHeight(summary, labelWidth);
                    Rect row = new Rect(0f, curY, viewRect.width, rowHeight);
                    Widgets.DrawHighlightIfMouseover(row);
                    Widgets.Label(new Rect(4f, curY + 4f, labelWidth, rowHeight - 8f), summary);

                    float buttonY = curY + (rowHeight - 28f) * 0.5f;
                    if (Widgets.ButtonText(new Rect(row.width - BuyButtonWidth - 4f, buttonY, BuyButtonWidth, 28f), "Buy"))
                    {
                        if (stock.TryPurchase(caravan, offer, out string failReason))
                        {
                            if (!stock.HasStock)
                            {
                                Close();
                            }
                        }
                        else if (!failReason.NullOrEmpty())
                        {
                            Messages.Message(failReason, MessageTypeDefOf.RejectInput, false);
                        }
                    }

                    curY += rowHeight + RowSpacing;
                }
            }

            Widgets.EndScrollView();
        }

        private float GetRowHeight(string summary, float labelWidth)
        {
            float textHeight = Text.CalcHeight(summary, labelWidth);
            return textHeight + RowPadding + 8f;
        }

        private float GetContentHeight(float labelWidth)
        {
            int count = 0;
            float height = 0f;

            foreach (OrphanOffer offer in stock.Offers)
            {
                if (offer.pawn == null || offer.pawn.Destroyed)
                {
                    continue;
                }

                count++;
                height += GetRowHeight(stock.GetOfferSummary(offer), labelWidth) + RowSpacing;
            }

            if (count == 0)
            {
                return 40f;
            }

            return height;
        }
    }
}
