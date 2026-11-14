using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RebornEvolvingMagic
{
    // Classe Base para Projéteis de Magia e Cultivo da Alma
    public class Projectile_CultivationSpell : Projectile
    {
        public string abilityDefName;
        public float baseDamage = 20f;
        public DamageDef customDamageDef = null;

        private bool trailInitialized = false;
        private Vector3 lastTrailPos = Vector3.zero;

        protected virtual Color PrimaryVFXColor { get { return RebornVFX.ColorSoul; } }
        protected virtual Color SecondaryVFXColor { get { return Color.white; } }
        protected virtual FleckDef TrailAccentFleck { get { return RebornVFXDefOf.Fleck_SoftSpark; } }
        protected virtual FleckDef ImpactShardFleck { get { return RebornVFXDefOf.Fleck_SoftSpark; } }
        protected virtual float TrailScale { get { return 0.75f; } }
        protected virtual float ImpactScale { get { return 1.5f; } }
        protected virtual SoundDef ImpactSound { get { return SoundDefOf.Psycast_Skip_Entry; } }

        protected override void Tick()
        {
            base.Tick();
            if (Destroyed || Map == null)
                return;

            Vector3 curPos = ExactPosition;
            if (!trailInitialized)
            {
                trailInitialized = true;
                lastTrailPos = curPos;
                EmitLaunchBurst(curPos, Map);
                return;
            }

            EmitFlightTrail(lastTrailPos, curPos, Map);
            lastTrailPos = curPos;
        }

        protected virtual void EmitLaunchBurst(Vector3 pos, Map map)
        {
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, pos, map, 0.45f, PrimaryVFXColor);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_GlowOrb, pos, map, 0.9f, Color.Lerp(PrimaryVFXColor, Color.white, 0.5f));
        }

        protected virtual void EmitFlightTrail(Vector3 prevPos, Vector3 curPos, Map map)
        {
            RebornVFX.PlayProjectileTrail(curPos, prevPos, map, PrimaryVFXColor, TrailAccentFleck, TrailScale);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            IntVec3 cell = Position;
            Vector3 impactPos = (hitThing != null) ? hitThing.DrawPos : cell.ToVector3Shifted();

            // Se bloqueado por escudo (ex: cinto de escudo vanilla), o projétil se dissipa sem tocar o alvo
            if (blockedByShield)
            {
                if (map != null)
                {
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, impactPos, map, 0.65f, RebornVFX.ColorPortal);
                }
                base.Impact(hitThing, true);
                return;
            }

            Pawn caster = launcher as Pawn;
            Hediff_SoulMatrix matrix = null;
            if (caster != null && caster.health != null && caster.health.hediffSet != null)
            {
                matrix = caster.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            }

            CultivatorAbilityRecord rec = (matrix != null && !string.IsNullOrEmpty(abilityDefName)) ? matrix.GetAbilityRecord(abilityDefName) : null;
            float pwrMult = (rec != null) ? rec.PowerMultiplier : 1.0f;
            float spellDamageMult = (matrix != null) ? matrix.SpellDamageMultiplier : 1.0f;

            float finalDamage = baseDamage * spellDamageMult * pwrMult;
            bool isCrit = false;
            if (matrix != null && UnityEngine.Random.value < matrix.CritChance)
            {
                isCrit = true;
                finalDamage *= matrix.CritDamageMultiplier;
            }

            DamageDef dDef = customDamageDef != null ? customDamageDef : (def.projectile != null && def.projectile.damageDef != null ? def.projectile.damageDef : DamageDefOf.Burn);

            // Se atingir um alvo sólido ou vivo
            if (hitThing != null)
            {
                DamageInfo dinfo = new DamageInfo(dDef, finalDamage, 2.0f, -1f, launcher);
                DamageWorker.DamageResult result = hitThing.TakeDamage(dinfo);

                bool absorbed = (result == null || result.totalDamageDealt <= 0.01f || result.deflected);

                if (map != null)
                {
                    RebornVFX.PlayImpactBurst(impactPos, map, PrimaryVFXColor, SecondaryVFXColor, ImpactScale * (isCrit ? 1.35f : 1.0f), ImpactShardFleck, isCrit ? 10 : 6, true, ImpactSound);
                    if (isCrit && matrix != null)
                    {
                        RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, impactPos, map, 1.5f, RebornVFX.ColorSkyGold, 0f, 240f);
                        MoteMaker.ThrowText(impactPos, map, string.Format("CRÍTICO! ({0:F1}x)", matrix.CritDamageMultiplier), new Color(1f, 0.85f, 0.2f), 2.5f);
                    }
                }

                // CRUCIAL: Só aplica efeitos nocivos secundários (fogo, hipotermia, paralisia) se o escudo NÃO absorveu o impacto!
                if (!absorbed)
                {
                    OnDamagedTarget(hitThing, caster, matrix, rec);
                }
            }
            else
            {
                // Impacto no solo ou célula vazia
                if (map != null)
                {
                    RebornVFX.PlayImpactBurst(impactPos, map, PrimaryVFXColor, SecondaryVFXColor, ImpactScale * 0.85f, ImpactShardFleck, 5, true, ImpactSound);
                }
                OnImpactCell(cell, map, caster);
            }

            base.Impact(hitThing, false);
        }

        protected virtual void OnDamagedTarget(Thing hitThing, Pawn caster, Hediff_SoulMatrix matrix, CultivatorAbilityRecord rec)
        {
        }

        protected virtual void OnImpactCell(IntVec3 cell, Map map, Pawn caster)
        {
        }
    }

    // 1. Projétil: Disparo da Alma
    public class Projectile_SoulBolt : Projectile_CultivationSpell
    {
        protected override Color PrimaryVFXColor { get { return RebornVFX.ColorSoul; } }
        protected override Color SecondaryVFXColor { get { return new Color(0.6f, 0.95f, 1f); } }

        public Projectile_SoulBolt()
        {
            baseDamage = 25f;
            customDamageDef = DamageDefOf.Burn;
        }

        protected override void OnDamagedTarget(Thing hitThing, Pawn caster, Hediff_SoulMatrix matrix, CultivatorAbilityRecord rec)
        {
            if (Map != null)
            {
                FleckMaker.ThrowLightningGlow(hitThing.DrawPos, Map, 1.5f);
            }
        }
    }

    // 2. Projétil: Flecha de Fogo
    public class Projectile_FireArrow : Projectile_CultivationSpell
    {
        protected override Color PrimaryVFXColor { get { return RebornVFX.ColorFire; } }
        protected override Color SecondaryVFXColor { get { return new Color(1f, 0.85f, 0.2f); } }
        protected override FleckDef TrailAccentFleck { get { return RebornVFXDefOf.Fleck_FlameWisp; } }
        protected override FleckDef ImpactShardFleck { get { return RebornVFXDefOf.Fleck_FlameWisp; } }
        protected override float TrailScale { get { return 0.82f; } }
        protected override float ImpactScale { get { return 1.65f; } }
        protected override SoundDef ImpactSound { get { return SoundDefOf.Interact_BeatFire; } }

        public Projectile_FireArrow()
        {
            baseDamage = 20f;
            customDamageDef = DamageDefOf.Flame;
        }

        protected override void EmitFlightTrail(Vector3 prevPos, Vector3 curPos, Map map)
        {
            base.EmitFlightTrail(prevPos, curPos, map);
            if (Rand.Chance(0.45f))
            {
                FleckMaker.ThrowFireGlow(curPos, map, 0.55f);
            }
        }

        protected override void OnDamagedTarget(Thing hitThing, Pawn caster, Hediff_SoulMatrix matrix, CultivatorAbilityRecord rec)
        {
            Pawn targetPawn = hitThing as Pawn;
            if (targetPawn != null && !targetPawn.Dead)
            {
                targetPawn.TryAttachFire(0.35f, caster);
            }
        }

        protected override void OnImpactCell(IntVec3 cell, Map map, Pawn caster)
        {
            if (map != null && cell.InBounds(map))
            {
                FireUtility.TryStartFireIn(cell, map, 0.35f, caster);
            }
        }
    }

    // 3. Projétil: Raio de Gelo
    public class Projectile_FrostRay : Projectile_CultivationSpell
    {
        protected override Color PrimaryVFXColor { get { return RebornVFX.ColorFrost; } }
        protected override Color SecondaryVFXColor { get { return new Color(0.85f, 0.98f, 1f); } }
        protected override FleckDef TrailAccentFleck { get { return RebornVFXDefOf.Fleck_IceCrystal; } }
        protected override FleckDef ImpactShardFleck { get { return RebornVFXDefOf.Fleck_IceCrystal; } }
        protected override float TrailScale { get { return 0.80f; } }
        protected override float ImpactScale { get { return 1.65f; } }
        protected override SoundDef ImpactSound { get { return SoundDefOf.Explosion_FirefoamPopper; } }

        public Projectile_FrostRay()
        {
            baseDamage = 30f;
            customDamageDef = DamageDefOf.Frostbite;
        }

        protected override void OnDamagedTarget(Thing hitThing, Pawn caster, Hediff_SoulMatrix matrix, CultivatorAbilityRecord rec)
        {
            Pawn targetPawn = hitThing as Pawn;
            if (targetPawn != null && !targetPawn.Dead && targetPawn.health != null && targetPawn.health.hediffSet != null)
            {
                Hediff hypo = targetPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
                if (hypo == null)
                {
                    hypo = HediffMaker.MakeHediff(HediffDefOf.Hypothermia, targetPawn);
                    hypo.Severity = 0.40f;
                    targetPawn.health.AddHediff(hypo);
                }
                else
                {
                    hypo.Severity += 0.35f;
                }
            }
        }
    }

    // 4. Projétil: Lança Celestial
    public class Projectile_SkyLance : Projectile_CultivationSpell
    {
        protected override Color PrimaryVFXColor { get { return RebornVFX.ColorSkyGold; } }
        protected override Color SecondaryVFXColor { get { return RebornVFX.ColorSoul; } }
        protected override float TrailScale { get { return 0.95f; } }
        protected override float ImpactScale { get { return 2.1f; } }
        protected override SoundDef ImpactSound { get { return SoundDefOf.Thunder_OnMap; } }

        public Projectile_SkyLance()
        {
            baseDamage = 50f;
            customDamageDef = DamageDefOf.Burn;
        }

        protected override void EmitLaunchBurst(Vector3 pos, Map map)
        {
            base.EmitLaunchBurst(pos, map);
            RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, pos, map, 1.3f, RebornVFX.ColorSkyGold, Rand.Range(0f, 360f), 260f);
        }

        protected override void OnDamagedTarget(Thing hitThing, Pawn caster, Hediff_SoulMatrix matrix, CultivatorAbilityRecord rec)
        {
            if (Map != null && hitThing != null)
            {
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_RuneCircleFast, hitThing.DrawPos, Map, 2.0f, RebornVFX.ColorSkyGold, 0f, 220f);
                RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_PowerBeam, hitThing.DrawPos + new Vector3(0f, 0f, 1.8f), Map, 1f, RebornVFX.ColorSkyGold, 0f, 0f, 0f, 0f, 0.16f, new Vector3(1.1f, 1f, 4.2f));
            }
            Pawn targetPawn = hitThing as Pawn;
            if (targetPawn != null && !targetPawn.Dead && targetPawn.stances != null && targetPawn.stances.stunner != null)
            {
                targetPawn.stances.stunner.StunFor(120, caster, true);
            }
        }
    }

    // 5. Projétil: Arco Voltaico (com ricochete em cadeia e raios reais em malha)
    public class Projectile_ChainLightning : Projectile_CultivationSpell
    {
        protected override Color PrimaryVFXColor { get { return RebornVFX.ColorLightning; } }
        protected override Color SecondaryVFXColor { get { return Color.white; } }
        protected override float TrailScale { get { return 0.85f; } }
        protected override float ImpactScale { get { return 1.85f; } }
        protected override SoundDef ImpactSound { get { return SoundDefOf.Thunder_OnMap; } }

        public Projectile_ChainLightning()
        {
            baseDamage = 45f;
            customDamageDef = DamageDefOf.Burn;
        }

        protected override void OnDamagedTarget(Thing hitThing, Pawn caster, Hediff_SoulMatrix matrix, CultivatorAbilityRecord rec)
        {
            Map map = Map;
            if (map == null || hitThing == null) return;

            // Arco relampejante principal ligando conjurador ao primeiro impacto
            if (caster != null && caster.Spawned && caster.Map == map)
            {
                RebornVFX.SpawnLightningArc(map, caster.DrawPos, hitThing.DrawPos, RebornVFX.ColorLightning, 0.72f, 20);
            }

            // Paralisia no alvo primário apenas se o dano não foi bloqueado por escudo
            Pawn primaryPawn = hitThing as Pawn;
            if (primaryPawn != null && !primaryPawn.Dead && primaryPawn.stances != null && primaryPawn.stances.stunner != null)
            {
                primaryPawn.stances.stunner.StunFor(150, caster, true);
            }

            // Ricochete em cadeia para inimigos próximos
            int tier = (rec != null) ? rec.masteryTier : 1;
            int maxChains = 1 + tier; // até 5 alvos

            List<Thing> hitTargets = new List<Thing>();
            hitTargets.Add(hitThing);

            Thing currentTarget = hitThing;
            float currentDmg = baseDamage * (matrix != null ? matrix.SpellDamageMultiplier : 1.0f) * (rec != null ? rec.PowerMultiplier : 1.0f) * 0.80f;

            while (currentTarget != null && hitTargets.Count < maxChains)
            {
                Thing nextTarget = null;
                float closestDist = 5.5f;

                List<Thing> candidates = GenRadial.RadialDistinctThingsAround(currentTarget.Position, map, 5.0f, true) != null
                    ? new List<Thing>(GenRadial.RadialDistinctThingsAround(currentTarget.Position, map, 5.0f, true))
                    : new List<Thing>();

                for (int i = 0; i < candidates.Count; i++)
                {
                    Thing c = candidates[i];
                    if (c != null && !hitTargets.Contains(c))
                    {
                        Pawn cp = c as Pawn;
                        if (cp != null && !cp.Dead && caster != null && cp.HostileTo(caster))
                        {
                            float dist = currentTarget.Position.DistanceTo(cp.Position);
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                nextTarget = cp;
                            }
                        }
                    }
                }

                if (nextTarget != null)
                {
                    hitTargets.Add(nextTarget);

                    RebornVFX.SpawnLightningArc(map, currentTarget.DrawPos, nextTarget.DrawPos, RebornVFX.ColorLightning, 0.62f, 22);
                    RebornVFX.PlayImpactBurst(nextTarget.DrawPos, map, RebornVFX.ColorLightning, Color.white, 1.45f, RebornVFXDefOf.Fleck_SoftSpark, 6, true, null);
                    MoteMaker.ThrowText(nextTarget.DrawPos, map, "ARCO!", new Color(0.3f, 0.85f, 1f), 2.0f);

                    DamageInfo chainDinfo = new DamageInfo(DamageDefOf.Burn, currentDmg, 2.0f, -1f, caster);
                    DamageWorker.DamageResult chainRes = nextTarget.TakeDamage(chainDinfo);

                    bool chainAbsorbed = (chainRes == null || chainRes.totalDamageDealt <= 0.01f || chainRes.deflected);
                    if (!chainAbsorbed)
                    {
                        Pawn nextPawn = nextTarget as Pawn;
                        if (nextPawn != null && !nextPawn.Dead && nextPawn.stances != null && nextPawn.stances.stunner != null)
                        {
                            nextPawn.stances.stunner.StunFor(100, caster, true);
                        }
                    }

                    currentDmg *= 0.80f;
                    currentTarget = nextTarget;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
