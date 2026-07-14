using HarmonyLib;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Affiche la maîtrise dans le panneau d'inspection de l'animal (là où le
    // joueur regarde) plutôt que dans l'onglet Santé. Une ligne par métier
    // appris : taille de pierre, boucherie.
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
    public static class Patch_InspectString
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            if (__instance.def.GetModExtension<ModExtension_MainsHabiles>() == null)
            {
                return;
            }
            AjouterLigne(__instance, Metier.Taille, "AAW_MetierTaille", ref __result);
            AjouterLigne(__instance, Metier.Boucherie, "AAW_MetierBoucherie", ref __result);
        }

        private static void AjouterLigne(Pawn pawn, Metier metier, string cle, ref string texte)
        {
            float niveau = MaitriseUtility.Niveau(pawn, metier);
            if (niveau <= 0f)
            {
                return;
            }
            string ligne = "AAW_InspectArtisanat".Translate(
                cle.Translate(), MaitriseUtility.Etiquette(niveau), (niveau * 100f).ToString("F0"));
            texte = texte.NullOrEmpty() ? ligne : texte + "\n" + ligne;
        }
    }
}
