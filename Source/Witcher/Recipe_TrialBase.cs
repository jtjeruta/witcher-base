using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    // Shared recipe-worker plumbing for the three witcher trial operations.
    // Subclasses provide the fever hediff def and the eligibility check.
    public abstract class Recipe_TrialBase : Recipe_Surgery
    {
        protected abstract HediffDef TrialFeverDef { get; }

        // Returns an AcceptanceReport describing why this trial cannot be performed on the patient,
        // or WasAccepted if it can be. Reasons are surfaced in the Add Bill UI.
        protected abstract AcceptanceReport IsPatientEligible(Pawn pawn);

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            yield return pawn.RaceProps.body.corePart;
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!(thing is Pawn pawn)) return false;
            if (!base.AvailableOnNow(thing, part)) return false;
            return IsPatientEligible(pawn).Accepted;
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part)
        {
            if (!(thing is Pawn pawn)) return false;
            return IsPatientEligible(pawn);
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            // The standard surgery flow has already consumed the ingredients before this point.
            // We just add the trial fever; survival vs death is decided by the disease + its TrialOutcome comp.
            if (pawn == null || pawn.Dead) return;

            if (pawn.health.hediffSet.HasHediff(TrialFeverDef))
            {
                Messages.Message(
                    $"{pawn.LabelShortCap} is already undergoing this trial.",
                    pawn,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            pawn.health.AddHediff(TrialFeverDef, null, null, null);
        }
    }
}
