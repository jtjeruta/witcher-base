using RimWorld;
using Verse;

namespace WitcherBase
{
    public class Recipe_TrialOfDreams : Recipe_TrialBase
    {
        protected override HediffDef TrialFeverDef => WitcherDefOf.Witcher_DreamsFever;

        protected override AcceptanceReport IsPatientEligible(Pawn pawn)
        {
            if (pawn == null) return false;
            if (!pawn.RaceProps.Humanlike) return new AcceptanceReport("Only humanlike pawns can undergo the trial of dreams.");
            if (pawn.genes == null) return new AcceptanceReport("Pawn cannot carry genes.");

            if (pawn.genes.Xenotype != WitcherDefOf.WitcherInitiate)
            {
                return new AcceptanceReport("Must have survived the trial of grasses.");
            }

            if (pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_DreamsFever))
            {
                return new AcceptanceReport("Already undergoing the trial.");
            }

            return AcceptanceReport.WasAccepted;
        }
    }
}
