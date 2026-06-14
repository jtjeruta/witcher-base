using RimWorld;
using Verse;

namespace WitcherBase
{
    public class Recipe_AdditionalMutagens : Recipe_TrialBase
    {
        protected override HediffDef TrialFeverDef => WitcherDefOf.Witcher_MutagensFever;

        protected override AcceptanceReport IsPatientEligible(Pawn pawn)
        {
            if (pawn == null) return false;
            if (!pawn.RaceProps.Humanlike) return new AcceptanceReport("Only humanlike pawns can receive additional mutagens.");
            if (pawn.genes == null) return new AcceptanceReport("Pawn cannot carry genes.");

            if (pawn.genes.Xenotype != WitcherDefOf.WitcherMaster)
            {
                return new AcceptanceReport("Must be a master witcher.");
            }

            if (pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_MutagensFever))
            {
                return new AcceptanceReport("Already undergoing additional mutagens.");
            }

            return AcceptanceReport.WasAccepted;
        }
    }
}
