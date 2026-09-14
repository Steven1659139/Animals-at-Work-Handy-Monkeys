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
        // Part prélevée sur chaque pile ; le JobGiver exige des piles au moins
        // aussi grosses.
        public const int IngredientsRequis = 20;
        private const int BaseKibble = 50;

        private Thing carriedMeat;
        private Thing carriedPlant;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref carriedMeat, "AAW_viandePortee");
            Scribe_References.Look(ref carriedPlant, "AAW_vegetalPorte");
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // On ne réserve que la part prélevée, pas la pile entière.
            return pawn.Reserve(job.targetA, job, 1, IngredientsRequis, null, errorOnFailed)
                && pawn.Reserve(job.targetB, job, 1, IngredientsRequis, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.C);
            AddFinishAction(delegate
            {
                PutBack(ref carriedMeat);
                PutBack(ref carriedPlant);
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOn(() => job.targetA.Thing.stackCount < IngredientsRequis);
            yield return Toils_General.Do(delegate
            {
                carriedMeat = Take(job.targetA.Thing);
                if (carriedMeat == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            });

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOn(() => job.targetB.Thing.stackCount < IngredientsRequis);
            yield return Toils_General.Do(delegate
            {
                carriedPlant = Take(job.targetB.Thing);
                if (carriedPlant == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            });

            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);

            Toil preparation = Toils_General.Wait(MaitriseUtility.WorkDuration(pawn, Metier.Boucherie));
            preparation.WithProgressBarToilDelay(TargetIndex.C);
            // Les deux ingrédients doivent encore être sur lui : un objet
            // détruit ou sorti de l'inventaire en cours de route arrête tout.
            preparation.FailOn(() => !StillCarries(carriedMeat) || !StillCarries(carriedPlant));
            Ambiance.Dress(preparation, TargetIndex.C, "Cook", "Recipe_CookMeal");
            yield return preparation;

            yield return Toils_General.Do(delegate
            {
                Thing workshop = job.targetC.Thing;
                Map map = pawn.Map;
                carriedMeat.Destroy();
                carriedPlant.Destroy();
                carriedMeat = null;
                carriedPlant = null;

                Thing kibble = ThingMaker.MakeThing(ThingDefOf.Kibble);
                kibble.stackCount = Mathf.RoundToInt(BaseKibble * MaitriseUtility.Yield(pawn, Metier.Boucherie));
                GenPlace.TryPlaceThing(kibble, workshop.Position, map, ThingPlaceMode.Near);

                MaitriseUtility.GainExperience(pawn, Metier.Boucherie);
            });
        }

        private bool StillCarries(Thing ingredient)
        {
            return ingredient != null && pawn.inventory.innerContainer.Contains(ingredient);
        }

        // Prélève la part sur la pile et la range dans l'inventaire du singe ;
        // null si l'inventaire a refusé (la part est alors au sol).
        private Thing Take(Thing stack)
        {
            Thing part = stack.SplitOff(IngredientsRequis);
            return OutilUtility.StoreInInventory(pawn, part, pawn.Position, pawn.Map) ? part : null;
        }

        // Job interrompu avant l'ouvrage : le singe repose l'ingrédient porté.
        private void PutBack(ref Thing ingredient)
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
