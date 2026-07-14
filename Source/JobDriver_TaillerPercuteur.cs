using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AnimalsAtWork.Monkeys
{
    // Faute de percuteur à ramasser, un singe maître rejoint un morceau de
    // pierre et s'y taille son propre outil, de la pierre du morceau.
    public class JobDriver_TaillerPercuteur : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil taille = Toils_General.Wait(MaitriseUtility.DureeTravail(pawn, Metier.Taille));
            taille.WithProgressBarToilDelay(TargetIndex.A);
            Ambiance.Habiller(taille, TargetIndex.A, "CutStone", "Recipe_MakeStoneBlocks");
            yield return taille;

            yield return Toils_General.Do(delegate
            {
                OutilUtility.TaillerDepuisMorceau(pawn, job.targetA.Thing, AAW_DefOf.AAW_Percuteur, 1);
                MaitriseUtility.GagnerExperience(pawn, Metier.Taille);
            });
        }
    }
}
