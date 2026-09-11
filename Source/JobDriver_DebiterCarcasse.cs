using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Le singe rejoint la carcasse près de l'atelier et la débite au couteau
    // de pierre : viande et cuir selon sa maîtrise de boucher, du sang au sol,
    // et le couteau s'use. Plus la bête est grosse, plus l'ouvrage est long
    // et sanglant.
    public class JobDriver_DebiterCarcasse : JobDriver_Ouvrage
    {
        protected override Metier MetierExerce => Metier.Boucherie;

        protected override ThingDef OutilRequis => AAW_DefOf.AAW_Couteau;

        protected override string Effet => "ButcherFlesh";

        protected override string Son => "Recipe_ButcherCorpseFlesh";

        protected override float FacteurDuree => FacteurGabarit();

        // Rapporté au petit gibier : 1 pour tout ce qui tient sous le plafond
        // novice, puis proportionnel au gabarit (un muffalo ≈ 5, un thrumbo ≈ 8).
        private float FacteurGabarit()
        {
            Corpse carcasse = (Corpse)job.targetA.Thing;
            return Mathf.Max(1f,
                carcasse.InnerPawn.RaceProps.baseBodySize / JobGiver_Tailleur.TailleGibierMax);
        }

        protected override void Terminer()
        {
            Corpse carcasse = (Corpse)job.targetA.Thing;
            IntVec3 position = carcasse.Position;
            Map map = pawn.Map;
            List<Thing> produits = carcasse.ButcherProducts(pawn,
                MaitriseUtility.Rendement(pawn, Metier.Boucherie)).ToList();
            ThingDef sang = carcasse.InnerPawn.RaceProps.BloodDef;
            int flaques = Mathf.Clamp(Mathf.RoundToInt(3f * FacteurGabarit()), 3, 12);
            carcasse.Destroy();

            if (sang != null)
            {
                FilthMaker.TryMakeFilth(position, map, sang, flaques);
            }
            for (int i = 0; i < produits.Count; i++)
            {
                GenPlace.TryPlaceThing(produits[i], position, map, ThingPlaceMode.Near);
            }

            MaitriseUtility.GagnerExperience(pawn, Metier.Boucherie);
            OutilUtility.UserOutil(pawn, AAW_DefOf.AAW_Couteau);
        }
    }
}
