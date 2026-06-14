using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public class TrialRewardConfig
    {
        public List<GeneDef> grantGenes;
        public List<GeneUpgradeEntry> geneUpgrades;
        public bool upgradeGenes;
        public HediffDef markerHediff;
        public HediffDef removeMarkerHediff;
    }
}
