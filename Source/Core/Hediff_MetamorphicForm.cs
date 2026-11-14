using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    public class Hediff_MetamorphicForm : HediffWithComps
    {
        public PawnKindDef formKindDef;
        public BloodlineStage formStage = BloodlineStage.Newborn;

        [Unsaved]
        private HediffStage cachedStage = null;

        public override string LabelBase
        {
            get
            {
                if (formKindDef == null) return "Metamorfose da Alma";
                string stageName = "Filhote";
                switch (formStage)
                {
                    case BloodlineStage.Juvenile: stageName = "Jovem"; break;
                    case BloodlineStage.Prime: stageName = "Adulto"; break;
                    case BloodlineStage.Elder: stageName = "Ancião"; break;
                }
                return string.Format("Metamorfose: {0} ({1})", formKindDef.LabelCap, stageName);
            }
        }

        public override string Description
        {
            get
            {
                if (formKindDef == null) return base.Description;
                return string.Format(
                    "O recipiente biológico deste colono foi completamente remodelado através da Pérola Mágica na forma física de um {0}.\n\n• Suas roupas e armas estão preservadas em um vácuo espiritual dimensional.\n• Suas capacidades de combate físico, vida, armadura natural e ataques espelham a besta assimilada.\n• Manipulação fina para bancadas complexas está limitada.",
                    formKindDef.LabelCap
                );
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;
                if (formKindDef == null || formKindDef.race == null) return base.CurStage;

                HediffStage stage = new HediffStage();
                stage.statOffsets = new List<StatModifier>();
                stage.statFactors = new List<StatModifier>();
                stage.capMods = new List<PawnCapacityModifier>();

                float stageDmgMult = 1.0f;
                float stageHealthMult = 1.0f;
                float stageSpeedMult = 1.0f;
                switch (formStage)
                {
                    case BloodlineStage.Newborn:
                        stageDmgMult = 0.60f;
                        stageHealthMult = 0.65f;
                        stageSpeedMult = 0.85f;
                        break;
                    case BloodlineStage.Juvenile:
                        stageDmgMult = 0.85f;
                        stageHealthMult = 0.85f;
                        stageSpeedMult = 0.95f;
                        break;
                    case BloodlineStage.Prime:
                        stageDmgMult = 1.00f;
                        stageHealthMult = 1.00f;
                        stageSpeedMult = 1.05f;
                        break;
                    case BloodlineStage.Elder:
                        stageDmgMult = 1.30f;
                        stageHealthMult = 1.35f;
                        stageSpeedMult = 1.15f;
                        break;
                }

                // 1. Armaduras Naturais da Criatura
                float sharpArmor = formKindDef.race.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, null) * stageHealthMult;
                float bluntArmor = formKindDef.race.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, null) * stageHealthMult;
                float heatArmor = formKindDef.race.GetStatValueAbstract(StatDefOf.ArmorRating_Heat, null) * stageHealthMult;

                float bodySize = (formKindDef.race.race != null) ? formKindDef.race.race.baseBodySize : 1.0f;
                if (bodySize >= 2.0f)
                {
                    sharpArmor = Mathf.Max(sharpArmor, 0.40f * stageHealthMult);
                    bluntArmor = Mathf.Max(bluntArmor, 0.35f * stageHealthMult);
                }

                if (sharpArmor > 0.01f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Sharp, value = sharpArmor });
                if (bluntArmor > 0.01f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Blunt, value = bluntArmor });
                if (heatArmor > 0.01f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Heat, value = heatArmor });

                // 2. Dano Corpo a Corpo Massivo
                float damageFactor = Mathf.Max(1.0f, bodySize * 0.95f) * stageDmgMult;
                stage.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeDamageFactor, value = damageFactor });
                stage.statOffsets.Add(new StatModifier { stat = StatDefOf.MeleeHitChance, value = 4.0f });

                // Redução de impacto para bestas colossais
                if (bodySize >= 3.0f)
                {
                    stage.statFactors.Add(new StatModifier { stat = StatDefOf.IncomingDamageFactor, value = 0.70f });
                }

                // 3. Capacidades Biológicas
                float beastMoveSpeed = formKindDef.race.GetStatValueAbstract(StatDefOf.MoveSpeed, null);
                if (beastMoveSpeed > 0.1f)
                {
                    float speedOffset = ((beastMoveSpeed * stageSpeedMult) - 4.6f) / 4.6f;
                    stage.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Moving, offset = speedOffset });
                }

                stage.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Sight, offset = 0.15f });
                stage.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Hearing, offset = 0.20f });
                stage.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Manipulation, offset = -0.60f });

                cachedStage = stage;
                return cachedStage;
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            cachedStage = null;
            if (pawn != null && pawn.health != null && pawn.health.hediffSet != null)
            {
                Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
                if (matrix != null && matrix.IsMetamorphosed)
                {
                    matrix.RevertToHuman(false);
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            if (pawn == null || !pawn.IsColonistPlayerControlled)
                yield break;

            Command_Action revertCmd = new Command_Action();
            revertCmd.defaultLabel = "Reverter Forma Humana";
            revertCmd.defaultDesc = "Desfaz a remodelação celular da Pérola Mágica, restaurando a anatomia humana e retirando todas as roupas e armas do vácuo espiritual.";
            revertCmd.icon = ContentFinder<Texture2D>.Get("UI/RebornMagic/Gizmo_Breakthrough", true);
            revertCmd.action = delegate
            {
                if (pawn.health != null && pawn.health.hediffSet != null)
                {
                    Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
                    if (matrix != null)
                    {
                        matrix.RevertToHuman(true);
                    }
                }
            };
            yield return revertCmd;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref formKindDef, "formKindDef");
            Scribe_Values.Look(ref formStage, "formStage", BloodlineStage.Newborn);
        }
    }
}
