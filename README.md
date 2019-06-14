# PCGactionAdventure

Core Features:
- Cellular automata 2D room generation
- Marching squares 3D room generation
- Joris Dorman's cycles implementation
- Points based distribution of enemies
- Souls like combat
- Boss battles
- Lock on targeting
- Multidirectional animation
- Weapon-based movesets/animations with animation events

## What this project is about:
Procedual Content Generation – generating something algorithmically – has seen little use in the Action-Adventure genre but new techniques such as Joris Dorman’s cyclic dungeon generation offer an interesting new chance to see an effective PCG Action-Adventure game. This project aims to use Dorman’s cycles alongside a points-based scaling distribution and cellular automata to create an Action-Adventure game that uses PCG effectively. Using Unity with C# this project can create a 2D implementation of a graph node generator - to show maps of the level and how the maps are being generated and a system to convert that graph to a 3D level. The game includes boss battles, souls like combat and more. A study using play testers found that there was some suggestive evidence to show patterns like Dorman’s and cellular automata can be effective in Action-Adventure games but scaling distribution requires further study to find anything tangible as the version of this project at the time didn't spawn enough enemies to test it. 

## Installation/Running:
The 'App' folder contains the latest build, but you can open the whole project in unity and its targeted version is 2018.5.1f1. The 'Assets' folder contains all scripts, objects and work on the project.

## Usage:
The game is made to be played with controller, but keyboard and mouse works too. The controls are as following:

- Left-Stick/WASD - move character
- Right-Stick/Mouse - move camera
- X/Left-Click - attack
- B/Left-Shift - HOLD to sprint, TAP to roll in left-stick direction
- Right-Stick-Down/Tab  - Lock-on to nearest front target


Thanks for viewing! - *Lawrence Taylor*

