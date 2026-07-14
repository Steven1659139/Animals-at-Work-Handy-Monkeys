using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnimalsAtWork.Monkeys
{
    // Le singe rejoint le morceau de pierre et le débite sur place en blocs.
    // Le rendement et la vitesse dépendent de sa maîtrise, et chaque ouvrage
    // use le percuteur qu'il porte.
    public class JobDriver_TaillerPierre : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            // Sans percuteur sur soi, pas de taille.
            this.FailOn(() => OutilUtility.OutilEquipe(pawn, AAW_DefOf.AAW_Percuteur) == null);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil taille = Toils_General.Wait(MaitriseUtility.DureeTravail(pawn, Metier.Taille));
            taille.WithProgressBarToilDelay(TargetIndex.A);
            Ambiance.Habiller(taille, TargetIndex.A, "CutStone", "Recipe_MakeStoneBlocks");
            yield return taille;

            yield return Toils_General.Do(delegate
            {
                Thing morceau = job.targetA.Thing;
                List<ThingDefCountClass> produits = morceau.def.butcherProducts;
                IntVec3 position = morceau.Position;
                Map map = pawn.Map;
                morceau.Destroy();

                if (produits != null)
                {
                    float rendement = MaitriseUtility.Rendement(pawn, Metier.Taille);
                    for (int i = 0; i < produits.Count; i++)
                    {
                        Thing blocs = ThingMaker.MakeThing(produits[i].thingDef);
                        blocs.stackCount = Mathf.Max(1, Mathf.RoundToInt(produits[i].count * rendement));
                        GenPlace.TryPlaceThing(blocs, position, map, ThingPlaceMode.Near);
                    }
                }

                MaitriseUtility.GagnerExperience(pawn, Metier.Taille);
                OutilUtility.UserOutil(pawn, AAW_DefOf.AAW_Percuteur);
            });
        }
    }
}
