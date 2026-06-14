using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompAbilityEffect_TameAnimal : CompAbilityEffect
    {
        public new CompProperties_AbilityTameAnimal Props => (CompProperties_AbilityTameAnimal)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent.pawn;
            Pawn animal = target.Pawn;
            if (!CanTameTarget(animal))
            {
                return;
            }

            // End any manhunter/maddened state first so the freshly-tamed animal is friendly.
            if (animal.InMentalState)
            {
                animal.mindState.mentalStateHandler.Reset();
            }

            animal.SetFaction(Faction.OfPlayer, caster);

            Messages.Message(
                caster.LabelShort + " tamed " + animal.LabelShort + " with Axii.",
                animal,
                MessageTypeDefOf.PositiveEvent);
        }

        private bool CanTameTarget(Pawn animal)
        {
            if (animal == null || !animal.Spawned || animal.Dead)
            {
                return false;
            }

            if (animal.RaceProps == null || !animal.RaceProps.Animal)
            {
                return false;
            }

            if (animal.Faction == Faction.OfPlayer)
            {
                return false;
            }

            return true;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return CanTameTarget(target.Pawn);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn animal = target.Pawn;
            if (!CanTameTarget(animal))
            {
                if (throwMessages)
                {
                    Messages.Message("Axii can only tame wild animals.", MessageTypeDefOf.RejectInput, historical: false);
                }

                return false;
            }

            return true;
        }
    }
}
