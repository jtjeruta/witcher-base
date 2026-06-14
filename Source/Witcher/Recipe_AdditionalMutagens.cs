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

            // Must be a fully trained witcher (survived the trial of dreams).
            if (!pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_FullyTrained))
            {
                return new AcceptanceReport("Must be a full witcher (trial of dreams not completed).");
            }

            if (pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_MutagensFever))
            {
                return new AcceptanceReport("Already undergoing additional mutagens.");
            }

            // If they already carry the top-tier movement gene, the upgrade is complete.
            if (pawn.genes.HasActiveGene(WitcherDefOf.MoveSpeed_VeryQuick))
            {
                return new AcceptanceReport("Already at peak mutation.");
            }

            return AcceptanceReport.WasAccepted;
        }
    }
}
