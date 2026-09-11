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
        protected virtual ThingDef OutilRequis => null;

        protected abstract string Effet { get; }

        protected abstract string Son { get; }

        // Multiplie la durée de base du métier (1 par défaut).
        protected virtual float FacteurDuree => 1f;

        // Le temps de travail écoulé, la cible est encore là : produire.
        protected abstract void Terminer();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            ThingDef outil = OutilRequis;
            if (outil != null)
            {
                this.FailOn(() => !OutilUtility.Porte(pawn, outil));
            }

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            int duree = Mathf.RoundToInt(
                MaitriseUtility.DureeTravail(pawn, MetierExerce) * FacteurDuree);
            Toil travail = Toils_General.Wait(duree);
            travail.WithProgressBarToilDelay(TargetIndex.A);
            Ambiance.Habiller(travail, TargetIndex.A, Effet, Son);
            yield return travail;

            yield return Toils_General.Do(Terminer);
        }
    }
}
