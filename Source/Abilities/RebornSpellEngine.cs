using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RebornEvolvingMagic
{
    public class RebornSpellExtension : DefModExtension
    {
        public string baseSpellId;
        public string schoolId;
        public SoulClassStage spellStage = SoulClassStage.Mortal;
        public int skillPointCost = 1;
        public float mpCost = 0f;
        public float spCost = 0f;
        public float mpPercentCost = 0f;
        public bool distanceQuadraticCost = false;
        public float baseDamage = 0f;
        public float baseHealOrShield = 0f;
        public float effectRadius = 0f;
        public float range = 20f;
        public int durationTicks = 600;
        public float armorPenetration = 0.20f;
        public int extraTargets = 1;
    }

    public class RebornSpellEntry
    {
        public int catalogNumber;
        public string baseSpellId;
        public string labelPT;
        public string schoolId;
        public string schoolLabelPT;
        public string shortDesc;

        private AbilityDef[] cachedDefs = null;

        public RebornSpellEntry(int num, string id, string label, string school, string schoolPT, string desc)
        {
            catalogNumber = num;
            baseSpellId = id;
            labelPT = label;
            schoolId = school;
            schoolLabelPT = schoolPT;
            shortDesc = desc;
        }

        public AbilityDef GetAbilityDef(SoulClassStage stage)
        {
            if (cachedDefs == null)
            {
                cachedDefs = new AbilityDef[5];
                cachedDefs[0] = DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_" + baseSpellId + "_Mortal");
                cachedDefs[1] = DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_" + baseSpellId + "_Earth");
                cachedDefs[2] = DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_" + baseSpellId + "_Sky");
                cachedDefs[3] = DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_" + baseSpellId + "_Master");
                cachedDefs[4] = DefDatabase<AbilityDef>.GetNamedSilentFail("REM_Ability_" + baseSpellId + "_Grandmaster");
            }
            int idx = (int)stage;
            if (idx < 0) idx = 0;
            if (idx > 4) idx = 4;
            return cachedDefs[idx];
        }
    }

    public static class RebornSpellDatabase
    {
        private static List<RebornSpellEntry> allSpells = null;

        public static List<RebornSpellEntry> AllSpells
        {
            get
            {
                if (allSpells == null)
                {
                    BuildCatalog();
                }
                return allSpells;
            }
        }

        public static RebornSpellEntry GetEntry(string baseSpellId)
        {
            List<RebornSpellEntry> list = AllSpells;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].baseSpellId == baseSpellId) return list[i];
            }
            return null;
        }

        private static void Add(int num, string id, string label, string school, string schoolPT, string desc)
        {
            allSpells.Add(new RebornSpellEntry(num, id, label, school, schoolPT, desc));
        }

        private static void BuildCatalog()
        {
            allSpells = new List<RebornSpellEntry>();

            // 1. Sombras & Morte (2x no escuro, enfraquecido no dia a céu aberto)
            Add(1, "ShadowStrike", "Shadow Strike", "ShadowsDeath", "Sombras & Morte", "Bote sombrio com dano cortante, crítico elevado e sangramento.");
            Add(2, "ShadowStep", "Shadow Step", "ShadowsDeath", "Sombras & Morte", "Teleporte tático nas sombras com bônus breve de esquiva.");
            Add(3, "ShadowCloak", "Shadow Cloak", "ShadowsDeath", "Sombras & Morte", "Véu de evasão sombria contra tiros e ataques.");
            Add(4, "DeathRay", "Death Ray", "ShadowsDeath", "Sombras & Morte", "Feixe necrótico que ignora armadura e reduz cicatrização.");
            Add(5, "NecroticTouch", "Necrotic Touch", "ShadowsDeath", "Sombras & Morte", "Aplica decomposição progressiva ou nuvem tóxica em cadáver.");
            Add(6, "DarknessDomain", "Darkness Domain", "ShadowsDeath", "Sombras & Morte", "Zona persistente de escuridão que desacelera e cega inimigos.");

            // 2. Alma & Mente
            Add(7, "SoulSpike", "Soul Spike", "SoulMind", "Alma & Mente", "Disparo perfurante linear que atravessa múltiplos alvos e atordoa.");
            Add(8, "SoulSlash", "Soul Slash", "SoulMind", "Alma & Mente", "Onda em arco espiritual que ignora parte da armadura física.");
            Add(9, "MindBarrier", "Mind Barrier", "SoulMind", "Alma & Mente", "Buff fixo de alta resistência psíquica e estabilidade mental.");
            Add(10, "SoulShield", "Soul Shield", "SoulMind", "Alma & Mente", "Barreira contínua reativa com imunidade psíquica/espiritual.");
            Add(11, "SpiritualScan", "Spiritual Scan", "SoulMind", "Alma & Mente", "Varredura que expõe pontos fracos (+crítico e precisão).");
            Add(12, "SoulSiphon", "Soul Siphon", "SoulMind", "Alma & Mente", "Feixe que causa dano à alma e drena HP, MP e XP.");
            Add(13, "SoulBreak", "Soul Break", "SoulMind", "Alma & Mente", "Sobrecarga neural não-letal para nocautear e capturar alvos.");
            Add(14, "SerenityAura", "Serenity Aura", "SoulMind", "Alma & Mente", "Pulso calmante que interrompe colapsos mentais aliados.");

            // 3. Fogo & Calor
            Add(15, "Fireball", "Fireball", "FireHeat", "Fogo & Calor", "Projétil explosivo de curto raio sem deixar rastro de fogo.");
            Add(16, "SolarSphere", "Solar Sphere", "FireHeat", "Fogo & Calor", "Esfera luminosa e térmica (24h) que ilumina e aquece salas.");
            Add(17, "Pyroblast", "Pyroblast", "FireHeat", "Fogo & Calor", "Bola de fogo colossal com explosão em área ampla.");
            Add(18, "BlazingLance", "Blazing Lance", "FireHeat", "Fogo & Calor", "Lança de fogo branco concentrada de altíssima perfuração.");
            Add(19, "FlameBurst", "Flame Burst", "FireHeat", "Fogo & Calor", "Onda circular de chamas ao redor com repulsão corpo a corpo.");
            Add(20, "InfernoWall", "Inferno Wall", "FireHeat", "Fogo & Calor", "Muralha circular de fogo que queima projéteis e reduz magias.");

            // 4. Sangue & Vida
            Add(21, "BloodLance", "Blood Lance", "BloodLife", "Sangue & Vida", "Lança de sangue endurecido com hemorragia severa.");
            Add(22, "CrimsonShield", "Crimson Shield", "BloodLife", "Sangue & Vida", "Barreira carmesim que reflete cortes e drena vida no melee.");
            Add(23, "BloodControl", "Blood Control", "BloodLife", "Sangue & Vida", "Espasmos sanguíneos ou fúria temporária contra aliados do alvo.");
            Add(24, "HealingLight", "Healing Light", "BloodLife", "Sangue & Vida", "Trata ferimentos, restaura partes arrancadas e dobra regen (24h).");
            Add(25, "VitalSurge", "Vital Surge", "BloodLife", "Sangue & Vida", "Remove debuffs de saúde e eleva imunidade/regen por 12h.");
            Add(26, "VitalityWave", "Vitality Wave", "BloodLife", "Sangue & Vida", "Pulso em área que estanca sangramentos e cura aliados próximos.");
            Add(27, "CellularPurification", "Cellular Purification", "BloodLife", "Sangue & Vida", "Purifica toxinas, intoxicação alimentar e acelera imunidade.");
            Add(28, "SoulResurrection", "Soul Resurrection", "BloodLife", "Sangue & Vida", "Ressuscita colono ou animal abatido recentemente.");

            // 5. Terra & Gravidade
            Add(29, "GravityWell", "Gravity Well", "EarthGravity", "Terra & Gravidade", "Singularidade que puxa e agrupa inimigos ao centro.");
            Add(30, "GravityCrush", "Gravity Crush", "EarthGravity", "Terra & Gravidade", "Esmagamento gravitacional contundente em área com imobilização.");
            Add(31, "EarthSpike", "Earth Spike", "EarthGravity", "Terra & Gravidade", "Emerge espinhos de rocha perfurantes na área-alvo.");
            Add(32, "StoneWall", "Stone Wall", "EarthGravity", "Terra & Gravidade", "Ergue linha instantânea de cobertura/barreiras de pedra.");
            Add(33, "SeismicImpact", "Seismic Impact", "EarthGravity", "Terra & Gravidade", "Pisão sísmico (SP) que atordoa em área e quebra coberturas.");

            // 6. Vento, Raio & Gelo
            Add(34, "WindBlade", "Wind Blade", "WindLightningFrost", "Vento, Raio & Gelo", "Lâmina veloz de vento cortante de longo alcance.");
            Add(35, "GaleSlash", "Gale Slash", "WindLightningFrost", "Vento, Raio & Gelo", "Varredura marcial (SP) em cone com empurrão em massa.");
            Add(36, "LightningBolt", "Lightning Bolt", "WindLightningFrost", "Vento, Raio & Gelo", "Raio concentrado instantâneo com alto dano e atordoamento.");
            Add(37, "IonicPulse", "Ionic Pulse", "WindLightningFrost", "Vento, Raio & Gelo", "Pulso elétrico em área com EMP prolongado contra mecanoides.");
            Add(38, "FrostNova", "Frost Nova", "WindLightningFrost", "Vento, Raio & Gelo", "Onda gélida circular com dano frio leve e hipotermia.");
            Add(39, "IceLance", "Ice Lance", "WindLightningFrost", "Vento, Raio & Gelo", "Lança maciça de gelo com alta perfuração de armadura.");
            Add(40, "GlacialZone", "Glacial Zone", "WindLightningFrost", "Vento, Raio & Gelo", "Nevasca persistente que congela inimigos e apaga incêndios.");

            // 7. Espaço & Tempo
            Add(41, "SpatialBlink", "Spatial Blink", "SpaceTime", "Espaço & Tempo", "Teleporte espacial instantâneo com custo proporcional à distância.");
            Add(42, "SpatialPull", "Spatial Pull", "SpaceTime", "Espaço & Tempo", "Puxa aliado distante ou caído para célula segura ao lado.");
            Add(43, "SpatialSlash", "Spatial Slash", "SpaceTime", "Espaço & Tempo", "Fenda dimensional que ignora 100% da armadura física.");
            Add(44, "SpaceFreeze", "Space Freeze", "SpaceTime", "Espaço & Tempo", "Trava coordenadas espaciais, imobilizando inimigos na área.");
            Add(45, "TimeDilation", "Time Dilation", "SpaceTime", "Espaço & Tempo", "Campo temporal em área que acelera aliados próximos.");
            Add(46, "TimeSlow", "Time Slow", "SpaceTime", "Espaço & Tempo", "Campo temporal em área que desacelera inimigos próximos.");
            Add(47, "TemporalStasis", "Temporal Stasis", "SpaceTime", "Espaço & Tempo", "Domo amplo que quase paralisa inimigos mantendo aliados normais.");
            Add(48, "ContinentalPortal", "Continental Portal", "SpaceTime", "Espaço & Tempo", "Salto espacial de longuíssima distância pelo mapa.");

            // 8. Suporte Universal & Marciais
            Add(49, "AetherShield", "Aether Shield", "SupportMartial", "Suporte & Marciais", "Escudo de durabilidade com autorregeneração por 12 horas.");
            Add(50, "ManaBarrier", "Mana Barrier", "SupportMartial", "Suporte & Marciais", "Barreira reativa que absorve todo dano consumindo MP.");
            Add(51, "BodyStrengthening", "Body Strengthening", "SupportMartial", "Suporte & Marciais", "Reforço muscular (+melee, +carga, resistência à dor).");
            Add(52, "SanctuaryDome", "Sanctuary Dome", "SupportMartial", "Suporte & Marciais", "Cúpula protetora estacionária no terreno contra combates.");
            Add(53, "ExecutionLunge", "Execution Lunge", "SupportMartial", "Suporte & Marciais", "Investida letal (SP) em feridos que devolve 50% SP no abate.");
            Add(54, "IntimidatingRoar", "Intimidating Roar", "SupportMartial", "Suporte & Marciais", "Rugido marcial (SP) que provoca foco e enfraquece inimigos.");
            Add(55, "BloodlustStance", "Bloodlust Stance", "SupportMartial", "Suporte & Marciais", "Postura ofensiva (SP) com fúria melee e redução extrema de dor.");

            // 9. Utilidade & Logística
            Add(56, "PurifyingRain", "Purifying Rain", "UtilityLogistics", "Utilidade & Logística", "Chuva localizada que apaga incêndios e limpa sujeira.");
            Add(57, "SpringBloom", "Spring Bloom", "UtilityLogistics", "Utilidade & Logística", "Avança plantações e árvores em 100% e elimina pragas (Blight).");
            Add(58, "MolecularRestoration", "Molecular Restoration", "UtilityLogistics", "Utilidade & Logística", "Repara instantaneamente estruturas e durabilidade de itens.");
            Add(59, "TelekineticGathering", "Telekinetic Gathering", "UtilityLogistics", "Utilidade & Logística", "Puxa itens soltos para os pés do conjurador (também automático).");

            // 10. Técnicas Supremas
            Add(60, "CelestialJudgment", "Celestial Judgment", "Pinnacle", "Técnicas Supremas", "Bombardeio múltiplo de raios celestiais na área-alvo.");
            Add(61, "AscendancyDecree", "Ascendancy Decree", "Pinnacle", "Técnicas Supremas", "Onda apoteótica que fere inimigos enquanto cura e protege aliados.");
        }

        public static void TryAutoCastTelekineticGathering(Pawn pawn, Hediff_SoulMatrix matrix)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null || pawn.Dead || pawn.Downed || pawn.Drafted) return;
            if (matrix == null || matrix.currentEnergy < 30f) return;

            CultivatorAbilityRecord rec = matrix.FindAbilityRecord("TelekineticGathering");
            if (rec == null || !rec.isLearned) return;

            RebornSpellEntry entry = GetEntry("TelekineticGathering");
            if (entry == null) return;
            AbilityDef aDef = entry.GetAbilityDef(rec.currentSpellStage);
            if (aDef == null) return;
            RebornSpellExtension ext = aDef.GetModExtension<RebornSpellExtension>();
            if (ext == null) return;

            float cost = ext.mpCost * rec.CostMultiplier;
            if (matrix.currentEnergy < cost * 2.5f) return;

            float radius = Mathf.Max(8f, ext.effectRadius);
            List<Thing> haulables = new List<Thing>();
            foreach (Thing t in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, radius, false))
            {
                if (t == null || t.Destroyed || !t.Spawned) continue;
                if (t.def.category != ThingCategory.Item) continue;
                if (t.IsForbidden(pawn)) continue;
                if (t.Position.DistanceTo(pawn.Position) <= 2.5f) continue;
                if (StoreUtility.IsInAnyStorage(t)) continue;
                haulables.Add(t);
                if (haulables.Count >= 6) break;
            }

            if (haulables.Count >= 3 && matrix.TryConsumeEnergy(cost))
            {
                for (int i = 0; i < haulables.Count; i++)
                {
                    Thing item = haulables[i];
                    RebornVFX.SpawnLightningArc(pawn.Map, item.DrawPos, pawn.DrawPos, RebornVFX.ColorSoul, 0.25f, 10);
                    item.Position = pawn.Position;
                }
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_SwirlIn, pawn.DrawPos, pawn.Map, 1.4f, RebornVFX.ColorSoul);
                matrix.AddAbilityProficiency("TelekineticGathering", 1.5f);
            }
        }
    }

    public class Projectile_RebornSpell : Projectile_CultivationSpell
    {
        public SoulClassStage spellStage = SoulClassStage.Mortal;
        public float explosionRadius = 0f;
        public float armorPen = 0.25f;
        public bool attachFire = false;
        public bool leaveTerrainFire = false;
        public bool applyHypothermia = false;
        public float bleedSeverity = 0f;
        public int pierceCount = 0;
        public Color tintColor = RebornVFX.ColorSoul;
        public Color accentColor = Color.white;
        public float lightModifier = 1.0f;

        protected override Color PrimaryVFXColor { get { return tintColor; } }
        protected override Color SecondaryVFXColor { get { return accentColor; } }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            IntVec3 cell = Position;
            Vector3 impactPos = (hitThing != null) ? hitThing.DrawPos : cell.ToVector3Shifted();

            if (blockedByShield)
            {
                base.Impact(hitThing, true);
                return;
            }

            Pawn caster = launcher as Pawn;
            Hediff_SoulMatrix matrix = (caster != null && caster.health != null && caster.health.hediffSet != null)
                ? caster.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix
                : null;
            CultivatorAbilityRecord rec = (matrix != null && !string.IsNullOrEmpty(abilityDefName)) ? matrix.GetAbilityRecord(abilityDefName) : null;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;
            float spellDamageMult = (matrix != null) ? matrix.SpellDamageMultiplier : 1.0f;
            float finalDmg = baseDamage * spellDamageMult * pwrMult * lightModifier;

            DamageDef dDef = customDamageDef != null ? customDamageDef : DamageDefOf.Burn;

            RebornCombatContext.ActiveSpellStage = spellStage;
            try
            {
                if (explosionRadius > 0.2f && map != null)
                {
                    // Explosão em área controlada (sem espalhar fogo no chão se leaveTerrainFire == false)
                    RebornVFX.PlayImpactBurst(impactPos, map, tintColor, accentColor, Mathf.Max(1.6f, explosionRadius * 0.9f), RebornVFXDefOf.Fleck_SoftSpark, 10, true, ImpactSound);
                    List<Thing> things = new List<Thing>(GenRadial.RadialDistinctThingsAround(cell, map, explosionRadius, true));
                    for (int i = 0; i < things.Count; i++)
                    {
                        Thing t = things[i];
                        if (t == null || t.Destroyed || t == caster) continue;
                        if (t is Pawn || t.def.category == ThingCategory.Building)
                        {
                            Pawn tp = t as Pawn;
                            if (tp != null && caster != null && !tp.HostileTo(caster)) continue;
                            DamageInfo dinfo = new DamageInfo(dDef, finalDmg, armorPen, -1f, launcher);
                            t.TakeDamage(dinfo);
                            if (tp != null && attachFire && !tp.Dead)
                            {
                                tp.TryAttachFire(0.35f, caster);
                            }
                        }
                    }
                    if (leaveTerrainFire)
                    {
                        FireUtility.TryStartFireIn(cell, map, 0.45f, caster);
                    }
                    Destroy();
                    return;
                }

                if (hitThing != null)
                {
                    DamageInfo dinfo = new DamageInfo(dDef, finalDmg, armorPen, -1f, launcher);
                    DamageWorker.DamageResult res = hitThing.TakeDamage(dinfo);
                    bool wasAbsorbed = (res == null || res.totalDamageDealt <= 0.01f || res.deflected);
                    if (map != null)
                    {
                        RebornVFX.PlayImpactBurst(impactPos, map, tintColor, accentColor, 1.5f, RebornVFXDefOf.Fleck_SoftSpark, 7, true, ImpactSound);
                    }
                    if (!wasAbsorbed)
                    {
                        Pawn targetPawn = hitThing as Pawn;
                        if (targetPawn != null && !targetPawn.Dead)
                        {
                            if (attachFire) targetPawn.TryAttachFire(0.30f, caster);
                            if (applyHypothermia && targetPawn.health != null)
                            {
                                Hediff hypo = targetPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
                                if (hypo == null)
                                {
                                    hypo = HediffMaker.MakeHediff(HediffDefOf.Hypothermia, targetPawn);
                                    hypo.Severity = 0.35f;
                                    targetPawn.health.AddHediff(hypo);
                                }
                                else hypo.Severity += 0.30f;
                            }
                            if (bleedSeverity > 0f && targetPawn.health != null && targetPawn.health.hediffSet != null)
                            {
                                DamageInfo extraBleed = new DamageInfo(DamageDefOf.Cut, bleedSeverity, armorPen, -1f, launcher);
                                targetPawn.TakeDamage(extraBleed);
                            }
                        }
                    }
                }
                else if (map != null)
                {
                    RebornVFX.PlayImpactBurst(impactPos, map, tintColor, accentColor, 1.2f, RebornVFXDefOf.Fleck_SoftSpark, 4, true, ImpactSound);
                    if (leaveTerrainFire) FireUtility.TryStartFireIn(cell, map, 0.35f, caster);
                }
                Destroy();
            }
            finally
            {
                RebornCombatContext.ActiveSpellStage = null;
            }
        }
    }

    public class RebornSpellZone : Thing
    {
        public string zoneKind = "DarknessDomain";
        public SoulClassStage spellStage = SoulClassStage.Mortal;
        public Pawn casterPawn;
        public float radius = 4.5f;
        public int ticksRemaining = 900;
        public float potency = 1.0f;
        public Color zoneColor = RebornVFX.ColorSoul;

        private static Material cachedDomeMat;
        private static Material cachedRuneMat;
        private static MaterialPropertyBlock sharedZoneProps;

        private static Material ZoneDomeMat
        {
            get
            {
                if (cachedDomeMat == null && UnityData.IsInMainThread)
                {
                    cachedDomeMat = MaterialPool.MatFrom("Motes/RebornMagic/HexShieldDome", ShaderDatabase.MoteGlow);
                }
                return cachedDomeMat;
            }
        }

        private static Material ZoneRuneMat
        {
            get
            {
                if (cachedRuneMat == null && UnityData.IsInMainThread)
                {
                    cachedRuneMat = MaterialPool.MatFrom("Motes/RebornMagic/RuneCircle", ShaderDatabase.MoteGlow);
                }
                return cachedRuneMat;
            }
        }

        public void Setup(string kind, SoulClassStage stage, Pawn caster, float r, int duration, float pot, Color col)
        {
            zoneKind = kind;
            spellStage = stage;
            casterPawn = caster;
            radius = Mathf.Max(1.5f, r);
            ticksRemaining = Mathf.Max(60, duration);
            potency = pot;
            zoneColor = col;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (sharedZoneProps == null) sharedZoneProps = new MaterialPropertyBlock();
            float t = Time.realtimeSinceStartup;
            float baseScale = radius * 1.9f;
            float pulse = 1.0f + 0.03f * Mathf.Sin(t * 2.5f);
            Vector3 center = Position.ToVector3Shifted();

            Material rm = ZoneRuneMat;
            if (rm != null)
            {
                Color rc = zoneColor;
                rc.a = 0.42f + 0.12f * Mathf.Sin(t * 2.2f);
                Vector3 rp = center;
                rp.y = AltitudeLayer.MoteLow.AltitudeFor();
                Matrix4x4 mat = Matrix4x4.TRS(rp, Quaternion.Euler(0f, (t * 22f) % 360f, 0f), new Vector3(baseScale * pulse, 1f, baseScale * pulse));
                sharedZoneProps.SetColor(ShaderPropertyIDs.Color, rc);
                Graphics.DrawMesh(MeshPool.plane10, mat, rm, 0, null, 0, sharedZoneProps);
            }

            if (zoneKind == "SanctuaryDome" || zoneKind == "TemporalStasis" || zoneKind == "DarknessDomain" || zoneKind == "GlacialZone")
            {
                Material dm = ZoneDomeMat;
                if (dm != null)
                {
                    Color dc = zoneColor;
                    dc.a = 0.30f + 0.10f * Mathf.Sin(t * 3.1f);
                    Vector3 dp = center;
                    dp.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                    Matrix4x4 mat = Matrix4x4.TRS(dp, Quaternion.Euler(0f, (t * -16f) % 360f, 0f), new Vector3(baseScale * pulse, 1f, baseScale * pulse));
                    sharedZoneProps.SetColor(ShaderPropertyIDs.Color, dc);
                    Graphics.DrawMesh(MeshPool.plane10, mat, dm, 0, null, 0, sharedZoneProps);
                }
            }
        }

        protected override void Tick()
        {
            base.Tick();
            ticksRemaining--;
            if (Destroyed || Map == null) return;

            if (this.IsHashIntervalTick(30))
            {
                Vector3 center = Position.ToVector3Shifted();
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFloor, center, Map, radius * 1.15f, zoneColor, (Find.TickManager.TicksGame * 2) % 360, 45f);
                if (zoneKind == "GlacialZone")
                {
                    GenTemperature.PushHeat(Position, Map, -22f * potency);
                    foreach (IntVec3 c in GenRadial.RadialCellsAround(Position, radius, true))
                    {
                        if (!c.InBounds(Map)) continue;
                        List<Thing> list = c.GetThingList(Map);
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            Fire f = list[i] as Fire;
                            if (f != null) f.Destroy();
                        }
                    }
                }
                else if (zoneKind == "SolarSphere")
                {
                    GenTemperature.PushHeat(Position, Map, 18f * potency);
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, center, Map, 2.2f, RebornVFX.ColorSkyGold);
                }
            }

            if (this.IsHashIntervalTick(60))
            {
                IReadOnlyList<Pawn> allPawns = Map.mapPawns.AllPawnsSpawned;
                for (int i = allPawns.Count - 1; i >= 0; i--)
                {
                    Pawn p = allPawns[i];
                    if (p == null || p.Dead) continue;
                    if (p.Position.DistanceTo(Position) > radius) continue;

                    bool isAlly = (casterPawn != null) ? !p.HostileTo(casterPawn) : (p.Faction != null && p.Faction.IsPlayer);

                    if (zoneKind == "DarknessDomain" && !isAlly)
                    {
                        Ability_RebornSpell.ApplyDebuffSafe(p, "TimeSlow", 90, spellStage, potency);
                        RebornCombatContext.ActiveSpellStage = spellStage;
                        try
                        {
                            p.TakeDamage(new DamageInfo(DamageDefOf.Burn, 4f * potency, 0.4f, -1f, casterPawn));
                        }
                        finally
                        {
                            RebornCombatContext.ActiveSpellStage = null;
                        }
                    }
                    else if (zoneKind == "GlacialZone" && !isAlly)
                    {
                        Ability_RebornSpell.ApplyDebuffSafe(p, "TimeSlow", 90, spellStage, potency);
                        if (RebornCombatContext.GetControlDurationMultiplier(p, spellStage) > 0f && p.health != null)
                        {
                            Hediff hypo = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
                            if (hypo == null)
                            {
                                hypo = HediffMaker.MakeHediff(HediffDefOf.Hypothermia, p);
                                hypo.Severity = 0.15f;
                                p.health.AddHediff(hypo);
                            }
                            else hypo.Severity = Mathf.Min(0.90f, hypo.Severity + 0.08f * potency);
                        }
                    }
                    else if (zoneKind == "TimeDilation" && isAlly)
                    {
                        Ability_RebornSpell.ApplyStanceSafe(p, "TimeDilation", 120, potency);
                    }
                    else if (zoneKind == "TimeSlow" && !isAlly)
                    {
                        Ability_RebornSpell.ApplyDebuffSafe(p, "TimeSlow", 120, spellStage, potency);
                    }
                    else if (zoneKind == "TemporalStasis" && !isAlly)
                    {
                        Ability_RebornSpell.ApplyDebuffSafe(p, "TemporalStasis", 120, spellStage, potency);
                    }
                    else if (zoneKind == "SanctuaryDome" && isAlly)
                    {
                        Hediff_AetherShield sh = p.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_AetherShield) as Hediff_AetherShield;
                        if (sh == null && RebornDefOf.REM_Hediff_AetherShield != null)
                        {
                            sh = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_AetherShield, p) as Hediff_AetherShield;
                            if (sh != null)
                            {
                                sh.maxShieldHP = 80f * potency;
                                sh.shieldHP = sh.maxShieldHP;
                                sh.ticksRemaining = 120;
                                p.health.AddHediff(sh);
                            }
                        }
                        else if (sh != null)
                        {
                            sh.ticksRemaining = Mathf.Max(sh.ticksRemaining, 120);
                        }
                    }
                }
            }

            if (ticksRemaining <= 0)
            {
                Destroy();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref zoneKind, "zoneKind", "DarknessDomain");
            Scribe_Values.Look(ref spellStage, "spellStage", SoulClassStage.Mortal);
            Scribe_References.Look(ref casterPawn, "casterPawn");
            Scribe_Values.Look(ref radius, "radius", 4.5f);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 900);
            Scribe_Values.Look(ref potency, "potency", 1.0f);
            Scribe_Values.Look(ref zoneColor, "zoneColor", RebornVFX.ColorSoul);
        }
    }

    public class Ability_RebornSpell : Ability
    {
        public Ability_RebornSpell() : base() { }
        public Ability_RebornSpell(Pawn pawn) : base(pawn) { }
        public Ability_RebornSpell(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public Ability_RebornSpell(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }
        public Ability_RebornSpell(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public static float GetShadowsLightMultiplier(Map map, IntVec3 pos)
        {
            if (map == null || !pos.InBounds(map) || map.glowGrid == null) return 1.0f;
            float glow = map.glowGrid.GroundGlowAt(pos);
            if (glow <= 0.30f) return 2.0f;
            if (!pos.Roofed(map) && glow >= 0.65f) return 0.55f;
            return 1.0f;
        }

        public static void ApplyDebuffSafe(Pawn target, string kind, int ticks, SoulClassStage attackStage, float mult, float periodicDmg = 0f)
        {
            if (target == null || target.Dead || target.health == null || target.health.hediffSet == null) return;
            if (RebornDefOf.REM_Hediff_CultivationDebuff == null) return;

            float ctrlMult = RebornCombatContext.GetControlDurationMultiplier(target, attackStage);
            if (ctrlMult <= 0.001f) return;

            int finalTicks = Mathf.Max(30, Mathf.RoundToInt((float)ticks * ctrlMult));
            Hediff_CultivationDebuff existing = null;
            List<Hediff> list = target.health.hediffSet.hediffs;
            for (int i = 0; i < list.Count; i++)
            {
                Hediff_CultivationDebuff d = list[i] as Hediff_CultivationDebuff;
                if (d != null && d.debuffKind == kind)
                {
                    existing = d;
                    break;
                }
            }
            if (existing == null)
            {
                existing = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_CultivationDebuff, target) as Hediff_CultivationDebuff;
                if (existing != null)
                {
                    existing.debuffKind = kind;
                    existing.ticksRemaining = finalTicks;
                    existing.effectMultiplier = mult;
                    existing.periodicDamage = periodicDmg;
                    existing.attackStage = attackStage;
                    target.health.AddHediff(existing);
                }
            }
            else
            {
                existing.ticksRemaining = Mathf.Max(existing.ticksRemaining, finalTicks);
                existing.effectMultiplier = Mathf.Max(existing.effectMultiplier, mult);
            }
        }

        public static void ApplyStanceSafe(Pawn target, string kind, int ticks, float mult)
        {
            if (target == null || target.Dead || target.health == null || target.health.hediffSet == null) return;
            if (RebornDefOf.REM_Hediff_MartialStance == null) return;

            Hediff_MartialStance existing = null;
            List<Hediff> list = target.health.hediffSet.hediffs;
            for (int i = 0; i < list.Count; i++)
            {
                Hediff_MartialStance st = list[i] as Hediff_MartialStance;
                if (st != null && st.stanceKind == kind)
                {
                    existing = st;
                    break;
                }
            }
            if (existing == null)
            {
                existing = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_MartialStance, target) as Hediff_MartialStance;
                if (existing != null)
                {
                    existing.stanceKind = kind;
                    existing.ticksRemaining = ticks;
                    existing.effectMultiplier = mult;
                    target.health.AddHediff(existing);
                }
            }
            else
            {
                existing.ticksRemaining = Mathf.Max(existing.ticksRemaining, ticks);
                existing.effectMultiplier = Mathf.Max(existing.effectMultiplier, mult);
            }
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (pawn == null || pawn.Dead || pawn.Map == null || pawn.health == null || pawn.health.hediffSet == null)
                return false;

            Hediff_SoulMatrix matrix = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null) return false;

            RebornSpellExtension ext = (def != null) ? def.GetModExtension<RebornSpellExtension>() : null;
            if (ext == null) return false;

            string spellId = ext.baseSpellId;
            CultivatorAbilityRecord rec = matrix.GetAbilityRecord(spellId);
            float costMult = (rec != null) ? rec.CostMultiplier : 1.0f;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;
            float lightMult = (ext.schoolId == "ShadowsDeath") ? GetShadowsLightMultiplier(pawn.Map, pawn.Position) : 1.0f;

            // Calcular custo final (Fixo / %MaxMP / Quadrático de Distância)
            float mpRequired = ext.mpCost;
            if (ext.mpPercentCost > 0f)
            {
                mpRequired = Mathf.Max(mpRequired, matrix.MaxEnergy * ext.mpPercentCost);
            }
            if (ext.distanceQuadraticCost)
            {
                float d = pawn.Position.DistanceTo(target.Cell);
                mpRequired = Mathf.Max(ext.mpCost, 0.4f * d * d);
            }
            mpRequired *= costMult;
            float spRequired = ext.spCost * costMult;

            if (mpRequired > 0.01f && matrix.currentEnergy + 0.01f < mpRequired)
            {
                Messages.Message(string.Format("Energia da Alma insuficiente! Necessário {0} MP.", Mathf.CeilToInt(mpRequired)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }
            if (spRequired > 0.01f && matrix.currentStamina + 0.01f < spRequired)
            {
                Messages.Message(string.Format("Vigor físico insuficiente! Necessário {0} SP.", Mathf.CeilToInt(spRequired)), pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            if (mpRequired > 0.01f && !matrix.TryConsumeEnergy(mpRequired)) return false;
            if (spRequired > 0.01f && !matrix.TryConsumeStamina(spRequired)) return false;

            Map map = pawn.Map;
            SoulClassStage sStage = ext.spellStage;
            float spellDamage = ext.baseDamage * matrix.SpellDamageMultiplier * pwrMult * lightMult;
            float spellPotency = pwrMult * lightMult * (1.0f + matrix.conductivity * 0.008f);

            RebornCombatContext.ActiveSpellStage = sStage;
            try
            {
                ExecuteSpellEffect(spellId, ext, matrix, map, target, spellDamage, spellPotency, pwrMult);
            }
            finally
            {
                RebornCombatContext.ActiveSpellStage = null;
            }

            float xpGain = (mpRequired + spRequired) * 0.5f;
            matrix.AddXP(Mathf.Max(4f, xpGain), true);
            matrix.AddAbilityProficiency(spellId, 6.0f);
            return true;
        }

        private void ExecuteSpellEffect(string id, RebornSpellExtension ext, Hediff_SoulMatrix matrix, Map map, LocalTargetInfo target, float finalDmg, float potency, float pwrMult)
        {
            IntVec3 cell = target.IsValid ? target.Cell : pawn.Position;
            if (!cell.InBounds(map)) cell = pawn.Position;
            Pawn tPawn = target.Thing as Pawn;
            SoulClassStage sStage = ext.spellStage;

            // --- 1. SOMBRAS & MORTE ---
            if (id == "ShadowStrike")
            {
                if (tPawn != null && !tPawn.Dead)
                {
                    IntVec3 destCell = CellFinder.RandomClosewalkCellNear(tPawn.Position, map, 1);
                    if (destCell.InBounds(map) && destCell.Walkable(map))
                    {
                        RebornVFX.PlayPortalWarp(pawn.DrawPos, destCell.ToVector3Shifted(), map, new Color(0.55f, 0.30f, 0.85f), 1.4f);
                        pawn.Position = destCell;
                        pawn.pather.StopDead();
                        pawn.Notify_Teleported(true, false);
                    }
                    tPawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, finalDmg * 1.4f, ext.armorPenetration, -1f, pawn));
                    MoteMaker.ThrowText(tPawn.DrawPos, map, "GOLPE SOMBRIO!", new Color(0.70f, 0.40f, 1f), 2.0f);
                }
                else SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Cut, ext, new Color(0.55f, 0.25f, 0.85f), Color.black, false, false, false, 8f);
                return;
            }
            if (id == "ShadowStep" || id == "SpatialBlink" || id == "ContinentalPortal")
            {
                if (!cell.Walkable(map)) cell = CellFinder.RandomClosewalkCellNear(cell, map, 2);
                if (cell.InBounds(map) && cell.Walkable(map))
                {
                    Color c = (id == "ShadowStep") ? new Color(0.55f, 0.30f, 0.85f) : RebornVFX.ColorPortal;
                    RebornVFX.PlayPortalWarp(pawn.DrawPos, cell.ToVector3Shifted(), map, c, 2.2f);
                    pawn.Position = cell;
                    pawn.pather.StopDead();
                    pawn.Notify_Teleported(true, false);
                    if (id == "ShadowStep") ApplyShadowCloak(pawn, 300, 0.50f);
                }
                return;
            }
            if (id == "ShadowCloak")
            {
                ApplyShadowCloak(pawn, ext.durationTicks, Mathf.Min(0.85f, 0.50f + 0.08f * (int)sStage));
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, new Color(0.45f, 0.30f, 0.70f), 1.8f, 140f);
                return;
            }
            if (id == "DeathRay")
            {
                if (tPawn != null)
                {
                    RebornVFX.SpawnLightningArc(map, pawn.DrawPos, tPawn.DrawPos, new Color(0.50f, 0.15f, 0.75f), 0.75f, 22);
                    tPawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, finalDmg, 0.95f, -1f, pawn));
                    ApplyDebuffSafe(tPawn, "Decay", ext.durationTicks, sStage, potency, ext.baseDamage * 0.15f);
                }
                return;
            }
            if (id == "NecroticTouch")
            {
                Corpse corpse = target.Thing as Corpse;
                if (corpse != null)
                {
                    RebornVFX.PlayImpactBurst(corpse.DrawPos, map, new Color(0.45f, 0.85f, 0.30f), Color.black, 2.5f);
                    DealRadialDamage(map, corpse.Position, ext.effectRadius, finalDmg, DamageDefOf.Burn, ext.armorPenetration, true, "Decay", ext.durationTicks, sStage, potency);
                    corpse.Destroy();
                }
                else if (tPawn != null)
                {
                    tPawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, finalDmg * 0.75f, ext.armorPenetration, -1f, pawn));
                    ApplyDebuffSafe(tPawn, "Decay", ext.durationTicks, sStage, potency, ext.baseDamage * 0.20f);
                }
                return;
            }
            if (id == "DarknessDomain")
            {
                SpawnZone(map, cell, "DarknessDomain", sStage, ext.effectRadius, ext.durationTicks, potency, new Color(0.35f, 0.20f, 0.55f));
                return;
            }

            // --- 2. ALMA & MENTE ---
            if (id == "SoulSpike")
            {
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorSoul, 1.8f, 150f);
                List<Thing> hits = CollectLineTargets(map, pawn.Position, cell, Mathf.Max(2, ext.extraTargets));
                for (int i = 0; i < hits.Count; i++)
                {
                    RebornVFX.SpawnLightningArc(map, pawn.DrawPos, hits[i].DrawPos, RebornVFX.ColorSoul, 0.45f, 14);
                    hits[i].TakeDamage(new DamageInfo(DamageDefOf.Psychic, finalDmg, 0.85f, -1f, pawn));
                    Pawn hp = hits[i] as Pawn;
                    if (hp != null && !hp.Dead && hp.stances != null && hp.stances.stunner != null && RebornCombatContext.GetControlDurationMultiplier(hp, sStage) > 0f)
                    {
                        hp.stances.stunner.StunFor(90, pawn, true);
                    }
                }
                if (hits.Count == 0) SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Psychic, ext, RebornVFX.ColorSoul, Color.white);
                return;
            }
            if (id == "SoulSlash" || id == "SpatialSlash" || id == "GaleSlash")
            {
                Color c = (id == "SpatialSlash") ? new Color(0.75f, 0.40f, 1f) : (id == "GaleSlash" ? RebornVFX.ColorWind : RebornVFX.ColorSoul);
                DamageDef dd = (id == "SpatialSlash") ? DamageDefOf.Cut : DamageDefOf.Cut;
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, c, 2.2f, 200f);
                DealArcDamage(map, pawn.Position, cell, ext.effectRadius, finalDmg, dd, (id == "SpatialSlash") ? 1.0f : ext.armorPenetration);
                return;
            }
            if (id == "MindBarrier")
            {
                Hediff_MindBarrier mb = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_MindBarrier) as Hediff_MindBarrier;
                if (mb == null && RebornDefOf.REM_Hediff_MindBarrier != null)
                {
                    mb = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_MindBarrier, pawn) as Hediff_MindBarrier;
                    if (mb != null) { mb.ticksRemaining = ext.durationTicks; mb.resistancePower = 1f + 0.5f * (int)sStage; pawn.health.AddHediff(mb); }
                }
                else if (mb != null) mb.ticksRemaining = Mathf.Max(mb.ticksRemaining, ext.durationTicks);

                Color mindCol = new Color(0.50f, 0.72f, 1.0f);
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, mindCol, 1.9f, 130f);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.75f, mindCol);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, map, 1.4f, Color.white);
                RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, map);
                MoteMaker.ThrowText(pawn.DrawPos, map, "BARREIRA MENTAL!", mindCol, 2.0f);
                return;
            }
            if (id == "SoulShield")
            {
                Hediff_SoulShield ss = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulShield) as Hediff_SoulShield;
                if (ss != null)
                {
                    pawn.health.RemoveHediff(ss);
                    MoteMaker.ThrowText(pawn.DrawPos, map, "ESCUDO DA ALMA: OFF", new Color(0.8f, 0.5f, 1f), 2.0f);
                }
                else if (RebornDefOf.REM_Hediff_SoulShield != null)
                {
                    ss = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_SoulShield, pawn) as Hediff_SoulShield;
                    if (ss != null)
                    {
                        ss.spellStage = sStage;
                        ss.reactiveCostMult = Mathf.Max(0.40f, 1.0f - 0.15f * (int)sStage);
                        pawn.health.AddHediff(ss);
                        Color soulCol = new Color(0.85f, 0.55f, 1f);
                        RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, soulCol, 2.2f, 150f);
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.85f, soulCol);
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, map, 1.5f, Color.white);
                        RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, map);
                        MoteMaker.ThrowText(pawn.DrawPos, map, "ESCUDO DA ALMA: ON", soulCol, 2.0f);
                    }
                }
                return;
            }
            if (id == "SpiritualScan")
            {
                ApplyStanceSafe(pawn, "SpiritualScan", ext.durationTicks, potency);
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorSkyGold, 3.0f, 160f);
                MoteMaker.ThrowText(pawn.DrawPos, map, "VARREDURA ESPIRITUAL!", RebornVFX.ColorSkyGold, 2.5f);
                return;
            }
            if (id == "SoulSiphon")
            {
                if (tPawn != null && !tPawn.Dead)
                {
                    RebornVFX.PlaySoulSiphon(tPawn.DrawPos, pawn, RebornVFX.ColorSoul);
                    tPawn.TakeDamage(new DamageInfo(DamageDefOf.Psychic, finalDmg, 0.80f, -1f, pawn));
                    matrix.RestoreEnergy(finalDmg * 0.5f, false);
                    HealPawnInjuries(pawn, finalDmg * 0.4f);
                }
                return;
            }
            if (id == "SoulBreak")
            {
                if (tPawn != null && !tPawn.Dead && tPawn.stances != null && tPawn.stances.stunner != null)
                {
                    RebornVFX.SpawnLightningArc(map, pawn.DrawPos, tPawn.DrawPos, new Color(0.6f, 0.4f, 1f), 0.55f, 18);
                    float ctrl = RebornCombatContext.GetControlDurationMultiplier(tPawn, sStage);
                    if (ctrl > 0f)
                    {
                        tPawn.stances.stunner.StunFor(Mathf.RoundToInt(240 * ctrl), pawn, true);
                        HealthUtility.AdjustSeverity(tPawn, HediffDefOf.Anesthetic, 0.55f * ctrl);
                    }
                }
                return;
            }
            if (id == "SerenityAura")
            {
                IReadOnlyList<Pawn> ps = map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < ps.Count; i++)
                {
                    Pawn ally = ps[i];
                    if (ally != null && !ally.Dead && !ally.HostileTo(pawn) && ally.Position.DistanceTo(cell) <= ext.effectRadius + 1f)
                    {
                        if (ally.InMentalState) ally.MentalState.RecoverFromState();
                        ApplyStanceSafe(ally, "Serenity", ext.durationTicks, potency);
                        RebornVFX.PlayHealingBloom(ally, RebornVFX.ColorSoul, true);
                    }
                }
                return;
            }

            // --- 3. FOGO & CALOR ---
            if (id == "Fireball")
            {
                SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Burn, ext, RebornVFX.ColorFire, new Color(1f, 0.85f, 0.2f), false, false, true);
                return;
            }
            if (id == "Pyroblast")
            {
                SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Bomb, ext, RebornVFX.ColorFire, Color.yellow, true, true);
                return;
            }
            if (id == "BlazingLance")
            {
                SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Burn, ext, new Color(1f, 0.90f, 0.65f), RebornVFX.ColorFire, false, true);
                return;
            }
            if (id == "SolarSphere")
            {
                SpawnZone(map, cell, "SolarSphere", sStage, ext.effectRadius, ext.durationTicks, potency, RebornVFX.ColorSkyGold);
                return;
            }
            if (id == "FlameBurst")
            {
                RebornVFX.PlayImpactBurst(pawn.DrawPos, map, RebornVFX.ColorFire, Color.yellow, ext.effectRadius * 1.1f);
                DealRadialDamage(map, pawn.Position, ext.effectRadius, finalDmg, DamageDefOf.Burn, ext.armorPenetration, false, null, 0, sStage, potency, true);
                return;
            }
            if (id == "InfernoWall")
            {
                if (RebornDefOf.REM_Hediff_InfernoWall != null)
                {
                    Hediff_InfernoWall iw = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_InfernoWall, pawn) as Hediff_InfernoWall;
                    if (iw != null)
                    {
                        iw.spellStage = sStage;
                        iw.ticksRemaining = ext.durationTicks;
                        iw.ringRadius = Mathf.Max(2.2f, ext.effectRadius);
                        iw.ringBurnDamage = finalDmg;
                        iw.projectileInterceptChance = Mathf.Min(0.85f, 0.45f + 0.08f * (int)sStage);
                        iw.magicDamageReduction = Mathf.Min(0.65f, 0.30f + 0.07f * (int)sStage);
                        pawn.health.AddHediff(iw);
                        RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorFire, Mathf.Max(2.2f, iw.ringRadius), 150f);
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.90f, RebornVFX.ColorFire);
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, map, 1.5f, new Color(1f, 0.85f, 0.25f));
                        RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, map);
                        MoteMaker.ThrowText(pawn.DrawPos, map, "MURALHA INFERNAL!", RebornVFX.ColorFire, 2.1f);
                    }
                }
                return;
            }

            // --- 4. SANGUE & VIDA ---
            if (id == "BloodLance")
            {
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.BloodLoss, 0.05f);
                SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Cut, ext, new Color(0.85f, 0.12f, 0.20f), Color.red, false, false, false, ext.baseDamage * 0.4f);
                return;
            }
            if (id == "CrimsonShield")
            {
                Color crimsonCol = new Color(0.92f, 0.15f, 0.24f);
                if (RebornDefOf.REM_Hediff_CrimsonShield != null)
                {
                    Hediff_CrimsonShield cs = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_CrimsonShield, pawn) as Hediff_CrimsonShield;
                    if (cs != null)
                    {
                        cs.maxShieldHP = ext.baseHealOrShield * potency;
                        cs.shieldHP = cs.maxShieldHP;
                        cs.ticksRemaining = ext.durationTicks;
                        pawn.health.AddHediff(cs);
                    }
                }
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, crimsonCol, 2.1f, 150f);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.85f, crimsonCol);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, map, 1.45f, Color.red);
                RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, map);
                MoteMaker.ThrowText(pawn.DrawPos, map, "ESCUDO CARMESIM!", crimsonCol, 2.0f);
                return;
            }
            if (id == "BloodControl")
            {
                if (tPawn != null && !tPawn.Dead && tPawn.RaceProps != null && tPawn.RaceProps.IsFlesh)
                {
                    ApplyDebuffSafe(tPawn, "SpaceFreeze", ext.durationTicks, sStage, potency);
                    tPawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, finalDmg * 0.5f, 0.9f, -1f, pawn));
                }
                return;
            }
            if (id == "HealingLight")
            {
                Pawn healTarget = (tPawn != null) ? tPawn : pawn;
                if (healTarget.health != null && healTarget.health.hediffSet != null)
                {
                    List<Hediff> hList = healTarget.health.hediffSet.hediffs;
                    for (int i = 0; i < hList.Count; i++)
                    {
                        Hediff_Injury inj = hList[i] as Hediff_Injury;
                        if (inj != null && inj.TendableNow()) inj.Tended(1.0f, 1.0f);
                    }
                    if (RebornDefOf.REM_Hediff_HealingLight != null)
                    {
                        Hediff_HealingLight hl = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_HealingLight, healTarget) as Hediff_HealingLight;
                        if (hl != null) { hl.ticksRemaining = ext.durationTicks; hl.regenFactor = 2.0f + 0.25f * (int)sStage; healTarget.health.AddHediff(hl); }
                    }
                    RebornVFX.PlayHealingBloom(healTarget, RebornVFX.ColorHealing, false);
                }
                return;
            }
            if (id == "VitalSurge" || id == "CellularPurification")
            {
                Pawn ht = (tPawn != null) ? tPawn : pawn;
                if (ht.health != null && ht.health.hediffSet != null)
                {
                    List<Hediff> toRemove = new List<Hediff>();
                    for (int i = 0; i < ht.health.hediffSet.hediffs.Count; i++)
                    {
                        Hediff h = ht.health.hediffSet.hediffs[i];
                        if (h.def == HediffDefOf.ToxicBuildup || h.def == HediffDefOf.FoodPoisoning) toRemove.Add(h);
                    }
                    for (int i = 0; i < toRemove.Count; i++) ht.health.RemoveHediff(toRemove[i]);

                    if (id == "VitalSurge" && RebornDefOf.REM_Hediff_VitalSurge != null)
                    {
                        Hediff_VitalSurge vs = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_VitalSurge, ht) as Hediff_VitalSurge;
                        if (vs != null) { vs.ticksRemaining = ext.durationTicks; vs.bonusMultiplier = potency; ht.health.AddHediff(vs); }
                    }
                    RebornVFX.PlayHealingBloom(ht, RebornVFX.ColorHealing, false);
                }
                return;
            }
            if (id == "VitalityWave")
            {
                IReadOnlyList<Pawn> ps = map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < ps.Count; i++)
                {
                    Pawn ally = ps[i];
                    if (ally != null && !ally.Dead && !ally.HostileTo(pawn) && ally.Position.DistanceTo(pawn.Position) <= ext.effectRadius)
                    {
                        HealPawnInjuries(ally, ext.baseHealOrShield * potency);
                        RebornVFX.PlayHealingBloom(ally, RebornVFX.ColorHealing, true);
                    }
                }
                return;
            }
            if (id == "SoulResurrection")
            {
                Corpse corpse = target.Thing as Corpse;
                if (corpse != null && corpse.InnerPawn != null)
                {
                    ResurrectionUtility.TryResurrectWithSideEffects(corpse.InnerPawn);
                    RebornVFX.PlayAscensionPillar(corpse.InnerPawn, RebornVFX.ColorHealing, "RESSURREIÇÃO DA ALMA!", 2.5f);
                }
                return;
            }

            // --- 5. TERRA & GRAVIDADE ---
            if (id == "GravityWell")
            {
                RebornVFX.PlayImpactBurst(cell.ToVector3Shifted(), map, new Color(0.55f, 0.35f, 0.90f), Color.black, ext.effectRadius);
                IReadOnlyList<Pawn> ps = map.mapPawns.AllPawnsSpawned;
                for (int i = ps.Count - 1; i >= 0; i--)
                {
                    Pawn p = ps[i];
                    if (p != null && !p.Dead && p.HostileTo(pawn) && p.Position.DistanceTo(cell) <= ext.effectRadius)
                    {
                        if (RebornCombatContext.GetControlDurationMultiplier(p, sStage) > 0f)
                        {
                            IntVec3 pullCell = CellFinder.RandomClosewalkCellNear(cell, map, 1);
                            if (pullCell.InBounds(map) && pullCell.Walkable(map))
                            {
                                p.Position = pullCell;
                                p.pather.StopDead();
                                p.Notify_Teleported(true, false);
                            }
                            ApplyDebuffSafe(p, "TimeSlow", ext.durationTicks, sStage, potency);
                        }
                    }
                }
                return;
            }
            if (id == "GravityCrush" || id == "SeismicImpact")
            {
                IntVec3 center = (id == "SeismicImpact") ? pawn.Position : cell;
                RebornVFX.PlayImpactBurst(center.ToVector3Shifted(), map, RebornVFX.ColorEarth, Color.white, ext.effectRadius * 1.1f, RebornVFXDefOf.Fleck_StoneShard, 12);
                DealRadialDamage(map, center, ext.effectRadius, finalDmg, DamageDefOf.Blunt, ext.armorPenetration, true, (id == "GravityCrush") ? "GravityCrush" : null, ext.durationTicks, sStage, potency);
                return;
            }
            if (id == "EarthSpike")
            {
                RebornVFX.PlayImpactBurst(cell.ToVector3Shifted(), map, RebornVFX.ColorEarth, Color.gray, ext.effectRadius, RebornVFXDefOf.Fleck_StoneShard, 10);
                DealRadialDamage(map, cell, ext.effectRadius, finalDmg, DamageDefOf.Stab, ext.armorPenetration, false, null, 0, sStage, potency);
                return;
            }
            if (id == "StoneWall")
            {
                ThingDef wallDef = DefDatabase<ThingDef>.GetNamedSilentFail("Barricade");
                ThingDef stoneStuff = DefDatabase<ThingDef>.GetNamedSilentFail("BlocksGranite");
                if (wallDef != null && stoneStuff != null)
                {
                    IntVec3 dir = (cell - pawn.Position);
                    IntVec3 perp = new IntVec3(-dir.z, 0, dir.x);
                    if (perp == IntVec3.Zero) perp = IntVec3.East;
                    int half = Mathf.Max(1, ext.extraTargets / 2);
                    for (int k = -half; k <= half; k++)
                    {
                        IntVec3 wc = new IntVec3(cell.x + (perp.x != 0 ? Math.Sign(perp.x) * k : 0), 0, cell.z + (perp.z != 0 ? Math.Sign(perp.z) * k : 0));
                        if (wc.InBounds(map) && wc.Standable(map))
                        {
                            Thing bar = ThingMaker.MakeThing(wallDef, stoneStuff);
                            bar.SetFaction(pawn.Faction);
                            GenSpawn.Spawn(bar, wc, map);
                        }
                    }
                }
                return;
            }

            // --- 6. VENTO, RAIO & GELO ---
            if (id == "WindBlade")
            {
                SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Cut, ext, RebornVFX.ColorWind, Color.white);
                return;
            }
            if (id == "LightningBolt")
            {
                if (tPawn != null)
                {
                    RebornVFX.SpawnLightningArc(map, pawn.DrawPos, tPawn.DrawPos, RebornVFX.ColorLightning, 0.75f, 18);
                    tPawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, finalDmg, ext.armorPenetration, -1f, pawn));
                    if (!tPawn.Dead && tPawn.stances != null && tPawn.stances.stunner != null && RebornCombatContext.GetControlDurationMultiplier(tPawn, sStage) > 0f)
                        tPawn.stances.stunner.StunFor(120, pawn, true);
                }
                return;
            }
            if (id == "IonicPulse")
            {
                RebornVFX.PlayImpactBurst(cell.ToVector3Shifted(), map, RebornVFX.ColorLightning, Color.white, ext.effectRadius * 1.1f);
                DealRadialDamage(map, cell, ext.effectRadius, finalDmg, DamageDefOf.EMP, 1.0f, true, null, 0, sStage, potency);
                return;
            }
            if (id == "FrostNova")
            {
                RebornVFX.PlayImpactBurst(pawn.DrawPos, map, RebornVFX.ColorFrost, Color.white, ext.effectRadius * 1.1f, RebornVFXDefOf.Fleck_IceCrystal, 12);
                DealRadialDamage(map, pawn.Position, ext.effectRadius, finalDmg, DamageDefOf.Frostbite, ext.armorPenetration, false, "TimeSlow", 240, sStage, potency);
                return;
            }
            if (id == "IceLance")
            {
                SpawnProjectile(map, target, ext.baseDamage, DamageDefOf.Frostbite, ext, RebornVFX.ColorFrost, Color.white, false, true);
                return;
            }
            if (id == "GlacialZone")
            {
                SpawnZone(map, cell, "GlacialZone", sStage, ext.effectRadius, ext.durationTicks, potency, RebornVFX.ColorFrost);
                return;
            }

            // --- 7. ESPAÇO & TEMPO ---
            if (id == "SpatialPull")
            {
                if (tPawn != null && tPawn.Spawned)
                {
                    IntVec3 safe = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 2);
                    if (safe.InBounds(map) && safe.Walkable(map))
                    {
                        RebornVFX.PlayPortalWarp(tPawn.DrawPos, safe.ToVector3Shifted(), map, RebornVFX.ColorPortal, 2.0f);
                        tPawn.Position = safe;
                        tPawn.pather.StopDead();
                        tPawn.Notify_Teleported(true, false);
                    }
                }
                return;
            }
            if (id == "SpaceFreeze")
            {
                RebornVFX.PlayImpactBurst(cell.ToVector3Shifted(), map, RebornVFX.ColorPortal, Color.white, ext.effectRadius);
                IReadOnlyList<Pawn> ps = map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < ps.Count; i++)
                {
                    if (ps[i] != null && !ps[i].Dead && ps[i].HostileTo(pawn) && ps[i].Position.DistanceTo(cell) <= ext.effectRadius)
                    {
                        ApplyDebuffSafe(ps[i], "SpaceFreeze", ext.durationTicks, sStage, potency);
                    }
                }
                return;
            }
            if (id == "TimeDilation" || id == "TimeSlow" || id == "TemporalStasis" || id == "SanctuaryDome")
            {
                SpawnZone(map, cell, id, sStage, ext.effectRadius, ext.durationTicks, potency, RebornVFX.ColorPortal);
                return;
            }

            // --- 8. SUPORTE UNIVERSAL & MARCIAIS ---
            if (id == "AetherShield")
            {
                if (RebornDefOf.REM_Hediff_AetherShield != null)
                {
                    Hediff_AetherShield ae = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_AetherShield) as Hediff_AetherShield;
                    float maxHp = ext.baseHealOrShield * potency;
                    if (ae == null)
                    {
                        ae = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_AetherShield, pawn) as Hediff_AetherShield;
                        if (ae != null) { ae.maxShieldHP = maxHp; ae.shieldHP = maxHp; ae.ticksRemaining = ext.durationTicks; ae.rechargeCooldownTicks = 0; pawn.health.AddHediff(ae); }
                    }
                    else { ae.maxShieldHP = maxHp; ae.shieldHP = maxHp; ae.ticksRemaining = ext.durationTicks; ae.rechargeCooldownTicks = 0; }
                    Color aeCol = new Color(0.50f, 0.90f, 1.0f);
                    RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, aeCol, 2.1f, 150f);
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.85f, aeCol);
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, map, 1.5f, Color.white);
                    RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, map);
                    MoteMaker.ThrowText(pawn.DrawPos, map, "ESCUDO DE ÉTER!", aeCol, 2.0f);
                }
                return;
            }
            if (id == "ManaBarrier")
            {
                Hediff_ManaBarrier mb = pawn.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ManaBarrier) as Hediff_ManaBarrier;
                if (mb != null)
                {
                    pawn.health.RemoveHediff(mb);
                    MoteMaker.ThrowText(pawn.DrawPos, map, "BARREIRA DE MANA: OFF", RebornVFX.ColorSoul, 2.0f);
                }
                else if (RebornDefOf.REM_Hediff_ManaBarrier != null)
                {
                    mb = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_ManaBarrier, pawn) as Hediff_ManaBarrier;
                    if (mb != null)
                    {
                        mb.spellStage = sStage;
                        mb.mpPerDamage = Mathf.Max(0.35f, 1.20f - 0.20f * (int)sStage);
                        mb.ticksRemaining = ext.durationTicks;
                        pawn.health.AddHediff(mb);
                        RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorSoul, 2.05f, 160f);
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pawn.DrawPos, map, 0.85f, RebornVFX.ColorSoul);
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pawn.DrawPos, map, 1.45f, Color.white);
                        RebornVFX.PlaySound(SoundDefOf.EnergyShield_Reset, pawn.Position, map);
                        MoteMaker.ThrowText(pawn.DrawPos, map, "BARREIRA DE MANA: ON", RebornVFX.ColorSoul, 2.0f);
                    }
                }
                return;
            }
            if (id == "BodyStrengthening")
            {
                ApplyStanceSafe(pawn, "BodyStrengthening", ext.durationTicks, potency);
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorEarth, 1.8f, 140f);
                return;
            }
            if (id == "BloodlustStance")
            {
                ApplyStanceSafe(pawn, "Bloodlust", ext.durationTicks, potency);
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, Color.red, 2.0f, 180f);
                return;
            }
            if (id == "ExecutionLunge")
            {
                if (tPawn != null && !tPawn.Dead)
                {
                    IntVec3 destCell = CellFinder.RandomClosewalkCellNear(tPawn.Position, map, 1);
                    if (destCell.InBounds(map) && destCell.Walkable(map))
                    {
                        pawn.Position = destCell;
                        pawn.pather.StopDead();
                        pawn.Notify_Teleported(true, false);
                    }
                    float bonus = (tPawn.health.summaryHealth.SummaryHealthPercent <= 0.45f || (tPawn.stances != null && tPawn.stances.stunner != null && tPawn.stances.stunner.Stunned)) ? 2.0f : 1.0f;
                    tPawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, finalDmg * bonus, ext.armorPenetration, -1f, pawn));
                    if (tPawn.Dead)
                    {
                        matrix.currentStamina = Mathf.Min(matrix.MaxStamina, matrix.currentStamina + ext.spCost * 0.5f);
                        MoteMaker.ThrowText(pawn.DrawPos, map, "EXECUÇÃO! (+50% SP)", Color.red, 2.2f);
                    }
                }
                return;
            }
            if (id == "IntimidatingRoar")
            {
                RebornVFX.PlayImpactBurst(pawn.DrawPos, map, new Color(1f, 0.5f, 0.2f), Color.red, ext.effectRadius);
                IReadOnlyList<Pawn> ps = map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < ps.Count; i++)
                {
                    if (ps[i] != null && !ps[i].Dead && ps[i].HostileTo(pawn) && ps[i].Position.DistanceTo(pawn.Position) <= ext.effectRadius)
                    {
                        ApplyDebuffSafe(ps[i], "IntimidatingRoar", ext.durationTicks, sStage, potency);
                    }
                }
                return;
            }

            // --- 9. UTILIDADE & LOGÍSTICA ---
            if (id == "PurifyingRain")
            {
                RebornVFX.PlayCastRuneCircle(cell.ToVector3Shifted(), map, RebornVFX.ColorFrost, ext.effectRadius, 90f);
                foreach (IntVec3 c in GenRadial.RadialCellsAround(cell, ext.effectRadius, true))
                {
                    if (!c.InBounds(map)) continue;
                    List<Thing> list = c.GetThingList(map);
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i] is Fire || list[i] is Filth) list[i].Destroy();
                    }
                }
                GenTemperature.PushHeat(cell, map, -18f);
                return;
            }
            if (id == "SpringBloom")
            {
                RebornVFX.PlayCastRuneCircle(cell.ToVector3Shifted(), map, RebornVFX.ColorHealing, ext.effectRadius, 90f);
                foreach (IntVec3 c in GenRadial.RadialCellsAround(cell, ext.effectRadius, true))
                {
                    if (!c.InBounds(map)) continue;
                    List<Thing> list = c.GetThingList(map);
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        Blight blight = list[i] as Blight;
                        if (blight != null) { blight.Destroy(); continue; }
                        Plant plant = list[i] as Plant;
                        if (plant != null)
                        {
                            plant.Growth = 1.0f;
                            if (map.mapDrawer != null) map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Things);
                        }
                    }
                }
                return;
            }
            if (id == "MolecularRestoration")
            {
                RebornVFX.PlayCastRuneCircle(cell.ToVector3Shifted(), map, RebornVFX.ColorSoul, ext.effectRadius, 120f);
                foreach (Thing t in GenRadial.RadialDistinctThingsAround(cell, map, ext.effectRadius, true))
                {
                    if (t != null && t.def.useHitPoints && t.HitPoints < t.MaxHitPoints)
                    {
                        if (t.def.category == ThingCategory.Building || t.def.category == ThingCategory.Item)
                        {
                            t.HitPoints = Mathf.Min(t.MaxHitPoints, t.HitPoints + Mathf.RoundToInt(ext.baseHealOrShield * potency));
                        }
                    }
                }
                return;
            }
            if (id == "TelekineticGathering")
            {
                RebornVFX.PlayCastRuneCircle(pawn.DrawPos, map, RebornVFX.ColorSoul, 2.2f, 150f);
                int moved = 0;
                foreach (Thing t in new List<Thing>(GenRadial.RadialDistinctThingsAround(pawn.Position, map, ext.effectRadius, false)))
                {
                    if (t != null && t.Spawned && t.def.category == ThingCategory.Item && !t.IsForbidden(pawn))
                    {
                        t.Position = pawn.Position;
                        moved++;
                        if (moved >= 12 + ext.extraTargets * 4) break;
                    }
                }
                return;
            }

            // --- 10. TÉCNICAS SUPREMAS ---
            if (id == "CelestialJudgment")
            {
                RebornVFX.PlayImpactBurst(cell.ToVector3Shifted(), map, RebornVFX.ColorSkyGold, Color.white, ext.effectRadius * 1.2f);
                DealRadialDamage(map, cell, ext.effectRadius, finalDmg, DamageDefOf.Bomb, 0.85f, true, null, 0, sStage, potency);
                return;
            }
            if (id == "AscendancyDecree")
            {
                RebornVFX.PlayAscensionPillar(pawn, RebornVFX.ColorSkyGold, "DECRETO DA ASCENDÊNCIA!", 3.2f);
                IReadOnlyList<Pawn> ps = map.mapPawns.AllPawnsSpawned;
                for (int i = ps.Count - 1; i >= 0; i--)
                {
                    Pawn p = ps[i];
                    if (p == null || p.Dead) continue;
                    if (p.Position.DistanceTo(pawn.Position) > ext.effectRadius) continue;
                    if (p.HostileTo(pawn))
                    {
                        p.TakeDamage(new DamageInfo(DamageDefOf.Psychic, finalDmg, 1.0f, -1f, pawn));
                    }
                    else
                    {
                        HealPawnInjuries(p, ext.baseHealOrShield * potency);
                    }
                }
                return;
            }
        }

        private void ApplyShadowCloak(Pawn target, int durationTicks, float dodge)
        {
            if (RebornDefOf.REM_Hediff_ShadowCloak == null) return;
            Hediff_ShadowCloak sc = target.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_ShadowCloak) as Hediff_ShadowCloak;
            if (sc == null)
            {
                sc = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_ShadowCloak, target) as Hediff_ShadowCloak;
                if (sc != null) { sc.ticksRemaining = durationTicks; sc.evasionBonus = dodge; target.health.AddHediff(sc); }
            }
            else { sc.ticksRemaining = Mathf.Max(sc.ticksRemaining, durationTicks); sc.evasionBonus = Mathf.Max(sc.evasionBonus, dodge); }
        }

        private void HealPawnInjuries(Pawn p, float pool)
        {
            if (p == null || p.health == null || p.health.hediffSet == null) return;
            List<Hediff> list = p.health.hediffSet.hediffs;
            for (int i = list.Count - 1; i >= 0 && pool > 0.1f; i--)
            {
                Hediff_Injury inj = list[i] as Hediff_Injury;
                if (inj != null && inj.Severity > 0.01f)
                {
                    float h = Mathf.Min(inj.Severity, pool);
                    inj.Heal(h);
                    pool -= h;
                }
            }
        }

        private void SpawnZone(Map map, IntVec3 cell, string kind, SoulClassStage stage, float radius, int duration, float potency, Color col)
        {
            if (RebornDefOf.REM_Thing_SpellZone == null || !cell.InBounds(map)) return;
            RebornSpellZone zone = ThingMaker.MakeThing(RebornDefOf.REM_Thing_SpellZone) as RebornSpellZone;
            if (zone != null)
            {
                zone.Setup(kind, stage, pawn, radius, duration, potency, col);
                GenSpawn.Spawn(zone, cell, map);
            }
        }

        private void SpawnProjectile(Map map, LocalTargetInfo target, float baseDmg, DamageDef dDef, RebornSpellExtension ext, Color c1, Color c2, bool leaveFire = false, bool hypo = false, bool attachFire = false, float bleed = 0f)
        {
            ThingDef pDef = RebornDefOf.REM_Proj_RebornSpell;
            if (pDef == null) return;
            Projectile_RebornSpell proj = GenSpawn.Spawn(pDef, pawn.Position, map) as Projectile_RebornSpell;
            if (proj != null)
            {
                proj.abilityDefName = ext.baseSpellId;
                proj.baseDamage = baseDmg;
                proj.customDamageDef = dDef;
                proj.spellStage = ext.spellStage;
                proj.explosionRadius = ext.effectRadius;
                proj.armorPen = ext.armorPenetration;
                proj.leaveTerrainFire = leaveFire;
                proj.applyHypothermia = hypo;
                proj.attachFire = attachFire;
                proj.bleedSeverity = bleed;
                proj.tintColor = c1;
                proj.accentColor = c2;
                proj.lightModifier = (ext.schoolId == "ShadowsDeath") ? GetShadowsLightMultiplier(map, pawn.Position) : 1.0f;
                proj.Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.All, false, null, null);
            }
        }

        private void DealRadialDamage(Map map, IntVec3 center, float radius, float dmg, DamageDef dDef, float ap, bool hitBuildings, string debuffKind, int debuffTicks, SoulClassStage sStage, float potency, bool attachFire = false)
        {
            List<Thing> things = new List<Thing>(GenRadial.RadialDistinctThingsAround(center, map, radius, true));
            for (int i = 0; i < things.Count; i++)
            {
                Thing t = things[i];
                if (t == null || t.Destroyed || t == pawn) continue;
                Pawn tp = t as Pawn;
                if (tp != null)
                {
                    if (!tp.HostileTo(pawn)) continue;
                    tp.TakeDamage(new DamageInfo(dDef, dmg, ap, -1f, pawn));
                    if (!string.IsNullOrEmpty(debuffKind) && !tp.Dead) ApplyDebuffSafe(tp, debuffKind, debuffTicks, sStage, potency);
                    if (attachFire && !tp.Dead) tp.TryAttachFire(0.30f, pawn);
                }
                else if (hitBuildings && t.def.category == ThingCategory.Building && t.Faction != pawn.Faction)
                {
                    t.TakeDamage(new DamageInfo(dDef, dmg * 1.25f, ap, -1f, pawn));
                }
            }
        }

        private List<Thing> CollectLineTargets(Map map, IntVec3 start, IntVec3 end, int maxCount)
        {
            List<Thing> res = new List<Thing>();
            foreach (IntVec3 c in GenSight.PointsOnLineOfSight(start, end))
            {
                if (!c.InBounds(map) || c == start) continue;
                List<Thing> list = c.GetThingList(map);
                for (int i = 0; i < list.Count; i++)
                {
                    Pawn tp = list[i] as Pawn;
                    if (tp != null && !tp.Dead && tp.HostileTo(pawn) && !res.Contains(tp))
                    {
                        res.Add(tp);
                        if (res.Count >= maxCount) return res;
                    }
                }
            }
            return res;
        }

        private void DealArcDamage(Map map, IntVec3 origin, IntVec3 dirCell, float radius, float dmg, DamageDef dDef, float ap)
        {
            Vector3 fwd = (dirCell - origin).ToVector3();
            if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();

            IReadOnlyList<Pawn> ps = map.mapPawns.AllPawnsSpawned;
            for (int i = ps.Count - 1; i >= 0; i--)
            {
                Pawn tp = ps[i];
                if (tp == null || tp.Dead || !tp.HostileTo(pawn)) continue;
                float dist = tp.Position.DistanceTo(origin);
                if (dist > radius) continue;
                Vector3 toT = (tp.Position - origin).ToVector3().normalized;
                if (Vector3.Dot(fwd, toT) >= 0.25f || dist <= 1.8f)
                {
                    tp.TakeDamage(new DamageInfo(dDef, dmg, ap, -1f, pawn));
                }
            }
        }
    }
}
