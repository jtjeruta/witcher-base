using RimWorld;
using Verse;

namespace WitcherBase
{
    public static class WitcherMonstersInterop
    {
        private const string ContractBeastHediffName = "WitcherMonsters_ContractBeast";

        public static bool IsContractBeast(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null || pawn.Dead)
            {
                return false;
            }

            HediffDef contractBeast = DefDatabase<HediffDef>.GetNamedSilentFail(ContractBeastHediffName);
            return contractBeast != null && pawn.health.hediffSet.HasHediff(contractBeast);
        }
    }
}
