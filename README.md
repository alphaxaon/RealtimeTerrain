
# Realtime Terrain Generation (WIP)

Generate random, procedural, seamless terrain tiles of any size in the Unity editor or at runtime.

![Realtime terrain generation](https://thumbs.gfycat.com/ShortFatalGoldenretriever-size_restricted.gif)

## Current Status

This is a "work-in-progress" project. The goal of this project is simple - to be able to generate realistic, beautiful terrain, to have the ability to fine-tune generation parameters to create varying biomes, and to be able to do all that from just a single (or small handful of) imported script(s).

## Installation

Simply import the file `TerrainGenerator.cs` into your Unity project, preferably in the same directory as all your other scripts.

## How To Use

### 1. Create a new Terrain object

Anywhere in your project hierarchy, right click then choose 3D Object > Terrain.

### 2. Add the script as a Component

Just drag and drop the script onto your terrain object.

Set the terrain `Width`, `Height`, and `Length` parameters.

Optionally customize the other values to your liking.

### 3. Click "Generate Terrain"

The script will generate the landscape onto your Terrain object.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details