using System;
using RimWorld;
using Verse;

namespace RebornEvolvingMagic
{
    public class CompProperties_UseEffect_AwakenSoul : CompProperties_UseEffect
    {
        public CompProperties_UseEffect_AwakenSoul()
        {
            compClass = typeof(CompUseEffect_AwakenSoul);
        }
    }

    public class CompUseEffect_AwakenSoul : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy == null || usedBy.Dead || usedBy.health == null || usedBy.health.hediffSet == null) return;

            Hediff_SoulMatrix existing = usedBy.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (existing == null)
            {
                Hediff created = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_SoulMatrix, usedBy);
                if (created != null)
                {
                    usedBy.health.AddHediff(created);
                    if (usedBy.Spawned && usedBy.Map != null)
                    {
                        RebornVFX.PlayAscensionPillar(usedBy, RebornVFX.ColorSoul, "MATRIZ DA ALMA DESPERTA!", 2.2f);
                    }
                    Messages.Message(string.Format("{0} despertou sua Matriz de Feitiço da Alma (Classe Mortal)!", usedBy.LabelShort), usedBy, MessageTypeDefOf.PositiveEvent);
                }
            }
            else
            {
                existing.RestoreEnergy(existing.MaxEnergy, true);
                existing.AddXP(50f, true);
                if (usedBy.Spawned && usedBy.Map != null)
                {
                    RebornVFX.PlayCastRuneCircle(usedBy.DrawPos, usedBy.Map, RebornVFX.ColorSoul, 2.2f, 140f);
                    RebornVFX.ThrowFleck(RebornVFXDefOf.Fleck_Shockwave, usedBy.DrawPos, usedBy.Map, 0.9f, RebornVFX.ColorSoul);
                }
                Messages.Message(string.Format("A Matriz de {0} absorveu a energia do cristal!", usedBy.LabelShort), usedBy, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
