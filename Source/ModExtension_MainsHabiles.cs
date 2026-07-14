using Verse;

namespace AnimalsAtWork.Monkeys
{
    // Étiquette d'espèce : seules les races portant cette extension peuvent
    // apprendre la taille de pierre. On l'ajoute au singe via un patch XML ;
    // tout autre mod peut rendre son espèce éligible avec :
    //   <Operation Class="PatchOperationAddModExtension">
    //     <xpath>Defs/ThingDef[defName="SonAnimal"]</xpath>
    //     <value><li Class="AnimalsAtWork.Monkeys.ModExtension_MainsHabiles"/></value>
    //   </Operation>
    public class ModExtension_MainsHabiles : DefModExtension
    {
    }
}
