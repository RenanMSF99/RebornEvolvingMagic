using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RebornEvolvingMagic
{
    // 1. JobDriver para a Meditação Contínua
    public class JobDriver_SoulMeditation : JobDriver
    {
        private const int GainIntervalTicks = 60;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil meditate = new Toil();
            meditate.defaultCompleteMode = ToilCompleteMode.Never;
            meditate.initAction = delegate
            {
                pawn.pather.StopDead();
            };

            meditate.tickAction = delegate
            {
                if (pawn.IsHashIntervalTick(GainIntervalTicks))
                {
                    if (pawn.health != null && pawn.health.hediffSet != null)
                    {
                        Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
                        if (matrix != null)
                        {
                            // A regeneração de MP da meditação (4x) ocorre em Hediff_SoulMatrix.PostTick; aqui aplicamos +100 XP/h
                            float xpPerSecond = (100f / 2500f) * (float)GainIntervalTicks;
                            matrix.AddXP(xpPerSecond, false);
                        }
                    }

                    if (pawn.Spawned && pawn.Map != null)
                    {
                        Vector3 mPos = pawn.DrawPos;
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, mPos, pawn.Map, 1.45f, RebornVFX.ColorSoul, Rand.Range(0f, 360f), 75f);
                        for (int i = 0; i < 3; i++)
                        {
                            Vector3 off = new Vector3(Rand.Range(-0.55f, 0.55f), 0f, Rand.Range(-0.35f, 0.35f));
                            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_SoftSpark, mPos + off, pawn.Map, Rand.Range(0.4f, 0.7f), Color.Lerp(RebornVFX.ColorSoul, Color.white, 0.4f), Rand.Range(0f, 360f), 160f, Rand.Range(-15f, 15f), Rand.Range(0.8f, 1.5f));
                        }
                    }
                }
            };

            yield return meditate;
        }
    }

    // 2. Habilidade de Ativar Meditação
    public class Ability_SoulMeditation : Ability
    {
        public Ability_SoulMeditation() : base() { }
        public Ability_SoulMeditation(Pawn pawn) : base(pawn) { }
        public Ability_SoulMeditation(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_SoulMeditation(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_SoulMeditation(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead) return false;

            Job job = JobMaker.MakeJob(RebornDefOf.REM_Job_SoulMeditation);
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            return true;
        }
    }

    // 3. Habilidade de Absorver Matriz da Alma de Cadáveres
    public class Ability_AbsorbSoulMatrix : Ability
    {
        public Ability_AbsorbSoulMatrix() : base() { }
        public Ability_AbsorbSoulMatrix(Pawn pawn) : base(pawn) { }
        public Ability_AbsorbSoulMatrix(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_AbsorbSoulMatrix(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_AbsorbSoulMatrix(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null) return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            if (!matrix.isMagicPearlBearer)
            {
                Messages.Message("Apenas o Portador da Pérola Mágica pode quebrar as leis biológicas e absorver resíduos de almas de cadáveres.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            Corpse corpse = target.Thing as Corpse;
            if (corpse == null && target.Cell.InBounds(pawn.Map))
            {
                List<Thing> list = target.Cell.GetThingList(pawn.Map);
                for (int i = 0; i < list.Count; i++)
                {
                    Corpse c = list[i] as Corpse;
                    if (c != null)
                    {
                        corpse = c;
                        break;
                    }
                }
            }

            if (corpse == null || corpse.Destroyed)
            {
                Messages.Message("Nenhum cadáver válido no local para absorção espiritual.", pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            float bodyFactor = (corpse.InnerPawn != null) ? Mathf.Clamp(corpse.InnerPawn.BodySize, 0.5f, 4.0f) : 1.0f;
            float energyGain = 40f * bodyFactor;
            float xpGain = 35f * bodyFactor;

            matrix.RestoreEnergy(energyGain, true);
            matrix.AddXP(xpGain, true);
            matrix.totalSoulsAbsorbed++;

            if (matrix.isMagicPearlBearer && corpse.InnerPawn != null && corpse.InnerPawn.kindDef != null)
            {
                matrix.RegisterOrFeedBloodline(corpse.InnerPawn.kindDef, 0.35f, true);

                if (matrix.IsMetamorphosed && matrix.activeForm != null && matrix.activeForm.kindDef != corpse.InnerPawn.kindDef)
                {
                    matrix.activeForm.AddProgress(0.10f, pawn);
                }
            }

            RebornVFX.PlaySoulSiphon(corpse.DrawPos, pawn, RebornVFX.ColorSoul);

            corpse.Destroy(DestroyMode.Vanish);
            return true;
        }
    }

    // 4. Habilidade de Ataque Espiritual (Disparo da Alma / Soul Bolt)
    public class Ability_SoulBolt : Ability
    {
        public Ability_SoulBolt() : base() { }
        public Ability_SoulBolt(Pawn pawn) : base(pawn) { }
        public Ability_SoulBolt(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_SoulBolt(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_SoulBolt(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            float energyCost = 20f;
            if (!matrix.TryConsumeEnergy(energyCost))
            {
                Messages.Message(string.Format("Energia espiritual insuficiente! Necessário {0} de Energia.", energyCost), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            // Efeitos visuais no conjurador
            RebornVFX.PlayCastRuneCircle(pawn.DrawPos, pawn.Map, RebornVFX.ColorSoul, 1.55f, 120f);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, pawn.Map, 1.1f, Color.Lerp(RebornVFX.ColorSoul, Color.white, 0.4f));

            // Disparar Projétil Balístico Nativo do Disparo da Alma
            ThingDef projDef = RebornDefOf.REM_Proj_SoulBolt;
            if (projDef != null)
            {
                Projectile_CultivationSpell proj = GenSpawn.Spawn(projDef, pawn.Position, pawn.Map) as Projectile_CultivationSpell;
                if (proj != null)
                {
                    proj.abilityDefName = def.defName;
                    proj.baseDamage = 25f;
                    proj.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.All, false, null, null);
                }
            }

            // Concede compressão de XP equivalente à energia gasta (50 MP rendem 25 XP)
            float xpGain = energyCost * 0.5f;
            matrix.AddXP(xpGain, true);
            matrix.AddAbilityProficiency(def.defName, 5.0f);

            return true;
        }
    }
}
