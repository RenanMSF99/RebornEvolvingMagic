using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RebornEvolvingMagic
{
    [StaticConstructorOnStartup]
    public class ITab_Pawn_SoulMatrix : ITab
    {
        private static readonly Vector2 NormalSize = new Vector2(680f, 580f);
        private static readonly Vector2 PearlBearerSize = new Vector2(680f, 700f);

        private static readonly Texture2D BarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.10f, 0.12f, 0.16f, 0.85f));
        private static readonly Texture2D StaminaFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.25f, 0.82f, 0.40f, 0.95f));
        private static readonly Texture2D EnergyFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.20f, 0.65f, 0.95f, 0.95f));
        private static readonly Texture2D XpFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.95f, 0.75f, 0.20f, 0.95f));
        private static readonly Texture2D MaturationFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.85f, 0.45f, 0.95f));
        private static readonly Texture2D MasteryFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.30f, 0.75f, 0.95f, 0.95f));

        private static int currentSubTab = 0; // 0 = Visão Geral & Atributos, 1 = Grimório de Técnicas
        private static string grimoireFilter = "All";
        private Vector2 bloodlineScrollPos = Vector2.zero;
        private Vector2 grimoireScrollPos = Vector2.zero;

        public ITab_Pawn_SoulMatrix()
        {
            size = NormalSize;
            labelKey = "REM_TabSoulMatrix";
        }

        public override bool IsVisible
        {
            get
            {
                Pawn p = CurrentPawn;
                if (p == null || p.Dead || p.RaceProps == null || !p.RaceProps.Humanlike) return false;
                if (Prefs.DevMode || DebugSettings.godMode) return true;
                return p.Faction != null && p.Faction.IsPlayer;
            }
        }

        private Pawn CurrentPawn
        {
            get
            {
                Pawn p = SelPawn;
                if (p != null) return p;
                Corpse c = SelThing as Corpse;
                if (c != null) return c.InnerPawn;
                return null;
            }
        }

        protected override void FillTab()
        {
            Pawn pawn = CurrentPawn;
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null) return;

            // Auto-despertar instantâneo para qualquer colono do jogador
            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null && pawn.Faction != null && pawn.Faction.IsPlayer && pawn.RaceProps != null && pawn.RaceProps.Humanlike)
            {
                matrix = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_SoulMatrix, pawn) as Hediff_SoulMatrix;
                if (matrix != null)
                {
                    pawn.health.AddHediff(matrix);
                    if (RebornDefOf.REM_Trait_MagicPearlBearer != null && pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(RebornDefOf.REM_Trait_MagicPearlBearer))
                    {
                        matrix.isMagicPearlBearer = true;
                    }
                    matrix.SyncAbilities();
                }
            }

            if (matrix == null) return;

            // Ajuste dinâmico de tamanho conforme a aba e a posse da Pérola Mágica
            if (currentSubTab == 0 && matrix.isMagicPearlBearer)
            {
                size = PearlBearerSize;
            }
            else
            {
                size = NormalSize;
            }

            Rect fullRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(14f);
            GUI.BeginGroup(fullRect);

            // 1. Cabeçalho Principal (Estágio e Nível)
            Text.Font = GameFont.Medium;
            GUI.color = matrix.StageConfig.ThemeColor;
            Rect headerRect = new Rect(0f, 0f, fullRect.width, 28f);
            string headerText = string.Format("★ {0} ({1}) - Nível {2} / {3}",
                matrix.StageConfig.LabelPT,
                matrix.StageConfig.LabelEN,
                matrix.stageLevel,
                matrix.MaxLevelForStage
            );
            Widgets.Label(headerRect, headerText);

            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Rect descRect = new Rect(0f, 28f, fullRect.width, 22f);
            Widgets.Label(descRect, matrix.StageConfig.Description);

            // 2. Seletor de Sub-Abas
            float tabBtnWidth = (fullRect.width - 10f) / 2f;
            Rect tab0Rect = new Rect(0f, 52f, tabBtnWidth, 26f);
            Rect tab1Rect = new Rect(tabBtnWidth + 10f, 52f, tabBtnWidth, 26f);

            GUI.color = (currentSubTab == 0) ? new Color(1f, 0.88f, 0.35f) : new Color(0.70f, 0.70f, 0.70f);
            if (Widgets.ButtonText(tab0Rect, "✦ Visão Geral & Atributos ✦"))
            {
                currentSubTab = 0;
            }

            GUI.color = (currentSubTab == 1) ? new Color(0.35f, 0.85f, 1.0f) : new Color(0.70f, 0.70f, 0.70f);
            if (Widgets.ButtonText(tab1Rect, "📖 Grimório de Técnicas 📖"))
            {
                currentSubTab = 1;
            }

            Widgets.DrawLineHorizontal(0f, 82f, fullRect.width);

            // 3. Renderização do Conteúdo da Sub-Aba Selecionada
            if (currentSubTab == 0)
            {
                DrawOverviewSubTab(matrix, pawn, fullRect);
            }
            else
            {
                DrawGrimoireSubTab(matrix, pawn, fullRect);
            }

            GUI.EndGroup();
        }

        private void DrawOverviewSubTab(Hediff_SoulMatrix matrix, Pawn pawn, Rect fullRect)
        {
            float curY = 88f;
            float colWidth = (fullRect.width - 20f) / 2f;

            // Coluna Esquerda: Vigor (SP) e Energia (MP)
            Rect leftCol = new Rect(0f, curY, colWidth, 110f);

            // Barra de Vigor / Estamina
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.85f, 1f, 0.85f);
            Widgets.Label(new Rect(leftCol.x, leftCol.y, leftCol.width, 16f), string.Format("Vigor Físico (SP): {0} / {1}", Mathf.RoundToInt(matrix.currentStamina), Mathf.RoundToInt(matrix.MaxStamina)));

            Rect staminaBarRect = new Rect(leftCol.x, leftCol.y + 17f, leftCol.width, 18f);
            float staminaPct = Mathf.Clamp01(matrix.currentStamina / Mathf.Max(1f, matrix.MaxStamina));
            Widgets.FillableBar(staminaBarRect, staminaPct, StaminaFillTex, BarBgTex, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(staminaBarRect, string.Format("{0:F0}% (+{1:F1} SP/s)", staminaPct * 100f, matrix.StaminaRegenPerSecond));
            Text.Anchor = TextAnchor.UpperLeft;

            // Barra de Energia da Alma
            GUI.color = new Color(0.80f, 0.90f, 1f);
            Widgets.Label(new Rect(leftCol.x, leftCol.y + 40f, leftCol.width, 16f), string.Format("Energia da Alma (MP): {0} / {1}", Mathf.RoundToInt(matrix.currentEnergy), Mathf.RoundToInt(matrix.MaxEnergy)));

            Rect energyBarRect = new Rect(leftCol.x, leftCol.y + 57f, leftCol.width, 18f);
            float energyPct = Mathf.Clamp01(matrix.currentEnergy / Mathf.Max(1f, matrix.MaxEnergy));
            Widgets.FillableBar(energyBarRect, energyPct, EnergyFillTex, BarBgTex, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(energyBarRect, string.Format("{0} / {1} MP", Mathf.RoundToInt(matrix.currentEnergy), Mathf.RoundToInt(matrix.MaxEnergy)));
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(leftCol.x, leftCol.y + 78f, leftCol.width, 30f), string.Format("• Passiva: +{0:F0}/h ({1:F1}%) | Sono: +{2:F0}/h | Medit.: +{3:F0}/h", matrix.PassiveRegenPerHour, matrix.PassiveRegenPercentPerHour * 100f, matrix.PassiveRegenPerHour * 2f, matrix.PassiveRegenPerHour * 4f));

            string regenStatus = (matrix.manaRegenCooldownTicks > 0)
                ? string.Format("EM PAUSA PÓS-CONJURAÇÃO ({0:F1}s)", (float)matrix.manaRegenCooldownTicks / 60f)
                : string.Format("+{0:F1} MP/h (+{1:F2}/s)", matrix.CurrentEffectiveRegenPerHour, matrix.CurrentEffectiveRegenPerHour / 2500f);
            string energyTip = string.Format(
                "ENERGIA DA ALMA (MP): {0:F0} / {1:F0}\n• Taxa Atual: {2}\n• Base: 15% do MaxMP/h × (1 + 0.8% CND + 0.2% SEN) = {3:F1}%/h\n• Passiva (1x): +{4:F1} MP/h\n• Em Combate / Convocado (0.5x): +{5:F1} MP/h\n• Dormindo (2x): +{6:F1} MP/h\n• Meditando (4x): +{7:F1} MP/h\n• Pausa após gastar MP: 3.0s",
                matrix.currentEnergy,
                matrix.MaxEnergy,
                regenStatus,
                matrix.PassiveRegenPercentPerHour * 100f,
                matrix.PassiveRegenPerHour,
                matrix.PassiveRegenPerHour * 0.5f,
                matrix.PassiveRegenPerHour * 2.0f,
                matrix.PassiveRegenPerHour * 4.0f);
            TooltipHandler.TipRegion(energyBarRect, energyTip);

            // Coluna Direita: Compressão de XP e Batismo
            Rect rightCol = new Rect(colWidth + 20f, curY, colWidth, 110f);

            Text.Font = GameFont.Tiny;
            GUI.color = new Color(1f, 0.92f, 0.70f);
            Widgets.Label(new Rect(rightCol.x, rightCol.y, rightCol.width, 16f), string.Format("Compressão de XP: Nível {0}/{1}", matrix.stageLevel, matrix.MaxLevelForStage));

            Rect xpBarRect = new Rect(rightCol.x, rightCol.y + 17f, rightCol.width, 18f);
            float xpPct = Mathf.Clamp01(matrix.currentXP / Mathf.Max(1f, matrix.RequiredXPForCurrentLevel));
            Widgets.FillableBar(xpBarRect, xpPct, XpFillTex, BarBgTex, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(xpBarRect, string.Format("{0} / {1} XP ({2:F0}%)", Mathf.RoundToInt(matrix.currentXP), Mathf.RoundToInt(matrix.RequiredXPForCurrentLevel), xpPct * 100f));
            Text.Anchor = TextAnchor.UpperLeft;

            float actionBtnWidth = (DebugSettings.godMode || Prefs.DevMode) ? (rightCol.width - 100f) : rightCol.width;
            Rect btnRect = new Rect(rightCol.x, rightCol.y + 40f, actionBtnWidth, 32f);
            if (matrix.CanBreakthrough || DebugSettings.godMode)
            {
                GUI.color = new Color(1f, 0.90f, 0.35f);
                if (Widgets.ButtonText(btnRect, "✦ Realizar Batismo das Leis ✦"))
                {
                    matrix.TryBreakthrough();
                }
            }
            else if (matrix.stageLevel >= matrix.MaxLevelForStage)
            {
                GUI.color = Color.gray;
                Widgets.ButtonText(btnRect, "Comprimir Alma ao Máximo", false);
            }
            else
            {
                GUI.color = Color.gray;
                Widgets.ButtonText(btnRect, string.Format("Próximo Nível: +5 Pontos Livres", matrix.stageLevel), false);
            }

            // Ferramenta DEV rápida no painel
            if (DebugSettings.godMode || Prefs.DevMode)
            {
                Rect dev1Rect = new Rect(rightCol.x + actionBtnWidth + 4f, rightCol.y + 40f, 44f, 32f);
                GUI.color = new Color(1f, 0.75f, 0.25f);
                if (Widgets.ButtonText(dev1Rect, "+1 Nv"))
                {
                    matrix.Dev_AdvanceLevels(1);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                Rect dev10Rect = new Rect(rightCol.x + actionBtnWidth + 52f, rightCol.y + 40f, 44f, 32f);
                GUI.color = new Color(1f, 0.60f, 0.20f);
                if (Widgets.ButtonText(dev10Rect, "+10"))
                {
                    matrix.Dev_AdvanceLevels(10);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
            }

            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rightCol.x, rightCol.y + 78f, rightCol.width, 30f), string.Format("+5 Atr & +2 Hab/nível | Pontos de Habilidade: {0}", matrix.skillPoints));

            curY += 114f;
            Widgets.DrawLineHorizontal(0f, curY, fullRect.width);
            curY += 6f;

            // Seção dos 5 Atributos Primários
            Rect attrHeaderRect = new Rect(0f, curY, fullRect.width - 200f, 22f);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Widgets.Label(attrHeaderRect, "Atributos Primários do Cultivador");

            Rect pointsBadgeRect = new Rect(fullRect.width - 200f, curY, 200f, 22f);
            if (matrix.unallocatedPoints > 0)
            {
                GUI.color = new Color(1f, 0.85f, 0.20f);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(pointsBadgeRect, string.Format("★ Pontos Disponíveis: +{0}", matrix.unallocatedPoints));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                GUI.color = Color.gray;
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(pointsBadgeRect, "0 Pontos Disponíveis");
                Text.Anchor = TextAnchor.UpperLeft;
            }
            curY += 24f;

            // Renderização de cada linha de atributo
            DrawAttributeRow(0, "Força (STR)", matrix.strength, CultivatorAttributeType.Strength, matrix, ref curY, fullRect.width);
            DrawAttributeRow(1, "Constituição (CON)", matrix.constitution, CultivatorAttributeType.Constitution, matrix, ref curY, fullRect.width);
            DrawAttributeRow(2, "Agilidade (AGI)", matrix.agility, CultivatorAttributeType.Agility, matrix, ref curY, fullRect.width);
            DrawAttributeRow(3, "Sentidos (SEN)", matrix.senses, CultivatorAttributeType.Senses, matrix, ref curY, fullRect.width);
            DrawAttributeRow(4, "Condutividade (CND)", matrix.conductivity, CultivatorAttributeType.Conductivity, matrix, ref curY, fullRect.width);

            curY += 4f;
            Widgets.DrawLineHorizontal(0f, curY, fullRect.width);
            curY += 8f;

            // Seção Inferior: Pérola Mágica (ou Caminho Humano Padrão)
            if (matrix.isMagicPearlBearer)
            {
                Rect pearlHeaderRect = new Rect(0f, curY, fullRect.width, 24f);
                Text.Font = GameFont.Small;
                GUI.color = new Color(1f, 0.85f, 0.35f);
                Widgets.Label(pearlHeaderRect, string.Format("★ A Pérola Mágica: Galeria de Linhagens Catalogadas ({0})", matrix.catalogedBloodlines != null ? matrix.catalogedBloodlines.Count : 0));
                curY += 24f;

                if (matrix.IsMetamorphosed)
                {
                    Rect formActiveRect = new Rect(0f, curY, fullRect.width - 190f, 26f);
                    GUI.color = new Color(0.40f, 0.95f, 0.60f);
                    Text.Font = GameFont.Small;
                    Widgets.Label(formActiveRect, string.Format("Forma Ativa: {0} [{1}]", matrix.activeForm.Label, matrix.activeForm.StageLabelPT));

                    Rect revertBtnRect = new Rect(fullRect.width - 180f, curY, 180f, 24f);
                    GUI.color = new Color(1f, 0.55f, 0.45f);
                    if (Widgets.ButtonText(revertBtnRect, "⟲ Reverter Forma Humana"))
                    {
                        matrix.RevertToHuman(true);
                    }
                    curY += 28f;
                }

                if (matrix.catalogedBloodlines == null || matrix.catalogedBloodlines.Count == 0)
                {
                    Rect emptyRect = new Rect(0f, curY, fullRect.width, 45f);
                    GUI.color = Color.gray;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(emptyRect, "Nenhuma espécie animal assimilada até o momento.\nUse o feitiço 'Absorver Alma de Cadáver' em animais caídos pelo mapa para extrair seu código genético e gravá-lo na Pérola Mágica.");
                }
                else
                {
                    float viewHeight = matrix.catalogedBloodlines.Count * 52f;
                    Rect outRect = new Rect(0f, curY, fullRect.width, fullRect.height - curY - 4f);
                    Rect viewRect = new Rect(0f, 0f, fullRect.width - 18f, Mathf.Max(viewHeight, outRect.height));

                    Widgets.BeginScrollView(outRect, ref bloodlineScrollPos, viewRect);
                    float rowY = 0f;

                    for (int i = 0; i < matrix.catalogedBloodlines.Count; i++)
                    {
                        CatalogedBloodline bl = matrix.catalogedBloodlines[i];
                        if (bl == null || bl.kindDef == null) continue;

                        Rect rowRect = new Rect(0f, rowY, viewRect.width, 48f);
                        Widgets.DrawHighlightIfMouseover(rowRect);

                        Text.Font = GameFont.Small;
                        GUI.color = Color.white;
                        Widgets.Label(new Rect(5f, rowY + 3f, 210f, 20f), bl.Label);

                        GUI.color = bl.StageColor;
                        Text.Font = GameFont.Tiny;
                        Widgets.Label(new Rect(5f, rowY + 23f, 210f, 18f), string.Format("[{0}] | {1} assimilados", bl.StageLabelPT, bl.corpsesAbsorbed));

                        Rect matBarRect = new Rect(220f, rowY + 12f, 160f, 18f);
                        float progressPct = Mathf.Clamp01(bl.stageProgress);
                        Widgets.FillableBar(matBarRect, progressPct, MaturationFillTex, BarBgTex, true);
                        Text.Anchor = TextAnchor.MiddleCenter;
                        GUI.color = Color.white;
                        Widgets.Label(matBarRect, string.Format("Maturação: {0:F0}%", progressPct * 100f));
                        Text.Anchor = TextAnchor.UpperLeft;

                        float cost = matrix.CalculateMetamorphosisEnergyCost(bl);
                        Rect actionBtnRect = new Rect(viewRect.width - 150f, rowY + 7f, 145f, 28f);

                        if (matrix.activeForm == bl)
                        {
                            GUI.color = Color.cyan;
                            Widgets.ButtonText(actionBtnRect, "✓ Forma Ativa", false);
                        }
                        else
                        {
                            bool canAfford = matrix.currentEnergy >= cost;
                            GUI.color = canAfford ? new Color(0.95f, 0.85f, 0.40f) : Color.gray;
                            string btnText = string.Format("Transformar ({0} E)", Mathf.RoundToInt(cost));
                            if (Widgets.ButtonText(actionBtnRect, btnText))
                            {
                                matrix.TryMetamorphose(bl);
                            }
                        }

                        rowY += 50f;
                    }

                    Widgets.EndScrollView();
                }
            }
            else
            {
                Rect commonNoteRect = new Rect(0f, curY + 4f, fullRect.width, 45f);
                GUI.color = new Color(0.80f, 0.88f, 0.95f);
                Text.Font = GameFont.Tiny;
                Widgets.Label(commonNoteRect, "★ O Caminho do Cultivo Humano Padrão:\nComo um cultivador sem a mística Pérola Mágica, sua anatomia permanece humana. Seu poder ascende através do refinamento dos 5 atributos com os pontos ganhos a cada nível, fortalecimento das reservas de vigor e compressão da alma rumo ao Batismo das Leis.");
            }
        }

        private void DrawGrimoireSubTab(Hediff_SoulMatrix matrix, Pawn pawn, Rect fullRect)
        {
            float curY = 88f;

            // Barra Superior: Pontos de Habilidade & Ferramentas DEV
            Rect headerStrip = new Rect(0f, curY, fullRect.width, 26f);
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 0.85f, 0.25f);
            Widgets.Label(new Rect(0f, curY + 2f, 280f, 22f), string.Format("★ Pontos de Habilidade: {0}", matrix.skillPoints));

            if (Prefs.DevMode || DebugSettings.godMode)
            {
                GUI.color = new Color(1f, 0.72f, 0.25f);
                if (Widgets.ButtonText(new Rect(fullRect.width - 280f, curY, 75f, 24f), "+10 Pts"))
                {
                    matrix.skillPoints += 10;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
                if (Widgets.ButtonText(new Rect(fullRect.width - 200f, curY, 96f, 24f), "Aprender All"))
                {
                    List<RebornSpellEntry> all = RebornSpellDatabase.AllSpells;
                    for (int i = 0; i < all.Count; i++)
                    {
                        CultivatorAbilityRecord r = matrix.GetAbilityRecord(all[i].baseSpellId);
                        r.isLearned = true;
                    }
                    matrix.SyncAbilities();
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
                if (Widgets.ButtonText(new Rect(fullRect.width - 99f, curY, 99f, 24f), "Evoluir All"))
                {
                    List<RebornSpellEntry> all = RebornSpellDatabase.AllSpells;
                    for (int i = 0; i < all.Count; i++)
                    {
                        CultivatorAbilityRecord r = matrix.FindAbilityRecord(all[i].baseSpellId);
                        if (r != null && r.isLearned)
                        {
                            r.currentSpellStage = (matrix.currentStage > SoulClassStage.Grandmaster) ? SoulClassStage.Grandmaster : matrix.currentStage;
                        }
                    }
                    matrix.SyncAbilities();
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
            }
            curY += 28f;

            // Banner do Foco de Meditação
            Rect focusBannerRect = new Rect(0f, curY, fullRect.width, 26f);
            Widgets.DrawLightHighlight(focusBannerRect);
            Text.Font = GameFont.Tiny;
            string focusedDefName = matrix.meditationFocusAbility;
            if (!string.IsNullOrEmpty(focusedDefName))
            {
                RebornSpellEntry fEntry = RebornSpellDatabase.GetEntry(focusedDefName);
                string fLabel = (fEntry != null) ? fEntry.labelPT : focusedDefName;
                GUI.color = new Color(1f, 0.85f, 0.25f);
                Widgets.Label(new Rect(6f, curY + 4f, fullRect.width - 130f, 20f), string.Format("★ Foco na Meditação: {0} (+10 Prof/h)", fLabel));

                GUI.color = new Color(1f, 0.50f, 0.40f);
                if (Widgets.ButtonText(new Rect(fullRect.width - 120f, curY + 2f, 115f, 22f), "Desmarcar Foco"))
                {
                    matrix.meditationFocusAbility = null;
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                }
            }
            else
            {
                GUI.color = Color.gray;
                Widgets.Label(new Rect(6f, curY + 4f, fullRect.width, 20f), "★ Foco na Meditação: Nenhuma técnica fixada (Clique em 'Focar' para treinar enquanto medita)");
            }
            curY += 29f;

            // Filtros por Escola / Estado (2 linhas compactas)
            string[] filterIds = new string[]
            {
                "All", "Learned", "Equipped", "ShadowsDeath", "SoulMind", "FireHeat", "BloodLife",
                "EarthGravity", "WindLightningFrost", "SpaceTime", "SupportMartial", "UtilityLogistics", "Pinnacle"
            };
            string[] filterLabels = new string[]
            {
                "Todas (61)", "Aprendidas", "Equipadas", "Sombras", "Alma", "Fogo", "Sangue",
                "Terra/Grav", "Vento/Gelo", "Espaço/Tempo", "Marcial/Sup", "Utilidade", "Supremas"
            };

            float btnW7 = (fullRect.width - 18f) / 7f;
            float btnW6 = (fullRect.width - 15f) / 6f;
            Text.Font = GameFont.Tiny;
            for (int f = 0; f < 7; f++)
            {
                Rect fRect = new Rect(f * (btnW7 + 3f), curY, btnW7, 20f);
                GUI.color = (grimoireFilter == filterIds[f]) ? new Color(0.35f, 0.85f, 1f) : new Color(0.75f, 0.75f, 0.75f);
                if (Widgets.ButtonText(fRect, filterLabels[f])) { grimoireFilter = filterIds[f]; grimoireScrollPos = Vector2.zero; }
            }
            curY += 22f;
            for (int f = 7; f < 13; f++)
            {
                Rect fRect = new Rect((f - 7) * (btnW6 + 3f), curY, btnW6, 20f);
                GUI.color = (grimoireFilter == filterIds[f]) ? new Color(0.35f, 0.85f, 1f) : new Color(0.75f, 0.75f, 0.75f);
                if (Widgets.ButtonText(fRect, filterLabels[f])) { grimoireFilter = filterIds[f]; grimoireScrollPos = Vector2.zero; }
            }
            curY += 24f;
            Widgets.DrawLineHorizontal(0f, curY, fullRect.width);
            curY += 5f;

            // Lista filtrada das 61 magias
            List<RebornSpellEntry> filtered = GetFilteredSpells(matrix);
            Rect outRect = new Rect(0f, curY, fullRect.width, fullRect.height - curY - 4f);
            float viewHeight = filtered.Count * 66f;
            Rect viewRect = new Rect(0f, 0f, fullRect.width - 18f, Mathf.Max(viewHeight, outRect.height));

            Widgets.BeginScrollView(outRect, ref grimoireScrollPos, viewRect);
            float rowY = 0f;
            for (int i = 0; i < filtered.Count; i++)
            {
                RebornSpellEntry entry = filtered[i];
                CultivatorAbilityRecord rec = matrix.FindAbilityRecord(entry.baseSpellId);
                DrawGrimoireAbilityRow(entry, rec, matrix, pawn, rowY, viewRect.width);
                rowY += 66f;
            }
            Widgets.EndScrollView();
        }

        private List<RebornSpellEntry> GetFilteredSpells(Hediff_SoulMatrix matrix)
        {
            List<RebornSpellEntry> all = RebornSpellDatabase.AllSpells;
            if (grimoireFilter == "All") return all;

            List<RebornSpellEntry> result = new List<RebornSpellEntry>();
            for (int i = 0; i < all.Count; i++)
            {
                RebornSpellEntry e = all[i];
                if (grimoireFilter == "Learned")
                {
                    CultivatorAbilityRecord r = matrix.FindAbilityRecord(e.baseSpellId);
                    if (r != null && r.isLearned) result.Add(e);
                }
                else if (grimoireFilter == "Equipped")
                {
                    CultivatorAbilityRecord r = matrix.FindAbilityRecord(e.baseSpellId);
                    if (r != null && r.isLearned && r.isEquipped) result.Add(e);
                }
                else if (e.schoolId == grimoireFilter)
                {
                    result.Add(e);
                }
            }
            return result;
        }

        private void DrawGrimoireAbilityRow(RebornSpellEntry entry, CultivatorAbilityRecord rec, Hediff_SoulMatrix matrix, Pawn pawn, float rowY, float rowWidth)
        {
            Rect rowRect = new Rect(0f, rowY, rowWidth, 62f);
            Widgets.DrawHighlightIfMouseover(rowRect);

            bool learned = (rec != null && rec.isLearned);
            SoulClassStage shownStage = learned ? rec.currentSpellStage : SoulClassStage.Mortal;
            AbilityDef aDef = entry.GetAbilityDef(shownStage);
            RebornSpellExtension ext = (aDef != null) ? aDef.GetModExtension<RebornSpellExtension>() : null;

            // Ícone da magia (44x44)
            Rect iconRect = new Rect(4f, rowY + 9f, 44f, 44f);
            Texture2D iconTex = (aDef != null && aDef.uiIcon != null) ? aDef.uiIcon : BaseContent.BadTex;
            GUI.color = learned ? Color.white : new Color(0.65f, 0.65f, 0.65f);
            GUI.DrawTexture(iconRect, iconTex);

            // Título + Estágio + Custo
            float textX = 54f;
            Text.Font = GameFont.Small;
            Color stColor = SoulStageDatabase.GetConfig(shownStage).ThemeColor;
            GUI.color = learned ? stColor : new Color(0.82f, 0.82f, 0.85f);
            string stageTag = learned ? (" [" + rec.StageShortLabel + "]") : " [Não Aprendida]";
            Widgets.Label(new Rect(textX, rowY + 2f, 220f, 22f), entry.labelPT + stageTag);

            Text.Font = GameFont.Tiny;
            string costSummary = "";
            if (ext != null)
            {
                if (ext.mpCost > 0f && ext.spCost > 0f) costSummary = string.Format("{0:F0} MP + {1:F0} SP", ext.mpCost, ext.spCost);
                else if (ext.mpPercentCost > 0f) costSummary = string.Format("{0:F0}% MP (min {1:F0})", ext.mpPercentCost * 100f, ext.mpCost);
                else if (ext.distanceQuadraticCost) costSummary = string.Format("0.4*d^2 MP (min {0:F0})", ext.mpCost);
                else if (ext.mpCost > 0f) costSummary = string.Format("{0:F0} MP", ext.mpCost);
                else if (ext.spCost > 0f) costSummary = string.Format("{0:F0} SP", ext.spCost);
            }

            if (!learned)
            {
                GUI.color = Color.gray;
                Widgets.Label(new Rect(textX, rowY + 24f, rowWidth - textX - 135f, 34f), string.Format("{0} ({1})\n{2}", entry.schoolLabelPT, costSummary, entry.shortDesc));

                int ptCost = (ext != null) ? ext.skillPointCost : 1;
                bool canAfford = (matrix.skillPoints >= ptCost) || DebugSettings.godMode;
                GUI.color = canAfford ? new Color(0.35f, 0.95f, 0.55f) : Color.gray;
                if (Widgets.ButtonText(new Rect(rowWidth - 125f, rowY + 14f, 120f, 32f), string.Format("Aprender ({0} Pt)", ptCost)))
                {
                    matrix.TryLearnSpell(entry.baseSpellId, ptCost);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
            }
            else
            {
                GUI.color = rec.TierColor;
                Widgets.Label(new Rect(textX, rowY + 24f, 220f, 18f), string.Format("[{0}] Custo x{1:F2} | Poder x{2:F2}", rec.TierLabel, rec.CostMultiplier, rec.PowerMultiplier));

                GUI.color = new Color(0.75f, 0.80f, 0.85f);
                Widgets.Label(new Rect(textX, rowY + 41f, 220f, 18f), string.Format("{0} • {1}", entry.schoolLabelPT, costSummary));

                // Barra de Proficiência
                Rect barRect = new Rect(275f, rowY + 20f, 130f, 20f);
                if (rec.masteryTier >= 4)
                {
                    Widgets.FillableBar(barRect, 1.0f, MasteryFillTex, BarBgTex, true);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color = new Color(1f, 0.90f, 0.35f);
                    Widgets.Label(barRect, "★ MESTRIA MÁXIMA");
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else
                {
                    float pct = Mathf.Clamp01(rec.proficiency / 100f);
                    Widgets.FillableBar(barRect, pct, MasteryFillTex, BarBgTex, true);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color = Color.white;
                    Widgets.Label(barRect, string.Format("Prof: {0:F0}/100", rec.proficiency));
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                // Botão Equipar / Desequipar
                float btnX = 412f;
                GUI.color = rec.isEquipped ? new Color(0.35f, 0.95f, 0.55f) : new Color(0.75f, 0.75f, 0.75f);
                if (Widgets.ButtonText(new Rect(btnX, rowY + 6f, 82f, 24f), rec.isEquipped ? "Equipado ✓" : "Equipar"))
                {
                    matrix.ToggleEquipSpell(entry.baseSpellId);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                // Botão Focar Meditação
                bool isFocused = (matrix.meditationFocusAbility == entry.baseSpellId);
                GUI.color = isFocused ? new Color(1f, 0.85f, 0.20f) : Color.white;
                if (Widgets.ButtonText(new Rect(btnX, rowY + 32f, 82f, 24f), isFocused ? "★ Foco" : "Focar"))
                {
                    matrix.meditationFocusAbility = isFocused ? null : entry.baseSpellId;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }

                // Botão Evoluir Separadamente (quando spell < cultivatorStage e <= Grandmaster) + Botão Esquecer
                bool behindStage = ((int)rec.currentSpellStage < (int)matrix.currentStage) && (rec.currentSpellStage < SoulClassStage.Grandmaster);
                if (behindStage)
                {
                    bool canEvolve = rec.CanEvolveStage(matrix.currentStage);
                    GUI.color = canEvolve ? new Color(1f, 0.85f, 0.25f) : Color.gray;
                    Rect evoRect = new Rect(btnX + 85f, rowY + 6f, 78f, 24f);
                    if (Widgets.ButtonText(evoRect, canEvolve ? "⬆ Evoluir" : "Perito 3"))
                    {
                        if (canEvolve)
                        {
                            matrix.TryManualEvolveSpell(entry.baseSpellId);
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        }
                        else
                        {
                            Messages.Message("Esta técnica precisa atingir ao menos Maestria [Perito] (Nível 3) antes de evoluir de estágio.", pawn, MessageTypeDefOf.RejectInput);
                        }
                    }
                    TooltipHandler.TipRegion(evoRect, canEvolve
                        ? "Evolui a técnica para o próximo estágio (reinicia Maestria para Iniciado/Adepto)."
                        : "Requer Maestria [Perito] (Nível 3) para evoluir para o próximo estágio.");
                }

                Rect forgetRect = behindStage
                    ? new Rect(btnX + 85f, rowY + 32f, 78f, 24f)
                    : new Rect(btnX + 85f, rowY + 18f, 78f, 26f);
                int ptRefund = (ext != null) ? ext.skillPointCost : 1;
                GUI.color = new Color(1f, 0.45f, 0.42f);
                if (Widgets.ButtonText(forgetRect, "Esquecer"))
                {
                    if (Event.current != null && Event.current.shift)
                    {
                        matrix.TryForgetSpell(entry.baseSpellId, ptRefund);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    else
                    {
                        string confirmMsg = string.Format(
                            "Tem certeza que deseja esquecer a técnica {0} [{1} - {2}]?\n\n• +{3} Ponto(s) de Habilidade serão devolvidos.\n• O estágio e a maestria desta técnica serão reiniciados.\n\n(Dica: Segure Shift ao clicar em 'Esquecer' para pular esta confirmação.)",
                            entry.labelPT, rec.StageShortLabel, rec.TierLabel, ptRefund);
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(confirmMsg, delegate
                        {
                            matrix.TryForgetSpell(entry.baseSpellId, ptRefund);
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }));
                    }
                }
                TooltipHandler.TipRegion(forgetRect, "Esquece esta técnica, removendo-a da barra e devolvendo o Ponto de Habilidade gasto.\n(Segure Shift + Clique para esquecer instantaneamente sem confirmação.)");

                // Botão DEV +25
                if (DebugSettings.godMode || Prefs.DevMode)
                {
                    GUI.color = new Color(0.95f, 0.65f, 0.25f);
                    if (Widgets.ButtonText(new Rect(rowWidth - 40f, rowY + 16f, 36f, 28f), "+25"))
                    {
                        rec.AddProficiency(25f, pawn, matrix.currentStage);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                }
            }

            string tipText = (aDef != null)
                ? string.Format("{0}\n\n{1}\n\n• Escola: {2}\n• Custo base: {3}", aDef.label.ToUpper(), aDef.description, entry.schoolLabelPT, costSummary)
                : entry.shortDesc;
            TooltipHandler.TipRegion(rowRect, tipText);
        }

        private void DrawAttributeRow(int index, string label, int value, CultivatorAttributeType attrType, Hediff_SoulMatrix matrix, ref float curY, float totalWidth)
        {
            Rect rowRect = new Rect(0f, curY, totalWidth, 23f);
            Widgets.DrawHighlightIfMouseover(rowRect);

            // Botão único (+1 / Shift +5 / Alt +10)
            int ptsToAdd = 1;
            if (Event.current != null && Event.current.alt)
            {
                ptsToAdd = 10;
            }
            else if (Event.current != null && Event.current.shift)
            {
                ptsToAdd = 5;
            }
            string btnLabel = (ptsToAdd == 10) ? "+10" : ((ptsToAdd == 5) ? "+5" : "+");

            Rect btn1Rect = new Rect(0f, curY, 22f, 20f);
            if (matrix.unallocatedPoints >= 1)
            {
                GUI.color = new Color(1f, 0.85f, 0.25f);
                if (Widgets.ButtonText(btn1Rect, btnLabel))
                {
                    if (matrix.TryAllocatePoints(attrType, ptsToAdd))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                }
            }
            else
            {
                GUI.color = new Color(0.40f, 0.40f, 0.40f);
                Widgets.ButtonText(btn1Rect, btnLabel, true, false, false);
            }
            TooltipHandler.TipRegion(btn1Rect, "Clique: Adicionar +1 ponto\nShift + Clique: Adicionar +5 pontos\nAlt + Clique: Adicionar +10 pontos");

            // Nome do Atributo
            Rect nameRect = new Rect(26f, curY + 1f, 150f, 20f);
            Text.Font = GameFont.Small;
            Text.WordWrap = false;
            GUI.color = Color.white;
            Widgets.Label(nameRect, label);

            // Quantidade / Valor do Atributo (coluna dedicada, sempre visível e destacada em ciano)
            Rect valueRect = new Rect(178f, curY + 1f, 36f, 20f);
            Text.Font = GameFont.Small;
            GUI.color = new Color(0.35f, 0.85f, 1f);
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(valueRect, value.ToString());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;

            // Efeitos e Bônus Derivados
            Rect detailsRect = new Rect(218f, curY + 2f, totalWidth - 218f, 18f);
            Text.Font = GameFont.Tiny;
            Text.WordWrap = false;
            GUI.color = new Color(0.82f, 0.85f, 0.88f);

            string detailsText = "";
            string tipText = "";

            switch (attrType)
            {
                case CultivatorAttributeType.Strength:
                    float meleeDmg = value * 1.5f;
                    float carry = value * 3.0f;
                    float mining = value * 1.2f;
                    float armorPen = 0.80f * ((float)value / (value + 150f)) * 100f;
                    float stunCh = matrix.StunChance * 100f;
                    detailsText = string.Format("Dano Melee: +{0:F0}% | Carga: +{1:F0} kg | Mineração: +{2:F0}%", meleeDmg, carry, mining);
                    tipText = string.Format("FORÇA (STR) — POTÊNCIA TITÂNICA:\n• +1.5% de Dano Físico Melee por ponto (Atual: +{0:F1}%)\n• +3.0 kg de Capacidade de Transporte (Atual: +{1:F0} kg)\n• Penetração de Armadura física aumentada (Atual: +{2:F1}%)\n• Chance de atordoar/desequilibrar alvos em combate corporal (Atual: {3:F1}%)\n• +1.2% de Velocidade de Mineração e +1.0% de Construção\n• +1.5 de Vigor Máximo (SP) por ponto", meleeDmg, carry, armorPen, stunCh);
                    break;

                case CultivatorAttributeType.Constitution:
                    float hpBonus = value * 1.0f;
                    float mitigation = (1.0f - matrix.DamageMitigationFactor) * 100f;
                    float tempRange = value * 0.5f;
                    detailsText = string.Format("Vida: +{0:F0}% | Mitigação: {1:F1}% | Temp: -{2:F0}/+{2:F0}°C", hpBonus, mitigation, tempRange);
                    tipText = string.Format("CONSTITUIÇÃO (CON) — CORPO DE DIAMANTE:\n• +1.0% de Vida Máxima (HealthScale) por ponto (Atual: +{0:F1}%)\n• Mitigação passiva de dano recebido (Atual: {1:F1}%)\n• Amplitude Térmica: suporta -{2:F1}°C de frio e +{2:F1}°C de calor\n• +1.0% de velocidade de ganho de imunidade a doenças por ponto\n• +0.8% de resistência a toxinas e poluição ambiental por ponto\n• Reduz a necessidade de comida (-0.4%/pt) e sono (-0.5%/pt)\n• +2.0% de taxa de cicatrização natural de ferimentos por ponto\n• +2.5 de Vigor Máximo (SP) e +1.5% na regeneração de vigor", hpBonus, mitigation, tempRange);
                    break;

                case CultivatorAttributeType.Agility:
                    float speedBonus = value * 0.06f;
                    float dodgeBonus = 0.75f * ((float)value / (value + 200f)) * 100f;
                    float projDodge = matrix.ProjectileDodgeChance * 100f;
                    float cdRed = Mathf.Min(70f, value * 0.5f);
                    detailsText = string.Format("Velocidade: +{0:F2} c/s | Desvio: {1:F1}% | Cadência: +{2:F0}%", speedBonus, projDodge, cdRed);
                    tipText = string.Format("AGILIDADE (AGI) — VELOCIDADE DIVINA & REFLEXO:\n• Aumenta a velocidade de movimento no grid sem teto (+0.06 c/s linear contínuo por ponto - Atual: +{0:F2} c/s)\n• Esquiva em combate corpo a corpo (Atual: +{1:F1}%)\n• Esquiva Fantasmagórica: chance de desviar de tiros e projéteis balísticos (Atual: {2:F1}%)\n• Acelera a cadência de ataques corporais (-{3:F0}% de cooldown melee)\n• +1.0% de velocidade em tarefas manuais finas (alfaiataria, forja, cirurgia)", speedBonus, dodgeBonus, projDodge, cdRed);
                    break;

                case CultivatorAttributeType.Senses:
                    float shootBonus = value * 0.4f;
                    float critCh = matrix.CritChance * 100f;
                    float critDmg = matrix.CritDamageMultiplier;
                    float mentalStab = value * 0.2f;
                    detailsText = string.Format("Precisão: +{0:F0}% | Crítico: {1:F1}% ({2:F1}x) | Estabilidade: +{3:F0}%", shootBonus, critCh, critDmg, mentalStab);
                    tipText = string.Format("SENTIDOS (SEN) — PERCEPÇÃO ABSOLUTA & LETALIDADE:\n• +0.4% de Precisão de Tiro à distância por ponto (Atual: +{0:F1}%)\n• Chance de Acerto Crítico Físico e Espiritual (Atual: {1:F1}%)\n• Multiplicador de Dano Crítico nos pontos vitais (Atual: {2:F2}x de dano)\n• +0.4% de Visão e Audição por ponto\n• +0.8% na velocidade de aprendizado global (GlobalLearningFactor)\n• Estabilidade Mental: reduz em -0.2% por ponto o limiar de colapso mental", shootBonus, critCh, critDmg);
                    break;

                case CultivatorAttributeType.Conductivity:
                    float mpMult = Hediff_SoulMatrix.CalculateConductivityEnergyMultiplier(value);
                    float mpPct = (mpMult - 1.0f) * 100f;
                    float mpMax = matrix.MaxEnergy;
                    float spellDmg = value * 1.5f;
                    float nature = value * 0.8f;
                    detailsText = string.Format("Mana Máx: +{0:F1}% ({1:F0} MP) | Dano Feitiço: +{2:F0}% | Natureza: +{3:F0}%", mpPct, mpMax, spellDmg, nature);
                    tipText = string.Format("CONDUTIVIDADE (CND) — CANAL ESPIRITUAL & FAÍXA ÉPICA:\n• +3.0% composto até 50 CND e +0.5% linear acima de 50 sobre o BaseMP do Estágio (Atual: +{0:F1}% / {1:F0} MP)\n• +1.5% de dano em feitiços por ponto (Atual: +{2:F1}%)\n• +1.5% de potência e durabilidade em escudos e curas\n• +0.8% na regeneração passiva de mana por hora (base de 15% do MaxMP/h)\n• +1.0% de Sensibilidade Psíquica e +0.8% de afinidade natural", mpPct, mpMax, spellDmg);
                    break;
            }

            Widgets.Label(detailsRect, detailsText);
            Text.WordWrap = true;
            TooltipHandler.TipRegion(rowRect, tipText);

            curY += 21f;
        }
    }
}
