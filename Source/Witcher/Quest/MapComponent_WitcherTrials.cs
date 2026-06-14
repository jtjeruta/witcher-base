using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public class MountainsTrialState : IExposable
    {
        public Pawn trialPawn;
        public List<Pawn> beasts;
        public string signalSuccess;
        public string signalFail;
        public bool signalSent;

        public void ExposeData()
        {
            Scribe_References.Look(ref trialPawn, "trialPawn");
            Scribe_Collections.Look(ref beasts, "beasts", LookMode.Reference);
            Scribe_Values.Look(ref signalSuccess, "signalSuccess");
            Scribe_Values.Look(ref signalFail, "signalFail");
            Scribe_Values.Look(ref signalSent, "signalSent", false);
        }
    }

    public class MapComponent_WitcherTrials : MapComponent
    {
        private List<MountainsTrialState> activeTrials = new List<MountainsTrialState>();

        public MapComponent_WitcherTrials(Map map) : base(map)
        {
        }

        public void RegisterMountainsTrial(MountainsTrialState state)
        {
            activeTrials.Add(state);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            for (int i = activeTrials.Count - 1; i >= 0; i--)
            {
                MountainsTrialState trial = activeTrials[i];
                if (trial == null || trial.signalSent)
                {
                    activeTrials.RemoveAt(i);
                    continue;
                }

                if (TryResolveTrial(trial))
                {
                    activeTrials.RemoveAt(i);
                }
            }
        }

        private static bool TryResolveTrial(MountainsTrialState trial)
        {
            if (trial.trialPawn == null || trial.trialPawn.Destroyed)
            {
                SendFail(trial);
                return true;
            }

            if (trial.trialPawn.Dead && !AllBeastsDead(trial.beasts))
            {
                SendFail(trial);
                return true;
            }

            if (AllBeastsDead(trial.beasts))
            {
                if (!trial.trialPawn.Dead)
                {
                    SendSuccess(trial);
                }
                else
                {
                    SendFail(trial);
                }

                return true;
            }

            return false;
        }

        private static bool AllBeastsDead(List<Pawn> beasts)
        {
            if (beasts == null || beasts.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < beasts.Count; i++)
            {
                Pawn beast = beasts[i];
                if (beast != null && !beast.Dead && !beast.Destroyed)
                {
                    return false;
                }
            }

            return true;
        }

        private static void SendSuccess(MountainsTrialState trial)
        {
            if (trial.signalSent) return;
            trial.signalSent = true;
            Find.SignalManager.SendSignal(new Signal(trial.signalSuccess));
        }

        private static void SendFail(MountainsTrialState trial)
        {
            if (trial.signalSent) return;
            trial.signalSent = true;
            Find.SignalManager.SendSignal(new Signal(trial.signalFail));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeTrials, "activeTrials", LookMode.Deep);
        }
    }
}
