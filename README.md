# Procedural Island Generation Tool
A Procedural Generation tool for creating quickly generating island meshes which are fully textured, filled with vegetation
and featuring 3 auto generated LOD variants of those same island meshes.

See my portfolio for additional information: https://marknicholsondev.github.io//

## List of Features

| Name  | Description |
| ------------- | ------------- |
| Triplanar Mapping  | Used to correctly texture steeper generated surfaces such as mountain sides |
| Custom water shader |  Created with the help of Catlike Coding tutorials to improve the visual appeal of the project |
| Height-based texturing | Texturing is controlled by Texture Data layer presets which can be created and switched out for different biomes  |
| Basic vegetation system |  Done by reusing the layers system from the texturing, instead using percentage values for spawning done per vertice of a chunk |
| LOD switching system  |  The islands have a chunk based LOD system with auto generated LOD models of each island, done by skipping vertices in mesh generation |
| Auto Updating functionality  |  There is an option to turn on auto-update so you can see the impact of parameters in real time  |
| Perlin Noise generator  | Basis of the entire project which controls how the islands are generated and has a lot of fine tuning options  |

### Notes
The auto update feature can sometimes cause stutters with particularly demanding settings being turned on such as turning on vegetation
or switching the seed of the noise map which causes all islands to re-generate.

## Using the Project:
The UI of the tool is built directly into the inspector window found on the right hand side of the Unity window.

To run the project at default settings click the play button and a world of island chunks will be generated, the seed can be changed to
generate a new set or another set of islands.

## Known Bugs
There can be a issue with the auto-update functionality not correctly updating the texture scriptable object, this is due to the mesh heights not being updated for the texture shader. To solve the issue the UpdateMeshHeights() method can be commented out and then save, then comment back in and the islands should be textured again. This can be found in /Scripts/Data/TextureData.cs at the bottom of the script.
