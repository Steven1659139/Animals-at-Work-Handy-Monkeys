using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Faute de couteau à ramasser, un singe au percuteur rejoint un morceau
    // de pierre et en frappe des éclats tranchants : trois couteaux, dont un
    // qu'il garde sur lui. Le percuteur s'use à l'ouvrage.
    public class JobDriver_TaillerCouteaux : JobDriver_Ouvrage
    {
        private const int KnivesPerChunk = 3;

        protected override Metier MetierExerce => Metier.Taille;

        protected override ThingDef RequiredTool => AAW_DefOf.AAW_Percuteur;

        protected override string Effect => "CutStone";

        protected override string Sound => "Recipe_MakeStoneBlocks";

        protected override void Terminer()
        {
            OutilUtility.KnapFromChunk(pawn, job.targetA.Thing,
                AAW_DefOf.AAW_Couteau, KnivesPerChunk);
            OutilUtility.WearTool(pawn, AAW_DefOf.AAW_Percuteur);
            MaitriseUtility.GainExperience(pawn, Metier.Taille);
        }
    }
}
