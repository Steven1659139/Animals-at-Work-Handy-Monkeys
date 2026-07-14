using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimalsAtWork.Monkeys
{
    public enum TacheAtelier
    {
        Taille,
        Croquettes,
        Boucherie
    }

    public class CompProperties_Atelier : CompProperties
    {
        public CompProperties_Atelier()
        {
            compClass = typeof(CompAtelier);
        }
    }

    // Les tâches autorisées à l'atelier, cochées bâtiment par bâtiment
    // (gizmos à bascule) et lues par le JobGiver. Tout est permis par défaut.
    public class CompAtelier : ThingComp
    {
        private static Texture2D iconeBoucherie;

        private bool taille = true;
        private bool croquettes = true;
        private bool boucherie = true;

        public bool Autorise(TacheAtelier tache)
        {
            switch (tache)
            {
                case TacheAtelier.Taille: return taille;
                case TacheAtelier.Croquettes: return croquettes;
                default: return boucherie;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref taille, "AAW_taille", true);
            Scribe_Values.Look(ref croquettes, "AAW_croquettes", true);
            Scribe_Values.Look(ref boucherie, "AAW_boucherie", true);
            // Migration des sauvegardes à vocation unique (avant les cases à cocher).
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string vocation = null;
                Scribe_Values.Look(ref vocation, "AAW_vocation");
                if (vocation != null && vocation != "Tout")
                {
                    taille = vocation == "Taille";
                    croquettes = vocation == "Croquettes";
                    boucherie = vocation == "Boucherie";
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }
            yield return new Command_Toggle
            {
                defaultLabel = "AAW_VocationTaille".Translate(),
                defaultDesc = "AAW_ToggleTailleDesc".Translate(),
                icon = AAW_DefOf.AAW_Percuteur.uiIcon,
                isActive = () => taille,
                toggleAction = delegate { taille = !taille; }
            };
            yield return new Command_Toggle
            {
                defaultLabel = "AAW_VocationCroquettes".Translate(),
                defaultDesc = "AAW_ToggleCroquettesDesc".Translate(),
                icon = ThingDefOf.Kibble.uiIcon,
                isActive = () => croquettes,
                toggleAction = delegate { croquettes = !croquettes; }
            };
            yield return new Command_Toggle
            {
                defaultLabel = "AAW_VocationBoucherie".Translate(),
                defaultDesc = "AAW_ToggleBoucherieDesc".Translate(),
                icon = IconeBoucherie(),
                isActive = () => boucherie,
                toggleAction = delegate { boucherie = !boucherie; }
            };
        }

        public override string CompInspectStringExtra()
        {
            return "AAW_TachesLabel".Translate(ListeTaches());
        }

        private string ListeTaches()
        {
            StringBuilder liste = new StringBuilder();
            if (taille) Ajouter(liste, "AAW_VocationTaille".Translate());
            if (croquettes) Ajouter(liste, "AAW_VocationCroquettes".Translate());
            if (boucherie) Ajouter(liste, "AAW_VocationBoucherie".Translate());
            return liste.Length > 0 ? liste.ToString() : "AAW_TachesAucune".Translate().ToString();
        }

        private static void Ajouter(StringBuilder liste, string tache)
        {
            if (liste.Length > 0)
            {
                liste.Append(", ");
            }
            liste.Append(tache);
        }

        private Texture2D IconeBoucherie()
        {
            if (iconeBoucherie == null)
            {
                iconeBoucherie = DefDatabase<ThingDef>.GetNamedSilentFail("Meat_Cow")?.uiIcon
                    ?? parent.def.uiIcon;
            }
            return iconeBoucherie;
        }
    }
}
