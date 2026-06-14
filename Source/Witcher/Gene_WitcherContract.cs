using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace WitcherBase
{
    public class Gene_WitcherContract : Gene
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo> baseGizmos = base.GetGizmos();
            if (baseGizmos != null)
            {
                foreach (Gizmo gizmo in baseGizmos)
                {
                    yield return gizmo;
                }
            }

            Pawn holder = pawn;
            if (holder == null || !holder.IsColonistPlayerControlled)
            {
                yield break;
            }

            if (WitcherDefOf.Witcher_TrialOfMountains == null || !WitcherDefOf.Witcher_TrialOfMountains.IsFinished)
            {
                yield break;
            }

            if (WitcherXenotypes.HasActiveMountainsTrial(holder))
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "Take witcher contract",
                defaultDesc = "Accept a contract to hunt a beast sighted on a nearby tile. Travel there and slay it to complete the Trial of Mountains.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Genes/Gene_StrongMeleeDamage", true),
                action = BeginContract
            };
        }

        private void BeginContract()
        {
            Pawn holder = pawn;
            if (holder == null || holder.Map == null)
            {
                return;
            }

            if (WitcherDefOf.Witcher_TrialOfMountainsQuest == null)
            {
                Log.Error("[Witcher] Trial of Mountains quest def is missing.");
                return;
            }

            Slate slate = new Slate();
            slate.Set("trialPawn", holder);
            slate.Set("map", holder.Map);
            slate.Set("points", 400f);

            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(WitcherDefOf.Witcher_TrialOfMountainsQuest, slate);
            if (quest == null)
            {
                return;
            }

            Find.LetterStack.ReceiveLetter(
                "Witcher contract accepted",
                $"{holder.LabelShort} has taken a witcher contract. A beast lair has appeared on a nearby tile. Send {holder.LabelShort} there to hunt it down.",
                LetterDefOf.PositiveEvent,
                LookTargets.Invalid,
                null,
                quest);
        }
    }
}
