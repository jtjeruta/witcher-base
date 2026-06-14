using Verse;

namespace WitcherBase
{
    public class HediffComp_QuenShield : HediffComp
    {
        private float energy;

        public HediffCompProperties_QuenShield Props => (HediffCompProperties_QuenShield)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            energy = Props.maxEnergy;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref energy, "quenEnergy", Props.maxEnergy);
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamage)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamage);
            if (totalDamage <= 0f)
            {
                return;
            }

            energy -= totalDamage;
            if (energy <= 0f)
            {
                Pawn.health.RemoveHediff(parent);
            }
        }

        public override string CompTipStringExtra =>
            "Quen shield: " + energy.ToString("F0") + " / " + Props.maxEnergy.ToString("F0");
    }
}
