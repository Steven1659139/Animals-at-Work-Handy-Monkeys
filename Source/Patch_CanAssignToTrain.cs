using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Restreint la taille de pierre aux espèces « mains habiles ». Le filtre
    // vanilla par espèce (specialTrainables) est verrouillé derrière Odyssey
    // (ModsConfig.OdysseyActive dans CanAssignToTrain) : on greffe donc le
    // nôtre en aval de la vérification standard.
    [HarmonyPatch(typeof(Pawn_TrainingTracker), nameof(Pawn_TrainingTracker.CanAssignToTrain),
        new[] { typeof(TrainableDef), typeof(ThingDef), typeof(bool), typeof(Pawn) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
    public static class Patch_CanAssignToTrain
    {
        public static void Postfix(TrainableDef td, ThingDef pawnDef, ref bool visible, ref AcceptanceReport __result)
        {
            if (td != AAW_DefOf.AAW_Artisanat || !__result.Accepted)
            {
                return;
            }
            if (pawnDef.GetModExtension<ModExtension_MainsHabiles>() == null)
            {
                visible = false;
                __result = false;
            }
        }
    }
}
