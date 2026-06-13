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

            if (!pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_Initiate))
            {
                return new AcceptanceReport("Must have survived the trial of grasses.");
            }

            if (pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_DreamsFever))
            {
                return new AcceptanceReport("Already undergoing the trial.");
            }

            // Already a full witcher (carries a Dreams-tier gene)? No reason to repeat.
            if (pawn.genes != null && pawn.genes.HasActiveGene(WitcherDefOf.Aggression_DeadCalm))
            {
                return new AcceptanceReport("Already a full witcher.");
            }

            return AcceptanceReport.WasAccepted;
        }
    }
}
