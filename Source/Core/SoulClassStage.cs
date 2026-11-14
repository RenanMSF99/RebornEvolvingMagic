using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    public enum SoulClassStage
    {
        Mortal = 0,
        Earth = 1,
        Sky = 2,
        Master = 3,
        Grandmaster = 4,
        Lord = 5,
        King = 6,
        Angelic = 7,
        Ascendancy = 8
    }

    public enum CultivatorAttributeType
    {
        Strength,
        Constitution,
        Agility,
        Senses,
        Conductivity
    }

    public struct SoulStageConfig
    {
        public SoulClassStage Stage;
        public string LabelPT;
        public string LabelEN;
        public string Description;
        public float MaxEnergy;
        public float PassiveRegenPerHour;
        public float BaseXP;
        public Color ThemeColor;

        public SoulStageConfig(SoulClassStage stage, string labelPT, string labelEN, string description, float maxEnergy, float passiveRegenPerHour, float baseXP, Color themeColor)
        {
            Stage = stage;
            LabelPT = labelPT;
            LabelEN = labelEN;
            Description = description;
            MaxEnergy = maxEnergy;
            PassiveRegenPerHour = passiveRegenPerHour;
            BaseXP = baseXP;
            ThemeColor = themeColor;
        }
    }

    public static class SoulStageDatabase
    {
        public static readonly SoulStageConfig[] Stages = new SoulStageConfig[]
        {
            new SoulStageConfig(
                SoulClassStage.Mortal,
                "Classe Mortal",
                "Mortal Class",
                "A alma recém-desperta nos 30.000 Mundos. Foco no cultivo, meditação, fortalecimento dos 5 atributos e expansão da Matriz da Alma.",
                100f,
                15f,
                100f,
                new Color(0.70f, 0.85f, 0.95f)
            ),
            new SoulStageConfig(
                SoulClassStage.Earth,
                "Classe Terra",
                "Earth Class",
                "O primeiro estágio sobre-humano. A alma densificada permite a manifestação dos primeiros feitiços elementais e o fortalecimento físico das Artes Marciais.",
                250f,
                40f,
                250f,
                new Color(0.60f, 0.85f, 0.45f)
            ),
            new SoulStageConfig(
                SoulClassStage.Sky,
                "Classe Céu",
                "Sky Class",
                "O cultivador atinge a densidade da alma capaz de desafiar a gravidade das Leis do Mundo. Libera feitiços celestiais avançados, movimentação aérea de longo alcance e uma reserva colossal de Energia da Alma.",
                500f,
                75f,
                500f,
                new Color(0.25f, 0.75f, 1.0f)
            ),
            new SoulStageConfig(
                SoulClassStage.Master,
                "Classe Mestre",
                "Master Class",
                "A alma entra em ressonância refinada com o éter cósmico. Técnicas evoluídas alcançam potência devastadora e começam a suprimir completamente ataques de reinos inferiores.",
                1000f,
                150f,
                1000f,
                new Color(0.75f, 0.40f, 1.0f)
            ),
            new SoulStageConfig(
                SoulClassStage.Grandmaster,
                "Classe Grão-Mestre",
                "Grandmaster Class",
                "O ápice mortal antes da compreensão das Leis. Um Grão-Mestre comanda torrentes abissais de energia e manifesta técnicas de escala cataclísmica.",
                2500f,
                350f,
                2500f,
                new Color(1.0f, 0.82f, 0.20f)
            ),
            new SoulStageConfig(
                SoulClassStage.Lord,
                "Classe Lorde",
                "Lord Class",
                "Início da assimilação e compreensão das Leis Universais.",
                3500f,
                500f,
                3500f,
                new Color(1.0f, 0.55f, 0.20f)
            ),
            new SoulStageConfig(
                SoulClassStage.King,
                "Classe Rei",
                "King Class",
                "Domínio completo sobre ao menos uma Lei Fundamental do universo.",
                5000f,
                750f,
                5000f,
                new Color(1.0f, 0.30f, 0.35f)
            ),
            new SoulStageConfig(
                SoulClassStage.Angelic,
                "Classe Angélica",
                "Angelic Class",
                "Existência transcendente que dobra as leis do espaço, vida e morte.",
                6000f,
                1000f,
                6000f,
                new Color(0.95f, 0.95f, 1.0f)
            ),
            new SoulStageConfig(
                SoulClassStage.Ascendancy,
                "Ascendência",
                "Ascendancy",
                "O pináculo supremo acima dos 30.000 Mundos.",
                7500f,
                1500f,
                7500f,
                new Color(1.0f, 0.92f, 0.55f)
            )
        };

        public static SoulStageConfig GetConfig(SoulClassStage stage)
        {
            int idx = (int)stage;
            if (idx >= Stages.Length) idx = Stages.Length - 1;
            return Stages[idx];
        }

        private static float[] cachedBaseXP = null;
        private static float[] cachedTotalXP = null;

        private static void EnsureXPCache()
        {
            if (cachedBaseXP != null) return;
            int count = Enum.GetValues(typeof(SoulClassStage)).Length;
            cachedBaseXP = new float[count];
            cachedTotalXP = new float[count];

            // Nível 1 do Estágio Mortal começa com 100 XP
            cachedBaseXP[0] = 100f;

            for (int s = 0; s < count; s++)
            {
                if (s > 0)
                {
                    // O XP da primeira fase do próximo estágio é a quantidade de XP necessária durante todo o estágio anterior
                    cachedBaseXP[s] = cachedTotalXP[s - 1];
                }
                int maxLvl = (s + 1) * 10;
                float sum = 0f;
                for (int lvl = 1; lvl <= maxLvl; lvl++)
                {
                    sum += (float)Math.Round(cachedBaseXP[s] * Math.Pow(1.10, lvl - 1));
                }
                cachedTotalXP[s] = sum;
            }
        }

        public static float GetBaseXPForStage(SoulClassStage stage)
        {
            EnsureXPCache();
            int idx = (int)stage;
            if (idx >= cachedBaseXP.Length) idx = cachedBaseXP.Length - 1;
            return cachedBaseXP[idx];
        }

        public static float GetTotalXPForStage(SoulClassStage stage)
        {
            EnsureXPCache();
            int idx = (int)stage;
            if (idx >= cachedTotalXP.Length) idx = cachedTotalXP.Length - 1;
            return cachedTotalXP[idx];
        }
    }

    public static class RebornDefOf
    {
        public static HediffDef REM_Hediff_SoulMatrix
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_SoulMatrix"); }
        }

        public static JobDef REM_Job_SoulMeditation
        {
            get { return DefDatabase<JobDef>.GetNamedSilentFail("REM_Job_SoulMeditation"); }
        }

        public static AbilityDef REM_Ability_Meditate
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_Meditate"); }
        }

        public static AbilityDef REM_Ability_AbsorbSoul
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_AbsorbSoul"); }
        }

        public static AbilityDef REM_Ability_SoulBolt
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_SoulBolt"); }
        }

        public static AbilityDef REM_Ability_SkyStep
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_SkyStep"); }
        }

        public static AbilityDef REM_Ability_SkyLance
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_SkyLance"); }
        }

        // Habilidades Novas (Mortal, Terra, Céu)
        public static AbilityDef REM_Ability_WindDash
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_WindDash"); }
        }

        public static AbilityDef REM_Ability_FireArrow
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_FireArrow"); }
        }

        public static AbilityDef REM_Ability_Heal
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_Heal"); }
        }

        public static AbilityDef REM_Ability_Haste
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_Haste"); }
        }

        public static AbilityDef REM_Ability_StoneSkin
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_StoneSkin"); }
        }

        public static AbilityDef REM_Ability_AcceleratedRegen
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_AcceleratedRegen"); }
        }

        public static AbilityDef REM_Ability_FrostRay
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_FrostRay"); }
        }

        public static AbilityDef REM_Ability_PsionicShield
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_PsionicShield"); }
        }

        public static AbilityDef REM_Ability_ChainLightning
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_ChainLightning"); }
        }

        public static AbilityDef REM_Ability_Teleport
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_Teleport"); }
        }

        public static AbilityDef REM_Ability_HeavensBlessing
        {
            get { return DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_HeavensBlessing"); }
        }

        // Hediffs das Técnicas
        public static HediffDef REM_Hediff_StoneSkin
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_StoneSkin"); }
        }

        public static HediffDef REM_Hediff_AcceleratedRegen
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_AcceleratedRegen"); }
        }

        public static HediffDef REM_Hediff_PsionicShield
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_PsionicShield"); }
        }

        public static HediffDef REM_Hediff_Haste
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_Haste"); }
        }

        public static HediffDef REM_Hediff_HeavensBlessing
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_HeavensBlessing"); }
        }

        public static TraitDef REM_Trait_FirstbornSoul
        {
            get { return DefDatabase<TraitDef>.GetNamedSilentFail("REM_Trait_FirstbornSoul"); }
        }

        public static TraitDef REM_Trait_MagicPearlBearer
        {
            get { return DefDatabase<TraitDef>.GetNamedSilentFail("REM_Trait_MagicPearlBearer"); }
        }

        public static HediffDef REM_Hediff_MetamorphicForm
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_MetamorphicForm"); }
        }

        public static ThingDef REM_Item_MagicPearl
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Item_MagicPearl"); }
        }

        // Projéteis de Magia e Cultivo
        public static ThingDef REM_Proj_SoulBolt
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Proj_SoulBolt"); }
        }

        public static ThingDef REM_Proj_FireArrow
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Proj_FireArrow"); }
        }

        public static ThingDef REM_Proj_FrostRay
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Proj_FrostRay"); }
        }

        public static ThingDef REM_Proj_SkyLance
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Proj_SkyLance"); }
        }

        public static ThingDef REM_Proj_ChainLightning
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Proj_ChainLightning"); }
        }

        public static ThingDef REM_Proj_RebornSpell
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Proj_RebornSpell"); }
        }

        public static ThingDef REM_Thing_SpellZone
        {
            get { return DefDatabase<ThingDef>.GetNamedSilentFail("REM_Thing_SpellZone"); }
        }

        // Novos Hediffs do Catálogo Geral
        public static HediffDef REM_Hediff_SoulShield
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_SoulShield"); }
        }

        public static HediffDef REM_Hediff_ManaBarrier
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_ManaBarrier"); }
        }

        public static HediffDef REM_Hediff_AetherShield
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_AetherShield"); }
        }

        public static HediffDef REM_Hediff_CrimsonShield
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_CrimsonShield"); }
        }

        public static HediffDef REM_Hediff_InfernoWall
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_InfernoWall"); }
        }

        public static HediffDef REM_Hediff_MindBarrier
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_MindBarrier"); }
        }

        public static HediffDef REM_Hediff_ShadowCloak
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_ShadowCloak"); }
        }

        public static HediffDef REM_Hediff_VitalSurge
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_VitalSurge"); }
        }

        public static HediffDef REM_Hediff_HealingLight
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_HealingLight"); }
        }

        public static HediffDef REM_Hediff_MartialStance
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_MartialStance"); }
        }

        public static HediffDef REM_Hediff_CultivationDebuff
        {
            get { return DefDatabase<HediffDef>.GetNamedSilentFail("REM_Hediff_CultivationDebuff"); }
        }
    }
}
