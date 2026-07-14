using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnimalsAtWork.Monkeys
{
    // Le singe ramasse la viande, puis les végétaux (portés dans son
    // inventaire), puis travaille à l'atelier et produit des croquettes.
    // Rendement selon la maîtrise (colon : 20 + 20 -> 50 croquettes ;
    // singe : 60 à 90 % de ça). Interrompu, il repose ce qu'il transporte.
    public class JobDriver_PreparerCroquettes : JobDriver
    {
        private const int IngredientsRequis = 20;
        private const int CroquettesBase = 50;

        private Thing viandePortee;
        private Thing vegetalPorte;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref viandePortee, "AAW_viandePortee");
            Scribe_References.Look(ref vegetalPorte, "AAW_vegetalPorte");
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.C);
            AddFinishAction(delegate
            {
                Reposer(ref viandePortee);
                Reposer(ref vegetalPorte);
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOn(() => job.targetA.Thing.stackCount < IngredientsRequis);
            yield return Toils_General.Do(delegate
            {
                viandePortee = Prendre(job.targetA.Thing);
                if (viandePortee == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            });

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOn(() => job.targetB.Thing.stackCount < IngredientsRequis);
            yield return Toils_General.Do(delegate
            {
                vegetalPorte = Prendre(job.targetB.Thing);
                if (vegetalPorte == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            });

            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);

            Toil preparation = Toils_General.Wait(MaitriseUtility.DureeTravail(pawn, Metier.Boucherie));
            preparation.WithProgressBarToilDelay(TargetIndex.C);
            preparation.FailOn(() => viandePortee == null || vegetalPorte == null);
            Ambiance.Habiller(preparation, TargetIndex.C, "Cook", "Recipe_CookMeal");
            yield return preparation;

            yield return Toils_General.Do(delegate
            {
                Thing atelier = job.targetC.Thing;
                Map map = pawn.Map;
                viandePortee.Destroy();
                vegetalPorte.Destroy();
                viandePortee = null;
                vegetalPorte = null;

                Thing croquettes = ThingMaker.MakeThing(ThingDefOf.Kibble);
                croquettes.stackCount = Mathf.RoundToInt(CroquettesBase * MaitriseUtility.Rendement(pawn, Metier.Boucherie));
                GenPlace.TryPlaceThing(croquettes, atelier.Position, map, ThingPlaceMode.Near);

                MaitriseUtility.GagnerExperience(pawn, Metier.Boucherie);
            });
        }

        // Prélève la part sur la pile et la range dans l'inventaire du singe.
        private Thing Prendre(Thing pile)
        {
            Thing part = pile.SplitOff(IngredientsRequis);
            if (part.Spawned)
            {
                part.DeSpawn();
            }
            if (!pawn.inventory.innerContainer.TryAdd(part, false))
            {
                GenPlace.TryPlaceThing(part, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                return null;
            }
            return part;
        }

        // Job interrompu avant l'ouvrage : le singe repose l'ingrédient porté.
        private void Reposer(ref Thing ingredient)
        {
            if (ingredient != null && !ingredient.Destroyed)
            {
                Map map = pawn.MapHeld;
                if (map != null && pawn.inventory.innerContainer.Contains(ingredient))
                {
                    pawn.inventory.innerContainer.TryDrop(ingredient, pawn.PositionHeld, map,
                        ThingPlaceMode.Near, out _);
                }
            }
            ingredient = null;
        }
    }
}
