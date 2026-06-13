using RimWorld;
using Verse;

namespace WitcherBase
{
    public class Recipe_TrialOfGrasses : Recipe_TrialBase
    {
        protected override HediffDef TrialFeverDef => WitcherDefOf.Witcher_GrassesFever;

        protected override AcceptanceReport IsPatientEligible(Pawn pawn)
        {
            if (pawn == null) return false;
            if (!pawn.RaceProps.Humanlike) return new AcceptanceReport("Only humanlike pawns can undergo the trial of grasses.");

            if (pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_Initiate))
            {
                return new AcceptanceReport("Already a witcher initiate.");
            }

            if (pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_GrassesFever))
            {
                return new AcceptanceReport("Already undergoing the trial.");
            }

            return AcceptanceReport.WasAccepted;
        }
    }
}
