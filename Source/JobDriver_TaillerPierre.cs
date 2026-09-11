using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Le singe rejoint le morceau de pierre et le débite sur place en blocs.
    // Le rendement et la vitesse dépendent de sa maîtrise, et chaque ouvrage
    // use le percuteur qu'il porte.
    public class JobDriver_TaillerPierre : JobDriver_Ouvrage
    {
        protected override Metier MetierExerce => Metier.Taille;

        protected override ThingDef OutilRequis => AAW_DefOf.AAW_Percuteur;

        protected override string Effet => "CutStone";

        protected override string Son => "Recipe_MakeStoneBlocks";

        protected override void Terminer()
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
        }
    }
}
