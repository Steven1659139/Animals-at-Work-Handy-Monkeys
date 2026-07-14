using HarmonyLib;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    [StaticConstructorOnStartup]
    public static class MonkeysInit
    {
        static MonkeysInit()
        {
            new Harmony("royaltea.animalsatwork.monkeys").PatchAll();
            Log.Message("[Animals at Work — Handy Monkeys] Assembly chargée, patch Harmony actif.");
        }
    }
}
