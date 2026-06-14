using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WitcherBase
{
    public class CompAbilityEffect_Knockback : CompAbilityEffect
    {
        public new CompProperties_AbilityKnockback Props => (CompProperties_AbilityKnockback)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent.pawn;
            Map map = caster?.Map;
            if (map == null)
            {
                return;
            }

            // Prefer the explicit radius from the comp; fall back to the AbilityDef's
            // effect radius. (Reading the stat off the def directly returned 0, which
            // gave the AoE Aard an empty target scan -> nothing was ever thrown.)
            float radius = Props.radius > 0f ? Props.radius : parent.def.EffectRadius;

            List<Pawn> victims = new List<Pawn>();
            if (Props.cone)
            {
                CollectConeVictims(caster, target, map, radius, victims);
            }
            else if (!Props.useEffectRadius && target.Pawn != null)
            {
                victims.Add(target.Pawn);
            }
            else
            {
                IntVec3 center = target.IsValid ? target.Cell : caster.Position;
                foreach (Thing thing in GenRadial.RadialDistinctThingsAround(center, map, radius, true))
                {
                    if (thing is Pawn pawn && pawn != caster && pawn.Spawned && !pawn.Dead)
                    {
                        if (!Props.targetHostilesOnly || pawn.HostileTo(caster))
                        {
                            victims.Add(pawn);
                        }
                    }
                }
            }

            foreach (Pawn victim in victims)
            {
                ApplyKnockback(caster, victim, map);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            if (!Props.cone)
            {
                return;
            }

            Pawn caster = parent.pawn;
            Map map = caster?.Map;
            if (map == null)
            {
                return;
            }

            float radius = Props.radius > 0f ? Props.radius : parent.def.EffectRadius;
            List<IntVec3> coneCells = GetConeCells(caster, target, map, radius);
            if (coneCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(coneCells);
            }
            else
            {
                GenDraw.DrawRadiusRing(caster.Position, radius);
            }
        }

        private void CollectConeVictims(Pawn caster, LocalTargetInfo target, Map map, float radius, List<Pawn> victims)
        {
            ConeQuery cone = ConeQuery.Create(caster, target, Props.coneAngle);
            if (!cone.Valid)
            {
                return;
            }

            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(caster.Position, map, radius, false))
            {
                if (!(thing is Pawn pawn) || pawn == caster || !pawn.Spawned || pawn.Dead)
                {
                    continue;
                }

                if (Props.targetHostilesOnly && !pawn.HostileTo(caster))
                {
                    continue;
                }

                if (cone.ContainsCell(pawn.Position))
                {
                    victims.Add(pawn);
                }
            }
        }

        private List<IntVec3> GetConeCells(Pawn caster, LocalTargetInfo target, Map map, float radius)
        {
            List<IntVec3> cells = new List<IntVec3>();
            ConeQuery cone = ConeQuery.Create(caster, target, Props.coneAngle);
            if (!cone.Valid)
            {
                return cells;
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(caster.Position, radius, true))
            {
                if (!InMapBounds(map, cell))
                {
                    continue;
                }

                if (cone.ContainsCell(cell))
                {
                    cells.Add(cell);
                }
            }

            return cells;
        }

        private static bool InMapBounds(Map map, IntVec3 cell)
        {
            return cell.x >= 0 && cell.z >= 0 && cell.x < map.Size.x && cell.z < map.Size.z;
        }

        private void ApplyKnockback(Pawn caster, Pawn victim, Map map)
        {
            if (victim == null || victim.Dead || !victim.Spawned)
            {
                return;
            }

            DamageInfo damageInfo = new DamageInfo(
                DamageDefOf.Blunt,
                Props.damageAmount,
                Props.armorPenetration,
                -1f,
                caster);
            victim.TakeDamage(damageInfo);

            // Damage can kill/despawn the pawn; pushing or stunning a dead/despawned
            // pawn NREs inside Notify_Teleported, so bail out if it's no longer valid.
            if (victim.Dead || !victim.Spawned)
            {
                return;
            }

            TryPushPawn(caster, victim, map, Props.pushDistance);

            if (victim.Dead || !victim.Spawned)
            {
                return;
            }

            if (Props.alwaysStun || (Props.stunChance > 0f && Rand.Chance(Props.stunChance)))
            {
                if (victim.stances != null && victim.stances.stunner != null)
                {
                    victim.stances.stunner.StunFor(Props.stunTicks, caster);
                }
            }
        }

        private static void TryPushPawn(Pawn caster, Pawn victim, Map map, int pushDistance)
        {
            int deltaX = victim.Position.x - caster.Position.x;
            int deltaZ = victim.Position.z - caster.Position.z;
            int dx = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
            int dz = deltaZ == 0 ? 0 : (deltaZ > 0 ? 1 : -1);
            if (dx == 0 && dz == 0)
            {
                dx = caster.Rotation.FacingCell.x;
                dz = caster.Rotation.FacingCell.z;
            }

            int destX = victim.Position.x;
            int destZ = victim.Position.z;
            for (int step = 0; step < pushDistance; step++)
            {
                int nextX = destX + dx;
                int nextZ = destZ + dz;
                if (!IsStandableCell(map, nextX, nextZ))
                {
                    break;
                }

                destX = nextX;
                destZ = nextZ;
            }

            if (destX != victim.Position.x || destZ != victim.Position.z)
            {
                // Stop any in-progress movement first; otherwise a charging pawn's
                // pather/job simply walks it back to where it was, masking the push.
                if (victim.pather != null)
                {
                    victim.pather.StopDead();
                }

                victim.Position = new IntVec3(destX, 0, destZ);

                // Reset job + tweened draw position so the teleport actually sticks.
                victim.Notify_Teleported(true, true);
            }
        }

        private static bool IsStandableCell(Map map, int x, int z)
        {
            if (x < 0 || z < 0 || x >= map.Size.x || z >= map.Size.z)
            {
                return false;
            }

            // Use Walkable (terrain/buildings only) rather than Standable so that a
            // cluster of knocked-back pawns don't block each other's destination cells.
            IntVec3 cell = new IntVec3(x, 0, z);
            return GenGrid.Walkable(cell, map);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (Props.cone || Props.useEffectRadius)
            {
                return true;
            }

            return target.Pawn != null && !target.Pawn.IsForbidden(parent.pawn);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null && target.Pawn.HostileTo(parent.pawn);
        }

        private struct ConeQuery
        {
            private IntVec3 origin;
            private double dirX;
            private double dirZ;
            private double dirLen;
            private double cosThreshold;

            public bool Valid { get; private set; }

            public static ConeQuery Create(Pawn caster, LocalTargetInfo target, float coneAngle)
            {
                ConeQuery query = new ConeQuery();
                query.origin = caster.Position;

                query.dirX = target.Cell.x - query.origin.x;
                query.dirZ = target.Cell.z - query.origin.z;
                if (query.dirX == 0d && query.dirZ == 0d)
                {
                    query.dirX = caster.Rotation.FacingCell.x;
                    query.dirZ = caster.Rotation.FacingCell.z;
                }

                query.dirLen = System.Math.Sqrt((query.dirX * query.dirX) + (query.dirZ * query.dirZ));
                if (query.dirLen <= 0d)
                {
                    return query;
                }

                double halfAngleRad = (coneAngle * 0.5d) * System.Math.PI / 180d;
                query.cosThreshold = System.Math.Cos(halfAngleRad);
                query.Valid = true;
                return query;
            }

            public bool ContainsCell(IntVec3 cell)
            {
                if (!Valid)
                {
                    return false;
                }

                double vX = cell.x - origin.x;
                double vZ = cell.z - origin.z;
                double vLen = System.Math.Sqrt((vX * vX) + (vZ * vZ));
                if (vLen <= 0d)
                {
                    return true;
                }

                double cosAngle = ((dirX * vX) + (dirZ * vZ)) / (dirLen * vLen);
                return cosAngle >= cosThreshold;
            }
        }
    }
}
