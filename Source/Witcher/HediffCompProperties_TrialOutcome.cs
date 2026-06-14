using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public class GeneUpgradeEntry
    {
        public GeneDef from;
        public GeneDef to;
    }

    public class HediffCompProperties_TrialOutcome : HediffCompProperties
    {
        // Survival chance rolled when the fever is applied. surviveChance applies to everyone.
        // idealSurviveChance, if >= 0, overrides it for ideal subjects (boys aged 8-12); used only
        // by the Trial of Grasses. Dreams and Mutagens are initiate-only, so they just set surviveChance.
        public float surviveChance = 1f;
        public float idealSurviveChance = -1f;

        // How long the cosmetic fever runs before resolving (1-2 days by default).
        public IntRange feverDurationTicks = new IntRange(60000, 120000);

        // Survivors climb to this severity before the fever breaks; doomed climb to 1.0.
        public float survivorPeakSeverity = 0.97f;

        public HediffDef markerHediff;

        // Optional: marker hediff to remove on survival (e.g. Dreams clears the initiate marker).
        public HediffDef removeMarkerHediff;

        public List<GeneDef> grantGenes;

        public bool upgradeGenes;

        public List<GeneUpgradeEntry> geneUpgrades;

        public string survivalLetterLabel;
        public string survivalLetterText;
        public string deathLetterLabel;
        public string deathLetterText;

        public HediffCompProperties_TrialOutcome()
        {
            compClass = typeof(HediffComp_TrialOutcome);
        }
    }
}
