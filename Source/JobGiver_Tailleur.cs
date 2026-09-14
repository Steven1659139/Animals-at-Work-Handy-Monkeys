using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimalsAtWork.Monkeys
{
    // Répartiteur d'artisanat. Priorités du singe dressé et oisif :
    // 1) travail immédiat : tailler un morceau (percuteur) ou débiter une
    //    carcasse admissible (couteau) près d'un atelier ;
    // 2) s'équiper de l'outil qui manque pour un travail en attente, et
    //    s'il n'en existe aucun, le façonner d'un morceau de pierre ;
    // 3) rapatrier un morceau éloigné vers l'atelier (percuteur en main) ;
    // 4) préparer des croquettes (viande + végétaux à proximité, sans outil).
    public class JobGiver_Tailleur : ThinkNode_JobGiver
    {
        public const float Radius = 12f;
        public const float RecallRadius = 40f;
        public const float MaxGameSize = 0.5f;
        private const int MaxPendingChunks = 3;

        // Throttle par singe des scans coûteux (même motif que le berger de
        // HerdingDogs) : sans lui, chaque singe oisif relançait jusqu'à une
        // demi-douzaine de recherches régionales à chaque décision. Bloqué,
        // le singe vaque et retente à la fenêtre suivante ; les ouvrages
        // durent bien plus longtemps que l'intervalle, la cadence de travail
        // ne change pas. Registre transitoire, minuscule (un entier par
        // singe croisé), volontairement pas sauvegardé.
        private const int ScanInterval = 180;
        private static readonly Dictionary<int, int> lastScans = new Dictionary<int, int>();

        private static bool CanScan(Pawn monkey)
        {
            int tick = Find.TickManager.TicksGame;
            // dernier <= tick garde contre le rechargement d'une partie plus
            // ancienne (l'horloge recule) : dans ce cas, on relance le scan.
            if (lastScans.TryGetValue(monkey.thingIDNumber, out int last)
                && last <= tick && tick - last < ScanInterval)
            {
                return false;
            }
            lastScans[monkey.thingIDNumber] = tick;
            return true;
        }

        protected override Job TryGiveJob(Pawn monkey)
        {
            Map map = monkey.Map;
            if (map == null || monkey.Faction != Faction.OfPlayer)
            {
                return null;
            }
            List<Thing> workshops = map.listerThings.ThingsOfDef(AAW_DefOf.AAW_AtelierDesSinges);
            if (workshops.Count == 0)
            {
                return null;
            }
            if (!CanScan(monkey))
            {
                return null;
            }

            Job work = ImmediateWork(workshops, map, monkey);
            if (work != null)
            {
                return work;
            }
            work = Equipment(workshops, map, monkey);
            if (work != null)
            {
                return work;
            }
            work = Recall(workshops, map, monkey);
            if (work != null)
            {
                return work;
            }
            return Kibble(workshops, map, monkey);
        }

        // --- Travail immédiat : taille (percuteur), boucherie (couteau) ---

        private static Job ImmediateWork(List<Thing> workshops, Map map, Pawn monkey)
        {
            bool hammerstone = OutilUtility.Carries(monkey, AAW_DefOf.AAW_Percuteur);
            bool knife = OutilUtility.Carries(monkey, AAW_DefOf.AAW_Couteau);
            if (!hammerstone && !knife)
            {
                return null;
            }
            for (int i = 0; i < workshops.Count; i++)
            {
                Thing workshop = workshops[i];
                CompAtelier comp = workshop.TryGetComp<CompAtelier>();

                if (hammerstone && Allowed(comp, TacheAtelier.Taille))
                {
                    Thing chunk = NearbyChunk(workshop, map, monkey, Radius);
                    if (chunk != null)
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_TaillerPierre, chunk);
                    }
                }
                if (knife && Allowed(comp, TacheAtelier.Boucherie))
                {
                    Thing carcass = NearbyCarcass(workshop, map, monkey);
                    if (carcass != null)
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_DebiterCarcasse, carcass);
                    }
                }
            }
            return null;
        }

        // --- S'équiper : chercher l'outil manquant, sinon le façonner ------

        private static Job Equipment(List<Thing> workshops, Map map, Pawn monkey)
        {
            bool hasHammerstone = OutilUtility.Carries(monkey, AAW_DefOf.AAW_Percuteur);
            bool hasKnife = OutilUtility.Carries(monkey, AAW_DefOf.AAW_Couteau);

            if (!hasHammerstone)
            {
                Thing chunk = ChunkToKnap(workshops, map, monkey);
                if (chunk != null)
                {
                    Job job = FindTool(map, monkey, AAW_DefOf.AAW_Percuteur, AAW_DefOf.AAW_PrendrePercuteur);
                    if (job != null)
                    {
                        return job;
                    }
                    // Aucun percuteur nulle part : un maître tailleur se taille
                    // le sien, dans le morceau même qui attend d'être taillé.
                    if (MaitriseUtility.IsMaster(monkey, Metier.Taille))
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_TaillerPercuteur, chunk);
                    }
                }
            }

            if (!hasKnife && ButcheryWorkExists(workshops, map, monkey))
            {
                Job job = FindTool(map, monkey, AAW_DefOf.AAW_Couteau, AAW_DefOf.AAW_PrendreCouteau);
                if (job != null)
                {
                    return job;
                }
                // Aucun couteau nulle part : au percuteur, on frappe des éclats.
                if (hasHammerstone)
                {
                    Thing chunk = ChunkForTool(workshops, map, monkey);
                    if (chunk != null)
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_TaillerCouteaux, chunk);
                    }
                }
            }
            return null;
        }

        // --- Rapatriement : ramener un morceau éloigné (percuteur requis) --

        private static Job Recall(List<Thing> workshops, Map map, Pawn monkey)
        {
            if (!OutilUtility.Carries(monkey, AAW_DefOf.AAW_Percuteur))
            {
                return null;
            }
            for (int i = 0; i < workshops.Count; i++)
            {
                Thing workshop = workshops[i];
                if (!Allowed(workshop.TryGetComp<CompAtelier>(), TacheAtelier.Taille))
                {
                    continue;
                }
                if (ChunksNear(workshop, map, monkey) >= MaxPendingChunks)
                {
                    continue;
                }
                Thing far = NearbyChunk(workshop, map, monkey, RecallRadius,
                    t => !NearKnappingWorkshop(t, workshops));
                if (far == null || !TryDropCell(workshop, map, monkey, out IntVec3 drop))
                {
                    continue;
                }
                Job recall = JobMaker.MakeJob(AAW_DefOf.AAW_RapporterMorceau, far, workshop, drop);
                recall.count = 1;
                return recall;
            }
            return null;
        }

        // --- Croquettes : tâche cochée, viande et végétaux ----------------

        private static Job Kibble(List<Thing> workshops, Map map, Pawn monkey)
        {
            for (int i = 0; i < workshops.Count; i++)
            {
                Thing workshop = workshops[i];
                if (!Allowed(workshop.TryGetComp<CompAtelier>(), TacheAtelier.Croquettes))
                {
                    continue;
                }
                Thing meat = NearbyFood(workshop, map, monkey, ThingCategoryDefOf.MeatRaw, null);
                if (meat == null)
                {
                    continue;
                }
                Thing plant = NearbyFood(workshop, map, monkey, ThingCategoryDefOf.PlantFoodRaw, meat);
                if (plant != null)
                {
                    return JobMaker.MakeJob(AAW_DefOf.AAW_PreparerCroquettes, meat, plant, workshop);
                }
            }
            return null;
        }

        // --- Aides ---------------------------------------------------------

        private static bool Allowed(CompAtelier comp, TacheAtelier task)
        {
            return comp == null || comp.Allowed(task);
        }

        // Un morceau qui attend d'être taillé près d'un atelier qui autorise
        // la taille, ou null s'il n'y a pas de travail de ce côté.
        private static Thing ChunkToKnap(List<Thing> workshops, Map map, Pawn monkey)
        {
            for (int i = 0; i < workshops.Count; i++)
            {
                if (!Allowed(workshops[i].TryGetComp<CompAtelier>(), TacheAtelier.Taille))
                {
                    continue;
                }
                Thing chunk = NearbyChunk(workshops[i], map, monkey, RecallRadius);
                if (chunk != null)
                {
                    return chunk;
                }
            }
            return null;
        }

        private static bool ButcheryWorkExists(List<Thing> workshops, Map map, Pawn monkey)
        {
            for (int i = 0; i < workshops.Count; i++)
            {
                if (Allowed(workshops[i].TryGetComp<CompAtelier>(), TacheAtelier.Boucherie)
                    && NearbyCarcass(workshops[i], map, monkey) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static Job FindTool(Map map, Pawn monkey, ThingDef tool, JobDef taken)
        {
            Thing found = GenClosest.ClosestThingReachable(
                monkey.Position, map, ThingRequest.ForDef(tool),
                PathEndMode.Touch, TraverseParms.For(monkey), 9999f,
                t => !t.IsForbidden(monkey) && monkey.CanReserve(t));
            return found != null ? JobMaker.MakeJob(taken, found) : null;
        }

        // Un morceau pour façonner des outils : près de n'importe quel
        // atelier, quelles que soient ses tâches. L'outillage est un
        // méta-travail, pas de la taille de blocs.
        private static Thing ChunkForTool(List<Thing> workshops, Map map, Pawn monkey)
        {
            for (int i = 0; i < workshops.Count; i++)
            {
                Thing chunk = NearbyChunk(workshops[i], map, monkey, RecallRadius);
                if (chunk != null)
                {
                    return chunk;
                }
            }
            return null;
        }

        private static Thing NearbyChunk(Thing workshop, Map map, Pawn monkey, float radius,
            System.Predicate<Thing> filter = null)
        {
            return GenClosest.ClosestThingReachable(
                workshop.Position, map, ThingRequest.ForGroup(ThingRequestGroup.Chunk),
                PathEndMode.Touch, TraverseParms.For(monkey), radius,
                t => !t.IsForbidden(monkey)
                     && t.def.butcherProducts != null && t.def.butcherProducts.Count > 0
                     && monkey.CanReserve(t)
                     && (filter == null || filter(t)));
        }

        private static Thing NearbyCarcass(Thing workshop, Map map, Pawn monkey)
        {
            // La maîtrise ne change pas d'une carcasse à l'autre : lue une fois,
            // pas dans le prédicat.
            bool master = MaitriseUtility.IsMaster(monkey, Metier.Boucherie);
            return GenClosest.ClosestThingReachable(
                workshop.Position, map, ThingRequest.ForGroup(ThingRequestGroup.Corpse),
                PathEndMode.Touch, TraverseParms.For(monkey), Radius,
                t => !t.IsForbidden(monkey)
                     && monkey.CanReserve(t)
                     && IsEligibleGame(t, master));
        }

        // Gibier admissible : animal sauvage frais, jamais les bêtes de la
        // colonie. Un novice s'en tient au petit gibier (pas plus gros qu'un
        // chat) ; un maître boucher débite au sol des bêtes de toute taille.
        private static bool IsEligibleGame(Thing t, bool master)
        {
            return t is Corpse carcass
                && carcass.InnerPawn.RaceProps.Animal
                && carcass.InnerPawn.Faction != Faction.OfPlayer
                && carcass.GetRotStage() == RotStage.Fresh
                && (master || carcass.InnerPawn.RaceProps.baseBodySize <= MaxGameSize);
        }

        private static Thing NearbyFood(Thing workshop, Map map, Pawn monkey,
            ThingCategoryDef category, Thing excluded)
        {
            return GenClosest.ClosestThingReachable(
                workshop.Position, map,
                ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree),
                PathEndMode.Touch, TraverseParms.For(monkey), Radius,
                t => !t.IsForbidden(monkey)
                     && t != excluded
                     && IsCategory(t, category)
                     && t.stackCount >= JobDriver_PreparerCroquettes.IngredientsRequis
                     && monkey.CanReserve(t));
        }

        private static bool IsCategory(Thing t, ThingCategoryDef category)
        {
            return t.def.thingCategories != null && t.def.thingCategories.Contains(category);
        }

        private static bool NearKnappingWorkshop(Thing chunk, List<Thing> workshops)
        {
            for (int i = 0; i < workshops.Count; i++)
            {
                if (Allowed(workshops[i].TryGetComp<CompAtelier>(), TacheAtelier.Taille)
                    && chunk.Position.InHorDistOf(workshops[i].Position, Radius))
                {
                    return true;
                }
            }
            return false;
        }

        private static int ChunksNear(Thing workshop, Map map, Pawn monkey)
        {
            int n = 0;
            List<Thing> chunks = map.listerThings.ThingsInGroup(ThingRequestGroup.Chunk);
            for (int i = 0; i < chunks.Count; i++)
            {
                Thing t = chunks[i];
                if (!t.IsForbidden(monkey)
                    && t.def.butcherProducts != null && t.def.butcherProducts.Count > 0
                    && t.Position.InHorDistOf(workshop.Position, Radius))
                {
                    n++;
                }
            }
            return n;
        }

        private static bool TryDropCell(Thing workshop, Map map, Pawn monkey, out IntVec3 drop)
        {
            return CellFinder.TryFindRandomCellNear(workshop.Position, map, 4,
                c => c.Standable(map)
                     && !c.IsForbidden(monkey)
                     && c.GetFirstItem(map) == null
                     && monkey.CanReserve(c)
                     && monkey.CanReach(c, PathEndMode.OnCell, Danger.Deadly),
                out drop);
        }
    }
}
