using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RebornEvolvingMagic
{
    // 1. Hediff: Pele de Pedra
    public class Hediff_StoneSkin : HediffWithComps
    {
        public int ticksRemaining = 1800; // 30 segundos
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s", Mathf.CeilToInt((float)ticksRemaining / 60f)); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statOffsets = new List<StatModifier>();
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Sharp, value = 0.60f });
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Blunt, value = 0.50f });
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Heat, value = 0.40f });
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = -1.0f }); // -20% mobilidade
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "PELE DE PEDRA DISSIPADA", new Color(0.7f, 0.7f, 0.7f), 2.5f);
                }
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 1800);
        }
    }

    // 2. Hediff: Regeneração Acelerada (Duração: 1 dia in-game / 60.000 ticks)
    public class Hediff_AcceleratedRegen : HediffWithComps
    {
        public int ticksRemaining = 60000; // 1 dia in-game (24h = 60.000 ticks)
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get
            {
                if (ticksRemaining >= 60000)
                {
                    return string.Format("{0:F1}d", (float)ticksRemaining / 60000f);
                }
                if (ticksRemaining >= 2500)
                {
                    return string.Format("{0:F1}h", (float)ticksRemaining / 2500f);
                }
                return string.Format("{0}m", Mathf.Max(1, Mathf.CeilToInt((float)ticksRemaining / 41.67f)));
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statFactors = new List<StatModifier>();
                s.statFactors.Add(new StatModifier { stat = StatDefOf.InjuryHealingFactor, value = 5.0f });
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;

            // A cada 60 ticks (1s), cicatriza 1.5 HP no ferimento mais grave
            if (pawn.IsHashIntervalTick(60) && pawn.health != null && pawn.health.hediffSet != null)
            {
                Hediff_Injury worst = null;
                for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
                {
                    Hediff_Injury inj = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
                    if (inj != null && inj.Severity > 0.01f && inj.CanHealNaturally())
                    {
                        if (worst == null || inj.Severity > worst.Severity)
                        {
                            worst = inj;
                        }
                    }
                }
                if (worst != null)
                {
                    worst.Heal(1.5f);
                    if (pawn.IsHashIntervalTick(180) && pawn.Spawned && pawn.Map != null)
                    {
                        FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 0.8f);
                    }
                }
            }

            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 60000);
        }
    }

    // 3. Hediff: Escudo Psiônico
    public class Hediff_PsionicShield : HediffWithComps
    {
        public float shieldHP = 150f;
        public float maxShieldHP = 150f;
        public int lastAbsorbTick = -999;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}/{1} HP", Mathf.RoundToInt(shieldHP), Mathf.RoundToInt(maxShieldHP)); }
        }

        public float AbsorbDamage(float amount, DamageInfo dinfo)
        {
            if (shieldHP <= 0.01f || amount <= 0f) return amount;

            float mitigated = Mathf.Min(shieldHP, amount);
            shieldHP -= mitigated;
            float remaining = amount - mitigated;

            if (Find.TickManager != null)
            {
                lastAbsorbTick = Find.TickManager.TicksGame;
            }

            if (pawn != null && pawn.Spawned && pawn.Map != null)
            {
                Vector3 pos = pawn.DrawPos;
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, pawn.Map, 0.72f, new Color(0.35f, 0.85f, 1f));
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pos, pawn.Map, 1.45f, Color.white);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, pos, pawn.Map, 1.55f, new Color(0.3f, 0.8f, 1f), Rand.Range(0f, 360f), 220f);
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }

            if (shieldHP <= 0.01f)
            {
                shieldHP = 0f;
                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    Vector3 pos = pawn.DrawPos;
                    RebornVFX.PlayImpactBurst(pos, pawn.Map, new Color(1f, 0.35f, 0.3f), new Color(0.35f, 0.8f, 1f), 1.9f, RebornVFXDefOf.Fleck_SoftSpark, 10, true, SoundDefOf.EnergyShield_Reset);
                    MoteMaker.ThrowText(pos, pawn.Map, string.Format("ESCUDO COLAPSADO! (-{0})", Mathf.RoundToInt(mitigated)), new Color(1f, 0.3f, 0.3f), 3.0f);
                }
                pawn.health.RemoveHediff(this);
            }
            else if (pawn != null && pawn.Spawned && pawn.Map != null)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("-{0} ESCUDO", Mathf.RoundToInt(mitigated)), new Color(0.35f, 0.75f, 1f), 2.0f);
            }

            return remaining;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shieldHP, "shieldHP", 150f);
            Scribe_Values.Look(ref maxShieldHP, "maxShieldHP", 150f);
        }
    }

    // 4. Hediff: Pressa
    public class Hediff_Haste : HediffWithComps
    {
        public int ticksRemaining = 1800; // 30 segundos
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s", Mathf.CeilToInt((float)ticksRemaining / 60f)); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statOffsets = new List<StatModifier>();
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 2.3f }); // +50% velocidade
                s.statFactors = new List<StatModifier>();
                s.statFactors.Add(new StatModifier { stat = StatDefOf.WorkSpeedGlobal, value = 1.40f }); // +40% trabalho
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 1800);
        }
    }

    // 5. Hediff: Benção dos Céus
    public class Hediff_HeavensBlessing : HediffWithComps
    {
        public int ticksRemaining = 2700; // 45 segundos
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s", Mathf.CeilToInt((float)ticksRemaining / 60f)); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statOffsets = new List<StatModifier>();
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Sharp, value = 1.00f });
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Blunt, value = 1.00f });
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Heat, value = 0.80f });
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 1.5f }); // Bônus em vez de perda!
                s.statFactors = new List<StatModifier>();
                s.statFactors.Add(new StatModifier { stat = StatDefOf.InjuryHealingFactor, value = 10.0f });
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;

            // A cada 30 ticks (0.5s), regeneração rápida em ferimentos
            if (pawn.IsHashIntervalTick(30) && pawn.health != null && pawn.health.hediffSet != null)
            {
                for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
                {
                    Hediff_Injury inj = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
                    if (inj != null && inj.Severity > 0.01f && inj.CanHealNaturally())
                    {
                        inj.Heal(2.0f);
                        break;
                    }
                }
            }

            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 2700);
        }
    }

    // 6. Hediff: Escudo da Alma (Soul Shield - Contínuo + Imunidade Reativa MP/SP)
    public class Hediff_SoulShield : HediffWithComps
    {
        public SoulClassStage spellStage = SoulClassStage.Mortal;
        public float drainPerSecondMP = 3.0f;
        public float drainPerSecondSP = 1.5f;
        public float reactiveCostMult = 1.0f;
        public int lastAbsorbTick = -999;

        public override string LabelInBrackets
        {
            get { return string.Format("{0} | -{1:F1} MP/s", SoulStageDatabase.GetConfig(spellStage).LabelPT, drainPerSecondMP); }
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn == null || pawn.Dead) return;
            if (pawn.IsHashIntervalTick(60))
            {
                Hediff_SoulMatrix matrix = (pawn.health != null && pawn.health.hediffSet != null)
                    ? pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix
                    : null;
                if (matrix == null || !matrix.TryConsumeEnergy(drainPerSecondMP * reactiveCostMult, false) || !matrix.TryConsumeStamina(drainPerSecondSP * reactiveCostMult))
                {
                    if (pawn.Spawned && pawn.Map != null)
                    {
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ESCUDO DA ALMA DESATIVADO", new Color(0.8f, 0.5f, 1f), 2.5f);
                    }
                    pawn.health.RemoveHediff(this);
                }
                else
                {
                    // Remove debuffs mentais/psíquicos ativos continuamente enquanto mantém o escudo
                    if (pawn.InMentalState)
                    {
                        pawn.MentalState.RecoverFromState();
                    }
                }
            }
        }

        public bool TryReactiveBlock(float baseMP, float baseSP, string label)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null) return false;
            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            float mpCost = baseMP * reactiveCostMult;
            float spCost = baseSP * reactiveCostMult;
            if (matrix.currentEnergy + 0.01f >= mpCost && matrix.currentStamina + 0.01f >= spCost)
            {
                matrix.TryConsumeEnergy(mpCost);
                matrix.TryConsumeStamina(spCost);
                if (Find.TickManager != null)
                {
                    lastAbsorbTick = Find.TickManager.TicksGame;
                }
                if (pawn.Spawned && pawn.Map != null)
                {
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, pawn.DrawPos, pawn.Map, 1.8f, new Color(0.8f, 0.45f, 1f), 0f, 220f);
                    SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, label, new Color(0.85f, 0.55f, 1f), 2.0f);
                }
                return true;
            }
            return false;
        }

        public float AbsorbDamageReactive(float amount)
        {
            if (amount <= 0f || pawn == null || pawn.health == null || pawn.health.hediffSet == null) return amount;
            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return amount;

            float fullMpCost = Mathf.Clamp(amount * 0.45f, 8f, 45f) * reactiveCostMult;
            float fullSpCost = fullMpCost * 0.5f;
            float fMp = (fullMpCost > 0.01f) ? Mathf.Clamp01(matrix.currentEnergy / fullMpCost) : 1f;
            float fSp = (fullSpCost > 0.01f) ? Mathf.Clamp01(matrix.currentStamina / fullSpCost) : 1f;
            float frac = Mathf.Min(fMp, fSp);

            if (frac <= 0.01f)
            {
                if (pawn.Spawned && pawn.Map != null)
                {
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ESCUDO DA ALMA ROMPIDO!", new Color(1f, 0.4f, 0.4f), 2.3f);
                }
                pawn.health.RemoveHediff(this);
                return amount;
            }

            matrix.TryConsumeEnergy(fullMpCost * frac);
            matrix.TryConsumeStamina(fullSpCost * frac);
            if (Find.TickManager != null)
            {
                lastAbsorbTick = Find.TickManager.TicksGame;
            }

            float mitigated = amount * frac;
            float remaining = amount - mitigated;

            if (pawn.Spawned && pawn.Map != null)
            {
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, pawn.DrawPos, pawn.Map, 1.8f, new Color(0.8f, 0.45f, 1f), 0f, 220f);
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }

            if (remaining > 0.01f)
            {
                if (pawn.Spawned && pawn.Map != null)
                {
                    RebornVFX.PlayImpactBurst(pawn.DrawPos, pawn.Map, new Color(0.85f, 0.45f, 1f), Color.white, 1.8f, RebornVFXDefOf.Fleck_SoftSpark, 8, true, SoundDefOf.EnergyShield_Reset);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("ALMA ROMPIDA! (-{0})", Mathf.RoundToInt(mitigated)), new Color(1f, 0.4f, 0.4f), 2.3f);
                }
                pawn.health.RemoveHediff(this);
            }
            else if (pawn.Spawned && pawn.Map != null)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("BLOQUEIO DA ALMA (-{0})", Mathf.RoundToInt(mitigated)), new Color(0.85f, 0.55f, 1f), 2.0f);
            }

            return remaining;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spellStage, "spellStage", SoulClassStage.Mortal);
            Scribe_Values.Look(ref drainPerSecondMP, "drainPerSecondMP", 3.0f);
            Scribe_Values.Look(ref drainPerSecondSP, "drainPerSecondSP", 1.5f);
            Scribe_Values.Look(ref reactiveCostMult, "reactiveCostMult", 1.0f);
        }
    }

    // 7. Hediff: Barreira de Mana (Mana Barrier - Absorção reativa por MP)
    public class Hediff_ManaBarrier : HediffWithComps
    {
        public SoulClassStage spellStage = SoulClassStage.Mortal;
        public float mpPerDamage = 1.0f;
        public int ticksRemaining = 7500; // ~3h in-game ou até desativar/acabar MP
        public int lastAbsorbTick = -999;

        public override string LabelInBrackets
        {
            get { return string.Format("{0:F2} MP/Dano", mpPerDamage); }
        }

        public float AbsorbDamage(float amount)
        {
            if (amount <= 0f || pawn == null || pawn.health == null || pawn.health.hediffSet == null) return amount;
            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return amount;

            float costPerDmg = Mathf.Max(0.05f, mpPerDamage);
            float maxAbsorbable = matrix.currentEnergy / costPerDmg;
            if (maxAbsorbable <= 0.01f)
            {
                if (pawn.Spawned && pawn.Map != null)
                {
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "BARREIRA DE MANA ROMPIDA!", new Color(1f, 0.4f, 0.3f), 2.5f);
                }
                pawn.health.RemoveHediff(this);
                return amount;
            }

            float mitigated = Mathf.Min(amount, maxAbsorbable);
            float spentMP = mitigated * costPerDmg;
            matrix.TryConsumeEnergy(spentMP);
            if (Find.TickManager != null)
            {
                lastAbsorbTick = Find.TickManager.TicksGame;
            }
            float remaining = amount - mitigated;

            if (pawn.Spawned && pawn.Map != null)
            {
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, pawn.Map, 0.85f, RebornVFX.ColorSoul);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, pawn.DrawPos, pawn.Map, 1.55f, RebornVFX.ColorSoul, Rand.Range(0f, 360f), 220f);
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }

            if (remaining > 0.01f)
            {
                if (pawn.Spawned && pawn.Map != null)
                {
                    RebornVFX.PlayImpactBurst(pawn.DrawPos, pawn.Map, RebornVFX.ColorSoul, Color.white, 1.8f, RebornVFXDefOf.Fleck_SoftSpark, 8, true, SoundDefOf.EnergyShield_Reset);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("BARREIRA ROMPIDA! (-{0})", Mathf.RoundToInt(mitigated)), new Color(1f, 0.4f, 0.3f), 2.4f);
                }
                pawn.health.RemoveHediff(this);
            }
            else if (pawn.Spawned && pawn.Map != null)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("BARREIRA -{0} MP (-{1})", Mathf.RoundToInt(spentMP), Mathf.RoundToInt(mitigated)), RebornVFX.ColorSoul, 1.9f);
            }

            return remaining;
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spellStage, "spellStage", SoulClassStage.Mortal);
            Scribe_Values.Look(ref mpPerDamage, "mpPerDamage", 1.0f);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 7500);
        }
    }

    // 8. Hediff: Escudo de Éter (Aether Shield - Duração 12h com autorregeneração leve)
    public class Hediff_AetherShield : HediffWithComps
    {
        public float shieldHP = 250f;
        public float maxShieldHP = 250f;
        public int ticksRemaining = 30000; // 12h in-game
        public int lastAbsorbTick = -999;
        public int rechargeCooldownTicks = 0;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}/{1} HP ({2:F1}h)", Mathf.RoundToInt(shieldHP), Mathf.RoundToInt(maxShieldHP), (float)ticksRemaining / 2500f); }
        }

        public float AbsorbDamage(float amount)
        {
            if (shieldHP <= 0.5f || amount <= 0f) return amount;

            float mitigated = Mathf.Min(shieldHP, amount);
            shieldHP -= mitigated;
            float remaining = amount - mitigated;
            if (Find.TickManager != null)
            {
                lastAbsorbTick = Find.TickManager.TicksGame;
            }

            if (pawn != null && pawn.Spawned && pawn.Map != null)
            {
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, pawn.Map, 0.80f, new Color(0.55f, 0.85f, 1f));
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, pawn.Map, 1.35f, Color.white);
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }

            if (shieldHP <= 0.01f)
            {
                shieldHP = 0f;
                rechargeCooldownTicks = 180;
                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    RebornVFX.PlayImpactBurst(pawn.DrawPos, pawn.Map, new Color(0.45f, 0.85f, 1f), Color.white, 1.75f, RebornVFXDefOf.Fleck_SoftSpark, 8, true, SoundDefOf.EnergyShield_Reset);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("ÉTER QUEBRADO! (-{0})", Mathf.RoundToInt(mitigated)), new Color(1f, 0.45f, 0.35f), 2.3f);
                }
            }
            else if (pawn != null && pawn.Spawned && pawn.Map != null)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("ÉTER -{0}", Mathf.RoundToInt(mitigated)), new Color(0.55f, 0.85f, 1f), 1.8f);
            }

            return remaining;
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (rechargeCooldownTicks > 0)
            {
                rechargeCooldownTicks--;
            }
            if (pawn != null && rechargeCooldownTicks <= 0 && pawn.IsHashIntervalTick(60) && shieldHP < maxShieldHP)
            {
                shieldHP = Mathf.Min(maxShieldHP, shieldHP + (maxShieldHP * 0.015f));
            }
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shieldHP, "shieldHP", 250f);
            Scribe_Values.Look(ref maxShieldHP, "maxShieldHP", 250f);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 30000);
            Scribe_Values.Look(ref rechargeCooldownTicks, "rechargeCooldownTicks", 0);
        }
    }

    // 9. Hediff: Escudo Carmesim (Crimson Shield - Absorve e reflete/drena no melee)
    public class Hediff_CrimsonShield : HediffWithComps
    {
        public float shieldHP = 200f;
        public float maxShieldHP = 200f;
        public float reflectRatio = 0.35f;
        public int ticksRemaining = 1800;
        public int lastAbsorbTick = -999;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}/{1} HP", Mathf.RoundToInt(shieldHP), Mathf.RoundToInt(maxShieldHP)); }
        }

        public float AbsorbDamage(float amount, DamageInfo dinfo)
        {
            if (shieldHP <= 0.01f || amount <= 0f) return amount;

            float mitigated = Mathf.Min(shieldHP, amount);
            shieldHP -= mitigated;
            float remaining = amount - mitigated;
            if (Find.TickManager != null)
            {
                lastAbsorbTick = Find.TickManager.TicksGame;
            }

            Pawn attacker = dinfo.Instigator as Pawn;
            bool isMelee = (dinfo.Weapon == null || dinfo.Weapon.IsMeleeWeapon);
            if (isMelee && attacker != null && attacker != pawn && !attacker.Dead)
            {
                float reflectDmg = Mathf.Max(2f, mitigated * reflectRatio);
                attacker.TakeDamage(new DamageInfo(DamageDefOf.Cut, reflectDmg, 0.35f, -1f, pawn));
                if (pawn.health != null && pawn.health.hediffSet != null)
                {
                    Hediff_Injury inj = pawn.health.hediffSet.hediffs.Find(delegate(Hediff h) { return h is Hediff_Injury; }) as Hediff_Injury;
                    if (inj != null) inj.Heal(reflectDmg * 0.4f);
                }
            }

            if (pawn != null && pawn.Spawned && pawn.Map != null)
            {
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, pawn.Map, 0.85f, new Color(0.92f, 0.18f, 0.25f));
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, pawn.Map, 1.35f, new Color(1f, 0.35f, 0.4f));
                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }

            if (shieldHP <= 0.01f)
            {
                shieldHP = 0f;
                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    RebornVFX.PlayImpactBurst(pawn.DrawPos, pawn.Map, new Color(0.92f, 0.15f, 0.22f), Color.red, 1.85f, RebornVFXDefOf.Fleck_SoftSpark, 9, true, SoundDefOf.EnergyShield_Reset);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("CARMESIM ROMPIDO! (-{0})", Mathf.RoundToInt(mitigated)), new Color(1f, 0.35f, 0.35f), 2.3f);
                }
                pawn.health.RemoveHediff(this);
            }
            else if (pawn != null && pawn.Spawned && pawn.Map != null)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("CARMESIM -{0}", Mathf.RoundToInt(mitigated)), new Color(0.95f, 0.25f, 0.32f), 1.8f);
            }

            return remaining;
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shieldHP, "shieldHP", 200f);
            Scribe_Values.Look(ref maxShieldHP, "maxShieldHP", 200f);
            Scribe_Values.Look(ref reflectRatio, "reflectRatio", 0.35f);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 1800);
        }
    }

    // 10. Hediff: Muralha Infernal (Inferno Wall - Anel de fogo defensivo em volta do conjurador)
    public class Hediff_InfernoWall : HediffWithComps
    {
        public SoulClassStage spellStage = SoulClassStage.Mortal;
        public int ticksRemaining = 900;
        public float ringRadius = 2.6f;
        public float ringBurnDamage = 10f;
        public float projectileInterceptChance = 0.55f;
        public float magicDamageReduction = 0.35f;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s", Mathf.CeilToInt((float)ticksRemaining / 60f)); }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (pawn != null && pawn.Spawned && pawn.Map != null && pawn.IsHashIntervalTick(30))
            {
                Vector3 center = pawn.DrawPos;
                for (int i = 0; i < 6; i++)
                {
                    float ang = i * 60f + ((Find.TickManager.TicksGame * 3) % 360);
                    Vector3 off = Vector3.forward.RotatedBy(ang) * ringRadius;
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_FlameWisp, center + off, pawn.Map, 0.9f, RebornVFX.ColorFire, ang, 120f);
                }

                if (pawn.IsHashIntervalTick(60))
                {
                    RebornCombatContext.ActiveSpellStage = spellStage;
                    try
                    {
                        IReadOnlyList<Pawn> allPawns = pawn.Map.mapPawns.AllPawnsSpawned;
                        for (int i = allPawns.Count - 1; i >= 0; i--)
                        {
                            Pawn other = allPawns[i];
                            if (other == null || other == pawn || other.Dead) continue;
                            if (!other.HostileTo(pawn)) continue;
                            if (other.Position.DistanceTo(pawn.Position) <= ringRadius + 0.6f)
                            {
                                other.TakeDamage(new DamageInfo(DamageDefOf.Burn, ringBurnDamage, 0.3f, -1f, pawn));
                                other.TryAttachFire(0.25f, pawn);
                            }
                        }
                    }
                    finally
                    {
                        RebornCombatContext.ActiveSpellStage = null;
                    }
                }
            }

            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spellStage, "spellStage", SoulClassStage.Mortal);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 900);
            Scribe_Values.Look(ref ringRadius, "ringRadius", 2.6f);
            Scribe_Values.Look(ref ringBurnDamage, "ringBurnDamage", 10f);
            Scribe_Values.Look(ref projectileInterceptChance, "projectileInterceptChance", 0.55f);
            Scribe_Values.Look(ref magicDamageReduction, "magicDamageReduction", 0.35f);
        }
    }

    // 11. Hediff: Barreira Mental (Mind Barrier - Duração fixa, resistência psíquica/mental)
    public class Hediff_MindBarrier : HediffWithComps
    {
        public int ticksRemaining = 2400;
        public float resistancePower = 1.0f;
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s", Mathf.CeilToInt((float)ticksRemaining / 60f)); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statFactors = new List<StatModifier>();
                s.statFactors.Add(new StatModifier { stat = StatDefOf.PsychicSensitivity, value = Mathf.Max(0.05f, 0.35f / Mathf.Max(1f, resistancePower * 0.5f)) });
                s.statOffsets = new List<StatModifier>();
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.MentalBreakThreshold, value = -0.35f });
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 2400);
            Scribe_Values.Look(ref resistancePower, "resistancePower", 1.0f);
        }
    }

    // 12. Hediff: Manto das Sombras (Shadow Cloak - Evasão e Furtividade)
    public class Hediff_ShadowCloak : HediffWithComps
    {
        public int ticksRemaining = 1200;
        public float evasionBonus = 0.65f;
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s ({1:F0}% Evasão)", Mathf.CeilToInt((float)ticksRemaining / 60f), evasionBonus * 100f); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statOffsets = new List<StatModifier>();
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.MeleeDodgeChance, value = Mathf.Min(0.85f, evasionBonus) });
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 1200);
            Scribe_Values.Look(ref evasionBonus, "evasionBonus", 0.65f);
        }
    }

    // 13. Hediff: Surto Vital (Vital Surge - Imunidade e regeneração natural por 12h)
    public class Hediff_VitalSurge : HediffWithComps
    {
        public int ticksRemaining = 30000;
        public float bonusMultiplier = 1.0f;
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get { return string.Format("{0:F1}h", (float)ticksRemaining / 2500f); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statOffsets = new List<StatModifier>();
                s.statOffsets.Add(new StatModifier { stat = StatDefOf.ImmunityGainSpeed, value = 0.60f * bonusMultiplier });
                s.naturalHealingFactor = 1.5f + (0.5f * bonusMultiplier);
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 30000);
            Scribe_Values.Look(ref bonusMultiplier, "bonusMultiplier", 1.0f);
        }
    }

    // 14. Hediff: Luz Curativa (Healing Light - Trata ferimentos incluindo partes arrancadas + 2x regen por 24h)
    public class Hediff_HealingLight : HediffWithComps
    {
        public int ticksRemaining = 60000;
        public float regenFactor = 2.0f;
        private HediffStage cachedStage = null;

        public override string LabelInBrackets
        {
            get { return string.Format("{0:F1}h (x{1:F1})", (float)ticksRemaining / 2500f, regenFactor); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statFactors = new List<StatModifier>();
                s.statFactors.Add(new StatModifier { stat = StatDefOf.InjuryHealingFactor, value = regenFactor });
                s.naturalHealingFactor = regenFactor;
                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;

            if (pawn != null && pawn.IsHashIntervalTick(300) && pawn.health != null && pawn.health.hediffSet != null)
            {
                // Fecha ferimentos abertos (tending automático sem curar HP direto)
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    Hediff_Injury inj = hediffs[i] as Hediff_Injury;
                    if (inj != null && inj.TendableNow())
                    {
                        inj.Tended(1.0f, 1.0f);
                        break;
                    }
                }

                // Regenera lentamente partes do corpo arrancadas
                if (pawn.IsHashIntervalTick(1500))
                {
                    for (int i = 0; i < hediffs.Count; i++)
                    {
                        Hediff_MissingPart missing = hediffs[i] as Hediff_MissingPart;
                        if (missing != null && missing.Part != null)
                        {
                            BodyPartRecord part = missing.Part;
                            pawn.health.RestorePart(part);
                            if (pawn.Spawned && pawn.Map != null)
                            {
                                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("PARTE RESTAURADA: {0}", part.Label), RebornVFX.ColorHealing, 3.0f);
                            }
                            break;
                        }
                    }
                }
            }

            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 60000);
            Scribe_Values.Look(ref regenFactor, "regenFactor", 2.0f);
        }
    }

    // 15. Hediff: Postura / Buff Genérico Parametrizado
    public class Hediff_MartialStance : HediffWithComps
    {
        public string stanceKind = "BodyStrengthening";
        public int ticksRemaining = 1800;
        public float effectMultiplier = 1.0f;
        private HediffStage cachedStage = null;

        public override string LabelBase
        {
            get
            {
                if (stanceKind == "Bloodlust") return "Postura Sanguinária";
                if (stanceKind == "Serenity") return "Aura de Serenidade";
                if (stanceKind == "TimeDilation") return "Dilatação Temporal";
                if (stanceKind == "SpiritualScan") return "Olho da Percepção Espiritual";
                return "Fortalecimento Corporal";
            }
        }

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s", Mathf.CeilToInt((float)ticksRemaining / 60f)); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statOffsets = new List<StatModifier>();
                s.statFactors = new List<StatModifier>();

                if (stanceKind == "Bloodlust")
                {
                    s.statOffsets.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 1.8f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeDamageFactor, value = 1.35f + 0.15f * effectMultiplier });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeCooldownFactor, value = 0.65f });
                    s.painFactor = 0.05f;
                }
                else if (stanceKind == "Serenity")
                {
                    s.statOffsets.Add(new StatModifier { stat = StatDefOf.MentalBreakThreshold, value = -0.50f });
                }
                else if (stanceKind == "TimeDilation")
                {
                    s.statOffsets.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 2.5f + effectMultiplier });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.WorkSpeedGlobal, value = 1.50f + 0.25f * effectMultiplier });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeCooldownFactor, value = 0.60f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.AimingDelayFactor, value = 0.65f });
                }
                else if (stanceKind == "SpiritualScan")
                {
                    s.statOffsets.Add(new StatModifier { stat = StatDefOf.ShootingAccuracyPawn, value = 0.25f });
                }
                else // BodyStrengthening
                {
                    s.statOffsets.Add(new StatModifier { stat = StatDefOf.CarryingCapacity, value = 45f * effectMultiplier });
                    s.statOffsets.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 1.0f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeDamageFactor, value = 1.25f + 0.20f * effectMultiplier });
                    s.painFactor = 0.50f;
                }

                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref stanceKind, "stanceKind", "BodyStrengthening");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 1800);
            Scribe_Values.Look(ref effectMultiplier, "effectMultiplier", 1.0f);
        }
    }

    // 16. Hediff: Debuff de Combate Parametrizado (Decay, SpaceFreeze, TimeSlow, GravityCrush, IntimidatingRoar)
    public class Hediff_CultivationDebuff : HediffWithComps
    {
        public string debuffKind = "TimeSlow";
        public int ticksRemaining = 600;
        public float effectMultiplier = 1.0f;
        public float periodicDamage = 0f;
        public SoulClassStage attackStage = SoulClassStage.Mortal;
        private HediffStage cachedStage = null;

        public override string LabelBase
        {
            get
            {
                if (debuffKind == "Decay") return "Decomposição Necrótica";
                if (debuffKind == "SpaceFreeze") return "Espaço Congelado";
                if (debuffKind == "TemporalStasis") return "Estase Temporal";
                if (debuffKind == "GravityCrush") return "Pressão Gravitacional";
                if (debuffKind == "IntimidatingRoar") return "Amedrontado";
                return "Lentidão Temporal";
            }
        }

        public override string LabelInBrackets
        {
            get { return string.Format("{0}s", Mathf.CeilToInt((float)ticksRemaining / 60f)); }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                HediffStage s = new HediffStage();
                s.statFactors = new List<StatModifier>();
                s.capMods = new List<PawnCapacityModifier>();

                if (debuffKind == "SpaceFreeze" || debuffKind == "GravityCrush")
                {
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 0.02f });
                    s.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Moving, setMax = 0.05f });
                }
                else if (debuffKind == "TemporalStasis")
                {
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 0.15f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.AimingDelayFactor, value = 3.0f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeCooldownFactor, value = 3.0f });
                }
                else if (debuffKind == "Decay")
                {
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 0.55f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.InjuryHealingFactor, value = 0.10f });
                    s.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Manipulation, offset = -0.35f });
                }
                else if (debuffKind == "IntimidatingRoar")
                {
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeDamageFactor, value = 0.70f });
                }
                else // TimeSlow
                {
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = 0.35f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.AimingDelayFactor, value = 2.0f });
                    s.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeCooldownFactor, value = 2.0f });
                }

                cachedStage = s;
                return cachedStage;
            }
        }

        public override void Tick()
        {
            base.Tick();
            ticksRemaining--;

            if (debuffKind == "Decay" && periodicDamage > 0f && pawn != null && pawn.IsHashIntervalTick(60))
            {
                RebornCombatContext.ActiveSpellStage = attackStage;
                try
                {
                    pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, periodicDamage, 1.0f));
                }
                finally
                {
                    RebornCombatContext.ActiveSpellStage = null;
                }
            }

            if (ticksRemaining <= 0)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref debuffKind, "debuffKind", "TimeSlow");
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 600);
            Scribe_Values.Look(ref effectMultiplier, "effectMultiplier", 1.0f);
            Scribe_Values.Look(ref periodicDamage, "periodicDamage", 0f);
            Scribe_Values.Look(ref attackStage, "attackStage", SoulClassStage.Mortal);
        }
    }
}
