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

            Toil ramasser = Toils_General.Wait(60);
            ramasser.WithProgressBarToilDelay(TargetIndex.A);
            yield return ramasser;

            yield return Toils_General.Do(delegate
            {
                Thing pile = job.targetA.Thing;
                IntVec3 position = pile.Position;
                Map map = pawn.Map;
                Thing unite = pile.SplitOff(1);
                if (unite.Spawned)
                {
                    unite.DeSpawn();
                }
                if (!pawn.inventory.innerContainer.TryAdd(unite, false))
                {
                    GenPlace.TryPlaceThing(unite, position, map, ThingPlaceMode.Near);
                }
            });
        }
    }
}
