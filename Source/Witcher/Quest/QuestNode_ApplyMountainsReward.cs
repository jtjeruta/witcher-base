using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WitcherBase
{
    public class QuestNode_ApplyMountainsReward : QuestNode
    {
        public SlateRef<string> inSignal;
        public SlateRef<string> successSignal;
        public SlateRef<string> failSignal;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Pawn pawn = slate.Get<Pawn>("trialPawn");
            string signal = inSignal.GetValue(slate);
            string success = successSignal.GetValue(slate);
            string fail = failSignal.GetValue(slate);
            if (pawn == null || string.IsNullOrEmpty(signal) || string.IsNullOrEmpty(success) || string.IsNullOrEmpty(fail))
            {
                return;
            }

            QuestGen.quest.AddPart(new QuestPart_WitcherMountainsReward
            {
                trialPawn = pawn,
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID(signal),
                successSignal = QuestGenUtility.HardcodedSignalWithQuestID(success),
                failSignal = QuestGenUtility.HardcodedSignalWithQuestID(fail)
            });
        }

        protected override bool TestRunInt(Slate slate)
        {
            return slate.Exists("trialPawn");
        }
    }

    public class QuestPart_WitcherMountainsReward : QuestPart
    {
        public Pawn trialPawn;
        public string inSignal;
        public string successSignal;
        public string failSignal;
        private bool applied;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (applied || signal.tag != inSignal)
            {
                return;
            }

            applied = true;
            Pawn pawn = trialPawn;
            if (pawn == null || pawn.Dead)
            {
                Find.SignalManager.SendSignal(new Signal(failSignal));
                return;
            }

            MountainsTrialRewards.Apply(pawn);
            Find.LetterStack.ReceiveLetter(
                $"Master witcher: {pawn.LabelShort}",
                $"{pawn.LabelShort} has fulfilled the contract and slain the beast. The Trial of Mountains is complete.",
                LetterDefOf.PositiveEvent,
                pawn);
            Find.SignalManager.SendSignal(new Signal(successSignal));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref trialPawn, "trialPawn");
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref successSignal, "successSignal");
            Scribe_Values.Look(ref failSignal, "failSignal");
            Scribe_Values.Look(ref applied, "applied", false);
        }
    }
}
