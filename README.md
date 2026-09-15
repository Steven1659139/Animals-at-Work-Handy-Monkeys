# Animals at Work — Handy Monkeys

Third module of the **Animals at Work** series for RimWorld 1.6: animals doing useful work.

## Features

- **"Crafting" training**, reserved for species with nimble hands, which is the monkey in the base game.
- **Monkey workshop.** A low workbench that stores up to three hammerstones, with one toggle per task (stonecutting, kibble, butchery).
- **Stonecutting.** Each monkey equips its own hammerstone, visible in its Gear tab, and cuts nearby stone chunks into blocks. The tool eventually breaks at work.
- **Chunk hauling.** When the bench runs out of chunks, monkeys drag distant ones back to the workshop from up to 40 cells away.
- **Butchery.** With a stone knife, monkeys butcher small wild carcasses near the workshop, producing meat and leather and never touching your own animals. A master butcher can field-dress a carcass of any size, though bigger animals take longer and bleed more.
- **Kibble.** With meat and vegetables within reach, monkeys make kibble to feed your other working animals.
- **Mastery.** Every job improves a monkey, from novice (slow, 60% yield) to master craftsman (fast, 90%). Stonecutting and butchery are learned separately, and kibble-making counts toward butchery. Progress is shown in the inspect pane, one line per craft.
- **Passing it on.** A novice working beside a master of the same craft learns twice as fast. When tools run out, a master knaps its own hammerstone, and any monkey holding a hammerstone can strike fresh stone knives from a chunk.

**Requires [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077).** Standalone module. English + French included.

## For animal mod authors

Already covered, when their mod is present: the gorilla (Odyssey), the gorillo (Kenshi Fauna), the sakarn (Vaelkorr Creatures), the red panda (Fluffy Fauna), the yeti (Nordberg) and the writhing puppet (Writhing Tree).

To add your own species, drop this in a file under your mod's `Patches/` folder. `MayRequire` makes the operation vanish when Handy Monkeys is not installed, so it is safe to ship unconditionally; RimWorld only reads it on a list item, hence the sequence:

```xml
<Operation Class="PatchOperationSequence">
  <operations>
    <li Class="PatchOperationAddModExtension" MayRequire="royaltea.animalsatwork.monkeys">
      <xpath>Defs/ThingDef[defName="YourAnimal"]</xpath>
      <value><li Class="AnimalsAtWork.Monkeys.ModExtension_MainsHabiles" /></value>
    </li>
  </operations>
</Operation>
```

## The series

| Module | Repo |
|---|---|
| Plowing | [Animals-at-Work-Plowing](https://github.com/Steven1659139/Animals-at-Work-Plowing) |
| Herding Dogs | [Animals-at-Work-Herding-Dogs](https://github.com/Steven1659139/Animals-at-Work-Herding-Dogs) |
| Handy Monkeys | *(this repo)* |

## Manual install

Clone or download this repository into your RimWorld `Mods` folder, then enable **Animals at Work — Handy Monkeys** in the in-game mod list (after Harmony).

## Building from source

```
cd Source && dotnet build
```

Targets `net472` against [Krafs.Rimworld.Ref](https://www.nuget.org/packages/Krafs.Rimworld.Ref); the DLL lands in `Assemblies/`.

---

By **Royal-Tea**.
