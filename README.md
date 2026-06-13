# Witcher Base

The foundation mod for a modular Witcher content suite for **RimWorld 1.6 (Biotech)**. It adds the Witcher trials as research-gated, Health-tab operations that transform colonists into witchers by granting Biotech genes — at great risk.

> Requires the **Biotech** DLC.

## Features

Research three trials, each unlocking a Health-tab operation that consumes herbal ingredients and induces a painful fever lasting one to two days. The outcome is sealed the moment the trial begins — no tending changes the result, and those who fail do not survive.

| Trial | Survival | Eligibility | Reward |
|-------|----------|-------------|--------|
| **Trial of Grasses** | ~30% for boys aged 8–12, far lower for everyone else | Any humanlike pawn | Disease resistance, robustness, fast healing, dark vision, sterility, faster movement, ugly features |
| **Trial of Dreams** | ~70% | Witcher initiates (Grasses survivors) only | Reduced sleep, pain resistance, dead calm, fast learning, dulled senses, low social impact, slow aging |
| **Additional Mutagens** | ~10% | Witcher initiates only | Pushes existing witcher genes to their extreme tier |

The **Trial of Mountains** research is included as a placeholder for a future quest-based final trial.

## Repository layout

```
About/                 Mod metadata (About.xml)
Assemblies/            Compiled Witcher.dll
Defs/                  XML definitions
  GeneDefs/            Custom genes
  HediffDefs/          Trial fevers + initiate marker
  RecipeDefs/          Trial operations
  ResearchProjectDefs/ Trial research
Source/                C# source (namespace WitcherBase)
  Witcher/             Recipe workers, hediff comp, gene rewards
  Witcher.csproj       Project file
  build.sh             Offline build script (invokes Roslyn csc directly)
LoadFolders.xml        Version loading rules
```

## Building

The C# assembly builds offline without NuGet by invoking the game's own Mono DLLs via Roslyn:

```bash
./Source/build.sh
```

This outputs `Assemblies/Witcher.dll`. The script auto-detects the RimWorld install; override paths with the `RIMWORLD_MANAGED`, `DOTNET`, and `CSC_DLL` environment variables if needed.

## Development

The mod is symlinked into the RimWorld `Mods/` folder during development, so XML and DLL changes are picked up on the next game restart without copying files.

## License

Personal project. The Witcher universe and its lore are the property of CD Projekt Red and Andrzej Sapkowski.
