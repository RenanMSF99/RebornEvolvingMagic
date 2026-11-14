using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RebornEvolvingMagic
{
    public class CompProperties_UseEffect_BindMagicPearl : CompProperties_UseEffect
    {
        public CompProperties_UseEffect_BindMagicPearl()
        {
            compClass = typeof(CompUseEffect_BindMagicPearl);
        }
    }

    public class CompUseEffect_BindMagicPearl : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (usedBy == null || usedBy.Dead || usedBy.health == null || usedBy.health.hediffSet == null) return;

            Hediff_SoulMatrix matrix = usedBy.health.hediffSet.GetFirstHediffOfDef(RebornDefOf.REM_Hediff_SoulMatrix) as Hediff_SoulMatrix;
            if (matrix == null)
            {
                matrix = HediffMaker.MakeHediff(RebornDefOf.REM_Hediff_SoulMatrix, usedBy) as Hediff_SoulMatrix;
                if (matrix != null)
                {
                    usedBy.health.AddHediff(matrix);
                }
            }

            if (matrix != null)
            {
                matrix.BindMagicPearl();
            }

            if (usedBy.story != null && usedBy.story.traits != null)
            {
                if (RebornDefOf.REM_Trait_MagicPearlBearer != null && !usedBy.story.traits.HasTrait(RebornDefOf.REM_Trait_MagicPearlBearer))
                {
                    usedBy.story.traits.GainTrait(new Trait(RebornDefOf.REM_Trait_MagicPearlBearer));
                }
            }

            if (usedBy.Spawned && usedBy.Map != null)
            {
                RebornVFX.PlayAscensionPillar(usedBy, RebornVFX.ColorSkyGold, "A PÉROLA MÁGICA FOI FUNDIDA À ALMA!", 2.8f);
            }

            if (usedBy.IsColonistPlayerControlled)
            {
                Find.LetterStack.ReceiveLetter(
                    "A Pérola Mágica Foi Fundida!",
                    string.Format("A misteriosa Pérola Mágica penetrou no âmago da Matriz da Alma de {0}!\n\nAgora {0} quebrou o teto biológico da sua espécie: ao devorar e absorver a alma de criaturas caídas, seus códigos genéticos serão gravados no catálogo dimensional da Pérola, permitindo-lhe remodelar seu corpo livremente através da Aba 'Alma'.", usedBy.LabelShort),
                    LetterDefOf.PositiveEvent,
                    usedBy
                );
            }
        }
    }
}
