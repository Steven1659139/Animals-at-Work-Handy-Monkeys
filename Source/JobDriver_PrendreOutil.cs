using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AnimalsAtWork.Monkeys
{
    // Le singe va chercher un outil de pierre (percuteur ou couteau) et le
    // garde sur lui (inventaire, visible dans l'onglet Équipement). L'outil
    // le suit partout et tombe au sol à sa mort.
    public class JobDriver_PrendreOutil : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil pickUp = Toils_General.Wait(60);
            pickUp.WithProgressBarToilDelay(TargetIndex.A);
            yield return pickUp;

            yield return Toils_General.Do(delegate
            {
                Thing stack = job.targetA.Thing;
                IntVec3 position = stack.Position;
                Map map = pawn.Map;
                OutilUtility.StoreInInventory(pawn, stack.SplitOff(1), position, map);
            });
        }
    }
}
