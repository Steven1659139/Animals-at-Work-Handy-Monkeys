using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AnimalsAtWork.Monkeys
{
    // Faute de couteau à ramasser, un singe au percuteur rejoint un morceau
    // de pierre et en frappe des éclats tranchants : trois couteaux, dont un
    // qu'il garde sur lui. Le percuteur s'use à l'ouvrage.
    public class JobDriver_TaillerCouteaux : JobDriver
    {
        private const int CouteauxParMorceau = 3;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => OutilUtility.OutilEquipe(pawn, AAW_DefOf.AAW_Percuteur) == null);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil taille = Toils_General.Wait(MaitriseUtility.DureeTravail(pawn, Metier.Taille));
            taille.WithProgressBarToilDelay(TargetIndex.A);
            Ambiance.Habiller(taille, TargetIndex.A, "CutStone", "Recipe_MakeStoneBlocks");
            yield return taille;

            yield return Toils_General.Do(delegate
            {
                OutilUtility.TaillerDepuisMorceau(pawn, job.targetA.Thing,
                    AAW_DefOf.AAW_Couteau, CouteauxParMorceau);
                OutilUtility.UserOutil(pawn, AAW_DefOf.AAW_Percuteur);
                MaitriseUtility.GagnerExperience(pawn, Metier.Taille);
            });
        }
    }
}
