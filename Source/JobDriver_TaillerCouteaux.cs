using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Faute de couteau à ramasser, un singe au percuteur rejoint un morceau
    // de pierre et en frappe des éclats tranchants : trois couteaux, dont un
    // qu'il garde sur lui. Le percuteur s'use à l'ouvrage.
    public class JobDriver_TaillerCouteaux : JobDriver_Ouvrage
    {
        private const int CouteauxParMorceau = 3;

        protected override Metier MetierExerce => Metier.Taille;

        protected override ThingDef OutilRequis => AAW_DefOf.AAW_Percuteur;

        protected override string Effet => "CutStone";

        protected override string Son => "Recipe_MakeStoneBlocks";

        protected override void Terminer()
        {
            OutilUtility.TaillerDepuisMorceau(pawn, job.targetA.Thing,
                AAW_DefOf.AAW_Couteau, CouteauxParMorceau);
            OutilUtility.UserOutil(pawn, AAW_DefOf.AAW_Percuteur);
            MaitriseUtility.GagnerExperience(pawn, Metier.Taille);
        }
    }
}
