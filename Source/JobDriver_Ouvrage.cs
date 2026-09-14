using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnimalsAtWork.Monkeys
{
    // Squelette commun des ouvrages faits sur place, sur une cible au sol
    // (morceau de pierre, carcasse) : réservation de la cible, trajet, temps
    // de travail habillé de l'effet et du son du métier, puis production.
    // Chaque ouvrage concret dit son métier, l'outil qu'il exige et ce qu'il
    // produit une fois le temps écoulé.
    public abstract class JobDriver_Ouvrage : JobDriver
    {
        protected abstract Metier MetierExerce { get; }

        // Outil que le singe doit porter du début à la fin ; null si aucun.
        protected virtual ThingDef RequiredTool => null;

        protected abstract string Effect { get; }

        protected abstract string Sound { get; }

        // Multiplie la durée de base du métier (1 par défaut).
        protected virtual float DurationFactor => 1f;

        // Le temps de travail écoulé, la cible est encore là : produire.
        protected abstract void Terminer();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            ThingDef tool = RequiredTool;
            if (tool != null)
            {
                this.FailOn(() => !OutilUtility.Carries(pawn, tool));
            }

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            int duration = Mathf.RoundToInt(
                MaitriseUtility.WorkDuration(pawn, MetierExerce) * DurationFactor);
            Toil work = Toils_General.Wait(duration);
            work.WithProgressBarToilDelay(TargetIndex.A);
            Ambiance.Dress(work, TargetIndex.A, Effect, Sound);
            yield return work;

            yield return Toils_General.Do(Terminer);
        }
    }
}
