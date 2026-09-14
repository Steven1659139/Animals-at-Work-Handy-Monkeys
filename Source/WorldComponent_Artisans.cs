using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Registre de la maîtrise des artisans (0 → 1), stocké au niveau du monde :
    // il suit chaque animal partout (caravanes comprises) et se sauvegarde,
    // sans polluer l'onglet Santé. Une progression par métier : la taille de
    // pierre et la boucherie s'apprennent chacune de leur côté.
    public class WorldComponent_Artisans : WorldComponent
    {
        // L'ancienne clé de sauvegarde (unique maîtrise) devient la taille :
        // les vétérans des vieilles parties restent des tailleurs accomplis.
        private Dictionary<int, float> knappers = new Dictionary<int, float>();
        private Dictionary<int, float> butchers = new Dictionary<int, float>();
        private List<int> tmpKnappingIds;
        private List<float> tmpKnappingXp;
        private List<int> tmpButcheryIds;
        private List<float> tmpButcheryXp;

        // Le registre est consulté depuis les prédicats de recherche du JobGiver
        // et depuis le panneau d'inspection, à chaque image : on garde
        // l'instance sous la main au lieu de parcourir les composants du monde
        // à chaque appel. Le constructeur la remplace à chaque nouveau monde.
        private static WorldComponent_Artisans instance;

        public WorldComponent_Artisans(World world) : base(world)
        {
            instance = this;
        }

        public static WorldComponent_Artisans Instance
        {
            get
            {
                if (instance == null || instance.world != Find.World)
                {
                    instance = Find.World.GetComponent<WorldComponent_Artisans>();
                }
                return instance;
            }
        }

        public float Level(Pawn pawn, Metier trade)
        {
            return Registre(trade).TryGetValue(pawn.thingIDNumber, out float xp) ? xp : 0f;
        }

        public void Gain(Pawn pawn, Metier trade, float gain)
        {
            Registre(trade)[pawn.thingIDNumber] = Mathf.Min(1f, Level(pawn, trade) + gain);
        }

        private Dictionary<int, float> Registre(Metier trade)
        {
            return trade == Metier.Boucherie ? butchers : knappers;
        }

        // Purge quotidienne : oublie les artisans qui n'existent plus.
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if ((knappers.Count == 0 && butchers.Count == 0)
                || Find.TickManager.TicksGame % 60000 != 317)
            {
                return;
            }
            HashSet<int> alive = new HashSet<int>();
            List<Pawn> pawns = PawnsFinder.All_AliveOrDead;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (!pawns[i].Dead)
                {
                    alive.Add(pawns[i].thingIDNumber);
                }
            }
            knappers.RemoveAll(paire => !alive.Contains(paire.Key));
            butchers.RemoveAll(paire => !alive.Contains(paire.Key));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref knappers, "AAW_experienceArtisans",
                LookMode.Value, LookMode.Value, ref tmpKnappingIds, ref tmpKnappingXp);
            Scribe_Collections.Look(ref butchers, "AAW_experienceBouchers",
                LookMode.Value, LookMode.Value, ref tmpButcheryIds, ref tmpButcheryXp);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (knappers == null)
                {
                    knappers = new Dictionary<int, float>();
                }
                if (butchers == null)
                {
                    butchers = new Dictionary<int, float>();
                }
            }
        }
    }
}
