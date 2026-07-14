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
        public const float Rayon = 12f;
        public const float RayonRapatriement = 40f;
        public const float TailleGibierMax = 0.5f;
        private const int IngredientsRequis = 20;
        private const int MorceauxEnAttenteMax = 3;

        // Throttle par singe des scans coûteux (même motif que le berger de
        // HerdingDogs) : sans lui, chaque singe oisif relançait jusqu'à une
        // demi-douzaine de recherches régionales à chaque décision. Bloqué,
        // le singe vaque et retente à la fenêtre suivante ; les ouvrages
        // durent bien plus longtemps que l'intervalle, la cadence de travail
        // ne change pas. Registre transitoire, minuscule (un entier par
        // singe croisé), volontairement pas sauvegardé.
        private const int IntervalleScan = 180;
        private static readonly Dictionary<int, int> derniersScans = new Dictionary<int, int>();

        private static bool PeutScanner(Pawn singe)
        {
            int tick = Find.TickManager.TicksGame;
            // dernier <= tick garde contre le rechargement d'une partie plus
            // ancienne (l'horloge recule) : dans ce cas, on relance le scan.
            if (derniersScans.TryGetValue(singe.thingIDNumber, out int dernier)
                && dernier <= tick && tick - dernier < IntervalleScan)
            {
                return false;
            }
            derniersScans[singe.thingIDNumber] = tick;
            return true;
        }

        protected override Job TryGiveJob(Pawn singe)
        {
            Map map = singe.Map;
            if (map == null || singe.Faction != Faction.OfPlayer)
            {
                return null;
            }
            List<Thing> ateliers = map.listerThings.ThingsOfDef(AAW_DefOf.AAW_AtelierDesSinges);
            if (ateliers.Count == 0)
            {
                return null;
            }
            if (!PeutScanner(singe))
            {
                return null;
            }

            Job travail = TravailImmediat(ateliers, map, singe);
            if (travail != null)
            {
                return travail;
            }
            travail = Equipement(ateliers, map, singe);
            if (travail != null)
            {
                return travail;
            }
            travail = Rapatriement(ateliers, map, singe);
            if (travail != null)
            {
                return travail;
            }
            return Croquettes(ateliers, map, singe);
        }

        // --- Travail immédiat : taille (percuteur), boucherie (couteau) ---

        private static Job TravailImmediat(List<Thing> ateliers, Map map, Pawn singe)
        {
            bool percuteur = OutilUtility.OutilEquipe(singe, AAW_DefOf.AAW_Percuteur) != null;
            bool couteau = OutilUtility.OutilEquipe(singe, AAW_DefOf.AAW_Couteau) != null;
            if (!percuteur && !couteau)
            {
                return null;
            }
            for (int i = 0; i < ateliers.Count; i++)
            {
                Thing atelier = ateliers[i];
                CompAtelier comp = atelier.TryGetComp<CompAtelier>();

                if (percuteur && Autorise(comp, TacheAtelier.Taille))
                {
                    Thing morceau = MorceauProche(atelier, map, singe, Rayon);
                    if (morceau != null)
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_TaillerPierre, morceau, atelier);
                    }
                }
                if (couteau && Autorise(comp, TacheAtelier.Boucherie))
                {
                    Thing carcasse = CarcasseProche(atelier, map, singe);
                    if (carcasse != null)
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_DebiterCarcasse, carcasse, atelier);
                    }
                }
            }
            return null;
        }

        // --- S'équiper : chercher l'outil manquant, sinon le façonner ------

        private static Job Equipement(List<Thing> ateliers, Map map, Pawn singe)
        {
            bool aPercuteur = OutilUtility.OutilEquipe(singe, AAW_DefOf.AAW_Percuteur) != null;
            bool aCouteau = OutilUtility.OutilEquipe(singe, AAW_DefOf.AAW_Couteau) != null;

            if (!aPercuteur && TravailTailleExiste(ateliers, map, singe))
            {
                Job job = ChercherOutil(map, singe, AAW_DefOf.AAW_Percuteur, AAW_DefOf.AAW_PrendrePercuteur);
                if (job != null)
                {
                    return job;
                }
                // Aucun percuteur nulle part : un maître tailleur se taille le sien.
                if (MaitriseUtility.EstMaitre(singe, Metier.Taille))
                {
                    Thing morceau = MorceauPourOutil(ateliers, map, singe);
                    if (morceau != null)
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_TaillerPercuteur, morceau);
                    }
                }
            }

            if (!aCouteau && TravailBoucherieExiste(ateliers, map, singe))
            {
                Job job = ChercherOutil(map, singe, AAW_DefOf.AAW_Couteau, AAW_DefOf.AAW_PrendreCouteau);
                if (job != null)
                {
                    return job;
                }
                // Aucun couteau nulle part : au percuteur, on frappe des éclats.
                if (aPercuteur)
                {
                    Thing morceau = MorceauPourOutil(ateliers, map, singe);
                    if (morceau != null)
                    {
                        return JobMaker.MakeJob(AAW_DefOf.AAW_TaillerCouteaux, morceau);
                    }
                }
            }
            return null;
        }

        // --- Rapatriement : ramener un morceau éloigné (percuteur requis) --

        private static Job Rapatriement(List<Thing> ateliers, Map map, Pawn singe)
        {
            if (OutilUtility.OutilEquipe(singe, AAW_DefOf.AAW_Percuteur) == null)
            {
                return null;
            }
            for (int i = 0; i < ateliers.Count; i++)
            {
                Thing atelier = ateliers[i];
                if (!Autorise(atelier.TryGetComp<CompAtelier>(), TacheAtelier.Taille))
                {
                    continue;
                }
                if (MorceauxPres(atelier, map, singe) >= MorceauxEnAttenteMax)
                {
                    continue;
                }
                Thing lointain = MorceauProche(atelier, map, singe, RayonRapatriement,
                    t => !PresDunAtelierDeTaille(t, ateliers));
                if (lointain == null || !TryCaseDepot(atelier, map, singe, out IntVec3 depot))
                {
                    continue;
                }
                Job rapatriement = JobMaker.MakeJob(AAW_DefOf.AAW_RapporterMorceau, lointain, atelier, depot);
                rapatriement.count = 1;
                return rapatriement;
            }
            return null;
        }

        // --- Croquettes : tâche cochée, viande et végétaux ----------------

        private static Job Croquettes(List<Thing> ateliers, Map map, Pawn singe)
        {
            for (int i = 0; i < ateliers.Count; i++)
            {
                Thing atelier = ateliers[i];
                if (!Autorise(atelier.TryGetComp<CompAtelier>(), TacheAtelier.Croquettes))
                {
                    continue;
                }
                Thing viande = NourritureProche(atelier, map, singe, ThingCategoryDefOf.MeatRaw, null);
                if (viande == null)
                {
                    continue;
                }
                Thing vegetal = NourritureProche(atelier, map, singe, ThingCategoryDefOf.PlantFoodRaw, viande);
                if (vegetal != null)
                {
                    return JobMaker.MakeJob(AAW_DefOf.AAW_PreparerCroquettes, viande, vegetal, atelier);
                }
            }
            return null;
        }

        // --- Aides ---------------------------------------------------------

        private static bool Autorise(CompAtelier comp, TacheAtelier tache)
        {
            return comp == null || comp.Autorise(tache);
        }

        private static bool TravailTailleExiste(List<Thing> ateliers, Map map, Pawn singe)
        {
            for (int i = 0; i < ateliers.Count; i++)
            {
                if (Autorise(ateliers[i].TryGetComp<CompAtelier>(), TacheAtelier.Taille)
                    && MorceauProche(ateliers[i], map, singe, RayonRapatriement) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TravailBoucherieExiste(List<Thing> ateliers, Map map, Pawn singe)
        {
            for (int i = 0; i < ateliers.Count; i++)
            {
                if (Autorise(ateliers[i].TryGetComp<CompAtelier>(), TacheAtelier.Boucherie)
                    && CarcasseProche(ateliers[i], map, singe) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static Job ChercherOutil(Map map, Pawn singe, ThingDef outil, JobDef prise)
        {
            Thing trouve = GenClosest.ClosestThingReachable(
                singe.Position, map, ThingRequest.ForDef(outil),
                PathEndMode.Touch, TraverseParms.For(singe), 9999f,
                t => !t.IsForbidden(singe) && singe.CanReserve(t));
            return trouve != null ? JobMaker.MakeJob(prise, trouve) : null;
        }

        // Un morceau pour façonner des outils : près de n'importe quel
        // atelier, quelles que soient ses tâches. L'outillage est un
        // méta-travail, pas de la taille de blocs.
        private static Thing MorceauPourOutil(List<Thing> ateliers, Map map, Pawn singe)
        {
            for (int i = 0; i < ateliers.Count; i++)
            {
                Thing morceau = MorceauProche(ateliers[i], map, singe, RayonRapatriement);
                if (morceau != null)
                {
                    return morceau;
                }
            }
            return null;
        }

        private static Thing MorceauProche(Thing atelier, Map map, Pawn singe, float rayon,
            System.Predicate<Thing> filtre = null)
        {
            return GenClosest.ClosestThingReachable(
                atelier.Position, map, ThingRequest.ForGroup(ThingRequestGroup.Chunk),
                PathEndMode.Touch, TraverseParms.For(singe), rayon,
                t => !t.IsForbidden(singe)
                     && t.Position.InHorDistOf(atelier.Position, rayon)
                     && t.def.butcherProducts != null && t.def.butcherProducts.Count > 0
                     && singe.CanReserve(t)
                     && (filtre == null || filtre(t)));
        }

        private static Thing CarcasseProche(Thing atelier, Map map, Pawn singe)
        {
            return GenClosest.ClosestThingReachable(
                atelier.Position, map, ThingRequest.ForGroup(ThingRequestGroup.Corpse),
                PathEndMode.Touch, TraverseParms.For(singe), Rayon,
                t => !t.IsForbidden(singe)
                     && t.Position.InHorDistOf(atelier.Position, Rayon)
                     && singe.CanReserve(t)
                     && EstGibierAdmissible(t, singe));
        }

        // Gibier admissible : animal sauvage frais, jamais les bêtes de la
        // colonie. Un novice s'en tient au petit gibier (pas plus gros qu'un
        // chat) ; un maître boucher débite au sol des bêtes de toute taille.
        public static bool EstGibierAdmissible(Thing t, Pawn singe)
        {
            return t is Corpse carcasse
                && carcasse.InnerPawn.RaceProps.Animal
                && carcasse.InnerPawn.Faction != Faction.OfPlayer
                && carcasse.GetRotStage() == RotStage.Fresh
                && (carcasse.InnerPawn.RaceProps.baseBodySize <= TailleGibierMax
                    || MaitriseUtility.EstMaitre(singe, Metier.Boucherie));
        }

        private static Thing NourritureProche(Thing atelier, Map map, Pawn singe,
            ThingCategoryDef categorie, Thing exclu)
        {
            return GenClosest.ClosestThingReachable(
                atelier.Position, map,
                ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree),
                PathEndMode.Touch, TraverseParms.For(singe), Rayon,
                t => !t.IsForbidden(singe)
                     && t.Position.InHorDistOf(atelier.Position, Rayon)
                     && t != exclu
                     && EstCategorie(t, categorie)
                     && t.stackCount >= IngredientsRequis
                     && singe.CanReserve(t));
        }

        private static bool EstCategorie(Thing t, ThingCategoryDef categorie)
        {
            return t.def.thingCategories != null && t.def.thingCategories.Contains(categorie);
        }

        private static bool PresDunAtelierDeTaille(Thing morceau, List<Thing> ateliers)
        {
            for (int i = 0; i < ateliers.Count; i++)
            {
                if (Autorise(ateliers[i].TryGetComp<CompAtelier>(), TacheAtelier.Taille)
                    && morceau.Position.InHorDistOf(ateliers[i].Position, Rayon))
                {
                    return true;
                }
            }
            return false;
        }

        private static int MorceauxPres(Thing atelier, Map map, Pawn singe)
        {
            int n = 0;
            List<Thing> morceaux = map.listerThings.ThingsInGroup(ThingRequestGroup.Chunk);
            for (int i = 0; i < morceaux.Count; i++)
            {
                Thing t = morceaux[i];
                if (!t.IsForbidden(singe)
                    && t.def.butcherProducts != null && t.def.butcherProducts.Count > 0
                    && t.Position.InHorDistOf(atelier.Position, Rayon))
                {
                    n++;
                }
            }
            return n;
        }

        private static bool TryCaseDepot(Thing atelier, Map map, Pawn singe, out IntVec3 depot)
        {
            return CellFinder.TryFindRandomCellNear(atelier.Position, map, 4,
                c => c.Standable(map)
                     && !c.IsForbidden(singe)
                     && c.GetFirstItem(map) == null
                     && singe.CanReserve(c)
                     && singe.CanReach(c, PathEndMode.OnCell, Danger.Deadly),
                out depot);
        }
    }
}
