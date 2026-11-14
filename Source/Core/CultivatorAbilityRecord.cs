using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    public class CultivatorAbilityRecord : IExposable
    {
        public string abilityDefName;
        public int masteryTier = 1; // 1 = Iniciado, 2 = Adepto, 3 = Perito, 4 = Mestre
        public float proficiency = 0f; // 0..100
        public bool isLearned = false;
        public bool isEquipped = false;
        public SoulClassStage currentSpellStage = SoulClassStage.Mortal;

        public CultivatorAbilityRecord()
        {
        }

        public CultivatorAbilityRecord(string defName)
        {
            this.abilityDefName = defName;
            this.masteryTier = 1;
            this.proficiency = 0f;
            this.isLearned = false;
            this.isEquipped = false;
            this.currentSpellStage = SoulClassStage.Mortal;
        }

        public string StageLabelPT
        {
            get
            {
                return SoulStageDatabase.GetConfig(currentSpellStage).LabelPT;
            }
        }

        public string StageShortLabel
        {
            get
            {
                switch (currentSpellStage)
                {
                    case SoulClassStage.Mortal: return "Mortal";
                    case SoulClassStage.Earth: return "Terra";
                    case SoulClassStage.Sky: return "Céu";
                    case SoulClassStage.Master: return "Mestre";
                    case SoulClassStage.Grandmaster: return "Grão-Mestre";
                    default: return currentSpellStage.ToString();
                }
            }
        }

        public string TierLabel
        {
            get
            {
                switch (masteryTier)
                {
                    case 1: return "Iniciado";
                    case 2: return "Adepto";
                    case 3: return "Perito";
                    case 4: return "Mestre";
                    default: return "Iniciado";
                }
            }
        }

        public Color TierColor
        {
            get
            {
                switch (masteryTier)
                {
                    case 1: return new Color(0.75f, 0.75f, 0.75f);
                    case 2: return new Color(0.35f, 0.85f, 0.45f);
                    case 3: return new Color(0.25f, 0.75f, 1.0f);
                    case 4: return new Color(1.0f, 0.82f, 0.20f);
                    default: return Color.white;
                }
            }
        }

        public float CostMultiplier
        {
            get
            {
                switch (masteryTier)
                {
                    case 1: return 1.0f;
                    case 2: return 0.90f;
                    case 3: return 0.80f;
                    case 4: return 0.75f;
                    default: return 1.0f;
                }
            }
        }

        public float PowerMultiplier
        {
            get
            {
                switch (masteryTier)
                {
                    case 1: return 1.0f;
                    case 2: return 1.15f;
                    case 3: return 1.30f;
                    case 4: return 1.50f;
                    default: return 1.0f;
                }
            }
        }

        public void AddProficiency(float amount, Pawn pawn)
        {
            AddProficiency(amount, pawn, currentSpellStage);
        }

        public void AddProficiency(float amount, Pawn pawn, SoulClassStage cultivatorStage)
        {
            if (amount <= 0f || masteryTier >= 4) return;

            int stageDiff = Mathf.Max(0, (int)cultivatorStage - (int)currentSpellStage);
            float catchupMult = 1.0f + (0.50f * (float)stageDiff);

            proficiency += amount * catchupMult;
            while (proficiency >= 100f && masteryTier < 4)
            {
                proficiency -= 100f;
                masteryTier++;
                if (masteryTier >= 4)
                {
                    proficiency = 0f;
                }

                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 1.5f);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("DOMÍNIO: {0}!", TierLabel.ToUpper()), TierColor, 3.0f);
                }

                if (pawn != null && pawn.IsColonistPlayerControlled)
                {
                    AbilityDef aDef = DefDatabase<AbilityDef>.GetNamedSilentFail(abilityDefName);
                    string label = (aDef != null) ? aDef.label : abilityDefName;
                    Messages.Message(string.Format("{0} atingiu o grau [{1}] na técnica '{2}'! O custo foi reduzido e a potência ampliada.", pawn.LabelShort, TierLabel, label), pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        public bool CanEvolveStage(SoulClassStage cultivatorStage)
        {
            if (!isLearned) return false;
            if (currentSpellStage >= SoulClassStage.Grandmaster) return false;
            if ((int)currentSpellStage >= (int)cultivatorStage) return false;
            return masteryTier >= 3;
        }

        public bool TryEvolveStage(Pawn pawn, SoulClassStage cultivatorStage, bool silentMessage)
        {
            if (!CanEvolveStage(cultivatorStage)) return false;

            int prevMastery = masteryTier;
            currentSpellStage = (SoulClassStage)((int)currentSpellStage + 1);
            masteryTier = (prevMastery >= 4) ? 2 : 1;
            proficiency = 0f;

            if (!silentMessage && pawn != null)
            {
                if (pawn.Spawned && pawn.Map != null)
                {
                    Color col = SoulStageDatabase.GetConfig(currentSpellStage).ThemeColor;
                    FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 1.8f);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("EVOLUIU: {0} [{1}]!", abilityDefName, StageShortLabel), col, 3.0f);
                }
                if (pawn.IsColonistPlayerControlled)
                {
                    Messages.Message(string.Format("A técnica '{0}' de {1} evoluiu para o estágio [{2}] (Maestria reiniciada em [{3}]).", abilityDefName, pawn.LabelShort, StageShortLabel, TierLabel), pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
            return true;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref abilityDefName, "abilityDefName");
            Scribe_Values.Look(ref masteryTier, "masteryTier", 1);
            Scribe_Values.Look(ref proficiency, "proficiency", 0f);
            Scribe_Values.Look(ref isLearned, "isLearned", false);
            Scribe_Values.Look(ref isEquipped, "isEquipped", false);
            Scribe_Values.Look(ref currentSpellStage, "currentSpellStage", SoulClassStage.Mortal);
        }
    }
}
