namespace AnimalsAtWork.Monkeys
{
    // Faute de percuteur à ramasser, un singe maître rejoint un morceau de
    // pierre et s'y taille son propre outil, de la pierre du morceau.
    public class JobDriver_TaillerPercuteur : JobDriver_Ouvrage
    {
        protected override Metier MetierExerce => Metier.Taille;

        protected override string Effect => "CutStone";

        protected override string Sound => "Recipe_MakeStoneBlocks";

        protected override void Terminer()
        {
            OutilUtility.KnapFromChunk(pawn, job.targetA.Thing, AAW_DefOf.AAW_Percuteur, 1);
            MaitriseUtility.GainExperience(pawn, Metier.Taille);
        }
    }
}
