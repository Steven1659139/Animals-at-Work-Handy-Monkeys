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
        private static Texture2D butcheryIcon;

        private bool knapping = true;
        private bool kibble = true;
        private bool butchery = true;

        public bool Allowed(TacheAtelier task)
        {
            switch (task)
            {
                case TacheAtelier.Taille: return knapping;
                case TacheAtelier.Croquettes: return kibble;
                default: return butchery;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref knapping, "AAW_taille", true);
            Scribe_Values.Look(ref kibble, "AAW_croquettes", true);
            Scribe_Values.Look(ref butchery, "AAW_boucherie", true);
            // Migration des sauvegardes à vocation unique (avant les cases à cocher).
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string calling = null;
                Scribe_Values.Look(ref calling, "AAW_vocation");
                if (calling != null && calling != "Tout")
                {
                    knapping = calling == "Taille";
                    kibble = calling == "Croquettes";
                    butchery = calling == "Boucherie";
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
                isActive = () => knapping,
                toggleAction = delegate { knapping = !knapping; }
            };
            yield return new Command_Toggle
            {
                defaultLabel = "AAW_VocationCroquettes".Translate(),
                defaultDesc = "AAW_ToggleCroquettesDesc".Translate(),
                icon = ThingDefOf.Kibble.uiIcon,
                isActive = () => kibble,
                toggleAction = delegate { kibble = !kibble; }
            };
            yield return new Command_Toggle
            {
                defaultLabel = "AAW_VocationBoucherie".Translate(),
                defaultDesc = "AAW_ToggleBoucherieDesc".Translate(),
                icon = ButcheryIcon(),
                isActive = () => butchery,
                toggleAction = delegate { butchery = !butchery; }
            };
        }

        public override string CompInspectStringExtra()
        {
            return "AAW_TachesLabel".Translate(TaskList());
        }

        private string TaskList()
        {
            StringBuilder list = new StringBuilder();
            if (knapping) Add(list, "AAW_VocationTaille".Translate());
            if (kibble) Add(list, "AAW_VocationCroquettes".Translate());
            if (butchery) Add(list, "AAW_VocationBoucherie".Translate());
            return list.Length > 0 ? list.ToString() : "AAW_TachesAucune".Translate().ToString();
        }

        private static void Add(StringBuilder list, string task)
        {
            if (list.Length > 0)
            {
                list.Append(", ");
            }
            list.Append(task);
        }

        private Texture2D ButcheryIcon()
        {
            if (butcheryIcon == null)
            {
                butcheryIcon = DefDatabase<ThingDef>.GetNamedSilentFail("Meat_Cow")?.uiIcon
                    ?? parent.def.uiIcon;
            }
            return butcheryIcon;
        }
    }
}
