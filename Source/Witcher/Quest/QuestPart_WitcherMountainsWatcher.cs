using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WitcherBase
{
    // Lightweight quest part that registers trial state with the map watcher.
    public class QuestPart_WitcherMountainsWatcher : QuestPart
    {
        public Pawn trialPawn;
        public List<Pawn> beasts = new List<Pawn>();
        public string signalSuccess;
        public string signalFail;

        private bool registered;

        public override void AssignDebugData()
        {
            base.AssignDebugData();
            RegisterWatcher();
        }

        public override void PostQuestAdded()
        {
            base.PostQuestAdded();
            RegisterWatcher();
        }

        private void RegisterWatcher()
        {
            if (registered) return;
            if (trialPawn == null || trialPawn.Map == null)
            {
                return;
            }

            MapComponent_WitcherTrials component = trialPawn.Map.GetComponent<MapComponent_WitcherTrials>();
            if (component == null)
            {
                return;
            }

            component.RegisterMountainsTrial(new MountainsTrialState
            {
                trialPawn = trialPawn,
                beasts = beasts,
                signalSuccess = signalSuccess,
                signalFail = signalFail
            });
            registered = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref trialPawn, "trialPawn");
            Scribe_Collections.Look(ref beasts, "beasts", LookMode.Reference);
            Scribe_Values.Look(ref signalSuccess, "signalSuccess");
            Scribe_Values.Look(ref signalFail, "signalFail");
            Scribe_Values.Look(ref registered, "registered", false);
        }
    }
}
