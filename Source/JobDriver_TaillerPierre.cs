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

        protected override ThingDef RequiredTool => AAW_DefOf.AAW_Percuteur;

        protected override string Effect => "CutStone";

        protected override string Sound => "Recipe_MakeStoneBlocks";

        protected override void Terminer()
        {
            Thing chunk = job.targetA.Thing;
            List<ThingDefCountClass> products = chunk.def.butcherProducts;
            IntVec3 position = chunk.Position;
            Map map = pawn.Map;
            chunk.Destroy();

            if (products != null)
            {
                float yield = MaitriseUtility.Yield(pawn, Metier.Taille);
                for (int i = 0; i < products.Count; i++)
                {
                    Thing blocks = ThingMaker.MakeThing(products[i].thingDef);
                    blocks.stackCount = Mathf.Max(1, Mathf.RoundToInt(products[i].count * yield));
                    GenPlace.TryPlaceThing(blocks, position, map, ThingPlaceMode.Near);
                }
            }

            MaitriseUtility.GainExperience(pawn, Metier.Taille);
            OutilUtility.WearTool(pawn, AAW_DefOf.AAW_Percuteur);
        }
    }
}
