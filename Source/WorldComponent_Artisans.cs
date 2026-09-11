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
        private Dictionary<int, float> tailleurs = new Dictionary<int, float>();
        private Dictionary<int, float> bouchers = new Dictionary<int, float>();
        private List<int> tmpIdsTaille;
        private List<float> tmpXpTaille;
        private List<int> tmpIdsBoucherie;
        private List<float> tmpXpBoucherie;

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

        public float Niveau(Pawn pawn, Metier metier)
        {
            return Registre(metier).TryGetValue(pawn.thingIDNumber, out float xp) ? xp : 0f;
        }

        public void Gagner(Pawn pawn, Metier metier, float gain)
        {
            Registre(metier)[pawn.thingIDNumber] = Mathf.Min(1f, Niveau(pawn, metier) + gain);
        }

        private Dictionary<int, float> Registre(Metier metier)
        {
            return metier == Metier.Boucherie ? bouchers : tailleurs;
        }

        // Purge quotidienne : oublie les artisans qui n'existent plus.
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if ((tailleurs.Count == 0 && bouchers.Count == 0)
                || Find.TickManager.TicksGame % 60000 != 317)
            {
                return;
            }
            HashSet<int> vivants = new HashSet<int>();
            List<Pawn> pawns = PawnsFinder.All_AliveOrDead;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (!pawns[i].Dead)
                {
                    vivants.Add(pawns[i].thingIDNumber);
                }
            }
            tailleurs.RemoveAll(paire => !vivants.Contains(paire.Key));
            bouchers.RemoveAll(paire => !vivants.Contains(paire.Key));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref tailleurs, "AAW_experienceArtisans",
                LookMode.Value, LookMode.Value, ref tmpIdsTaille, ref tmpXpTaille);
            Scribe_Collections.Look(ref bouchers, "AAW_experienceBouchers",
                LookMode.Value, LookMode.Value, ref tmpIdsBoucherie, ref tmpXpBoucherie);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (tailleurs == null)
                {
                    tailleurs = new Dictionary<int, float>();
                }
                if (bouchers == null)
                {
                    bouchers = new Dictionary<int, float>();
                }
            }
        }
    }
}
