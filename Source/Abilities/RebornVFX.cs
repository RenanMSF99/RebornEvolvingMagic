using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RebornEvolvingMagic
{
    public static class RebornVFXDefOf
    {
        public static FleckDef Fleck_RuneCircleFloor { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_RuneCircleFloor", false); } }
        public static FleckDef Fleck_RuneCircleFast { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_RuneCircleFast", false); } }
        public static FleckDef Fleck_SwirlIn { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_SwirlIn", false); } }
        public static FleckDef Fleck_SwirlOut { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_SwirlOut", false); } }
        public static FleckDef Fleck_Shockwave { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_Shockwave", false); } }
        public static FleckDef Fleck_GlowOrb { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_GlowOrb", false); } }
        public static FleckDef Fleck_SoftSpark { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_SoftSpark", false); } }
        public static FleckDef Fleck_EnergyStreak { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_EnergyStreak", false); } }
        public static FleckDef Fleck_IceCrystal { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_IceCrystal", false); } }
        public static FleckDef Fleck_FlameWisp { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_FlameWisp", false); } }
        public static FleckDef Fleck_StoneShard { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_StoneShard", false); } }
        public static FleckDef Fleck_HealingPetal { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_HealingPetal", false); } }
        public static FleckDef Fleck_PowerBeam { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_PowerBeam", false); } }
        public static FleckDef Fleck_OrbitOrb { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_OrbitOrb", false); } }
        public static FleckDef Fleck_Afterimage { get { return DefDatabase<FleckDef>.GetNamed("REM_Fleck_Afterimage", false); } }

        public static ThingDef Thing_LightningArc { get { return DefDatabase<ThingDef>.GetNamed("REM_Thing_LightningArc", false); } }
        public static ThingDef Flyer_WindDash { get { return DefDatabase<ThingDef>.GetNamed("REM_Flyer_WindDash", false); } }
        public static ThingDef Flyer_SkyStep { get { return DefDatabase<ThingDef>.GetNamed("REM_Flyer_SkyStep", false); } }
    }

    public static class RebornVFX
    {
        public static readonly Color ColorSoul = new Color(0.28f, 0.86f, 1.0f);
        public static readonly Color ColorFire = new Color(1.0f, 0.48f, 0.12f);
        public static readonly Color ColorFrost = new Color(0.55f, 0.90f, 1.0f);
        public static readonly Color ColorWind = new Color(0.38f, 0.96f, 0.65f);
        public static readonly Color ColorHealing = new Color(0.32f, 0.95f, 0.50f);
        public static readonly Color ColorEarth = new Color(0.82f, 0.72f, 0.45f);
        public static readonly Color ColorLightning = new Color(0.45f, 0.82f, 1.0f);
        public static readonly Color ColorSkyGold = new Color(1.0f, 0.85f, 0.28f);
        public static readonly Color ColorPortal = new Color(0.42f, 0.72f, 1.0f);

        public static void ThrowFleck(
            FleckDef def,
            Vector3 pos,
            Map map,
            float scale,
            Color color,
            float rotation = 0f,
            float rotationRate = 0f,
            float velocityAngle = 0f,
            float velocitySpeed = 0f,
            float? solidTimeOverride = null,
            Vector3? exactScale = null)
        {
            if (def == null || map == null || map.flecks == null || !pos.ShouldSpawnMotesAt(map))
                return;

            FleckCreationData data = FleckMaker.GetDataStatic(pos, map, def, scale);
            data.instanceColor = color;
            data.rotation = rotation;
            data.rotationRate = rotationRate;
            data.velocityAngle = velocityAngle;
            data.velocitySpeed = velocitySpeed;
            if (solidTimeOverride.HasValue)
                data.solidTimeOverride = solidTimeOverride;
            if (exactScale.HasValue)
                data.exactScale = exactScale;
            map.flecks.CreateFleck(data);
        }

        public static void ThrowOrbitFleck(Pawn pawn, FleckDef def, float scale, Color color, float radius = 0.65f, float orbitSpeed = 2.8f, float rotRate = 180f)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null || def == null)
                return;

            Vector3 pos = pawn.DrawPos;
            if (!pos.ShouldSpawnMotesAt(pawn.Map))
                return;

            Vector3 offset = Vector3.forward.RotatedBy(Rand.Range(0f, 360f)) * radius;
            if (def.useAttachLink)
            {
                FleckCreationData data = FleckMaker.GetDataAttachedOverlay(pawn, def, offset, scale, 0.55f);
                data.instanceColor = color;
                data.rotationRate = rotRate;
                data.orbitSpeed = orbitSpeed;
                data.orbitSnapStrength = 0.85f;
                pawn.Map.flecks.CreateFleck(data);
            }
            else
            {
                ThrowFleck(def, pos + offset, pawn.Map, scale, color, Rand.Range(0f, 360f), rotRate, Rand.Range(0f, 360f), 0.35f);
            }
        }

        public static void SpawnLightningArc(Map map, Vector3 start, Vector3 end, Color color, float width = 0.60f, int durationTicks = 18)
        {
            if (map == null || RebornVFXDefOf.Thing_LightningArc == null)
                return;

            IntVec3 cell = start.ToIntVec3();
            if (!cell.InBounds(map))
            {
                cell = end.ToIntVec3();
                if (!cell.InBounds(map))
                    return;
            }

            RebornLightningArc arc = ThingMaker.MakeThing(RebornVFXDefOf.Thing_LightningArc) as RebornLightningArc;
            if (arc != null)
            {
                arc.Setup(start, end, color, width, durationTicks);
                GenSpawn.Spawn(arc, cell, map, WipeMode.Vanish);
            }
        }

        public static void PlayCastRuneCircle(Vector3 pos, Map map, Color color, float scale = 2.0f, float rotSpeed = 110f)
        {
            if (map == null) return;
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, pos, map, scale, color, Rand.Range(0f, 360f), rotSpeed);
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, pos, map, scale * 0.72f, Color.Lerp(color, Color.white, 0.45f), Rand.Range(0f, 360f), -rotSpeed * 1.35f);
            ThrowFleck(RebornVFXDefOf.Fleck_SwirlIn, pos, map, scale * 0.95f, color, Rand.Range(0f, 360f), 260f);
        }

        public static void PlayImpactBurst(Vector3 pos, Map map, Color primaryColor, Color secondaryColor, float scale = 1.6f, FleckDef shardFleck = null, int shardCount = 6, bool shockwave = true, SoundDef sound = null)
        {
            if (map == null) return;

            ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pos, map, scale * 1.15f, Color.Lerp(primaryColor, Color.white, 0.55f), Rand.Range(0f, 360f), 0f);
            ThrowFleck(RebornVFXDefOf.Fleck_SwirlOut, pos, map, scale * 0.8f, primaryColor, Rand.Range(0f, 360f), 320f);
            if (shockwave)
            {
                ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, scale * 0.65f, secondaryColor, Rand.Range(0f, 360f), 140f);
            }

            FleckDef usedShard = shardFleck != null ? shardFleck : RebornVFXDefOf.Fleck_SoftSpark;
            for (int i = 0; i < shardCount; i++)
            {
                float ang = Rand.Range(0f, 360f);
                float spd = Rand.Range(1.8f, 4.4f) * Mathf.Max(0.75f, scale * 0.65f);
                Color c = (i % 2 == 0) ? primaryColor : secondaryColor;
                ThrowFleck(usedShard, pos + Vector3.forward.RotatedBy(ang) * 0.15f, map, Rand.Range(0.45f, 0.85f) * Mathf.Max(0.8f, scale * 0.55f), c, ang, Rand.Range(-320f, 320f), ang, spd);
                ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, pos, map, Rand.Range(0.5f, 0.9f), Color.Lerp(c, Color.white, 0.35f), ang, 0f, ang, spd * 1.25f);
            }

            if (sound != null)
            {
                sound.PlayOneShot(new TargetInfo(pos.ToIntVec3(), map));
            }
        }

        public static void PlaySound(SoundDef sound, IntVec3 cell, Map map)
        {
            if (sound != null && map != null && cell.InBounds(map))
            {
                sound.PlayOneShot(new TargetInfo(cell, map));
            }
        }

        public static void PlayCritFlash(Vector3 pos, Map map)
        {
            if (map == null || !pos.ShouldSpawnMotesAt(map)) return;
            ThrowFleck(RebornVFXDefOf.Fleck_SoftSpark, pos, map, 1.35f, ColorSkyGold, Rand.Range(0f, 360f), 260f);
            ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 0.72f, ColorSkyGold, 0f, 180f);
        }

        public static void PlayDodgeWhoosh(Vector3 pos, Map map)
        {
            if (map == null || !pos.ShouldSpawnMotesAt(map)) return;
            ThrowFleck(RebornVFXDefOf.Fleck_SwirlOut, pos, map, 1.35f, ColorWind, Rand.Range(0f, 360f), 320f);
            for (int i = 0; i < 4; i++)
            {
                float ang = i * 90f + Rand.Range(-25f, 25f);
                ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, pos, map, 0.75f, ColorWind, ang, 0f, ang, 3.0f);
            }
        }

        public static void PlayPortalWarp(Vector3 startPos, Vector3 endPos, Map map, Color color, float scale = 2.4f)
        {
            if (map == null) return;

            // Portal de partida (implosão espacial + runa no chão)
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, startPos, map, scale, color, Rand.Range(0f, 360f), 180f);
            ThrowFleck(RebornVFXDefOf.Fleck_SwirlIn, startPos, map, scale * 1.1f, Color.Lerp(color, Color.white, 0.25f), Rand.Range(0f, 360f), 380f);
            ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, startPos, map, scale * 0.85f, Color.white, 0f, 0f);
            FleckMaker.Static(startPos, map, FleckDefOf.PsycastSkipInnerExit, scale * 0.8f);

            // Portal de chegada (expansão espacial + anel de choque + partículas)
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, endPos, map, scale * 1.15f, color, Rand.Range(0f, 360f), -200f);
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, endPos, map, scale * 0.8f, Color.Lerp(color, Color.white, 0.5f), Rand.Range(0f, 360f), 260f);
            ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, endPos, map, scale * 0.65f, color, 0f, 120f);
            ThrowFleck(RebornVFXDefOf.Fleck_SwirlOut, endPos, map, scale * 0.9f, Color.Lerp(color, Color.white, 0.3f), Rand.Range(0f, 360f), -340f);
            FleckMaker.Static(endPos, map, FleckDefOf.PsycastSkipFlashEntry, scale * 0.9f);

            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f + Rand.Range(-15f, 15f);
                ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, endPos, map, Rand.Range(0.75f, 1.1f), Color.Lerp(color, Color.white, 0.45f), ang, 0f, ang, Rand.Range(3.0f, 5.2f));
            }

            if (SoundDefOf.Psycast_Skip_Entry != null)
            {
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(endPos.ToIntVec3(), map));
            }
        }

        public static void PlayHealingBloom(Pawn target, Color color, bool subtlePulse = false)
        {
            if (target == null || !target.Spawned || target.Map == null)
                return;

            Map map = target.Map;
            Vector3 pos = target.DrawPos;
            if (!pos.ShouldSpawnMotesAt(map))
                return;

            if (!subtlePulse)
            {
                PlayCastRuneCircle(pos, map, color, 2.0f, 105f);
                ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 0.7f, color, 0f, 90f);
                ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pos, map, 1.6f, Color.Lerp(color, Color.white, 0.5f));
                int petalCount = 7;
                for (int i = 0; i < petalCount; i++)
                {
                    float ang = Rand.Range(-40f, 40f);
                    Vector3 spawnPos = pos + new Vector3(Rand.Range(-0.55f, 0.55f), 0f, Rand.Range(-0.4f, 0.4f));
                    ThrowFleck(RebornVFXDefOf.Fleck_HealingPetal, spawnPos, map, Rand.Range(0.55f, 0.9f), color, Rand.Range(0f, 360f), Rand.Range(-160f, 160f), ang, Rand.Range(1.2f, 2.3f));
                    ThrowFleck(RebornVFXDefOf.Fleck_SoftSpark, spawnPos, map, Rand.Range(0.4f, 0.7f), Color.white, 0f, 180f, ang, Rand.Range(1.5f, 2.8f));
                }
                if (SoundDefOf.EnergyShield_Reset != null)
                {
                    SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(target.Position, map));
                }
            }
            else
            {
                Vector3 spawnPos = pos + new Vector3(Rand.Range(-0.45f, 0.45f), 0f, Rand.Range(-0.25f, 0.35f));
                ThrowFleck(RebornVFXDefOf.Fleck_HealingPetal, spawnPos, map, Rand.Range(0.45f, 0.70f), color, Rand.Range(0f, 360f), 110f, Rand.Range(-18f, 18f), Rand.Range(0.9f, 1.5f));
            }
        }

        public static void PlayStoneSkinCast(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null)
                return;

            Map map = pawn.Map;
            Vector3 pos = pawn.DrawPos;
            PlayCastRuneCircle(pos, map, ColorEarth, 2.2f, 80f);
            ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 0.8f, ColorEarth, 0f, 70f);
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f + Rand.Range(-10f, 10f);
                Vector3 offset = Vector3.forward.RotatedBy(ang) * 0.45f;
                ThrowFleck(RebornVFXDefOf.Fleck_StoneShard, pos + offset, map, Rand.Range(0.65f, 1.0f), ColorEarth, ang, Rand.Range(-220f, 220f), ang, Rand.Range(1.5f, 2.8f));
            }
            if (SoundDefOf.BulletImpact_Ground != null)
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(pawn.Position, map));
            }
        }

        public static void PlayAscensionPillar(Pawn pawn, Color color, string titleText = null, float beamWidth = 2.6f)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null)
                return;

            Map map = pawn.Map;
            Vector3 pos = pawn.DrawPos;
            Color brightColor = Color.Lerp(color, Color.white, 0.35f);

            // Pilar de Luz Celestial descendo sobre o cultivador
            Vector3 beamPos = pos + new Vector3(0f, 0f, 3.6f);
            ThrowFleck(RebornVFXDefOf.Fleck_PowerBeam, beamPos, map, 1f, brightColor, 0f, 0f, 0f, 0f, 0.55f, new Vector3(beamWidth, 1f, 9.0f));
            ThrowFleck(RebornVFXDefOf.Fleck_PowerBeam, beamPos, map, 1f, Color.white, 0f, 0f, 0f, 0f, 0.35f, new Vector3(beamWidth * 0.45f, 1f, 9.0f));

            // Grande arranjo rúnico duplo + onda de choque
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, pos, map, 3.8f, color, Rand.Range(0f, 360f), 115f, 0f, 0f, 0.75f);
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, pos, map, 2.7f, ColorSkyGold, Rand.Range(0f, 360f), -165f, 0f, 0f, 0.75f);
            ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 1.2f, brightColor, 0f, 90f);
            ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 0.85f, ColorSkyGold, 0f, -120f);
            ThrowFleck(RebornVFXDefOf.Fleck_SwirlOut, pos, map, 2.2f, color, Rand.Range(0f, 360f), 280f);
            ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pos, map, 3.2f, Color.white);

            for (int i = 0; i < 12; i++)
            {
                float ang = i * 30f + Rand.Range(-10f, 10f);
                float spd = Rand.Range(3.0f, 6.0f);
                ThrowFleck(RebornVFXDefOf.Fleck_SoftSpark, pos, map, Rand.Range(0.7f, 1.15f), (i % 2 == 0) ? brightColor : ColorSkyGold, ang, Rand.Range(-240f, 240f), ang, spd);
                ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, pos, map, Rand.Range(0.9f, 1.4f), Color.white, ang, 0f, ang, spd * 1.15f);
            }

            if (SoundDefOf.Psycast_Skip_Entry != null)
            {
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(pawn.Position, map));
            }
            if (SoundDefOf.Thunder_OnMap != null)
            {
                SoundDefOf.Thunder_OnMap.PlayOneShot(new TargetInfo(pawn.Position, map));
            }

            if (!string.IsNullOrEmpty(titleText))
            {
                MoteMaker.ThrowText(pos + new Vector3(0f, 0f, 0.5f), map, titleText, color, 4.0f);
            }
        }

        public static void PlaySoulSiphon(Vector3 corpsePos, Pawn caster, Color soulColor)
        {
            if (caster == null || !caster.Spawned || caster.Map == null)
                return;

            Map map = caster.Map;
            Vector3 casterPos = caster.DrawPos;
            Vector3 delta = casterPos - corpsePos;
            float dist = delta.MagnitudeHorizontal();
            float angleToCaster = dist > 0.05f ? delta.AngleFlat() : 0f;

            // Vórtice de extração no cadáver
            ThrowFleck(RebornVFXDefOf.Fleck_SwirlIn, corpsePos, map, 2.1f, soulColor, Rand.Range(0f, 360f), 320f);
            ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, corpsePos, map, 0.8f, Color.Lerp(soulColor, Color.white, 0.4f));

            // Feixe espiritual drenando até o cultivador
            if (dist > 0.4f)
            {
                SpawnLightningArc(map, corpsePos, casterPos, soulColor, 0.42f, 20);
                int particles = Mathf.Clamp(Mathf.CeilToInt(dist * 0.9f), 4, 14);
                for (int i = 0; i < particles; i++)
                {
                    float frac = (float)i / (float)particles;
                    Vector3 p = Vector3.Lerp(corpsePos, casterPos, frac * 0.7f) + new Vector3(Rand.Range(-0.25f, 0.25f), 0f, Rand.Range(-0.25f, 0.25f));
                    ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, p, map, Rand.Range(0.65f, 1.0f), Color.Lerp(soulColor, Color.white, 0.4f), angleToCaster, 0f, angleToCaster, Mathf.Max(4.0f, dist * 2.2f));
                    ThrowFleck(RebornVFXDefOf.Fleck_SoftSpark, p, map, Rand.Range(0.45f, 0.75f), soulColor, Rand.Range(0f, 360f), 200f, angleToCaster, Mathf.Max(3.5f, dist * 1.9f));
                }
            }

            // Runa de convergência e explosão no conjurador
            ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, casterPos, map, 1.9f, soulColor, 0f, -150f);
            ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, casterPos, map, 1.5f, Color.Lerp(soulColor, Color.white, 0.5f));
            if (SoundDefOf.Psycast_Skip_Entry != null)
            {
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(caster.Position, map));
            }
        }

        public static void PlayProjectileTrail(Vector3 curPos, Vector3 prevPos, Map map, Color primaryColor, FleckDef accentFleck, float scale = 0.75f)
        {
            if (map == null || !curPos.ShouldSpawnMotesAt(map))
                return;

            Vector3 dir = curPos - prevPos;
            float moveAngle = dir.sqrMagnitude > 0.0001f ? dir.AngleFlat() : 0f;
            float backAngle = moveAngle + 180f + Rand.Range(-18f, 18f);

            ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, curPos, map, scale * 0.9f, Color.Lerp(primaryColor, Color.white, 0.35f), Rand.Range(0f, 360f), 0f, backAngle, Rand.Range(0.4f, 1.0f));
            ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, curPos, map, scale * 0.8f, primaryColor, moveAngle, 0f, backAngle, Rand.Range(0.9f, 1.8f));

            if (accentFleck != null && Rand.Chance(0.65f))
            {
                Vector3 jitter = new Vector3(Rand.Range(-0.14f, 0.14f), 0f, Rand.Range(-0.14f, 0.14f));
                ThrowFleck(accentFleck, curPos + jitter, map, scale * Rand.Range(0.55f, 0.9f), primaryColor, Rand.Range(0f, 360f), Rand.Range(-180f, 180f), backAngle + Rand.Range(-25f, 25f), Rand.Range(0.6f, 1.5f));
            }
        }
    }

    public class RebornLightningArc : Thing
    {
        private Vector3 startPos;
        private Vector3 endPos;
        private Color arcColor = new Color(0.45f, 0.85f, 1f);
        private float width = 0.6f;
        private int durationTicks = 18;
        private int ageTicks = 0;

        private Mesh outerMesh;
        private Mesh coreMesh;
        private MaterialPropertyBlock outerProps;
        private MaterialPropertyBlock coreProps;

        private static Material cachedBeamMat;

        private static Material BeamMaterial
        {
            get
            {
                if (cachedBeamMat == null && UnityData.IsInMainThread)
                {
                    cachedBeamMat = MaterialPool.MatFrom("Motes/RebornMagic/LightningBeam", ShaderDatabase.MoteGlow);
                }
                return cachedBeamMat;
            }
        }

        public void Setup(Vector3 start, Vector3 end, Color color, float arcWidth, int duration)
        {
            startPos = start;
            endPos = end;
            arcColor = color;
            width = Mathf.Max(0.18f, arcWidth);
            durationTicks = Mathf.Max(6, duration);
            ageTicks = 0;
            RebuildMeshes();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref startPos, "startPos");
            Scribe_Values.Look(ref endPos, "endPos");
            Scribe_Values.Look(ref arcColor, "arcColor");
            Scribe_Values.Look(ref width, "width", 0.6f);
            Scribe_Values.Look(ref durationTicks, "durationTicks", 18);
            Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
        }

        protected override void Tick()
        {
            base.Tick();
            ageTicks++;
            if (ageTicks >= durationTicks)
            {
                Destroy(DestroyMode.Vanish);
                return;
            }

            if (ageTicks % 3 == 0 && UnityData.IsInMainThread)
            {
                RebuildMeshes();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            CleanupMeshes();
            base.DeSpawn(mode);
        }

        private void CleanupMeshes()
        {
            if (outerMesh != null)
            {
                UnityEngine.Object.Destroy(outerMesh);
                outerMesh = null;
            }
            if (coreMesh != null)
            {
                UnityEngine.Object.Destroy(coreMesh);
                coreMesh = null;
            }
        }

        private void RebuildMeshes()
        {
            CleanupMeshes();

            Vector3 delta = endPos - startPos;
            delta.y = 0f;
            float dist = delta.magnitude;
            if (dist < 0.05f)
                return;

            Vector3 forward = delta / dist;
            Vector3 right = new Vector3(-forward.z, 0f, forward.x);
            float yAlt = AltitudeLayer.MoteOverhead.AltitudeFor();

            int segments = Mathf.Clamp(Mathf.CeilToInt(dist * 2.1f), 5, 18);
            List<Vector3> pts = new List<Vector3>(segments + 1);
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / (float)segments;
                Vector3 p = Vector3.Lerp(startPos, endPos, t);
                p.y = yAlt;
                if (i > 0 && i < segments)
                {
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    float jitterAmt = Mathf.Min(0.85f, dist * 0.14f) * envelope;
                    p += right * Rand.Range(-jitterAmt, jitterAmt);
                }
                pts.Add(p);
            }

            outerMesh = BuildRibbonMesh(pts, right, width, yAlt, true);
            coreMesh = BuildRibbonMesh(pts, right, width * 0.36f, yAlt + 0.01f, false);
        }

        private Mesh BuildRibbonMesh(List<Vector3> pts, Vector3 perpFallback, float ribbonWidth, float yAlt, bool withForks)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            AddPolylineStrip(pts, perpFallback, ribbonWidth, yAlt, verts, uvs, tris);

            if (withForks && pts.Count >= 4)
            {
                int forkCount = Rand.RangeInclusive(1, 2);
                for (int f = 0; f < forkCount; f++)
                {
                    int idx = Rand.RangeInclusive(1, pts.Count - 2);
                    Vector3 origin = pts[idx];
                    Vector3 baseDir = (endPos - startPos);
                    baseDir.y = 0f;
                    if (baseDir.sqrMagnitude > 0.001f) baseDir.Normalize();
                    Vector3 branchDir = baseDir.RotatedBy(Rand.Range(-55f, 55f));
                    float branchLen = Mathf.Clamp((endPos - startPos).MagnitudeHorizontal() * 0.26f, 0.7f, 2.2f);
                    List<Vector3> forkPts = new List<Vector3>(4);
                    forkPts.Add(origin);
                    Vector3 perp = new Vector3(-branchDir.z, 0f, branchDir.x);
                    forkPts.Add(origin + branchDir * (branchLen * 0.45f) + perp * Rand.Range(-0.25f, 0.25f));
                    forkPts.Add(origin + branchDir * branchLen + perp * Rand.Range(-0.20f, 0.20f));
                    AddPolylineStrip(forkPts, perp, ribbonWidth * 0.55f, yAlt, verts, uvs, tris);
                }
            }

            Mesh m = new Mesh();
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateBounds();
            return m;
        }

        private static void AddPolylineStrip(List<Vector3> pts, Vector3 perpFallback, float ribbonWidth, float yAlt, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
        {
            float halfW = ribbonWidth * 0.5f;
            int baseVert = verts.Count;

            for (int i = 0; i < pts.Count; i++)
            {
                Vector3 tangent;
                if (i == 0) tangent = pts[1] - pts[0];
                else if (i == pts.Count - 1) tangent = pts[i] - pts[i - 1];
                else tangent = pts[i + 1] - pts[i - 1];
                tangent.y = 0f;
                Vector3 right = tangent.sqrMagnitude > 0.0001f
                    ? new Vector3(-tangent.z, 0f, tangent.x).normalized
                    : perpFallback;

                Vector3 center = pts[i];
                center.y = yAlt;
                verts.Add(center - right * halfW);
                verts.Add(center + right * halfW);

                float v = (float)i / (float)Mathf.Max(1, pts.Count - 1);
                uvs.Add(new Vector2(0f, v));
                uvs.Add(new Vector2(1f, v));
            }

            for (int i = 0; i < pts.Count - 1; i++)
            {
                int i0 = baseVert + i * 2;
                int i1 = i0 + 1;
                int i2 = i0 + 2;
                int i3 = i0 + 3;
                tris.Add(i0);
                tris.Add(i2);
                tris.Add(i1);
                tris.Add(i1);
                tris.Add(i2);
                tris.Add(i3);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Material mat = BeamMaterial;
            if (mat == null || outerMesh == null || coreMesh == null)
                return;

            float life = 1f - Mathf.Clamp01((float)ageTicks / (float)Mathf.Max(1, durationTicks));
            float alpha = Mathf.Pow(life, 1.2f);

            if (outerProps == null) outerProps = new MaterialPropertyBlock();
            if (coreProps == null) coreProps = new MaterialPropertyBlock();

            Color cOuter = new Color(arcColor.r, arcColor.g, arcColor.b, alpha * 0.9f);
            Color cCore = new Color(Mathf.Lerp(arcColor.r, 1f, 0.78f), Mathf.Lerp(arcColor.g, 1f, 0.78f), Mathf.Lerp(arcColor.b, 1f, 0.78f), alpha);

            outerProps.SetColor(ShaderPropertyIDs.Color, cOuter);
            coreProps.SetColor(ShaderPropertyIDs.Color, cCore);

            Graphics.DrawMesh(outerMesh, Matrix4x4.identity, mat, 0, null, 0, outerProps);
            Graphics.DrawMesh(coreMesh, Matrix4x4.identity, mat, 0, null, 0, coreProps);
        }
    }

    public class RebornDashFlyer : PawnFlyer
    {
        public bool reselectOnLand;
        public Color trailColor = new Color(0.38f, 0.96f, 0.65f);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref reselectOnLand, "reselectOnLand", false);
            Scribe_Values.Look(ref trailColor, "trailColor", RebornVFX.ColorWind);
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (!Destroyed && Map != null)
            {
                Vector3 pos = DrawPos;
                if (pos.ShouldSpawnMotesAt(Map))
                {
                    Vector3 dir = DestinationPos - startVec;
                    float ang = dir.sqrMagnitude > 0.001f ? dir.AngleFlat() : 0f;
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Afterimage, pos, Map, 1.05f, new Color(trailColor.r, trailColor.g, trailColor.b, 0.65f));
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, pos + new Vector3(Rand.Range(-0.25f, 0.25f), 0f, Rand.Range(-0.25f, 0.25f)), Map, Rand.Range(0.75f, 1.15f), Color.Lerp(trailColor, Color.white, 0.45f), ang + 180f, 0f, ang + 180f, 3.2f);
                    if (Rand.Chance(0.5f))
                    {
                        FleckMaker.ThrowDustPuff(pos, Map, 0.9f);
                    }
                }
            }
        }

        protected override void RespawnPawn()
        {
            Pawn landedPawn = FlyingPawn;
            Map map = Map;
            base.RespawnPawn();

            if (landedPawn != null && landedPawn.Spawned && map != null)
            {
                OnLanded(landedPawn, map);
                if (reselectOnLand && Find.Selector != null && !Find.Selector.IsSelected(landedPawn))
                {
                    Find.Selector.Select(landedPawn, false, false);
                }
            }
        }

        protected virtual void OnLanded(Pawn landedPawn, Map map)
        {
            Vector3 pos = landedPawn.DrawPos;
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 0.7f, trailColor);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, pos, map, 1.6f, trailColor, 0f, 180f);
            FleckMaker.ThrowDustPuff(pos, map, 1.4f);
            MoteMaker.ThrowText(pos, map, "PASSO DO VENTO!", trailColor, 2.0f);

            if (SoundDefOf.BulletImpact_Ground != null)
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(landedPawn.Position, map));
            }
        }
    }

    public class RebornSkyStepFlyer : RebornDashFlyer
    {
        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (!Destroyed && Map != null)
            {
                Vector3 pos = DrawPos;
                if (pos.ShouldSpawnMotesAt(Map))
                {
                    Color skyGlow = Color.Lerp(RebornVFX.ColorSoul, RebornVFX.ColorSkyGold, 0.35f);
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_SoftSpark, pos + new Vector3(Rand.Range(-0.35f, 0.35f), 0f, Rand.Range(-0.25f, 0.25f)), Map, Rand.Range(0.55f, 0.95f), skyGlow, Rand.Range(0f, 360f), 220f, Rand.Range(0f, 360f), 1.4f);
                }
            }
        }

        protected override void OnLanded(Pawn landedPawn, Map map)
        {
            Vector3 pos = landedPawn.DrawPos;
            RebornVFX.PlayCastRuneCircle(pos, map, RebornVFX.ColorSoul, 2.3f, 150f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 1.0f, RebornVFX.ColorSkyGold, 0f, 120f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_SwirlOut, pos, map, 1.9f, RebornVFX.ColorSoul, Rand.Range(0f, 360f), -260f);
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f;
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_EnergyStreak, pos, map, 0.95f, RebornVFX.ColorSkyGold, ang, 0f, ang, 4.2f);
            }
            MoteMaker.ThrowText(pos, map, "PASSO CELESTIAL!", new Color(0.25f, 0.85f, 1.0f), 2.5f);
            if (SoundDefOf.Psycast_Skip_Entry != null)
            {
                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(landedPawn.Position, map));
            }
        }
    }
}
