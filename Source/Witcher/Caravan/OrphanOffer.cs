using Verse;

namespace WitcherBase
{
    public class OrphanOffer : IExposable
    {
        public OrphanTier tier;
        public int price;
        public Pawn pawn;
        public string circumstanceLabel;

        public OrphanOffer()
        {
        }

        public OrphanOffer(OrphanTier tier, int price, Pawn pawn, string circumstanceLabel)
        {
            this.tier = tier;
            this.price = price;
            this.pawn = pawn;
            this.circumstanceLabel = circumstanceLabel;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tier, "tier");
            Scribe_Values.Look(ref price, "price");
            Scribe_Values.Look(ref circumstanceLabel, "circumstanceLabel");
            Scribe_References.Look(ref pawn, "pawn");
        }
    }
}
