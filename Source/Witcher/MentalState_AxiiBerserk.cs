using RimWorld;
using Verse;
using Verse.AI;

namespace WitcherBase
{
    public class MentalState_AxiiBerserk : MentalState_Berserk
    {
        public override bool ForceHostileTo(Thing thing)
        {
            if (thing == null || thing == pawn)
            {
                return false;
            }

            Pawn otherPawn = thing as Pawn;
            if (otherPawn != null && otherPawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            return thing.HostileTo(Faction.OfPlayer);
        }

        public override bool ForceHostileTo(Faction faction)
        {
            if (faction == null || faction == Faction.OfPlayer)
            {
                return false;
            }

            return faction.HostileTo(Faction.OfPlayer);
        }
    }
}
