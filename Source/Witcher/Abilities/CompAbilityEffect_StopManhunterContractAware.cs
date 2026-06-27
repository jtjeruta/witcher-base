using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompProperties_StopManhunterContractAware : CompProperties_StopManhunter
    {
        public CompProperties_StopManhunterContractAware()
        {
            compClass = typeof(CompAbilityEffect_StopManhunterContractAware);
        }
    }

    public class CompAbilityEffect_StopManhunterContractAware : CompAbilityEffect_StopManhunter
    {
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (WitcherMonstersInterop.IsContractBeast(target.Pawn))
            {
                return false;
            }

            return base.CanApplyOn(target, dest);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (WitcherMonstersInterop.IsContractBeast(target.Pawn))
            {
                return;
            }

            base.Apply(target, dest);
        }
    }
}
