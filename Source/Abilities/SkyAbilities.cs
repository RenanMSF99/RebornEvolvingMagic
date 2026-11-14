using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    // 1. Habilidade da Classe Céu: Passo Celestial (Sky Step)
    public class Ability_SkyStep : Ability
    {
        public Ability_SkyStep() : base() { }
        public Ability_SkyStep(Pawn pawn) : base(pawn) { }
        public Ability_SkyStep(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_SkyStep(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_SkyStep(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Sky)
            {
                Messages.Message("Necessário atingir a Classe Céu para manifestar o Passo Celestial.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            float energyCost = 30f;
            if (!matrix.TryConsumeEnergy(energyCost))
            {
                Messages.Message(string.Format("Energia espiritual insuficiente! Necessário {0} de Energia.", energyCost), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            IntVec3 targetCell = target.Cell;
            if (!targetCell.InBounds(pawn.Map) || !targetCell.Walkable(pawn.Map))
            {
                Messages.Message("Destino inválido para aterrissagem.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            IntVec3 startPos = pawn.Position;
            Vector3 startVec = pawn.DrawPos;
            Map map = pawn.Map;
            bool wasSelected = Find.Selector != null && Find.Selector.IsSelected(pawn);

            // Efeitos de partida celestial
            RebornVFX.PlayCastRuneCircle(startVec, map, RebornVFX.ColorSoul, 2.2f, 160f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, startVec, map, 0.85f, RebornVFX.ColorSkyGold);
            FleckMaker.Static(startPos, map, FleckDefOf.PsycastSkipInnerExit, 1.8f);
            RebornVFX.PlaySound(SoundDefOf.Psycast_Skip_Entry, startPos, map);

            // Transposição física aérea via PawnFlyer
            ThingDef flyerDef = RebornVFXDefOf.Flyer_SkyStep;
            RebornSkyStepFlyer flyer = (flyerDef != null) ? PawnFlyer.MakeFlyer(flyerDef, pawn, targetCell, null, null, false, null, this, target) as RebornSkyStepFlyer : null;
            if (flyer != null)
            {
                flyer.reselectOnLand = wasSelected;
                flyer.trailColor = RebornVFX.ColorSoul;
                GenSpawn.Spawn(flyer, startPos, map);
            }
            else
            {
                pawn.Position = targetCell;
                pawn.pather.StopDead();
                pawn.Notify_Teleported(true, false);

                Vector3 endVec = pawn.DrawPos;
                RebornVFX.PlayCastRuneCircle(endVec, map, RebornVFX.ColorSoul, 2.3f, 150f);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, endVec, map, 1.0f, RebornVFX.ColorSkyGold);
                MoteMaker.ThrowText(endVec, map, "PASSO CELESTIAL!", new Color(0.25f, 0.85f, 1.0f), 2.5f);
            }

            // Concede compressão de XP equivalente à energia gasta (50 MP rendem 25 XP)
            float xpGain = energyCost * 0.5f;
            matrix.AddXP(xpGain, true);
            return true;
        }
    }

    // 2. Habilidade da Classe Céu: Lança Celestial (Sky Lance)
    public class Ability_SkyLance : Ability
    {
        public Ability_SkyLance() : base() { }
        public Ability_SkyLance(Pawn pawn) : base(pawn) { }
        public Ability_SkyLance(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_SkyLance(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_SkyLance(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null || matrix.currentStage < SoulClassStage.Sky)
            {
                Messages.Message("Necessário atingir a Classe Céu para manifestar a Lança Celestial.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            float energyCost = 35f;
            if (!matrix.TryConsumeEnergy(energyCost))
            {
                Messages.Message(string.Format("Energia espiritual insuficiente! Necessário {0} de Energia.", energyCost), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Map map = pawn.Map;

            // Arranjo rúnico e clarão dourado no conjurador
            RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorSkyGold, 2.0f, 180f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, map, 1.5f, RebornVFX.ColorSkyGold);
            RebornVFX.PlaySound(SoundDefOf.Psycast_Skip_Entry, pawn.Position, map);

            // Disparar Projétil Balístico Nativo da Lança Celestial
            ThingDef projDef = RebornDefOf.REM_Proj_SkyLance;
            if (projDef != null)
            {
                Projectile_CultivationSpell proj = GenSpawn.Spawn(projDef, pawn.Position, map) as Projectile_CultivationSpell;
                if (proj != null)
                {
                    proj.abilityDefName = def.defName;
                    proj.baseDamage = 50f;
                    proj.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.All, false, null, null);
                }
            }

            // Concede compressão de XP equivalente à energia gasta (50 MP rendem 25 XP)
            float xpGain = energyCost * 0.5f;
            matrix.AddXP(xpGain, true);
            matrix.AddAbilityProficiency(def.defName, 6.0f);
            return true;
        }
    }
}
