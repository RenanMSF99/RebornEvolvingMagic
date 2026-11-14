using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    public class Hediff_SoulMatrix : HediffWithComps
    {
        public SoulClassStage currentStage = SoulClassStage.Mortal;
        public float currentEnergy = 100f;
        public int manaRegenCooldownTicks = 0;
        public float currentXP = 0f;
        public int totalSoulsAbsorbed = 0;
        private bool initialized = false;

        // Atributos Primários de Cultivo
        public int strength = 5;
        public int constitution = 5;
        public int agility = 5;
        public int senses = 5;
        public int conductivity = 5;
        public int unallocatedPoints = 0;
        public int skillPoints = 2;
        public int stageLevel = 1;
        public bool attributesInitialized = false;

        // Vigor / Estamina Física
        public float currentStamina = 100f;

        // Propriedades do Portador da Pérola Mágica
        public bool isMagicPearlBearer = false;
        public List<CatalogedBloodline> catalogedBloodlines = new List<CatalogedBloodline>();
        public CatalogedBloodline activeForm = null;
        public List<Apparel> spiritualApparel = new List<Apparel>();
        public ThingWithComps spiritualWeapon = null;
        private string savedActiveKindDefName = null;

        // Registro de Maestria e Habilidades do Cultivador
        public List<CultivatorAbilityRecord> abilityRecords = new List<CultivatorAbilityRecord>();
        public string meditationFocusAbility = null;

        private HediffStage cachedStage = null;

        public bool IsMetamorphosed
        {
            get { return activeForm != null; }
        }

        public SoulStageConfig StageConfig
        {
            get { return SoulStageDatabase.GetConfig(currentStage); }
        }

        public int MaxLevelForStage
        {
            get { return ((int)currentStage + 1) * 10; }
        }

        public float RequiredXPForCurrentLevel
        {
            get
            {
                float baseXP = SoulStageDatabase.GetBaseXPForStage(currentStage);
                return (float)Math.Round(baseXP * Math.Pow(1.10, stageLevel - 1));
            }
        }

        public static float CalculateConductivityEnergyMultiplier(int cnd)
        {
            int c = Mathf.Max(0, cnd);
            int expPart = Mathf.Min(c, 50);
            int linPart = Mathf.Max(0, c - 50);
            return Mathf.Pow(1.03f, (float)expPart) * (1.0f + (0.005f * (float)linPart));
        }

        public float MaxEnergy
        {
            get
            {
                return StageConfig.MaxEnergy * CalculateConductivityEnergyMultiplier(conductivity);
            }
        }

        public float PassiveRegenPercentPerHour
        {
            get { return 0.15f * (1.0f + (conductivity * 0.008f) + (senses * 0.002f)); }
        }

        public float PassiveRegenPerHour
        {
            get { return MaxEnergy * PassiveRegenPercentPerHour; }
        }

        public float CurrentEffectiveRegenPerHour
        {
            get
            {
                if (manaRegenCooldownTicks > 0) return 0f;
                if (pawn != null && pawn.CurJobDef != null)
                {
                    if (pawn.CurJobDef == RebornDefOf.REM_Job_SoulMeditation) return PassiveRegenPerHour * 4.0f;
                    if (pawn.CurJobDef == JobDefOf.LayDown) return PassiveRegenPerHour * 2.0f;
                }
                if (pawn != null && pawn.Drafted) return PassiveRegenPerHour * 0.5f;
                return PassiveRegenPerHour;
            }
        }

        public float MaxStamina
        {
            get { return 100f + (constitution * 2.5f) + (strength * 1.5f); }
        }

        public float StaminaRegenPerSecond
        {
            get { return 5.0f * (1.0f + (constitution * 0.015f)); }
        }

        public float CultivationHealthScaleMultiplier
        {
            get { return 1.0f + (constitution * 0.010f); }
        }

        public float MeleeDamageFactor
        {
            get { return 1.0f + (strength * 0.015f); }
        }

        public float SpellDamageMultiplier
        {
            get { return 1.0f + (conductivity * 0.015f); }
        }

        public float CritChance
        {
            get { return 0.60f * ((float)senses / (senses + 250f)); }
        }

        public float CritDamageMultiplier
        {
            get { return 1.5f + (1.5f * ((float)senses / (senses + 250f))); }
        }

        public float StunChance
        {
            get { return 0.50f * ((float)strength / (strength + 200f)); }
        }

        public float ProjectileDodgeChance
        {
            get { return 0.50f * ((float)agility / (agility + 220f)); }
        }

        public float DamageMitigationFactor
        {
            get { return Mathf.Max(0.30f, 1.0f - (0.70f * ((float)constitution / (constitution + 180f)))); }
        }

        public bool CanBreakthrough
        {
            get { return currentStage < SoulClassStage.Grandmaster && stageLevel >= MaxLevelForStage && currentXP >= RequiredXPForCurrentLevel; }
        }

        public override string LabelInBrackets
        {
            get
            {
                if (IsMetamorphosed)
                {
                    return string.Format("{0} Nv.{1} [{2}] | {3}/{4} MP", StageConfig.LabelPT, stageLevel, activeForm.Label, Mathf.RoundToInt(currentEnergy), Mathf.RoundToInt(MaxEnergy));
                }
                return string.Format("{0} Nv.{1} | {2}/{3} MP", StageConfig.LabelPT, stageLevel, Mathf.RoundToInt(currentEnergy), Mathf.RoundToInt(MaxEnergy));
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage != null) return cachedStage;

                HediffStage stage = new HediffStage();
                stage.statOffsets = new List<StatModifier>();
                stage.statFactors = new List<StatModifier>();
                stage.capMods = new List<PawnCapacityModifier>();

                // 1. Modificadores de Estatísticas (Stat Offsets)
                // Agilidade sem teto para velocidade de movimento (+0.06 c/s linear contínuo por ponto)
                float moveSpeed = agility * 0.06f;
                if (moveSpeed > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.MoveSpeed, value = moveSpeed });

                float dodge = 0.75f * ((float)agility / (agility + 200f));
                if (dodge > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.MeleeDodgeChance, value = dodge });

                float carrying = strength * 3.0f;
                if (carrying > 0.1f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.CarryingCapacity, value = carrying });

                float shootAcc = senses * 0.004f;
                if (shootAcc > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ShootingAccuracyPawn, value = shootAcc });

                float armorSharp = 0.40f * ((float)strength / (strength + 200f));
                if (armorSharp > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Sharp, value = armorSharp });

                float armorBlunt = 0.40f * ((float)strength / (strength + 200f));
                if (armorBlunt > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ArmorRating_Blunt, value = armorBlunt });

                // Amplitude Térmica por Constituição (-0.5C frio e +0.5C calor por ponto)
                float tempOffset = constitution * 0.5f;
                if (tempOffset > 0.01f)
                {
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ComfyTemperatureMin, value = -tempOffset });
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ComfyTemperatureMax, value = tempOffset });
                }

                // Imunidades e Tolerância Biológica por Constituição
                float immunity = constitution * 0.010f;
                if (immunity > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ImmunityGainSpeed, value = immunity });

                float toxicRes = Mathf.Min(1.0f, constitution * 0.008f);
                if (toxicRes > 0.001f)
                {
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ToxicResistance, value = toxicRes });
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ToxicEnvironmentResistance, value = toxicRes });
                }

                float painTolerance = Mathf.Min(0.95f, constitution * 0.004f);
                if (painTolerance > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.PainShockThreshold, value = painTolerance });

                // Tarefas de Esforço Bruto por Força
                float mining = strength * 0.012f;
                if (mining > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.MiningSpeed, value = mining });

                float construction = strength * 0.010f;
                if (construction > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.ConstructionSpeed, value = construction });

                // Tarefas Manuais Finas e Medicina por Agilidade
                float labor = agility * 0.010f;
                if (labor > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.GeneralLaborSpeed, value = labor });

                float surgery = agility * 0.008f;
                if (surgery > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.MedicalSurgerySuccessChance, value = surgery });

                float medTend = agility * 0.010f;
                if (medTend > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.MedicalTendSpeed, value = medTend });

                // Sensibilidade Psíquica e Afinidade com Natureza por Condutividade
                float psychic = conductivity * 0.010f;
                if (psychic > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.PsychicSensitivity, value = psychic });

                float plantSpeed = conductivity * 0.008f;
                if (plantSpeed > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.PlantWorkSpeed, value = plantSpeed });

                float tameChance = conductivity * 0.008f;
                if (tameChance > 0.001f)
                    stage.statOffsets.Add(new StatModifier { stat = StatDefOf.TameAnimalChance, value = tameChance });

                // 2. Fatores Multiplicativos (Stat Factors)
                float meleeDmgMult = 1.0f + (strength * 0.015f);
                stage.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeDamageFactor, value = meleeDmgMult });

                float meleeCooldownMult = Mathf.Max(0.30f, 1.0f - (agility * 0.005f));
                stage.statFactors.Add(new StatModifier { stat = StatDefOf.MeleeCooldownFactor, value = meleeCooldownMult });

                float learningMult = 1.0f + (senses * 0.008f);
                stage.statFactors.Add(new StatModifier { stat = StatDefOf.GlobalLearningFactor, value = learningMult });

                float incomingDmg = DamageMitigationFactor;
                stage.statFactors.Add(new StatModifier { stat = StatDefOf.IncomingDamageFactor, value = incomingDmg });

                // 3. Efeitos Nativos no Organismo (Fome, Sono, Sangramento e Cura)
                stage.hungerRateFactor = Mathf.Max(0.25f, 1.0f - (constitution * 0.004f));
                stage.restFallFactor = Mathf.Max(0.30f, 1.0f - (constitution * 0.005f));
                stage.totalBleedFactor = Mathf.Max(0.20f, 1.0f - (constitution * 0.005f));
                stage.naturalHealingFactor = 1.0f + (constitution * 0.020f);

                // 4. Capacidades Biológicas (PawnCapacityModifier)
                float sightOffset = senses * 0.004f;
                if (sightOffset > 0.001f)
                    stage.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Sight, offset = sightOffset });

                float hearingOffset = senses * 0.004f;
                if (hearingOffset > 0.001f)
                    stage.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Hearing, offset = hearingOffset });

                float manipOffset = agility * 0.003f;
                if (manipOffset > 0.001f)
                    stage.capMods.Add(new PawnCapacityModifier { capacity = PawnCapacityDefOf.Manipulation, offset = manipOffset });

                cachedStage = stage;
                return cachedStage;
            }
        }

        public void NotifyStatsChanged()
        {
            cachedStage = null;
            if (pawn != null && pawn.health != null && pawn.health.capacities != null)
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        public void InitializeAttributesFromSkills()
        {
            if (attributesInitialized) return;
            attributesInitialized = true;

            if (pawn == null || pawn.skills == null || pawn.skills.skills == null)
            {
                strength = 5;
                constitution = 5;
                agility = 5;
                senses = 5;
                conductivity = 5;
                unallocatedPoints = 0;
                stageLevel = 1;
                currentStamina = MaxStamina;
                NotifyStatsChanged();
                return;
            }

            float rawSTR = 0f;
            float rawCON = 0f;
            float rawAGI = 0f;
            float rawSEN = 0f;
            float rawCND = 0f;

            for (int i = 0; i < pawn.skills.skills.Count; i++)
            {
                SkillRecord skill = pawn.skills.skills[i];
                if (skill == null || skill.def == null) continue;
                int lvl = skill.Level;
                if (lvl <= 0) continue;

                // Calibração por Paixão: 0.25 (pouco interesse), 0.5 (interesse), 1.2 (muito interesse)
                float ptsPerLevel = 0.25f;
                if (skill.passion == Passion.Minor)
                {
                    ptsPerLevel = 0.50f;
                }
                else if (skill.passion == Passion.Major)
                {
                    ptsPerLevel = 1.20f;
                }

                float pts = lvl * ptsPerLevel;

                if (skill.def == SkillDefOf.Shooting)
                {
                    rawSTR += pts;
                }
                else if (skill.def == SkillDefOf.Melee)
                {
                    rawSTR += pts / 2f;
                    rawCON += pts / 2f;
                }
                else if (skill.def == SkillDefOf.Animals)
                {
                    rawSTR += pts;
                }
                else if (skill.def == SkillDefOf.Mining)
                {
                    rawCON += pts;
                }
                else if (skill.def == SkillDefOf.Crafting)
                {
                    rawCON += pts / 2f;
                    rawAGI += pts / 2f;
                }
                else if (skill.def == SkillDefOf.Medicine)
                {
                    rawAGI += pts;
                }
                else if (skill.def == SkillDefOf.Cooking)
                {
                    rawAGI += pts;
                }
                else if (skill.def == SkillDefOf.Social)
                {
                    rawSEN += pts;
                }
                else if (skill.def == SkillDefOf.Artistic)
                {
                    rawSEN += pts / 2f;
                    rawCND += pts / 2f;
                }
                else if (skill.def == SkillDefOf.Intellectual)
                {
                    rawSEN += pts / 2f;
                    rawCND += pts / 2f;
                }
                else if (skill.def == SkillDefOf.Plants)
                {
                    rawCND += pts;
                }
            }

            strength = Mathf.FloorToInt(rawSTR);
            constitution = Mathf.FloorToInt(rawCON);
            agility = Mathf.FloorToInt(rawAGI);
            senses = Mathf.FloorToInt(rawSEN);
            conductivity = Mathf.FloorToInt(rawCND);

            int total = strength + constitution + agility + senses + conductivity;

            // Regra de segurança: Mínimo de 25 pontos
            if (total < 25)
            {
                int deficit = 25 - total;
                int addEach = deficit / 5;
                int rem = deficit % 5;
                strength += addEach;
                constitution += addEach;
                agility += addEach;
                senses += addEach;
                conductivity += addEach;
                if (rem > 0) { strength++; rem--; }
                if (rem > 0) { constitution++; rem--; }
                if (rem > 0) { agility++; rem--; }
                if (rem > 0) { senses++; rem--; }
                total = strength + constitution + agility + senses + conductivity;
            }

            // Regra de segurança: Máximo de 110 pontos
            if (total > 110)
            {
                float factor = 110f / (float)total;
                strength = Mathf.FloorToInt(strength * factor);
                constitution = Mathf.FloorToInt(constitution * factor);
                agility = Mathf.FloorToInt(agility * factor);
                senses = Mathf.FloorToInt(senses * factor);
                conductivity = Mathf.FloorToInt(conductivity * factor);
            }

            if (stageLevel <= 0) stageLevel = 1;
            currentStamina = MaxStamina;
            currentEnergy = MaxEnergy;
            NotifyStatsChanged();
        }

        public bool TryAllocatePoint(CultivatorAttributeType attr)
        {
            return TryAllocatePoints(attr, 1);
        }

        public bool TryAllocatePoints(CultivatorAttributeType attr, int count)
        {
            if (unallocatedPoints <= 0 || count <= 0) return false;

            int toAllocate = Mathf.Min(count, unallocatedPoints);
            unallocatedPoints -= toAllocate;

            switch (attr)
            {
                case CultivatorAttributeType.Strength:
                    strength += toAllocate;
                    break;
                case CultivatorAttributeType.Constitution:
                    constitution += toAllocate;
                    currentStamina = Mathf.Min(currentStamina + (1.5f * (float)toAllocate), MaxStamina);
                    break;
                case CultivatorAttributeType.Agility:
                    agility += toAllocate;
                    break;
                case CultivatorAttributeType.Senses:
                    senses += toAllocate;
                    break;
                case CultivatorAttributeType.Conductivity:
                    float oldMax = MaxEnergy;
                    conductivity += toAllocate;
                    float delta = MaxEnergy - oldMax;
                    currentEnergy = Mathf.Min(currentEnergy + delta, MaxEnergy);
                    break;
            }

            NotifyStatsChanged();
            return true;
        }

        public bool TryConsumeStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                return true;
            }
            return false;
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (!initialized)
            {
                initialized = true;
                currentEnergy = MaxEnergy;
                currentXP = 0f;
                SyncAbilities();
            }
            if (!attributesInitialized)
            {
                InitializeAttributesFromSkills();
            }
        }

        public override void PostTick()
        {
            base.PostTick();
            if (pawn == null || pawn.Dead) return;

            if (!initialized)
            {
                initialized = true;
                currentEnergy = MaxEnergy;
                SyncAbilities();
            }

            if (!attributesInitialized)
            {
                InitializeAttributesFromSkills();
            }

            if (manaRegenCooldownTicks > 0)
            {
                manaRegenCooldownTicks--;
            }

            // Regeneração suave a cada segundo (60 ticks)
            if (pawn.IsHashIntervalTick(60))
            {
                bool isSleeping = (pawn.CurJobDef != null && pawn.CurJobDef == JobDefOf.LayDown);

                // 1. Regeneração de Energia da Alma (% MaxMP/h com pausa de 3s ao conjurar, 0.5x convocado, 2x sono, 4x meditação)
                float rate = CurrentEffectiveRegenPerHour;
                if (rate > 0f)
                {
                    float regenPerTick60 = (rate / 2500f) * 60f;
                    currentEnergy = Mathf.Min(MaxEnergy, currentEnergy + regenPerTick60);
                }

                // 2. Regeneração de Vigor / Estamina
                float staminaRate = StaminaRegenPerSecond * (isSleeping ? 2.0f : 1.0f);
                currentStamina = Mathf.Min(MaxStamina, currentStamina + staminaRate);

                // 3. Maturação por alimentação na forma de besta
                if (IsMetamorphosed && pawn.CurJobDef != null && pawn.CurJobDef == JobDefOf.Ingest)
                {
                    activeForm.AddProgress(0.015f, pawn);
                }
            }

            // Ganho suave de XP passivo por tarefas civis ou descanso a cada 250 ticks (~4.1 segundos)
            if (pawn.IsHashIntervalTick(250))
            {
                if (pawn.CurJobDef != null)
                {
                    if (pawn.CurJobDef == JobDefOf.LayDown)
                    {
                        // 15 XP por hora dormindo = (15 / 2500) * 250 = 1.5 XP
                        AddXP(1.5f, false);
                    }
                    else if (pawn.CurJobDef != JobDefOf.Wait_MaintainPosture && pawn.CurJobDef != RebornDefOf.REM_Job_SoulMeditation)
                    {
                        // 5 XP por hora em trabalhos diários da colônia = (5 / 2500) * 250 = 0.5 XP
                        AddXP(0.5f, false);
                    }
                    else if (pawn.CurJobDef == RebornDefOf.REM_Job_SoulMeditation)
                    {
                        // Foco na Meditação: se houver técnica fixada, ganha proficiência contínua (+10/hora)
                        if (!string.IsNullOrEmpty(meditationFocusAbility))
                        {
                            AddAbilityProficiency(meditationFocusAbility, 1.0f);
                        }
                    }

                    RebornSpellDatabase.TryAutoCastTelekineticGathering(pawn, this);
                }
            }
        }

        public void BindMagicPearl()
        {
            isMagicPearlBearer = true;
            if (pawn != null && pawn.story != null && pawn.story.traits != null)
            {
                if (RebornDefOf.REM_Trait_MagicPearlBearer != null && !pawn.story.traits.HasTrait(RebornDefOf.REM_Trait_MagicPearlBearer))
                {
                    pawn.story.traits.GainTrait(new Trait(RebornDefOf.REM_Trait_MagicPearlBearer));
                }
            }
            SyncAbilities();
        }

        public void RegisterOrFeedBloodline(PawnKindDef kind, float progressAmount, bool isCorpseAbsorption)
        {
            if (kind == null) return;
            if (catalogedBloodlines == null) catalogedBloodlines = new List<CatalogedBloodline>();

            CatalogedBloodline existing = null;
            for (int i = 0; i < catalogedBloodlines.Count; i++)
            {
                if (catalogedBloodlines[i].kindDef == kind)
                {
                    existing = catalogedBloodlines[i];
                    break;
                }
            }

            if (existing == null)
            {
                existing = new CatalogedBloodline(kind);
                catalogedBloodlines.Add(existing);

                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastSkipFlashEntry, 2.5f);
                    FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 2.0f);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("NOVA LINHAGEM: {0}!", existing.Label), new Color(0.35f, 1f, 0.75f), 3.5f);
                }

                if (pawn != null && pawn.IsColonistPlayerControlled)
                {
                    Find.LetterStack.ReceiveLetter(
                        "Linhagem Catalogada na Pérola Mágica",
                        string.Format("{0} absorveu os códigos genéticos e a alma de um {1}!\n\nA linhagem foi adicionada ao seu catálogo dimensional no estágio Filhote. Agora {0} pode remodelar sua carne e ossos nessa criatura a qualquer momento pela Aba 'Alma'.", pawn.LabelShort, existing.Label),
                        LetterDefOf.PositiveEvent,
                        pawn
                    );
                }
            }
            else
            {
                existing.corpsesAbsorbed++;
                existing.AddProgress(progressAmount, pawn);
            }
        }

        public float CalculateMetamorphosisEnergyCost(CatalogedBloodline bloodline)
        {
            if (bloodline == null || bloodline.kindDef == null) return 15f;
            float baseSize = 1.0f;
            if (bloodline.kindDef.RaceProps != null)
            {
                baseSize = bloodline.kindDef.RaceProps.baseBodySize;
            }
            float finalSize = baseSize * bloodline.BodySizeMultiplier;
            return 15f + Mathf.Max(0f, finalSize - 1.0f) * 20f;
        }

        public bool TryMetamorphose(CatalogedBloodline bloodline)
        {
            if (bloodline == null || bloodline.kindDef == null || pawn == null || pawn.Dead) return false;

            float cost = CalculateMetamorphosisEnergyCost(bloodline);
            if (currentEnergy + 0.01f < cost)
            {
                Messages.Message(string.Format("Energia insuficiente na Matriz! Necessário {0} de Energia (Atual: {1}).", Mathf.CeilToInt(cost), Mathf.FloorToInt(currentEnergy)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            if (!TryConsumeEnergy(cost))
            {
                return false;
            }

            if (IsMetamorphosed)
            {
                if (pawn.health != null && pawn.health.hediffSet != null)
                {
                    Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_MetamorphicForm);
                    if (existingHediff != null)
                    {
                        pawn.health.RemoveHediff(existingHediff);
                    }
                }
            }
            else
            {
                spiritualApparel.Clear();
                if (pawn.apparel != null && pawn.apparel.WornApparel != null)
                {
                    List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
                    for (int i = 0; i < worn.Count; i++)
                    {
                        Apparel ap = worn[i];
                        pawn.apparel.Remove(ap);
                        spiritualApparel.Add(ap);
                    }
                }

                spiritualWeapon = null;
                if (pawn.equipment != null && pawn.equipment.Primary != null)
                {
                    spiritualWeapon = pawn.equipment.Primary;
                    pawn.equipment.Remove(spiritualWeapon);
                }
            }

            activeForm = bloodline;

            if (pawn.health != null)
            {
                Hediff_MetamorphicForm formHediff = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_MetamorphicForm, pawn) as Hediff_MetamorphicForm;
                if (formHediff != null)
                {
                    formHediff.formKindDef = bloodline.kindDef;
                    formHediff.formStage = bloodline.stage;
                    pawn.health.AddHediff(formHediff);
                }
            }

            if (pawn.Drawer != null && pawn.Drawer.renderer != null)
            {
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }

            if (pawn.Spawned && pawn.Map != null)
            {
                FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastSkipFlashEntry, 2.5f);
                FleckMaker.ThrowLightningGlow(pawn.DrawPos, pawn.Map, 2.0f);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("METAMORFOSE: {0}!", bloodline.Label), bloodline.StageColor, 3.5f);
            }

            Messages.Message(string.Format("{0} remodelou seus ossos e carne, assumindo a forma de {1} [{2}]! Suas roupas e armas foram recolhidas ao vácuo espiritual.", pawn.LabelShort, bloodline.Label, bloodline.StageLabelPT), pawn, MessageTypeDefOf.PositiveEvent);
            return true;
        }

        public void RevertToHuman(bool removeHediff = true)
        {
            if (!IsMetamorphosed) return;

            activeForm = null;

            if (removeHediff && pawn.health != null && pawn.health.hediffSet != null)
            {
                Hediff formHediff = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_MetamorphicForm);
                if (formHediff != null)
                {
                    pawn.health.RemoveHediff(formHediff);
                }
            }

            if (spiritualApparel != null && spiritualApparel.Count > 0 && pawn.apparel != null)
            {
                for (int i = 0; i < spiritualApparel.Count; i++)
                {
                    Apparel ap = spiritualApparel[i];
                    if (ap != null && !ap.Destroyed)
                    {
                        try
                        {
                            pawn.apparel.Wear(ap, false, false);
                        }
                        catch
                        {
                            if (pawn.Spawned && pawn.Map != null)
                            {
                                GenPlace.TryPlaceThing(ap, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                            }
                        }
                    }
                }
                spiritualApparel.Clear();
            }

            if (spiritualWeapon != null && !spiritualWeapon.Destroyed && pawn.equipment != null)
            {
                try
                {
                    pawn.equipment.AddEquipment(spiritualWeapon);
                }
                catch
                {
                    if (pawn.Spawned && pawn.Map != null)
                    {
                        GenPlace.TryPlaceThing(spiritualWeapon, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                    }
                }
                spiritualWeapon = null;
            }

            if (pawn.Drawer != null && pawn.Drawer.renderer != null)
            {
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }

            if (pawn.Spawned && pawn.Map != null)
            {
                FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 2.0f);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "FORMA HUMANA RESTAURADA", new Color(0.8f, 0.9f, 1f), 3.0f);
            }

            Messages.Message(string.Format("{0} desfez a metamorfose e restaurou sua anatomia e equipamentos humanos.", pawn.LabelShort), pawn, MessageTypeDefOf.NeutralEvent);
        }

        public void AddXP(float amount, bool showMote)
        {
            if (amount <= 0f) return;

            if (stageLevel < MaxLevelForStage)
            {
                currentXP += amount;
                while (stageLevel < MaxLevelForStage && currentXP >= RequiredXPForCurrentLevel)
                {
                    currentXP -= RequiredXPForCurrentLevel;
                    stageLevel++;
                    unallocatedPoints += 5;
                    skillPoints += 2;

                    if (pawn != null && pawn.Spawned && pawn.Map != null)
                    {
                        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 2.0f);
                        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("NÍVEL {0}! (+5 ATR / +2 HAB)", stageLevel), new Color(1f, 0.85f, 0.2f), 3.0f);
                    }

                    if (pawn != null && pawn.IsColonistPlayerControlled)
                    {
                        Messages.Message(string.Format("{0} alcançou o Nível {1} da {2}! Recebeu +5 pontos de atributo e +2 pontos de habilidade no Grimório.", pawn.LabelShort, stageLevel, StageConfig.LabelPT), pawn, MessageTypeDefOf.PositiveEvent);
                    }
                }

                if (stageLevel >= MaxLevelForStage && currentXP > RequiredXPForCurrentLevel)
                {
                    currentXP = RequiredXPForCurrentLevel;
                }
            }
            else
            {
                bool wasReady = CanBreakthrough;
                currentXP = Mathf.Min(currentXP + amount, RequiredXPForCurrentLevel);

                if (showMote && pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("+{0} XP Alma", Mathf.RoundToInt(amount)), new Color(0.95f, 0.85f, 0.35f), 2.0f);
                }

                if (!wasReady && CanBreakthrough && pawn != null && pawn.IsColonistPlayerControlled && pawn.Spawned)
                {
                    Messages.Message(string.Format("{0} atingiu o limite da {1} (Nível {2})! A Matriz está totalmente comprimida e pronta para o Batismo das Leis!", pawn.LabelShort, StageConfig.LabelPT, stageLevel), pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        public void RestoreEnergy(float amount, bool showMote)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, MaxEnergy);
            if (showMote && pawn.Spawned && pawn.Map != null)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, string.Format("+{0} Energia", Mathf.RoundToInt(amount)), new Color(0.35f, 0.80f, 1f), 2.0f);
            }
        }

        public bool TryConsumeEnergy(float amount, bool triggerRegenDelay = true)
        {
            if (currentEnergy + 0.01f < amount)
                return false;

            currentEnergy = Mathf.Max(0f, currentEnergy - amount);
            if (triggerRegenDelay && amount > 0.01f)
            {
                manaRegenCooldownTicks = 180; // Pausa de 3 segundos (180 ticks) após gastar mana
            }
            return true;
        }

        public bool TryBreakthrough()
        {
            if (currentStage >= SoulClassStage.Grandmaster)
            {
                Messages.Message(string.Format("{0} já atingiu a Classe Grão-Mestre (limite atual antes da compreensão das Leis).", pawn.LabelShort), pawn, MessageTypeDefOf.NeutralEvent);
                return false;
            }

            if (!CanBreakthrough && !DebugSettings.godMode)
            {
                Messages.Message(string.Format("Compressão insuficiente. Alcance o Nível {0} com barra de XP cheia para o Batismo das Leis.", MaxLevelForStage), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            SoulClassStage prevStage = currentStage;
            currentStage = (SoulClassStage)((int)currentStage + 1);
            stageLevel = 1;
            currentXP = 0f;

            // Saltar de estágio garante pontos de atributos equivalente a todos os pontos ganhos durante o estágio anterior!
            int prevMaxLvl = ((int)prevStage + 1) * 10;
            int bonusPoints = prevMaxLvl * 5;
            unallocatedPoints += bonusPoints;

            currentEnergy = MaxEnergy;
            currentStamina = MaxStamina;

            // Evolução automática das magias aprendidas que atingiram pelo menos Perito (3)
            int evolvedCount = 0;
            int keptCount = 0;
            if (abilityRecords != null)
            {
                for (int i = 0; i < abilityRecords.Count; i++)
                {
                    CultivatorAbilityRecord rec = abilityRecords[i];
                    if (rec == null || !rec.isLearned) continue;
                    if (rec.CanEvolveStage(currentStage))
                    {
                        if (rec.TryEvolveStage(pawn, currentStage, true))
                        {
                            evolvedCount++;
                        }
                    }
                    else if ((int)rec.currentSpellStage < (int)currentStage && rec.currentSpellStage < SoulClassStage.Grandmaster)
                    {
                        keptCount++;
                    }
                }
            }

            if (pawn.Spawned && pawn.Map != null)
            {
                RebornVFX.PlayAscensionPillar(pawn, StageConfig.ThemeColor, string.Format("BATISMO DAS LEIS: {0}!", StageConfig.LabelPT.ToUpper()), 2.8f);
            }

            SyncAbilities();
            NotifyStatsChanged();

            if (pawn.IsColonistPlayerControlled)
            {
                string extraSpellNote = string.Format("\n• {0} técnica(s) evoluíram com você para o novo estágio; {1} permaneceram no estágio anterior por terem maestria menor que Perito (3).", evolvedCount, keptCount);
                Find.LetterStack.ReceiveLetter(
                    string.Format("Batismo das Leis: {0}", StageConfig.LabelPT),
                    string.Format("{0} rompeu os limites da alma e avançou para a {1} dos 30.000 Mundos!\n\n• Capacidade de Energia expandida para {2} MP\n• Regeneração aumentada para +{3:F0}/hora\n• Vigor e Energia completamente restaurados\n• +{4} pontos de atributos concedidos{5}",
                        pawn.LabelShort,
                        StageConfig.LabelPT,
                        Mathf.RoundToInt(MaxEnergy),
                        PassiveRegenPerHour,
                        bonusPoints,
                        extraSpellNote
                    ),
                    LetterDefOf.PositiveEvent,
                    pawn
                );
            }

            return true;
        }

        public CultivatorAbilityRecord FindAbilityRecord(string defName)
        {
            if (string.IsNullOrEmpty(defName) || abilityRecords == null) return null;
            for (int i = 0; i < abilityRecords.Count; i++)
            {
                if (abilityRecords[i].abilityDefName == defName)
                {
                    return abilityRecords[i];
                }
            }
            return null;
        }

        public CultivatorAbilityRecord GetAbilityRecord(string defName)
        {
            if (string.IsNullOrEmpty(defName)) return null;
            if (abilityRecords == null) abilityRecords = new List<CultivatorAbilityRecord>();

            for (int i = 0; i < abilityRecords.Count; i++)
            {
                if (abilityRecords[i].abilityDefName == defName)
                {
                    return abilityRecords[i];
                }
            }

            CultivatorAbilityRecord rec = new CultivatorAbilityRecord(defName);
            abilityRecords.Add(rec);
            return rec;
        }

        public void AddAbilityProficiency(string defName, float amount)
        {
            CultivatorAbilityRecord rec = GetAbilityRecord(defName);
            if (rec != null)
            {
                rec.AddProficiency(amount, pawn, currentStage);
            }
        }

        public bool TryLearnSpell(string baseSpellId, int pointCost)
        {
            if (string.IsNullOrEmpty(baseSpellId)) return false;
            CultivatorAbilityRecord rec = GetAbilityRecord(baseSpellId);
            if (rec.isLearned) return false;
            if (skillPoints < pointCost && !DebugSettings.godMode)
            {
                Messages.Message(string.Format("Pontos de Habilidade insuficientes! Necessário {0} pt(s).", pointCost), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            if (!DebugSettings.godMode)
            {
                skillPoints -= pointCost;
            }

            rec.isLearned = true;
            rec.isEquipped = true;
            rec.currentSpellStage = SoulClassStage.Mortal;
            rec.masteryTier = 1;
            rec.proficiency = 0f;

            SyncAbilities();
            return true;
        }

        public bool TryForgetSpell(string baseSpellId, int refundPoints)
        {
            if (string.IsNullOrEmpty(baseSpellId)) return false;
            CultivatorAbilityRecord rec = FindAbilityRecord(baseSpellId);
            if (rec == null || !rec.isLearned) return false;

            rec.isLearned = false;
            rec.isEquipped = false;
            rec.currentSpellStage = SoulClassStage.Mortal;
            rec.masteryTier = 1;
            rec.proficiency = 0f;

            if (meditationFocusAbility == baseSpellId)
            {
                meditationFocusAbility = null;
            }

            if (refundPoints > 0)
            {
                skillPoints += refundPoints;
            }

            // Se a magia esquecida for um escudo/buff contínuo ativo no peão, removê-lo
            if (pawn != null && pawn.health != null && pawn.health.hediffSet != null)
            {
                Hediff toRemove = null;
                if (baseSpellId == "SoulShield" && RebornDefOf.REM_Hediff_SoulShield != null)
                    toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulShield);
                else if (baseSpellId == "ManaBarrier" && RebornDefOf.REM_Hediff_ManaBarrier != null)
                    toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ManaBarrier);
                else if (baseSpellId == "AetherShield" && RebornDefOf.REM_Hediff_AetherShield != null)
                    toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_AetherShield);
                else if (baseSpellId == "CrimsonShield" && RebornDefOf.REM_Hediff_CrimsonShield != null)
                    toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_CrimsonShield);
                else if (baseSpellId == "InfernoWall" && RebornDefOf.REM_Hediff_InfernoWall != null)
                    toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_InfernoWall);
                else if (baseSpellId == "MindBarrier" && RebornDefOf.REM_Hediff_MindBarrier != null)
                    toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_MindBarrier);
                else if (baseSpellId == "ShadowCloak" && RebornDefOf.REM_Hediff_ShadowCloak != null)
                    toRemove = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ShadowCloak);

                if (toRemove != null)
                {
                    pawn.health.RemoveHediff(toRemove);
                }
            }

            SyncAbilities();

            RebornSpellEntry entry = RebornSpellDatabase.GetEntry(baseSpellId);
            string label = (entry != null) ? entry.labelPT : baseSpellId;
            if (pawn != null)
            {
                Messages.Message(string.Format("{0} esqueceu a técnica {1} (+{2} Pt de Habilidade devolvido).", pawn.LabelShort, label, refundPoints), pawn, MessageTypeDefOf.NeutralEvent);
            }
            return true;
        }

        public void ToggleEquipSpell(string baseSpellId)
        {
            CultivatorAbilityRecord rec = FindAbilityRecord(baseSpellId);
            if (rec == null || !rec.isLearned) return;
            rec.isEquipped = !rec.isEquipped;
            SyncAbilities();
        }

        public bool TryManualEvolveSpell(string baseSpellId)
        {
            CultivatorAbilityRecord rec = FindAbilityRecord(baseSpellId);
            if (rec == null) return false;
            if (rec.TryEvolveStage(pawn, currentStage, false))
            {
                SyncAbilities();
                return true;
            }
            return false;
        }

        public void SyncAbilities()
        {
            if (pawn == null || pawn.abilities == null) return;

            // Habilidades Inatas da Matriz / Pérola Mágica
            EnsureAbility(RebornDefOf.REM_Ability_Meditate);

            if (isMagicPearlBearer)
            {
                EnsureAbility(RebornDefOf.REM_Ability_AbsorbSoul);
            }
            else
            {
                RemoveAbility(RebornDefOf.REM_Ability_AbsorbSoul);
            }

            // Remover habilidades legadas caso existam em saves antigos
            RemoveAbility(RebornDefOf.REM_Ability_SoulBolt);
            RemoveAbility(RebornDefOf.REM_Ability_WindDash);
            RemoveAbility(RebornDefOf.REM_Ability_FireArrow);
            RemoveAbility(RebornDefOf.REM_Ability_Heal);
            RemoveAbility(RebornDefOf.REM_Ability_Haste);
            RemoveAbility(RebornDefOf.REM_Ability_StoneSkin);
            RemoveAbility(RebornDefOf.REM_Ability_AcceleratedRegen);
            RemoveAbility(RebornDefOf.REM_Ability_FrostRay);
            RemoveAbility(RebornDefOf.REM_Ability_PsionicShield);
            RemoveAbility(RebornDefOf.REM_Ability_SkyStep);
            RemoveAbility(RebornDefOf.REM_Ability_SkyLance);
            RemoveAbility(RebornDefOf.REM_Ability_ChainLightning);
            RemoveAbility(RebornDefOf.REM_Ability_Teleport);
            RemoveAbility(RebornDefOf.REM_Ability_HeavensBlessing);

            // Sincronizar catálogo de 61 magias x 5 estágios (Mortal..Grandmaster)
            List<RebornSpellEntry> allSpells = RebornSpellDatabase.AllSpells;
            for (int i = 0; i < allSpells.Count; i++)
            {
                RebornSpellEntry entry = allSpells[i];
                CultivatorAbilityRecord rec = FindAbilityRecord(entry.baseSpellId);
                for (int s = 0; s <= (int)SoulClassStage.Grandmaster; s++)
                {
                    SoulClassStage st = (SoulClassStage)s;
                    AbilityDef aDef = entry.GetAbilityDef(st);
                    if (aDef == null) continue;

                    bool shouldHave = (rec != null && rec.isLearned && rec.isEquipped && rec.currentSpellStage == st);
                    if (shouldHave)
                    {
                        EnsureAbility(aDef);
                    }
                    else
                    {
                        RemoveAbility(aDef);
                    }
                }
            }
        }

        private void EnsureAbility(AbilityDef aDef)
        {
            if (aDef == null || pawn == null || pawn.abilities == null) return;
            if (pawn.abilities.GetAbility(aDef) == null)
            {
                pawn.abilities.GainAbility(aDef);
            }
        }

        private void RemoveAbility(AbilityDef aDef)
        {
            if (aDef == null || pawn == null || pawn.abilities == null) return;
            if (pawn.abilities.GetAbility(aDef) != null)
            {
                pawn.abilities.RemoveAbility(aDef);
            }
        }

        public void Dev_AdvanceLevels(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (stageLevel < MaxLevelForStage)
                {
                    stageLevel++;
                    unallocatedPoints += 5;
                    skillPoints += 2;
                }
                else
                {
                    currentXP = RequiredXPForCurrentLevel;
                    break;
                }
            }
            NotifyStatsChanged();
        }

        public void Dev_AddAttributePoints(int points)
        {
            unallocatedPoints += points;
            NotifyStatsChanged();
        }

        public void Dev_SetStage(SoulClassStage targetStage)
        {
            if (targetStage > SoulClassStage.Grandmaster)
            {
                targetStage = SoulClassStage.Grandmaster;
            }
            currentStage = targetStage;
            stageLevel = 1;
            currentXP = 0f;
            currentEnergy = MaxEnergy;
            currentStamina = MaxStamina;
            SyncAbilities();
            NotifyStatsChanged();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            if (pawn == null || !pawn.IsColonistPlayerControlled)
                yield break;

            if (!IsMetamorphosed)
            {
                Command_Action cmdMeditate = new Command_Action();
                cmdMeditate.defaultLabel = "Meditar na Matriz";
                cmdMeditate.defaultDesc = string.Format("Ordena o colono a sentar imediatamente para circular energia espiritual por sua Matriz da Alma.\n\n• Recupera +{0:F0} MP/hora (4x a regeneração passiva)\n• Comprime +100 XP/hora para avanço de nível e Batismo das Leis.", PassiveRegenPerHour * 4.0f);
                cmdMeditate.icon = ContentFinder<Texture2D>.Get("UI/RebornMagic/Gizmo_Breakthrough", true);
                cmdMeditate.action = delegate
                {
                    if (pawn.jobs != null)
                    {
                        Verse.AI.Job job = JobMaker.MakeJob(RebornDefOf.REM_Job_SoulMeditation);
                        pawn.jobs.TryTakeOrderedJob(job, Verse.AI.JobTag.Misc);
                    }
                };
                yield return cmdMeditate;
            }

            // Ferramentas de Desenvolvedor (Dev Mode)
            if (Prefs.DevMode)
            {
                Command_Action devLvl1 = new Command_Action();
                devLvl1.defaultLabel = "DEV: +1 Nível";
                devLvl1.defaultDesc = "Avança 1 nível imediatamente e concede 5 pontos livres de atributo.";
                devLvl1.action = delegate
                {
                    Dev_AdvanceLevels(1);
                };
                yield return devLvl1;

                Command_Action devLvl10 = new Command_Action();
                devLvl10.defaultLabel = "DEV: +10 Níveis";
                devLvl10.defaultDesc = "Avança 10 níveis imediatamente e concede 50 pontos livres de atributo.";
                devLvl10.action = delegate
                {
                    Dev_AdvanceLevels(10);
                };
                yield return devLvl10;

                Command_Action devPts = new Command_Action();
                devPts.defaultLabel = "DEV: +50 Pontos";
                devPts.defaultDesc = "Concede 50 pontos livres de atributo.";
                devPts.action = delegate
                {
                    Dev_AddAttributePoints(50);
                };
                yield return devPts;

                Command_Action devMaxXP = new Command_Action();
                devMaxXP.defaultLabel = "DEV: Max XP";
                devMaxXP.defaultDesc = "Comprime o XP ao máximo do nível atual (disponibiliza Batismo das Leis se estiver no nível máximo do estágio).";
                devMaxXP.action = delegate
                {
                    currentXP = RequiredXPForCurrentLevel;
                };
                yield return devMaxXP;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentStage, "currentStage", SoulClassStage.Mortal);
            Scribe_Values.Look(ref currentEnergy, "currentEnergy", 100f);
            Scribe_Values.Look(ref manaRegenCooldownTicks, "manaRegenCooldownTicks", 0);
            Scribe_Values.Look(ref currentXP, "currentXP", 0f);
            Scribe_Values.Look(ref totalSoulsAbsorbed, "totalSoulsAbsorbed", 0);
            Scribe_Values.Look(ref initialized, "initialized", true);

            Scribe_Values.Look(ref strength, "strength", 5);
            Scribe_Values.Look(ref constitution, "constitution", 5);
            Scribe_Values.Look(ref agility, "agility", 5);
            Scribe_Values.Look(ref senses, "senses", 5);
            Scribe_Values.Look(ref conductivity, "conductivity", 5);
            Scribe_Values.Look(ref unallocatedPoints, "unallocatedPoints", 0);
            Scribe_Values.Look(ref skillPoints, "skillPoints", 2);
            Scribe_Values.Look(ref stageLevel, "stageLevel", 1);
            Scribe_Values.Look(ref attributesInitialized, "attributesInitialized", false);
            Scribe_Values.Look(ref currentStamina, "currentStamina", 100f);

            Scribe_Values.Look(ref isMagicPearlBearer, "isMagicPearlBearer", false);
            Scribe_Collections.Look(ref catalogedBloodlines, "catalogedBloodlines", LookMode.Deep);
            Scribe_Collections.Look(ref spiritualApparel, "spiritualApparel", LookMode.Deep);
            Scribe_Deep.Look(ref spiritualWeapon, "spiritualWeapon");
            Scribe_Collections.Look(ref abilityRecords, "abilityRecords", LookMode.Deep);
            Scribe_Values.Look(ref meditationFocusAbility, "meditationFocusAbility", null);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                savedActiveKindDefName = (activeForm != null && activeForm.kindDef != null) ? activeForm.kindDef.defName : null;
            }
            Scribe_Values.Look(ref savedActiveKindDefName, "savedActiveKindDefName", null);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (abilityRecords == null)
                {
                    abilityRecords = new List<CultivatorAbilityRecord>();
                }
                if (!string.IsNullOrEmpty(savedActiveKindDefName) && catalogedBloodlines != null)
                {
                    for (int i = 0; i < catalogedBloodlines.Count; i++)
                    {
                        if (catalogedBloodlines[i].kindDef != null && catalogedBloodlines[i].kindDef.defName == savedActiveKindDefName)
                        {
                            activeForm = catalogedBloodlines[i];
                            break;
                        }
                    }
                }
                SyncAbilities();
                NotifyStatsChanged();
            }
        }
    }
}
