using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WitcherBase
{
    public class QuestNode_SpawnGreatBeast : QuestNode
    {
        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Map map = slate.Get<Map>("map");
            Pawn trialPawn = slate.Get<Pawn>("trialPawn");

            if (map == null || trialPawn == null)
            {
                Log.Error("[Witcher] Trial of Mountains quest is missing map or trialPawn.");
                return;
            }

            List<Pawn> beasts = GreatBeastSpawner.SpawnRandomBeasts(map, out string beastDescription);
            slate.Set("beastDescription", beastDescription);
            slate.Set("greatBeasts", beasts);

            var watcher = new QuestPart_WitcherMountainsWatcher
            {
                trialPawn = trialPawn,
                beasts = beasts,
                signalSuccess = QuestGenUtility.HardcodedSignalWithQuestID("Witcher_BeastKilled"),
                signalFail = QuestGenUtility.HardcodedSignalWithQuestID("Witcher_TrialFailed")
            };
            QuestGen.quest.AddPart(watcher);

            Find.LetterStack.ReceiveLetter(
                "Trial of Mountains: beast arrived",
                $"{trialPawn.LabelShort}'s trial has begun. {beastDescription.CapitalizeFirst()} has appeared nearby.",
                LetterDefOf.ThreatBig,
                new LookTargets(beasts));
        }

        protected override bool TestRunInt(Slate slate)
        {
            return slate.Exists("map") && slate.Exists("trialPawn");
        }
    }
}
