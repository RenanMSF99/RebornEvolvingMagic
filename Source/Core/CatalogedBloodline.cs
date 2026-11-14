using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    public enum BloodlineStage
    {
        Newborn = 0,
        Juvenile = 1,
        Prime = 2,
        Elder = 3
    }

    public class CatalogedBloodline : IExposable
    {
        public PawnKindDef kindDef;
        public BloodlineStage stage = BloodlineStage.Newborn;
        public float stageProgress = 0f;
        public int corpsesAbsorbed = 0;

        [Unsaved]
        private Graphic cachedGraphic = null;

        public CatalogedBloodline()
        {
        }

        public CatalogedBloodline(PawnKindDef kind)
        {
            kindDef = kind;
            stage = BloodlineStage.Newborn;
            stageProgress = 0f;
            corpsesAbsorbed = 1;
        }

        public string Label
        {
            get
            {
                if (kindDef == null) return "Desconhecido";
                return kindDef.LabelCap;
            }
        }

        public string StageLabelPT
        {
            get
            {
                switch (stage)
                {
                    case BloodlineStage.Newborn: return "Filhote";
                    case BloodlineStage.Juvenile: return "Jovem";
                    case BloodlineStage.Prime: return "Adulto / Pico";
                    case BloodlineStage.Elder: return "Ancião";
                    default: return "Desconhecido";
                }
            }
        }

        public Color StageColor
        {
            get
            {
                switch (stage)
                {
                    case BloodlineStage.Newborn: return new Color(0.65f, 0.85f, 1f);
                    case BloodlineStage.Juvenile: return new Color(0.45f, 0.90f, 0.45f);
                    case BloodlineStage.Prime: return new Color(1f, 0.85f, 0.25f);
                    case BloodlineStage.Elder: return new Color(1f, 0.45f, 0.25f);
                    default: return Color.white;
                }
            }
        }

        public float BodySizeMultiplier
        {
            get
            {
                switch (stage)
                {
                    case BloodlineStage.Newborn: return 0.55f;
                    case BloodlineStage.Juvenile: return 0.80f;
                    case BloodlineStage.Prime: return 1.00f;
                    case BloodlineStage.Elder: return 1.25f;
                    default: return 1.0f;
                }
            }
        }

        public float DamageMultiplier
        {
            get
            {
                switch (stage)
                {
                    case BloodlineStage.Newborn: return 0.60f;
                    case BloodlineStage.Juvenile: return 0.85f;
                    case BloodlineStage.Prime: return 1.00f;
                    case BloodlineStage.Elder: return 1.30f;
                    default: return 1.0f;
                }
            }
        }

        public float HealthMultiplier
        {
            get
            {
                switch (stage)
                {
                    case BloodlineStage.Newborn: return 0.65f;
                    case BloodlineStage.Juvenile: return 0.85f;
                    case BloodlineStage.Prime: return 1.00f;
                    case BloodlineStage.Elder: return 1.35f;
                    default: return 1.0f;
                }
            }
        }

        public float SpeedMultiplier
        {
            get
            {
                switch (stage)
                {
                    case BloodlineStage.Newborn: return 0.85f;
                    case BloodlineStage.Juvenile: return 0.95f;
                    case BloodlineStage.Prime: return 1.05f;
                    case BloodlineStage.Elder: return 1.15f;
                    default: return 1.0f;
                }
            }
        }

        public Graphic GetGraphic()
        {
            if (cachedGraphic != null) return cachedGraphic;
            if (kindDef == null) return null;

            Graphic baseGraphic = null;
            if (kindDef.lifeStages != null && kindDef.lifeStages.Count > 0)
            {
                PawnKindLifeStage pkls = kindDef.lifeStages[kindDef.lifeStages.Count - 1];
                if (pkls.bodyGraphicData != null)
                {
                    baseGraphic = pkls.bodyGraphicData.Graphic;
                }
            }

            if (baseGraphic == null && kindDef.race != null && kindDef.race.graphicData != null)
            {
                baseGraphic = kindDef.race.graphicData.Graphic;
            }

            cachedGraphic = baseGraphic;
            return cachedGraphic;
        }

        public void AddProgress(float amount, Pawn pawn)
        {
            if (amount <= 0f) return;

            if (stage >= BloodlineStage.Elder)
            {
                stageProgress = 1.0f;
                return;
            }

            stageProgress += amount;
            if (stageProgress >= 1.0f)
            {
                stageProgress = 0f;
                stage = (BloodlineStage)((int)stage + 1);

                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastSkipInnerExit, 2.2f);
                    FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 1.8f);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("SURTO DE MATURAÇÃO: {0} ({1})!", Label, StageLabelPT), StageColor, 3.5f);
                }

                if (pawn != null && pawn.IsColonistPlayerControlled)
                {
                    Messages.Message(
                        string.Format("★ A Pérola Mágica remodelou e amadureceu a linhagem de {0} para o estágio [{1}]!", Label, StageLabelPT),
                        pawn,
                        MessageTypeDefOf.PositiveEvent
                    );
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref kindDef, "kindDef");
            Scribe_Values.Look(ref stage, "stage", BloodlineStage.Newborn);
            Scribe_Values.Look(ref stageProgress, "stageProgress", 0f);
            Scribe_Values.Look(ref corpsesAbsorbed, "corpsesAbsorbed", 1);
        }
    }
}
