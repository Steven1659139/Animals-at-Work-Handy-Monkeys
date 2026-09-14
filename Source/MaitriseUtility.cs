using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Les deux savoir-faire d'un singe artisan : chacun progresse de son côté.
    public enum Metier
    {
        Taille,
        Boucherie
    }

    // La maîtrise d'artisan, adossée au registre de monde WorldComponent_Artisans.
    public static class MaitriseUtility
    {
        public const float MasterThreshold = 0.7f;
        private const float GainPerWork = 0.012f;
        private const float MentorRadius = 10f;
        private const float MentorMultiplier = 2f;

        public static float Level(Pawn pawn, Metier trade)
        {
            return WorldComponent_Artisans.Instance.Level(pawn, trade);
        }

        public static bool IsMaster(Pawn pawn, Metier trade)
        {
            return Level(pawn, trade) >= MasterThreshold;
        }

        public static void GainExperience(Pawn pawn, Metier trade)
        {
            float gain = GainPerWork;
            if (!IsMaster(pawn, trade) && NearbyWorkingMentor(pawn, trade))
            {
                gain *= MentorMultiplier;
            }
            WorldComponent_Artisans.Instance.Gain(pawn, trade, gain);
        }

        public static string Tag(float level)
        {
            if (level >= MasterThreshold) return "AAW_NiveauMaitre".Translate();
            if (level >= 0.35f) return "AAW_NiveauCalleuses".Translate();
            return "AAW_NiveauNovice".Translate();
        }

        // Rendement : 60 % novice → 90 % maître (colon = 100 %).
        public static float Yield(Pawn pawn, Metier trade)
        {
            return 0.6f + 0.3f * Level(pawn, trade);
        }

        // Durée de travail : 1800 ticks novice → 900 maître.
        public static int WorkDuration(Pawn pawn, Metier trade)
        {
            return 1800 - (int)(900f * Level(pawn, trade));
        }

        // Transmission du savoir : un maître du même métier, à l'ouvrage à
        // proximité, double la progression de l'élève. Un maître tailleur
        // n'apprend rien à un apprenti boucher.
        private static bool NearbyWorkingMentor(Pawn apprentice, Metier trade)
        {
            if (!apprentice.Spawned)
            {
                return false;
            }
            var neighbors = apprentice.Map.mapPawns.SpawnedPawnsInFaction(apprentice.Faction);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Pawn neighbor = neighbors[i];
                if (neighbor != apprentice
                    && neighbor.Position.InHorDistOf(apprentice.Position, MentorRadius)
                    && neighbor.def.HasModExtension<ModExtension_MainsHabiles>()
                    && IsMaster(neighbor, trade)
                    && MetierDuJob(neighbor.CurJobDef) == trade)
                {
                    return true;
                }
            }
            return false;
        }

        // Le métier qu'exerce un job donné, ou null pour tout le reste
        // (déplacements, prises d'outil, rapatriements...).
        public static Metier? MetierDuJob(JobDef job)
        {
            if (job == AAW_DefOf.AAW_TaillerPierre
                || job == AAW_DefOf.AAW_TaillerPercuteur
                || job == AAW_DefOf.AAW_TaillerCouteaux)
            {
                return Metier.Taille;
            }
            if (job == AAW_DefOf.AAW_DebiterCarcasse
                || job == AAW_DefOf.AAW_PreparerCroquettes)
            {
                return Metier.Boucherie;
            }
            return null;
        }
    }
}
