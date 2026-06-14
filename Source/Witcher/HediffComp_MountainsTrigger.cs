using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace WitcherBase
{
    public class HediffComp_MountainsTrigger : HediffComp
    {
        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            var baseGizmos = base.CompGetGizmos();
            if (baseGizmos != null)
            {
                foreach (var gizmo in baseGizmos)
                {
                    yield return gizmo;
                }
            }

            Pawn pawn = Pawn;
            if (pawn == null || !pawn.IsColonistPlayerControlled)
            {
                yield break;
            }

            if (WitcherDefOf.Witcher_TrialOfMountains == null || !WitcherDefOf.Witcher_TrialOfMountains.IsFinished)
            {
                yield break;
            }

            if (pawn.health.hediffSet.HasHediff(WitcherDefOf.Witcher_Master))
            {
                yield break;
            }

            if (HasActiveMountainsTrial(pawn))
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "Take witcher contract",
                defaultDesc = "Accept a contract to hunt a beast sighted on a nearby tile. Travel there and slay it to complete the Trial of Mountains.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Genes/Gene_StrongMeleeDamage", true),
                action = BeginTrial
            };
        }

        private void BeginTrial()
        {
            Pawn pawn = Pawn;
            if (pawn == null || pawn.Map == null)
            {
                return;
            }

            if (WitcherDefOf.Witcher_TrialOfMountainsQuest == null)
            {
                Log.Error("[Witcher] Trial of Mountains quest def is missing.");
                return;
            }

            Slate slate = new Slate();
            slate.Set("trialPawn", pawn);
            slate.Set("map", pawn.Map);
            slate.Set("points", 400f);

            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(WitcherDefOf.Witcher_TrialOfMountainsQuest, slate);
            if (quest == null)
            {
                return;
            }

            Find.LetterStack.ReceiveLetter(
                "Witcher contract accepted",
                $"{pawn.LabelShort} has taken a witcher contract. A beast lair has appeared on a nearby tile. Send {pawn.LabelShort} there to hunt it down.",
                LetterDefOf.PositiveEvent,
                LookTargets.Invalid,
                null,
                quest);
        }

        private static bool HasActiveMountainsTrial(Pawn pawn)
        {
            foreach (Quest quest in Find.QuestManager.QuestsListForReading)
            {
                if (quest.State != QuestState.Ongoing)
                {
                    continue;
                }

                foreach (QuestPart part in quest.PartsListForReading)
                {
                    if (part is QuestPart_WitcherMountainsWatcher watcher && watcher.trialPawn == pawn)
                    {
                        return true;
                    }

                    if (part is QuestPart_WitcherMountainsReward reward && reward.trialPawn == pawn)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
