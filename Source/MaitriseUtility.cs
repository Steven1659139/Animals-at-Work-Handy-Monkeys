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
        public const float SeuilMaitre = 0.7f;
        private const float GainParOuvrage = 0.012f;
        private const float RayonMentor = 10f;
        private const float MultiplicateurMentor = 2f;

        public static float Niveau(Pawn pawn, Metier metier)
        {
            return WorldComponent_Artisans.Instance.Niveau(pawn, metier);
        }

        public static bool EstMaitre(Pawn pawn, Metier metier)
        {
            return Niveau(pawn, metier) >= SeuilMaitre;
        }

        public static void GagnerExperience(Pawn pawn, Metier metier)
        {
            float gain = GainParOuvrage;
            if (!EstMaitre(pawn, metier) && MentorAuTravailProche(pawn, metier))
            {
                gain *= MultiplicateurMentor;
            }
            WorldComponent_Artisans.Instance.Gagner(pawn, metier, gain);
        }

        public static string Etiquette(float niveau)
        {
            if (niveau >= SeuilMaitre) return "AAW_NiveauMaitre".Translate();
            if (niveau >= 0.35f) return "AAW_NiveauCalleuses".Translate();
            return "AAW_NiveauNovice".Translate();
        }

        // Rendement : 60 % novice → 90 % maître (colon = 100 %).
        public static float Rendement(Pawn pawn, Metier metier)
        {
            return 0.6f + 0.3f * Niveau(pawn, metier);
        }

        // Durée de travail : 1800 ticks novice → 900 maître.
        public static int DureeTravail(Pawn pawn, Metier metier)
        {
            return 1800 - (int)(900f * Niveau(pawn, metier));
        }

        // Transmission du savoir : un maître du même métier, à l'ouvrage à
        // proximité, double la progression de l'élève. Un maître tailleur
        // n'apprend rien à un apprenti boucher.
        private static bool MentorAuTravailProche(Pawn eleve, Metier metier)
        {
            if (!eleve.Spawned)
            {
                return false;
            }
            var voisins = eleve.Map.mapPawns.SpawnedPawnsInFaction(eleve.Faction);
            for (int i = 0; i < voisins.Count; i++)
            {
                Pawn voisin = voisins[i];
                if (voisin != eleve
                    && voisin.Position.InHorDistOf(eleve.Position, RayonMentor)
                    && voisin.def.HasModExtension<ModExtension_MainsHabiles>()
                    && EstMaitre(voisin, metier)
                    && MetierDuJob(voisin.CurJobDef) == metier)
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
