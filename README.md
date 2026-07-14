# Animals at Work — Handy Monkeys

*Every beast earns its keep.*

Third module of the **Animals at Work** series for RimWorld 1.6: animals doing useful, autonomous work.

## Features

- **"Crafting" training**, reserved for nimble-handed species (the monkey in the base game).
- **Monkey workshop.** A low workbench that stores up to three hammerstones, with per-task toggles (stonecutting / kibble / butchery).
- **Stonecutting.** Each monkey equips its own hammerstone (visible in its Gear tab), cuts nearby stone chunks into blocks, and eventually shatters its tool at work.
- **Chunk hauling.** When the bench runs dry, monkeys drag distant chunks (up to 40 cells) back to the workshop.
- **Butchery.** With a stone knife, monkeys butcher small wild carcasses near the workshop: meat and leather, never your own beasts. A master butcher field-dresses carcasses of any size, though bigger beasts just take longer (and bleed more).
- **Kibble.** With meat and vegetables nearby, monkeys feed your other working beasts.
- **Mastery.** Every job makes a monkey better, from novice (slow, 60% yield) to master craftsman (fast, 90%). Stonecutting and butchery are learned separately; kibble-making counts toward butchery. Progress shows in the inspect pane, one line per craft.
- **Passing it on.** A novice working beside a master of the same craft learns twice as fast. When tools run out, a master knaps its own hammerstone, and any monkey with a hammerstone strikes fresh stone knives from a chunk.

**Requires [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077).** Standalone module. English + French included.

## For animal mod authors

Make your species eligible for crafting with a two-line patch:

```xml
<Operation Class="PatchOperationAddModExtension">
  <xpath>Defs/ThingDef[defName="YourAnimal"]</xpath>
  <value><li Class="AnimalsAtWork.Monkeys.ModExtension_MainsHabiles" /></value>
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
