using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    [StaticConstructorOnStartup]
    public static class RebornVFX_Patches
    {
        private static Material cachedShieldMat;
        private static Material cachedAuraRuneMat;
        private static MaterialPropertyBlock sharedPropBlock;

        static RebornVFX_Patches()
        {
            Harmony harmony = new Harmony("RebornEvolvingMagic.VFX");

            MethodInfo pawnDraw = AccessTools.Method(typeof(Pawn), "DynamicDrawPhaseAt", new Type[] { typeof(DrawPhase), typeof(Vector3), typeof(bool) });
            if (pawnDraw != null)
            {
                harmony.Patch(pawnDraw, null, new HarmonyMethod(typeof(RebornVFX_Patches), "DrawActiveBuffs_Postfix"));
            }

            MethodInfo pawnTick = AccessTools.Method(typeof(Pawn), "TickInterval", new Type[] { typeof(int) });
            if (pawnTick != null)
            {
                harmony.Patch(pawnTick, null, new HarmonyMethod(typeof(RebornVFX_Patches), "PawnBuffVFXTick_Postfix"));
            }
        }

        private static Material ShieldMaterial
        {
            get
            {
                if (cachedShieldMat == null && UnityData.IsInMainThread)
                {
                    cachedShieldMat = MaterialPool.MatFrom("Motes/RebornMagic/HexShieldDome", ShaderDatabase.MoteGlow);
                }
                return cachedShieldMat;
            }
        }

        private static Material AuraRuneMaterial
        {
            get
            {
                if (cachedAuraRuneMat == null && UnityData.IsInMainThread)
                {
                    cachedAuraRuneMat = MaterialPool.MatFrom("Motes/RebornMagic/RuneCircle", ShaderDatabase.MoteGlow);
                }
                return cachedAuraRuneMat;
            }
        }

        private static void DrawDomeOverlay(Vector3 drawPos, Color lowCol, Color highCol, float hpRatio, int lastAbsorbTick, float baseScale, float rotSpeed, float baseAlpha)
        {
            Material sm = ShieldMaterial;
            if (sm == null) return;

            float t = Time.realtimeSinceStartup;
            int sinceHit = (Find.TickManager != null) ? (Find.TickManager.TicksGame - lastAbsorbTick) : 999;
            float hitFlash = (sinceHit >= 0 && sinceHit < 16) ? (1f - ((float)sinceHit / 16f)) : 0f;

            float pulse = 1.0f + 0.035f * Mathf.Sin(t * 4.2f) + hitFlash * 0.14f;
            float scale = baseScale * pulse;
            float angle = (t * rotSpeed) % 360f;

            Color baseCol = Color.Lerp(lowCol, highCol, Mathf.Clamp01(hpRatio));
            Color finalCol = Color.Lerp(baseCol, Color.white, hitFlash * 0.65f);
            finalCol.a = Mathf.Clamp01(baseAlpha + 0.16f * Mathf.Sin(t * 3.0f) + hitFlash * 0.42f);

            Vector3 domePos = drawPos;
            domePos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Matrix4x4 mat = Matrix4x4.TRS(domePos, Quaternion.Euler(0f, angle, 0f), new Vector3(scale, 1f, scale));
            sharedPropBlock.SetColor(ShaderPropertyIDs.Color, finalCol);
            Graphics.DrawMesh(MeshPool.plane10, mat, sm, 0, null, 0, sharedPropBlock);
        }

        private static void DrawRuneOverlay(Vector3 drawPos, Color col, float baseScale, float rotSpeed, AltitudeLayer altLayer)
        {
            Material rm = AuraRuneMaterial;
            if (rm == null) return;

            float t = Time.realtimeSinceStartup;
            float scale = baseScale + 0.06f * Mathf.Sin(t * 2.8f);
            float angle = (t * rotSpeed) % 360f;

            Vector3 runePos = drawPos;
            runePos.y = altLayer.AltitudeFor();
            Matrix4x4 mat = Matrix4x4.TRS(runePos, Quaternion.Euler(0f, angle, 0f), new Vector3(scale, 1f, scale));
            sharedPropBlock.SetColor(ShaderPropertyIDs.Color, col);
            Graphics.DrawMesh(MeshPool.plane10, mat, rm, 0, null, 0, sharedPropBlock);
        }

        public static void DrawActiveBuffs_Postfix(Pawn __instance, DrawPhase phase, Vector3 drawLoc)
        {
            if (phase != DrawPhase.Draw || __instance == null || !__instance.Spawned || __instance.Dead)
                return;
            if (__instance.health == null || __instance.health.hediffSet == null)
                return;

            HediffSet hs = __instance.health.hediffSet;
            if (sharedPropBlock == null) sharedPropBlock = new MaterialPropertyBlock();
            Vector3 pos = __instance.DrawPos;

            // 1. Escudo Psiônico (cúpula hexagonal translúcida giratória)
            HediffDef shieldDef = RebornDefOf.REM_Hediff_PsionicShield;
            if (shieldDef != null)
            {
                Hediff_PsionicShield shield = hs.GetFirstHediffOfDef(shieldDef) as Hediff_PsionicShield;
                if (shield != null && shield.shieldHP > 0.01f)
                {
                    float hpRatio = Mathf.Clamp01(shield.shieldHP / Mathf.Max(1f, shield.maxShieldHP));
                    DrawDomeOverlay(pos, new Color(1.0f, 0.55f, 0.25f), new Color(0.25f, 0.85f, 1.0f), hpRatio, shield.lastAbsorbTick, 1.95f, 26f, 0.42f);
                }
            }

            // 2. Escudo de Éter (cúpula ciano-azulada + anel rúnico sutil no solo)
            if (RebornDefOf.REM_Hediff_AetherShield != null)
            {
                Hediff_AetherShield aether = hs.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_AetherShield) as Hediff_AetherShield;
                if (aether != null && aether.shieldHP > 0.5f)
                {
                    float hpRatio = Mathf.Clamp01(aether.shieldHP / Mathf.Max(1f, aether.maxShieldHP));
                    DrawDomeOverlay(pos, new Color(0.25f, 0.55f, 0.85f), new Color(0.50f, 0.90f, 1.0f), hpRatio, aether.lastAbsorbTick, 2.02f, 22f, 0.40f);
                    DrawRuneOverlay(pos, new Color(0.45f, 0.88f, 1.0f, 0.34f), 2.10f, -20f, AltitudeLayer.MoteLow);
                }
            }

            // 3. Escudo Carmesim (cúpula escarlate + círculo rúnico de sangue)
            if (RebornDefOf.REM_Hediff_CrimsonShield != null)
            {
                Hediff_CrimsonShield crimson = hs.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_CrimsonShield) as Hediff_CrimsonShield;
                if (crimson != null && crimson.shieldHP > 0.01f)
                {
                    float hpRatio = Mathf.Clamp01(crimson.shieldHP / Mathf.Max(1f, crimson.maxShieldHP));
                    DrawDomeOverlay(pos, new Color(0.55f, 0.08f, 0.12f), new Color(0.95f, 0.18f, 0.28f), hpRatio, crimson.lastAbsorbTick, 1.98f, -28f, 0.44f);
                    DrawRuneOverlay(pos, new Color(0.92f, 0.15f, 0.25f, 0.45f), 2.15f, 34f, AltitudeLayer.MoteLow);
                }
            }

            // 4. Barreira de Mana (cúpula arcana + runa rápida sob os pés)
            if (RebornDefOf.REM_Hediff_ManaBarrier != null)
            {
                Hediff_ManaBarrier manaBarrier = hs.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ManaBarrier) as Hediff_ManaBarrier;
                if (manaBarrier != null)
                {
                    DrawDomeOverlay(pos, RebornVFX.ColorSoul, new Color(0.45f, 0.92f, 1.0f), 1.0f, manaBarrier.lastAbsorbTick, 1.92f, 32f, 0.36f);
                    DrawRuneOverlay(pos, new Color(0.30f, 0.85f, 1.0f, 0.48f), 2.05f, 45f, AltitudeLayer.MoteLow);
                }
            }

            // 5. Escudo da Alma (cúpula ametista-violeta + runa espiritual)
            if (RebornDefOf.REM_Hediff_SoulShield != null)
            {
                Hediff_SoulShield soulShield = hs.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulShield) as Hediff_SoulShield;
                if (soulShield != null)
                {
                    DrawDomeOverlay(pos, new Color(0.65f, 0.30f, 0.95f), new Color(0.85f, 0.55f, 1.0f), 1.0f, soulShield.lastAbsorbTick, 2.08f, -24f, 0.38f);
                    DrawRuneOverlay(pos, new Color(0.85f, 0.50f, 1.0f, 0.46f), 2.20f, 36f, AltitudeLayer.MoteLow);
                }
            }

            // 6. Barreira Mental (halo rúnico índigo + cúpula translúcida sutil)
            if (RebornDefOf.REM_Hediff_MindBarrier != null && hs.HasHediff(RebornDefOf.REM_Hediff_MindBarrier))
            {
                DrawDomeOverlay(pos, new Color(0.40f, 0.60f, 1.0f), new Color(0.55f, 0.78f, 1.0f), 1.0f, -999, 1.85f, 18f, 0.25f);
                DrawRuneOverlay(pos, new Color(0.50f, 0.72f, 1.0f, 0.50f), 1.35f, -40f, AltitudeLayer.MoteOverhead);
            }

            // 7. Muralha Infernal (anel rúnico flamejante escalado ao raio + cúpula ígnea)
            if (RebornDefOf.REM_Hediff_InfernoWall != null)
            {
                Hediff_InfernoWall inferno = hs.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_InfernoWall) as Hediff_InfernoWall;
                if (inferno != null)
                {
                    float ringScale = Mathf.Max(2.2f, inferno.ringRadius * 0.9f);
                    DrawRuneOverlay(pos, new Color(1.0f, 0.50f, 0.12f, 0.58f), ringScale, 42f, AltitudeLayer.MoteLow);
                    DrawDomeOverlay(pos, RebornVFX.ColorFire, new Color(1.0f, 0.80f, 0.25f), 1.0f, -999, 2.00f, 30f, 0.28f);
                }
            }

            // 8. Manto das Sombras (véu rúnico sombrio sob os pés)
            if (RebornDefOf.REM_Hediff_ShadowCloak != null && hs.HasHediff(RebornDefOf.REM_Hediff_ShadowCloak))
            {
                DrawRuneOverlay(pos, new Color(0.45f, 0.20f, 0.80f, 0.42f), 1.85f, -36f, AltitudeLayer.MoteLow);
            }

            // 9. Bênção dos Céus (círculo rúnico dourado sutil girando sob os pés)
            HediffDef heavensDef = DefDatabase<HediffDef>.GetNamed("REM_Hediff_HeavensBlessing", false);
            if (heavensDef != null && hs.HasHediff(heavensDef))
            {
                Color gold = new Color(1.0f, 0.85f, 0.28f, __instance.Drafted ? 0.55f : 0.28f);
                DrawRuneOverlay(pos, gold, 2.05f, -32f, AltitudeLayer.MoteLow);
            }
        }

        public static void PawnBuffVFXTick_Postfix(Pawn __instance, int delta)
        {
            if (__instance == null || !__instance.Spawned || __instance.Dead || __instance.Map == null)
                return;
            if (__instance.health == null || __instance.health.hediffSet == null)
                return;
            if (!__instance.IsHashIntervalTick(24))
                return;
            if (!__instance.DrawPos.ShouldSpawnMotesAt(__instance.Map))
                return;

            HediffSet hs = __instance.health.hediffSet;

            // Pele de Pedra: fragmentos terrosos sutilmente orbitando
            if (RebornDefOf.REM_Hediff_StoneSkin != null && hs.HasHediff(RebornDefOf.REM_Hediff_StoneSkin))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_StoneShard, 0.48f, RebornVFX.ColorEarth, 0.60f, 2.0f, 120f);
            }

            // Celeridade / Haste: faíscas e filetes esverdeados velozes
            if (RebornDefOf.REM_Hediff_Haste != null && hs.HasHediff(RebornDefOf.REM_Hediff_Haste))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_SoftSpark, 0.52f, RebornVFX.ColorWind, 0.55f, 4.4f, 240f);
            }

            // Regeneração Acelerada / Luz Curativa / Surto Vital: pétalas de vida subindo suavemente
            if (((RebornDefOf.REM_Hediff_AcceleratedRegen != null && hs.HasHediff(RebornDefOf.REM_Hediff_AcceleratedRegen))
                || (RebornDefOf.REM_Hediff_VitalSurge != null && hs.HasHediff(RebornDefOf.REM_Hediff_VitalSurge))
                || (RebornDefOf.REM_Hediff_HealingLight != null && hs.HasHediff(RebornDefOf.REM_Hediff_HealingLight)))
                && __instance.IsHashIntervalTick(48))
            {
                RebornVFX.PlayHealingBloom(__instance, RebornVFX.ColorHealing, true);
            }

            // Bênção dos Céus: orbes celestiais dourados orbitando
            HediffDef heavensDef = DefDatabase<HediffDef>.GetNamed("REM_Hediff_HeavensBlessing", false);
            if (heavensDef != null && hs.HasHediff(heavensDef))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_OrbitOrb, 0.58f, RebornVFX.ColorSkyGold, 0.72f, 3.1f, 180f);
            }

            // Escudos: partículas orbitando para indicar proteção ativa
            if (RebornDefOf.REM_Hediff_AetherShield != null)
            {
                Hediff_AetherShield ae = hs.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_AetherShield) as Hediff_AetherShield;
                if (ae != null && ae.shieldHP > 0.5f)
                {
                    RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_OrbitOrb, 0.52f, new Color(0.45f, 0.88f, 1.0f), 0.78f, 2.8f, 160f);
                }
            }

            if (RebornDefOf.REM_Hediff_CrimsonShield != null && hs.HasHediff(RebornDefOf.REM_Hediff_CrimsonShield))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_OrbitOrb, 0.54f, new Color(0.95f, 0.18f, 0.25f), 0.78f, -3.2f, -180f);
            }

            if (RebornDefOf.REM_Hediff_ManaBarrier != null && hs.HasHediff(RebornDefOf.REM_Hediff_ManaBarrier))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_SoftSpark, 0.55f, RebornVFX.ColorSoul, 0.76f, 3.6f, 220f);
            }

            if (RebornDefOf.REM_Hediff_SoulShield != null && hs.HasHediff(RebornDefOf.REM_Hediff_SoulShield))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_OrbitOrb, 0.58f, new Color(0.82f, 0.45f, 1.0f), 0.82f, -2.9f, -190f);
            }

            if (RebornDefOf.REM_Hediff_MindBarrier != null && hs.HasHediff(RebornDefOf.REM_Hediff_MindBarrier))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_SoftSpark, 0.48f, new Color(0.50f, 0.70f, 1.0f), 0.64f, 2.4f, 140f);
            }

            if (RebornDefOf.REM_Hediff_ShadowCloak != null && hs.HasHediff(RebornDefOf.REM_Hediff_ShadowCloak))
            {
                RebornVFX.ThrowOrbitFleck(__instance, RebornVFXDefOf.Fleck_SwirlIn, 0.65f, new Color(0.45f, 0.22f, 0.75f), 0.55f, -3.5f, -200f);
            }
        }
    }
}
