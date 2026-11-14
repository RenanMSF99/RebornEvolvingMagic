using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    public static class RebornCombatContext
    {
        public static SoulClassStage? ActiveSpellStage = null;

        public static SoulClassStage ResolveAttackStage(DamageInfo dinfo)
        {
            if (ActiveSpellStage.HasValue)
            {
                return ActiveSpellStage.Value;
            }
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker != null && attacker.health != null && attacker.health.hediffSet != null)
            {
                Hediff_SoulMatrix attackerMatrix = attacker.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
                if (attackerMatrix != null)
                {
                    return attackerMatrix.currentStage;
                }
            }
            return SoulClassStage.Mortal;
        }

        public static float GetSuppressionDamageMultiplier(SoulClassStage defenderStage, SoulClassStage attackStage)
        {
            int gap = (int)defenderStage - (int)attackStage;
            if (gap <= 0) return 1.0f;
            if (gap == 1) return 0.25f;
            if (gap == 2) return 0.05f;
            return 0.01f;
        }

        public static float GetControlDurationMultiplier(Pawn target, SoulClassStage attackStage)
        {
            if (target == null || target.Dead || target.health == null || target.health.hediffSet == null) return 1.0f;

            Hediff_SoulShield soulShield = target.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulShield) as Hediff_SoulShield;
            if (soulShield != null && soulShield.TryReactiveBlock(15f, 8f, "IMUNE (ESCUDO DA ALMA)!"))
            {
                return 0.0f;
            }

            Hediff_SoulMatrix targetMatrix = target.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            SoulClassStage defenderStage = (targetMatrix != null) ? targetMatrix.currentStage : SoulClassStage.Mortal;

            int gap = (int)defenderStage - (int)attackStage;
            if (gap <= 0) return 1.0f;
            if (gap == 1) return 0.25f;

            if (target.Spawned && target.Map != null)
            {
                MoteMaker.ThrowText(target.DrawPos, target.Map, "IMUNE (TIER)!", new Color(1f, 0.85f, 0.25f), 2.2f);
            }
            return 0.0f;
        }
    }

    [StaticConstructorOnStartup]
    public static class RebornHarmonyPatches
    {
        private static readonly Material EnergyBarMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.20f, 0.70f, 1.0f, 0.85f));
        private static readonly Material BarBgMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.65f, 0.65f));

        static RebornHarmonyPatches()
        {
            Harmony harmony = new Harmony("reborn.evolvingmagic.harmony");

            // 1. Injetar a Aba ITab_Pawn_SoulMatrix dinamicamente em Verse.Thing.GetInspectTabs
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Thing), "GetInspectTabs"),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "GetInspectTabs_Postfix")
                );
                Log.Message("[Reborn Magic] Patch de ITab aplicado com sucesso em Verse.Thing.GetInspectTabs.");
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patch em Verse.Thing.GetInspectTabs: " + ex);
            }

            // 2. Patch Pawn.DrawGUIOverlay para renderizar a barra flutuante sobre a cabeça
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(PawnUIOverlay), "DrawPawnGUIOverlay"),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "DrawSoulEnergyBar_Postfix")
                );
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patch em DrawPawnGUIOverlay: " + ex);
            }

            // 3. Patch Pawn.SpawnSetup para auto-despertar a Matriz em todos os colonos humanoides
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Pawn), "SpawnSetup"),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "CheckAwaken_Postfix")
                );
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patch em SpawnSetup: " + ex);
            }

            // 4. Patches de Renderização para Metamorfose com parâmetro rotOverride correto
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(PawnRenderer), "DynamicDrawPhaseAt", new Type[] { typeof(DrawPhase), typeof(Vector3), typeof(Rot4?), typeof(bool) }),
                    prefix: new HarmonyMethod(typeof(RebornHarmonyPatches), "DynamicDrawPhaseAt_Prefix")
                );

                harmony.Patch(
                    original: AccessTools.Method(typeof(PawnRenderer), "RenderPawnAt", new Type[] { typeof(Vector3), typeof(Rot4?), typeof(bool) }),
                    prefix: new HarmonyMethod(typeof(RebornHarmonyPatches), "RenderPawnAt_Prefix")
                );
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patches em PawnRenderer: " + ex);
            }

            // 5. Patch para desabilitar trabalhos manuais complexos em forma animal
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Pawn), "WorkTypeIsDisabled", new Type[] { typeof(WorkTypeDef) }),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "WorkTypeIsDisabled_Postfix")
                );
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patch em WorkTypeIsDisabled: " + ex);
            }

            // 6. Patches de Herança Física dos Atributos da Besta (Vida, Tamanho e Ataques Naturais)
            try
            {
                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(Pawn), "HealthScale"),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "HealthScale_Postfix")
                );

                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(Pawn), "BodySize"),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "BodySize_Postfix")
                );

                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(Pawn), "Tools"),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "Tools_Postfix")
                );
                Log.Message("[Reborn Magic] Patches de herança de HealthScale, BodySize e Tools aplicados.");
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patches de atributos da besta: " + ex);
            }

            // 7. Patch para ganho de XP da Alma através de trabalho nas habilidades
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Pawn_SkillTracker), "Learn", new Type[] { typeof(SkillDef), typeof(float), typeof(bool), typeof(bool) }),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "SkillLearn_Postfix")
                );
                Log.Message("[Reborn Magic] Patch de ganho de XP por tarefas (Pawn_SkillTracker.Learn) aplicado.");
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patch em Pawn_SkillTracker.Learn: " + ex);
            }

            // 8. Patch Pawn.PreApplyDamage para absorção de dano pelo Escudo Psiônico
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Pawn), "PreApplyDamage"),
                    prefix: new HarmonyMethod(typeof(RebornHarmonyPatches), "PreApplyDamage_Prefix")
                );
                Log.Message("[Reborn Magic] Patch de absorção de dano (Pawn.PreApplyDamage) aplicado.");
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patch em Pawn.PreApplyDamage: " + ex);
            }

            // 9. Patch Pawn.GetGizmos para ferramenta DEV de Estágio em qualquer peão/inimigo
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Pawn), "GetGizmos"),
                    postfix: new HarmonyMethod(typeof(RebornHarmonyPatches), "Pawn_GetGizmos_Postfix")
                );
            }
            catch (Exception ex)
            {
                Log.Error("[Reborn Magic] Erro ao aplicar patch em Pawn.GetGizmos: " + ex);
            }

            Log.Message("[Reborn Magic] Todos os módulos do Harmony foram inicializados com sucesso.");
        }

        public static bool PreApplyDamage_Prefix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (__instance == null || __instance.health == null || __instance.health.hediffSet == null) return true;

            Hediff_SoulMatrix matrixTarget = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;

            // 0. Supressão de Tier ("Quase Imunidade" contra ataques e magias de tiers mais baixos)
            SoulClassStage defenderStage = (matrixTarget != null) ? matrixTarget.currentStage : SoulClassStage.Mortal;
            SoulClassStage attackStage = RebornCombatContext.ResolveAttackStage(dinfo);
            float tierMult = RebornCombatContext.GetSuppressionDamageMultiplier(defenderStage, attackStage);
            if (tierMult < 0.999f)
            {
                dinfo.SetAmount(dinfo.Amount * tierMult);
                if (__instance.Spawned && __instance.Map != null && Rand.Chance(0.45f))
                {
                    int redPct = Mathf.RoundToInt((1.0f - tierMult) * 100f);
                    MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, string.Format("TIER -{0}%", redPct), new Color(1f, 0.85f, 0.35f), 1.8f);
                }
            }

            // 0b. Escudo da Alma (Mitigação/Imunidade reativa contra ataques psíquicos/espirituais/mágicos; se romper, o restante do dano passa)
            Hediff_SoulShield soulShield = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulShield) as Hediff_SoulShield;
            if (soulShield != null && (RebornCombatContext.ActiveSpellStage.HasValue || (dinfo.Def != null && (dinfo.Def == DamageDefOf.Psychic || dinfo.Def == DamageDefOf.Stun))))
            {
                float rem = soulShield.AbsorbDamageReactive(dinfo.Amount);
                if (rem <= 0.01f)
                {
                    absorbed = true;
                    return false;
                }
                dinfo.SetAmount(rem);
            }

            // 0c. Muralha Infernal (queima projéteis à distância e reduz dano mágico)
            Hediff_InfernoWall inferno = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_InfernoWall) as Hediff_InfernoWall;
            bool isRanged = (dinfo.Weapon != null && dinfo.Weapon.IsRangedWeapon) || (dinfo.Def != null && dinfo.Def.isRanged);
            if (inferno != null)
            {
                if (isRanged && UnityEngine.Random.value < inferno.projectileInterceptChance)
                {
                    if (__instance.Spawned && __instance.Map != null)
                    {
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_FlameWisp, __instance.DrawPos, __instance.Map, 1.3f, RebornVFX.ColorFire);
                        MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "INCINERADO!", RebornVFX.ColorFire, 1.8f);
                    }
                    absorbed = true;
                    return false;
                }
                if (RebornCombatContext.ActiveSpellStage.HasValue)
                {
                    dinfo.SetAmount(dinfo.Amount * (1.0f - inferno.magicDamageReduction));
                    if (__instance.Spawned && __instance.Map != null)
                    {
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_FlameWisp, __instance.DrawPos, __instance.Map, 1.1f, RebornVFX.ColorFire);
                    }
                }
            }

            // 1. Esquiva Fantasmagórica de Projéteis + Manto das Sombras
            Hediff_ShadowCloak shadowCloak = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ShadowCloak) as Hediff_ShadowCloak;
            float cloakDodge = (shadowCloak != null) ? shadowCloak.evasionBonus : 0f;
            float matrixDodge = (matrixTarget != null) ? matrixTarget.ProjectileDodgeChance : 0f;
            float totalRangedDodge = Mathf.Min(0.90f, matrixDodge + cloakDodge);

            if (!__instance.Dead && !__instance.Downed && isRanged && totalRangedDodge > 0.001f && UnityEngine.Random.value < totalRangedDodge)
            {
                if (__instance.Spawned && __instance.Map != null)
                {
                    RebornVFX.PlayDodgeWhoosh(__instance.DrawPos, __instance.Map);
                    MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, (cloakDodge > 0f) ? "SOMBRA EVADIU!" : "ESQUIVOU!", new Color(0.4f, 1f, 0.5f), 2.0f);
                }
                absorbed = true;
                return false;
            }

            // 2. Absorção em cadeia com transbordo (overflow): cada escudo mitiga até seu limite restante e repassa o excesso
            Hediff_AetherShield aether = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_AetherShield) as Hediff_AetherShield;
            if (aether != null && aether.shieldHP > 0.5f)
            {
                float rem = aether.AbsorbDamage(dinfo.Amount);
                if (rem <= 0.01f)
                {
                    absorbed = true;
                    return false;
                }
                dinfo.SetAmount(rem);
            }

            Hediff_CrimsonShield crimson = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_CrimsonShield) as Hediff_CrimsonShield;
            if (crimson != null && crimson.shieldHP > 0.01f)
            {
                float rem = crimson.AbsorbDamage(dinfo.Amount, dinfo);
                if (rem <= 0.01f)
                {
                    absorbed = true;
                    return false;
                }
                dinfo.SetAmount(rem);
            }

            Hediff_ManaBarrier manaBarrier = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ManaBarrier) as Hediff_ManaBarrier;
            if (manaBarrier != null)
            {
                float rem = manaBarrier.AbsorbDamage(dinfo.Amount);
                if (rem <= 0.01f)
                {
                    absorbed = true;
                    return false;
                }
                dinfo.SetAmount(rem);
            }

            Hediff_PsionicShield shield = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_PsionicShield) as Hediff_PsionicShield;
            if (shield != null && shield.shieldHP > 0.01f)
            {
                float rem = shield.AbsorbDamage(dinfo.Amount, dinfo);
                if (rem <= 0.01f)
                {
                    absorbed = true;
                    return false;
                }
                dinfo.SetAmount(rem);
            }

            // 3. Efeitos de Combate do Atacante Cultivador (Crítico por Sentidos e Stun por Força)
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker != null && attacker != __instance && attacker.health != null && attacker.health.hediffSet != null)
            {
                // Quebrar Manto das Sombras ao atacar (caso o atacante seja o próprio usuário invisível)
                Hediff_ShadowCloak atkCloak = attacker.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ShadowCloak) as Hediff_ShadowCloak;
                if (atkCloak != null)
                {
                    attacker.health.RemoveHediff(atkCloak);
                }

                Hediff_SoulMatrix matrixAttacker = attacker.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
                if (matrixAttacker != null)
                {
                    // Acerto Crítico Físico ou Mágico
                    if (UnityEngine.Random.value < matrixAttacker.CritChance)
                    {
                        float critMult = matrixAttacker.CritDamageMultiplier;
                        dinfo.SetAmount(dinfo.Amount * critMult);
                        if (__instance.Spawned && __instance.Map != null)
                        {
                            RebornVFX.PlayCritFlash(__instance.DrawPos, __instance.Map);
                            MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, string.Format("CRÍTICO! ({0:F1}x)", critMult), new Color(1f, 0.85f, 0.2f), 2.5f);
                        }
                    }

                    // Chance de Atordoamento/Desequilíbrio em Combate Corpo a Corpo por Força
                    bool isMelee = (dinfo.Weapon == null || dinfo.Weapon.IsMeleeWeapon);
                    if (isMelee && UnityEngine.Random.value < matrixAttacker.StunChance)
                    {
                        if (__instance.stances != null && __instance.stances.stunner != null)
                        {
                            __instance.stances.stunner.StunFor(60, attacker, true);
                            if (__instance.Spawned && __instance.Map != null)
                            {
                                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, __instance.DrawPos, __instance.Map, 0.7f, RebornVFX.ColorEarth);
                                MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "ATORDOADO!", new Color(1f, 0.5f, 0.3f), 2.0f);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public static void HealthScale_Postfix(Pawn __instance, ref float __result)
        {
            if (__instance == null || __instance.health == null || __instance.health.hediffSet == null) return;
            Hediff_SoulMatrix matrix = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix != null)
            {
                if (matrix.IsMetamorphosed && matrix.activeForm != null && matrix.activeForm.kindDef != null)
                {
                    PawnKindDef kind = matrix.activeForm.kindDef;
                    if (kind.race != null && kind.race.race != null)
                    {
                        float beastScale = kind.race.race.baseHealthScale;
                        float mult = matrix.activeForm.HealthMultiplier;
                        __result = Mathf.Max(1.0f, beastScale * mult);
                    }
                }
                // Fortalecimento físico biológico por cultivo da Constituição (+0.5% por ponto)
                __result *= matrix.CultivationHealthScaleMultiplier;
            }
        }

        public static void BodySize_Postfix(Pawn __instance, ref float __result)
        {
            if (__instance == null || __instance.health == null || __instance.health.hediffSet == null) return;
            Hediff_SoulMatrix matrix = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix != null && matrix.IsMetamorphosed && matrix.activeForm != null && matrix.activeForm.kindDef != null)
            {
                PawnKindDef kind = matrix.activeForm.kindDef;
                if (kind.race != null && kind.race.race != null)
                {
                    float beastSize = kind.race.race.baseBodySize;
                    float mult = matrix.activeForm.BodySizeMultiplier;
                    __result = Mathf.Max(0.2f, beastSize * mult);
                }
            }
        }

        public static void Tools_Postfix(Pawn __instance, ref List<Tool> __result)
        {
            if (__instance == null || __instance.health == null || __instance.health.hediffSet == null) return;
            Hediff_SoulMatrix matrix = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix != null && matrix.IsMetamorphosed && matrix.activeForm != null && matrix.activeForm.kindDef != null)
            {
                PawnKindDef kind = matrix.activeForm.kindDef;
                if (kind.race != null && kind.race.tools != null && kind.race.tools.Count > 0)
                {
                    __result = kind.race.tools;
                }
            }
        }

        public static void SkillLearn_Postfix(Pawn_SkillTracker __instance, SkillDef sDef, float xp, Pawn ___pawn)
        {
            if (__instance == null || sDef == null || xp <= 0.0001f) return;
            Pawn pawn = ___pawn;
            if (pawn == null || pawn.Dead || pawn.Faction == null || !pawn.Faction.IsPlayer) return;

            if (pawn.health == null || pawn.health.hediffSet == null) return;
            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return;

            SkillRecord sr = __instance.GetSkill(sDef);
            int skillLvl = (sr != null) ? sr.Level : 0;

            // Tarefas básicas dão 1xp por ponto de trabalho no nível 0;
            // Para cada nível superior, o ganho de xp aumenta em 1 (1 + level):
            // Nv 0: 1x, Nv 1: 2x, Nv 2: 3x, Nv 10: 11x...
            float levelMult = 1.0f + (float)skillLvl;

            // Paixão: Baixa (None) = 0.5x, Normal (Minor) = 1.0x, Ardente (Major) = 2.0x
            float passionMult = 1.0f;
            if (sr != null)
            {
                if (sr.passion == Passion.None)
                    passionMult = 0.5f;
                else if (sr.passion == Passion.Minor)
                    passionMult = 1.0f;
                else if (sr.passion == Passion.Major)
                    passionMult = 2.0f;
            }

            float soulXP = xp * levelMult * passionMult;
            matrix.AddXP(soulXP, false);
        }

        public static IEnumerable<InspectTabBase> GetInspectTabs_Postfix(IEnumerable<InspectTabBase> __result, Thing __instance)
        {
            bool alreadyHas = false;
            if (__result != null)
            {
                foreach (InspectTabBase tab in __result)
                {
                    if (tab is ITab_Pawn_SoulMatrix)
                    {
                        alreadyHas = true;
                    }
                    yield return tab;
                }
            }

            if (!alreadyHas)
            {
                Pawn pawn = __instance as Pawn;
                if (pawn != null && pawn.RaceProps != null && pawn.RaceProps.Humanlike)
                {
                    InspectTabBase soulTab = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_SoulMatrix));
                    if (soulTab != null)
                    {
                        yield return soulTab;
                    }
                }
            }
        }

        public static void CheckAwaken_Postfix(Pawn __instance)
        {
            if (__instance == null || __instance.Dead || __instance.RaceProps == null || !__instance.RaceProps.Humanlike) return;
            if (__instance.Faction == null || !__instance.Faction.IsPlayer) return;

            Hediff_SoulMatrix matrix = null;
            if (__instance.health != null && __instance.health.hediffSet != null)
            {
                matrix = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
                if (matrix == null)
                {
                    matrix = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_SoulMatrix, __instance) as Hediff_SoulMatrix;
                    if (matrix != null)
                    {
                        __instance.health.AddHediff(matrix);
                    }
                }
            }

            if (matrix != null)
            {
                if (__instance.story != null && __instance.story.traits != null)
                {
                    if (RebornDefOf.REM_Trait_MagicPearlBearer != null && __instance.story.traits.HasTrait(RebornDefOf.REM_Trait_MagicPearlBearer))
                    {
                        matrix.isMagicPearlBearer = true;
                    }
                }
                matrix.SyncAbilities();
            }
        }

        public static bool DynamicDrawPhaseAt_Prefix(PawnRenderer __instance, Pawn ___pawn, DrawPhase phase, Vector3 drawLoc, Rot4? rotOverride)
        {
            if (___pawn == null || !___pawn.Spawned || ___pawn.Dead || ___pawn.health == null || ___pawn.health.hediffSet == null)
                return true;

            Hediff_SoulMatrix matrix = ___pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix != null && matrix.IsMetamorphosed && matrix.activeForm != null)
            {
                if (phase == DrawPhase.Draw)
                {
                    Graphic beastGraphic = matrix.activeForm.GetGraphic();
                    if (beastGraphic != null)
                    {
                        Rot4 facing = rotOverride.HasValue ? rotOverride.Value : ___pawn.Rotation;
                        beastGraphic.Draw(drawLoc, facing, ___pawn, 0f);
                    }
                }
                return false;
            }
            return true;
        }

        public static bool RenderPawnAt_Prefix(PawnRenderer __instance, Pawn ___pawn, Vector3 drawLoc, Rot4? rotOverride)
        {
            if (___pawn == null || ___pawn.Dead || ___pawn.health == null || ___pawn.health.hediffSet == null)
                return true;

            Hediff_SoulMatrix matrix = ___pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix != null && matrix.IsMetamorphosed && matrix.activeForm != null)
            {
                Graphic beastGraphic = matrix.activeForm.GetGraphic();
                if (beastGraphic != null)
                {
                    Rot4 facing = rotOverride.HasValue ? rotOverride.Value : ___pawn.Rotation;
                    beastGraphic.Draw(drawLoc, facing, ___pawn, 0f);
                }
                return false;
            }
            return true;
        }

        public static void WorkTypeIsDisabled_Postfix(Pawn __instance, WorkTypeDef w, ref bool __result)
        {
            if (__result) return;
            if (__instance == null || __instance.health == null || __instance.health.hediffSet == null || w == null) return;

            Hediff_SoulMatrix matrix = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix != null && matrix.IsMetamorphosed)
            {
                string defName = w.defName;
                if (defName == "Cooking" || defName == "Smithing" || defName == "Tailoring" || defName == "Crafting" || defName == "Art" || defName == "Research")
                {
                    __result = true;
                }
            }
        }

        public static void DrawSoulEnergyBar_Postfix(PawnUIOverlay __instance, Pawn ___pawn)
        {
            if (___pawn == null || !___pawn.Spawned || ___pawn.Dead || !___pawn.Drafted) return;
            if (___pawn.health == null || ___pawn.health.hediffSet == null) return;

            Hediff_SoulMatrix matrix = ___pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return;

            Vector3 drawPos = ___pawn.DrawPos;
            drawPos.z += 0.85f;

            float pct = Mathf.Clamp01(matrix.currentEnergy / Mathf.Max(1f, matrix.MaxEnergy));
            GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
            {
                center = drawPos,
                size = new Vector2(0.55f, 0.08f),
                fillPercent = pct,
                filledMat = EnergyBarMat,
                unfilledMat = BarBgMat,
                margin = 0.02f
            });
        }

        public static void Pawn_GetGizmos_Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!Prefs.DevMode && !DebugSettings.godMode) return;
            if (__instance == null || __instance.Dead || __instance.RaceProps == null || !__instance.RaceProps.Humanlike) return;

            List<Gizmo> list = new List<Gizmo>(__result);
            Command_Action cmdStage = new Command_Action();
            cmdStage.defaultLabel = "DEV: Estágio (Tier)";
            cmdStage.defaultDesc = "Define manualmente a Matriz da Alma e o Estágio (Mortal → Grão-Mestre) deste peão ou inimigo para testar Supressão de Tier.";
            cmdStage.action = delegate
            {
                List<FloatMenuOption> opts = new List<FloatMenuOption>();
                opts.Add(new FloatMenuOption("Remover Matriz (Mortal 0 Comum)", delegate
                {
                    if (__instance.health != null && __instance.health.hediffSet != null)
                    {
                        Hediff existing = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix);
                        if (existing != null) __instance.health.RemoveHediff(existing);
                    }
                }));
                for (int s = 0; s <= (int)SoulClassStage.Grandmaster; s++)
                {
                    SoulClassStage st = (SoulClassStage)s;
                    SoulStageConfig cfg = SoulStageDatabase.GetConfig(st);
                    opts.Add(new FloatMenuOption(string.Format("Estágio {0}: {1}", s, cfg.LabelPT), delegate
                    {
                        if (__instance.health == null || __instance.health.hediffSet == null) return;
                        Hediff_SoulMatrix m = __instance.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
                        if (m == null)
                        {
                            m = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_SoulMatrix, __instance) as Hediff_SoulMatrix;
                            if (m != null) __instance.health.AddHediff(m);
                        }
                        if (m != null)
                        {
                            m.Dev_SetStage(st);
                        }
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(opts));
            };
            list.Add(cmdStage);
            __result = list;
        }
    }
}
