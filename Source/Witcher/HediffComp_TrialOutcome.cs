using System;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public class HediffComp_TrialOutcome : HediffComp
    {
        private const int IdealMinAge = 8;
        private const int IdealMaxAge = 12;

        private bool rolled;
        private bool willSurvive;
        private int durationTicks;
        private bool outcomeApplied;
        private bool resolved;
        private float rolledValue = -1f;
        private float chanceUsed = -1f;
        private bool wasIdealSubject;

        public HediffCompProperties_TrialOutcome Props =>
            (HediffCompProperties_TrialOutcome)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            RollOutcome();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            var pawn = Pawn;
            if (pawn == null || pawn.Dead) return;
            if (!rolled) RollOutcome();

            float progress = durationTicks <= 0 ? 1f : (float)parent.ageTicks / durationTicks;
            if (progress > 1f) progress = 1f;

            float peak = willSurvive ? Props.survivorPeakSeverity : 1f;
            parent.Severity = progress * peak;

            if (progress >= 1f && !outcomeApplied)
            {
                outcomeApplied = true;
                if (willSurvive)
                {
                    pawn.health.RemoveHediff(parent);
                }
                else
                {
                    parent.Severity = 1f;
                    pawn.health.CheckForStateChange(null, parent);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            var pawn = Pawn;
            if (pawn == null || pawn.Dead) return;
            if (!rolled || !willSurvive) return;
            if (resolved) return;
            resolved = true;

            try
            {
                TrialRewards.Apply(pawn, Props);
            }
            catch (Exception ex)
            {
                Log.Error($"[Witcher] Error applying trial rewards to {pawn?.LabelShort}: {ex}");
            }

            SendSurvivalLetter(pawn);
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit)
        {
            base.Notify_PawnDied(dinfo, culprit);
            if (resolved) return;
            resolved = true;

            var pawn = Pawn;
            if (pawn == null) return;

            SendDeathLetter(pawn);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref rolled, "witcher_trialRolled", false);
            Scribe_Values.Look(ref willSurvive, "witcher_trialWillSurvive", false);
            Scribe_Values.Look(ref durationTicks, "witcher_trialDurationTicks", 0);
            Scribe_Values.Look(ref outcomeApplied, "witcher_trialOutcomeApplied", false);
            Scribe_Values.Look(ref resolved, "witcher_trialResolved", false);
            Scribe_Values.Look(ref rolledValue, "witcher_trialRolledValue", -1f);
            Scribe_Values.Look(ref chanceUsed, "witcher_trialChanceUsed", -1f);
            Scribe_Values.Look(ref wasIdealSubject, "witcher_trialWasIdealSubject", false);
        }

        // Appended to the Health-tab hediff tooltip. Guarded so it only shows when dev mode is on.
        public override string CompTipStringExtra
        {
            get
            {
                if (!Prefs.DevMode) return null;
                if (!rolled) return "[DEV] Witcher trial: outcome not yet rolled";

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("[DEV] Witcher trial (predetermined outcome):");
                sb.AppendLine($"  Result: {(willSurvive ? "SURVIVE" : "DIE")}");
                sb.AppendLine($"  Survival chance: {chanceUsed.ToStringPercent()}");
                sb.AppendLine($"  Rolled: {rolledValue:F3} (survives if < {chanceUsed:F3})");
                sb.AppendLine($"  Ideal subject: {wasIdealSubject.ToStringYesNo()}");
                sb.Append($"  Fever progress: {parent.ageTicks}/{durationTicks} ticks");
                return sb.ToString();
            }
        }

        private void RollOutcome()
        {
            if (rolled) return;

            var pawn = Pawn;
            if (pawn == null) return;

            rolled = true;
            float chance = Props.surviveChance;
            wasIdealSubject = Props.idealSurviveChance >= 0f && IsIdealSubject(pawn);
            if (wasIdealSubject)
            {
                chance = Props.idealSurviveChance;
            }
            chanceUsed = chance;
            rolledValue = Rand.Value;
            willSurvive = rolledValue < chance;
            durationTicks = Props.feverDurationTicks.RandomInRange;
        }

        private static bool IsIdealSubject(Pawn pawn)
        {
            return pawn.gender == Gender.Male
                && pawn.ageTracker.AgeBiologicalYears >= IdealMinAge
                && pawn.ageTracker.AgeBiologicalYears <= IdealMaxAge;
        }

        private void SendSurvivalLetter(Pawn pawn)
        {
            if (string.IsNullOrEmpty(Props.survivalLetterLabel)) return;
            if (Find.LetterStack == null) return;

            var label = ResolveTemplate(Props.survivalLetterLabel, pawn);
            var text = ResolveTemplate(Props.survivalLetterText, pawn);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, pawn);
        }

        private void SendDeathLetter(Pawn pawn)
        {
            if (string.IsNullOrEmpty(Props.deathLetterLabel)) return;
            if (Find.LetterStack == null) return;

            var label = ResolveTemplate(Props.deathLetterLabel, pawn);
            var text = ResolveTemplate(Props.deathLetterText, pawn);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawn);
        }

        private static TaggedString ResolveTemplate(string template, Pawn pawn)
        {
            if (string.IsNullOrEmpty(template)) return TaggedString.Empty;
            return template.Formatted(pawn.Named("PAWN"));
        }
    }
}
