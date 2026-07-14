using RimWorld;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    [DefOf]
    public static class AAW_DefOf
    {
        public static JobDef AAW_TaillerPierre;
        public static JobDef AAW_PreparerCroquettes;
        public static JobDef AAW_PrendrePercuteur;
        public static JobDef AAW_RapporterMorceau;
        public static JobDef AAW_TaillerPercuteur;
        public static JobDef AAW_DebiterCarcasse;
        public static JobDef AAW_PrendreCouteau;
        public static JobDef AAW_TaillerCouteaux;
        public static ThingDef AAW_AtelierDesSinges;
        public static ThingDef AAW_Percuteur;
        public static ThingDef AAW_Couteau;
        public static TrainableDef AAW_Artisanat;

        static AAW_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AAW_DefOf));
        }
    }
}
