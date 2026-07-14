using RimWorld;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Les outils de pierre du singe artisan : percuteur (percussion) et
    // couteau (coupe). Portés dans l'inventaire (onglet Équipement), usés
    // à chaque ouvrage, brisés au bout de leur vie.
    public static class OutilUtility
    {
        private const int UsureParOuvrage = 4; // percuteur ~25 ouvrages, couteau ~15

        // L'outil de ce type que l'animal porte sur lui, s'il y en a un.
        public static Thing OutilEquipe(Pawn pawn, ThingDef outil)
        {
            if (pawn.inventory == null)
            {
                return null;
            }
            ThingOwner contenu = pawn.inventory.innerContainer;
            for (int i = 0; i < contenu.Count; i++)
            {
                if (contenu[i].def == outil)
                {
                    return contenu[i];
                }
            }
            return null;
        }

        // L'outil porté s'use ; brisé, le singe ira s'en procurer un neuf.
        public static void UserOutil(Pawn pawn, ThingDef outil)
        {
            Thing porte = OutilEquipe(pawn, outil);
            if (porte == null)
            {
                return;
            }
            porte.HitPoints -= UsureParOuvrage;
            if (porte.HitPoints <= 0)
            {
                string nom = porte.def.label;
                porte.Destroy();
                Messages.Message("AAW_OutilBrise".Translate(pawn.LabelShortCap, nom),
                    pawn, MessageTypeDefOf.NeutralEvent);
            }
        }

        // Façonne `quantite` outils de la pierre du morceau ; le singe en
        // garde un sur lui (au sol si l'inventaire refuse), le reste au sol.
        public static void TaillerDepuisMorceau(Pawn pawn, Thing morceau, ThingDef outil, int quantite)
        {
            IntVec3 position = morceau.Position;
            Map map = pawn.Map;
            ThingDef etoffe = EtoffeDuMorceau(morceau, outil);
            morceau.Destroy();

            Thing production = ThingMaker.MakeThing(outil, etoffe);
            production.stackCount = quantite;
            Thing unite = production.stackCount > 1 ? production.SplitOff(1) : production;
            if (!pawn.inventory.innerContainer.TryAdd(unite, false))
            {
                GenPlace.TryPlaceThing(unite, position, map, ThingPlaceMode.Near);
            }
            if (unite != production)
            {
                GenPlace.TryPlaceThing(production, position, map, ThingPlaceMode.Near);
            }
        }

        // L'outil hérite de la pierre du morceau (blocs = étoffe Stony).
        private static ThingDef EtoffeDuMorceau(Thing morceau, ThingDef outil)
        {
            var produits = morceau.def.butcherProducts;
            if (produits != null)
            {
                for (int i = 0; i < produits.Count; i++)
                {
                    ThingDef d = produits[i].thingDef;
                    if (d.IsStuff && d.stuffProps.categories.Contains(StuffCategoryDefOf.Stony))
                    {
                        return d;
                    }
                }
            }
            return GenStuff.DefaultStuffFor(outil);
        }
    }
}
