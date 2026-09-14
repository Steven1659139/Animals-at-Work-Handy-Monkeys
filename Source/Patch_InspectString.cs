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
            AddLine(__instance, Metier.Taille, "AAW_MetierTaille", ref __result);
            AddLine(__instance, Metier.Boucherie, "AAW_MetierBoucherie", ref __result);
        }

        private static void AddLine(Pawn pawn, Metier trade, string key, ref string text)
        {
            float level = MaitriseUtility.Level(pawn, trade);
            if (level <= 0f)
            {
                return;
            }
            string line = "AAW_InspectArtisanat".Translate(
                key.Translate(), MaitriseUtility.Tag(level), (level * 100f).ToString("F0"));
            text = text.NullOrEmpty() ? line : text + "\n" + line;
        }
    }
}
