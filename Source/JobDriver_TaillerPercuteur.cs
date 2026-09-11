namespace AnimalsAtWork.Monkeys
{
    // Faute de percuteur à ramasser, un singe maître rejoint un morceau de
    // pierre et s'y taille son propre outil, de la pierre du morceau.
    public class JobDriver_TaillerPercuteur : JobDriver_Ouvrage
    {
        protected override Metier MetierExerce => Metier.Taille;

        protected override string Effet => "CutStone";

        protected override string Son => "Recipe_MakeStoneBlocks";

        protected override void Terminer()
        {
            OutilUtility.TaillerDepuisMorceau(pawn, job.targetA.Thing, AAW_DefOf.AAW_Percuteur, 1);
            MaitriseUtility.GagnerExperience(pawn, Metier.Taille);
        }
    }
}
