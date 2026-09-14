using RimWorld;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Les outils de pierre du singe artisan : percuteur (percussion) et
    // couteau (coupe). Portés dans l'inventaire (onglet Équipement), usés
    // à chaque ouvrage, brisés au bout de leur vie.
    public static class OutilUtility
    {
        private const int WearPerWork = 4; // percuteur ~25 ouvrages, couteau ~15

        // L'outil de ce type que l'animal porte sur lui, s'il y en a un.
        public static Thing EquippedTool(Pawn pawn, ThingDef tool)
        {
            ThingOwner contents = pawn.inventory.innerContainer;
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i].def == tool)
                {
                    return contents[i];
                }
            }
            return null;
        }

        public static bool Carries(Pawn pawn, ThingDef tool)
        {
            return EquippedTool(pawn, tool) != null;
        }

        // Range l'objet dans l'inventaire du singe. Refusé (inventaire plein),
        // il est posé au sol près de la position donnée. Vrai s'il est sur lui.
        public static bool StoreInInventory(Pawn pawn, Thing item, IntVec3 position, Map map)
        {
            if (item.Spawned)
            {
                item.DeSpawn();
            }
            if (pawn.inventory.innerContainer.TryAdd(item, false))
            {
                return true;
            }
            GenPlace.TryPlaceThing(item, position, map, ThingPlaceMode.Near);
            return false;
        }

        // L'outil porté s'use ; brisé, le singe ira s'en procurer un neuf.
        public static void WearTool(Pawn pawn, ThingDef tool)
        {
            Thing carried = EquippedTool(pawn, tool);
            if (carried == null)
            {
                return;
            }
            carried.HitPoints -= WearPerWork;
            if (carried.HitPoints <= 0)
            {
                string name = carried.def.label;
                carried.Destroy();
                Messages.Message("AAW_OutilBrise".Translate(pawn.LabelShortCap, name),
                    pawn, MessageTypeDefOf.NeutralEvent);
            }
        }

        // Façonne `quantite` outils de la pierre du morceau ; le singe en
        // garde un sur lui (au sol si l'inventaire refuse), le reste au sol.
        public static void KnapFromChunk(Pawn pawn, Thing chunk, ThingDef tool, int count)
        {
            IntVec3 position = chunk.Position;
            Map map = pawn.Map;
            ThingDef stuff = StuffOfChunk(chunk, tool);
            chunk.Destroy();

            Thing production = ThingMaker.MakeThing(tool, stuff);
            production.stackCount = count;
            Thing unit = production.stackCount > 1 ? production.SplitOff(1) : production;
            StoreInInventory(pawn, unit, position, map);
            if (unit != production)
            {
                GenPlace.TryPlaceThing(production, position, map, ThingPlaceMode.Near);
            }
        }

        // L'outil hérite de la pierre du morceau (blocs = étoffe Stony).
        private static ThingDef StuffOfChunk(Thing chunk, ThingDef tool)
        {
            var products = chunk.def.butcherProducts;
            if (products != null)
            {
                for (int i = 0; i < products.Count; i++)
                {
                    ThingDef d = products[i].thingDef;
                    if (d.IsStuff && d.stuffProps.categories.Contains(StuffCategoryDefOf.Stony))
                    {
                        return d;
                    }
                }
            }
            return GenStuff.DefaultStuffFor(tool);
        }
    }
}
