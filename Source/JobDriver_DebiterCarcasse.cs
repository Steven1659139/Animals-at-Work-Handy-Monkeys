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

        protected override ThingDef RequiredTool => AAW_DefOf.AAW_Couteau;

        protected override string Effect => "ButcherFlesh";

        protected override string Sound => "Recipe_ButcherCorpseFlesh";

        protected override float DurationFactor => SizeFactor();

        // Rapporté au petit gibier : 1 pour tout ce qui tient sous le plafond
        // novice, puis proportionnel au gabarit (un muffalo ≈ 5, un thrumbo ≈ 8).
        private float SizeFactor()
        {
            Corpse carcass = (Corpse)job.targetA.Thing;
            return Mathf.Max(1f,
                carcass.InnerPawn.RaceProps.baseBodySize / JobGiver_Tailleur.MaxGameSize);
        }

        protected override void Terminer()
        {
            Corpse carcass = (Corpse)job.targetA.Thing;
            IntVec3 position = carcass.Position;
            Map map = pawn.Map;
            List<Thing> products = carcass.ButcherProducts(pawn,
                MaitriseUtility.Yield(pawn, Metier.Boucherie)).ToList();
            ThingDef blood = carcass.InnerPawn.RaceProps.BloodDef;
            int puddles = Mathf.Clamp(Mathf.RoundToInt(3f * SizeFactor()), 3, 12);
            carcass.Destroy();

            if (blood != null)
            {
                FilthMaker.TryMakeFilth(position, map, blood, puddles);
            }
            for (int i = 0; i < products.Count; i++)
            {
                GenPlace.TryPlaceThing(products[i], position, map, ThingPlaceMode.Near);
            }

            MaitriseUtility.GainExperience(pawn, Metier.Boucherie);
            OutilUtility.WearTool(pawn, AAW_DefOf.AAW_Couteau);
        }
    }
}
