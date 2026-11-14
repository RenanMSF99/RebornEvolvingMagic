using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RebornEvolvingMagic
{
    // 1. Passo do Vento (Wind Dash)
    public class Ability_WindDash : Ability
    {
        public Ability_WindDash() : base() { }
        public Ability_WindDash(Pawn pawn) : base(pawn) { }
        public Ability_WindDash(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_WindDash(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_WindDash(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float staminaCost = 25f * costMult;

            if (!matrix.TryConsumeStamina(staminaCost))
            {
                Messages.Message(string.Format("Vigor físico insuficiente! Necessário {0} SP.", Mathf.CeilToInt(staminaCost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            IntVec3 targetCell = target.Cell;
            if (!targetCell.InBounds(pawn.Map) || !targetCell.Walkable(pawn.Map) || targetCell.DistanceTo(pawn.Position) > 8.9f)
            {
                Messages.Message("Destino inválido para o Passo do Vento (alcance máximo de 8 células).", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            IntVec3 startPos = pawn.Position;
            Vector3 startVec = pawn.DrawPos;
            Map map = pawn.Map;
            bool wasSelected = Find.Selector != null && Find.Selector.IsSelected(pawn);

            RebornVFX.PlayCastRuneCircle(startVec, map, RebornVFX.ColorWind, 1.65f, 180f);
            FleckMaker.ThrowDustPuff(startVec, map, 1.4f);

            ThingDef flyerDef = RebornVFXDefOf.Flyer_WindDash;
            RebornDashFlyer flyer = (flyerDef != null) ? PawnFlyer.MakeFlyer(flyerDef, pawn, targetCell, null, null, false, null, this, target) as RebornDashFlyer : null;
            if (flyer != null)
            {
                flyer.reselectOnLand = wasSelected;
                flyer.trailColor = RebornVFX.ColorWind;
                GenSpawn.Spawn(flyer, startPos, map);
            }
            else
            {
                pawn.Position = targetCell;
                pawn.pather.StopDead();
                pawn.Notify_Teleported(true, false);

                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.7f, RebornVFX.ColorWind);
                MoteMaker.ThrowText(pawn.DrawPos, map, "PASSO DO VENTO!", new Color(0.4f, 0.9f, 0.5f), 2.0f);
            }

            matrix.AddXP(staminaCost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);
            return true;
        }
    }

    // 2. Flecha de Fogo (Fire Arrow)
    public class Ability_FireArrow : Ability
    {
        public Ability_FireArrow() : base() { }
        public Ability_FireArrow(Pawn pawn) : base(pawn) { }
        public Ability_FireArrow(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_FireArrow(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_FireArrow(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;
            float cost = 15f * costMult;

            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Map map = pawn.Map;

            RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorFire, 1.7f, 140f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_FlameWisp, pawn.DrawPos, map, 1.1f, RebornVFX.ColorFire);

            // Disparar Projétil Balístico Nativo da Flecha de Fogo
            ThingDef projDef = RebornDefOf.REM_Proj_FireArrow;
            if (projDef != null)
            {
                Projectile_CultivationSpell proj = GenSpawn.Spawn(projDef, pawn.Position, map) as Projectile_CultivationSpell;
                if (proj != null)
                {
                    proj.abilityDefName = def.defName;
                    proj.baseDamage = 20f;
                    proj.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.All, false, null, null);
                }
            }

            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);
            return true;
        }
    }

    // 3. Curar (Heal)
    public class Ability_Heal : Ability
    {
        public Ability_Heal() : base() { }
        public Ability_Heal(Pawn pawn) : base(pawn) { }
        public Ability_Heal(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_Heal(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_Heal(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            Pawn targetPawn = target.Thing as Pawn;
            if (targetPawn == null) targetPawn = pawn;

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            int tier = (rec != null) ? rec.masteryTier : 1;
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;

            float cost = ((tier >= 4) ? 60f : 25f) * costMult;
            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP para canalizar a Cura.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Map map = pawn.Map;
            if (targetPawn != pawn)
            {
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorHealing, 1.75f, 110f);
            }
            RebornVFX.PlayHealingBloom(targetPawn, RebornVFX.ColorHealing, false);

            float healPool = (40f + (matrix.conductivity * 0.5f)) * pwrMult;
            if (tier >= 2) healPool *= 1.25f;
            if (tier >= 3) healPool *= 1.50f;

            // Curar ferimentos e estancar sangramentos
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            for (int i = 0; i < targetPawn.health.hediffSet.hediffs.Count; i++)
            {
                Hediff_Injury inj = targetPawn.health.hediffSet.hediffs[i] as Hediff_Injury;
                if (inj != null && inj.Severity > 0.01f)
                {
                    injuries.Add(inj);
                }
            }

            for (int i = 0; i < injuries.Count && healPool > 0.01f; i++)
            {
                float healAmt = Mathf.Min(injuries[i].Severity, healPool);
                injuries[i].Heal(healAmt);
                healPool -= healAmt;
            }

            // Grau Mestre (Tier 4): Regenerar membro perdido ou remover cicatriz
            if (tier >= 4)
            {
                Hediff_MissingPart missing = null;
                for (int i = 0; i < targetPawn.health.hediffSet.hediffs.Count; i++)
                {
                    Hediff_MissingPart mp = targetPawn.health.hediffSet.hediffs[i] as Hediff_MissingPart;
                    if (mp != null)
                    {
                        missing = mp;
                        break;
                    }
                }
                if (missing != null)
                {
                    targetPawn.health.RestorePart(missing.Part);
                    RebornVFX.PlayAscensionPillar(targetPawn, RebornVFX.ColorHealing, "MEMBRO REGENERADO!", 1.8f);
                }
            }

            MoteMaker.ThrowText(targetPawn.DrawPos, map, "CURA ESPIRITUAL!", new Color(0.3f, 1f, 0.6f), 2.5f);
            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 6.0f);
            return true;
        }
    }

    // 4. Pressa (Haste)
    public class Ability_Haste : Ability
    {
        public Ability_Haste() : base() { }
        public Ability_Haste(Pawn pawn) : base(pawn) { }
        public Ability_Haste(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_Haste(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_Haste(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float mpCost = 20f * costMult;
            float spCost = 15f * costMult;

            if (matrix.currentEnergy < mpCost || matrix.currentStamina < spCost)
            {
                Messages.Message(string.Format("Recursos insuficientes! Necessário {0} MP e {1} SP.", Mathf.CeilToInt(mpCost), Mathf.CeilToInt(spCost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            matrix.TryConsumeEnergy(mpCost);
            matrix.TryConsumeStamina(spCost);

            Hediff haste = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_Haste, pawn);
            if (haste != null)
            {
                pawn.health.AddHediff(haste);
            }

            RebornVFX.PlayCastRuneCircle(pawn.DrawPos, pawn.Map, RebornVFX.ColorWind, 2.0f, 200f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, pawn.Map, 0.7f, RebornVFX.ColorWind);
            RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, pawn.Map);
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "PRESSA!", new Color(1f, 0.9f, 0.2f), 2.5f);

            matrix.AddXP((mpCost + spCost) * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);
            return true;
        }
    }

    // 5. Pele de Pedra (Stone Skin)
    public class Ability_StoneSkin : Ability
    {
        public Ability_StoneSkin() : base() { }
        public Ability_StoneSkin(Pawn pawn) : base(pawn) { }
        public Ability_StoneSkin(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_StoneSkin(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_StoneSkin(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Earth)
            {
                Messages.Message("Necessário atingir a Classe Terra para manifestar a Pele de Pedra.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float cost = 35f * costMult;

            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Hediff stone = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_StoneSkin, pawn);
            if (stone != null)
            {
                pawn.health.AddHediff(stone);
            }

            RebornVFX.PlayStoneSkinCast(pawn);
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "PELE DE PEDRA!", new Color(0.6f, 0.6f, 0.65f), 2.5f);

            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);
            return true;
        }
    }

    // 6. Regeneração Acelerada (Accelerated Regen)
    public class Ability_AcceleratedRegen : Ability
    {
        public Ability_AcceleratedRegen() : base() { }
        public Ability_AcceleratedRegen(Pawn pawn) : base(pawn) { }
        public Ability_AcceleratedRegen(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_AcceleratedRegen(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_AcceleratedRegen(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Earth)
            {
                Messages.Message("Necessário atingir a Classe Terra para manifestar a Regeneração Acelerada.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float cost = 40f * costMult;

            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Hediff_AcceleratedRegen regen = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_AcceleratedRegen) as Hediff_AcceleratedRegen;
            if (regen != null)
            {
                regen.ticksRemaining = 60000;
            }
            else
            {
                regen = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_AcceleratedRegen, pawn) as Hediff_AcceleratedRegen;
                if (regen != null)
                {
                    regen.ticksRemaining = 60000;
                    pawn.health.AddHediff(regen);
                }
            }

            RebornVFX.PlayHealingBloom(pawn, RebornVFX.ColorHealing, false);
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "REGENERAÇÃO ACELERADA!", new Color(0.3f, 0.9f, 0.6f), 2.5f);

            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);
            return true;
        }
    }

    // 7. Raio de Gelo (Frost Ray)
    public class Ability_FrostRay : Ability
    {
        public Ability_FrostRay() : base() { }
        public Ability_FrostRay(Pawn pawn) : base(pawn) { }
        public Ability_FrostRay(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_FrostRay(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_FrostRay(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Earth)
            {
                Messages.Message("Necessário atingir a Classe Terra para manifestar o Raio de Gelo.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;
            float cost = 30f * costMult;

            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Map map = pawn.Map;

            RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorFrost, 1.8f, -150f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_IceCrystal, pawn.DrawPos, map, 1.15f, RebornVFX.ColorFrost, 0f, 180f);

            // Disparar Projétil Balístico Nativo do Raio de Gelo
            ThingDef projDef = RebornDefOf.REM_Proj_FrostRay;
            if (projDef != null)
            {
                Projectile_CultivationSpell proj = GenSpawn.Spawn(projDef, pawn.Position, map) as Projectile_CultivationSpell;
                if (proj != null)
                {
                    proj.abilityDefName = def.defName;
                    proj.baseDamage = 30f;
                    proj.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.All, false, null, null);
                }
            }

            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);
            return true;
        }
    }

    // 8. Escudo Psiônico (Psionic Shield)
    public class Ability_PsionicShield : Ability
    {
        public Ability_PsionicShield() : base() { }
        public Ability_PsionicShield(Pawn pawn) : base(pawn) { }
        public Ability_PsionicShield(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_PsionicShield(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_PsionicShield(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Earth)
            {
                Messages.Message("Necessário atingir a Classe Terra para manifestar o Escudo Psiônico.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;
            float cost = 45f * costMult;

            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Hediff_PsionicShield shield = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_PsionicShield) as Hediff_PsionicShield;
            float hp = (150f + (matrix.conductivity * 2f)) * pwrMult;

            if (shield == null)
            {
                shield = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_PsionicShield, pawn) as Hediff_PsionicShield;
                shield.shieldHP = hp;
                shield.maxShieldHP = hp;
                pawn.health.AddHediff(shield);
            }
            else
            {
                shield.shieldHP = hp;
                shield.maxShieldHP = hp;
            }

            RebornVFX.PlayCastRuneCircle(pawn.DrawPos, pawn.Map, RebornVFX.ColorSoul, 2.15f, 160f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, pawn.Map, 0.85f, RebornVFX.ColorSoul);
            RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, pawn.Map);
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ESCUDO PSIÔNICO ATIVO!", new Color(0.2f, 0.75f, 1f), 3.0f);

            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);
            return true;
        }
    }

    // 9. Arco Voltaico (Chain Lightning)
    public class Ability_ChainLightning : Ability
    {
        public Ability_ChainLightning() : base() { }
        public Ability_ChainLightning(Pawn pawn) : base(pawn) { }
        public Ability_ChainLightning(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_ChainLightning(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_ChainLightning(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Sky)
            {
                Messages.Message("Necessário atingir a Classe Céu para manifestar o Arco Voltaico.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;
            float cost = 50f * costMult;

            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Map map = pawn.Map;

            RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorLightning, 2.0f, 220f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.75f, Color.white);

            // Disparar Projétil Balístico Nativo do Arco Voltaico (com ricochete em cadeia no impacto)
            ThingDef projDef = RebornDefOf.REM_Proj_ChainLightning;
            if (projDef != null)
            {
                Projectile_CultivationSpell proj = GenSpawn.Spawn(projDef, pawn.Position, map) as Projectile_CultivationSpell;
                if (proj != null)
                {
                    proj.abilityDefName = def.defName;
                    proj.baseDamage = 45f;
                    proj.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.All, false, null, null);
                }
            }

            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 6.0f);
            return true;
        }
    }

    // 10. Teleporte (Teleport)
    public class Ability_Teleport : Ability
    {
        public Ability_Teleport() : base() { }
        public Ability_Teleport(Pawn pawn) : base(pawn) { }
        public Ability_Teleport(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_Teleport(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_Teleport(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Sky)
            {
                Messages.Message("Necessário atingir a Classe Céu para manifestar o Teleporte.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float cost = 40f * costMult;

            if (!matrix.TryConsumeEnergy(cost))
            {
                Messages.Message(string.Format("Energia insuficiente! Necessário {0} MP.", Mathf.CeilToInt(cost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            int tier = (rec != null) ? rec.masteryTier : 1;

            float maxRange = 20f;
            if (tier == 2) maxRange = 35f;
            else if (tier == 3) maxRange = 50f;
            else if (tier >= 4) maxRange = 9999f; // Mapa inteiro!

            IntVec3 targetCell = target.Cell;
            if (!targetCell.InBounds(pawn.Map) || !targetCell.Walkable(pawn.Map))
            {
                Messages.Message("Destino inválido para o Teleporte.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            if (pawn.Position.DistanceTo(targetCell) > maxRange)
            {
                Messages.Message(string.Format("Destino além do alcance atual de teleporte ({0} células). Aumente a Maestria para expandir o alcance.", Mathf.RoundToInt(maxRange)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Map map = pawn.Map;
            Vector3 startPos = pawn.DrawPos;

            pawn.Position = targetCell;
            pawn.pather.StopDead();
            pawn.Notify_Teleported(true, false);

            Vector3 endPos = pawn.DrawPos;
            RebornVFX.PlayPortalWarp(startPos, endPos, map, RebornVFX.ColorPortal, 2.5f);
            MoteMaker.ThrowText(endPos, map, "TELEPORTE!", new Color(0.3f, 0.75f, 1f), 3.0f);

            matrix.AddXP(cost * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 6.0f);
            return true;
        }
    }

    // 11. Benção dos Céus (Blessing of the Heavens)
    public class Ability_HeavensBlessing : Ability
    {
        public Ability_HeavensBlessing() : base() { }
        public Ability_HeavensBlessing(Pawn pawn) : base(pawn) { }
        public Ability_HeavensBlessing(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_HeavensBlessing(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_HeavensBlessing(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Sky)
            {
                Messages.Message("Necessário atingir a Classe Céu para manifestar a Benção dos Céus.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(def.defName);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;

            float mpCost = 80f * costMult;
            float spCost = 40f * costMult;

            if (matrix.currentEnergy < mpCost || matrix.currentStamina < spCost)
            {
                Messages.Message(string.Format("Recursos insuficientes! Necessário {0} MP e {1} SP.", Mathf.CeilToInt(mpCost), Mathf.CeilToInt(spCost)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            matrix.TryConsumeEnergy(mpCost);
            matrix.TryConsumeStamina(spCost);

            // 1. Escudo Psiônico de 750 HP (ampliado por pwrMult)
            Hediff_PsionicShield shield = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_PsionicShield) as Hediff_PsionicShield;
            float hp = 750f * pwrMult;

            if (shield == null)
            {
                shield = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_PsionicShield, pawn) as Hediff_PsionicShield;
                shield.shieldHP = hp;
                shield.maxShieldHP = hp;
                pawn.health.AddHediff(shield);
            }
            else
            {
                shield.shieldHP = hp;
                shield.maxShieldHP = hp;
            }

            // 2. Buff Divino Quíntuplo (Pele de Pedra 5x sem perda de velocidade + Regeneração 5x)
            Hediff blessing = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_HeavensBlessing, pawn);
            if (blessing != null)
            {
                pawn.health.AddHediff(blessing);
            }

            Map map = pawn.Map;
            RebornVFX.PlayAscensionPillar(pawn, RebornVFX.ColorSkyGold, "BÊNÇÃO DOS CÉUS!", 2.5f);

            matrix.AddXP((mpCost + spCost) * 0.5f, true);
            matrix.AddAbilityProficiency(def.defName, 8.0f);
            return true;
        }
    }
}
