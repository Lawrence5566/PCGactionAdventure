# PCGactionAdventure

Core Features:
- Cellular automata 2D room generation
Starts with empty map of 1s and 0s, 1 = wall 0 = space. RoomGenerator.cs generates a room space of 1s and 0s using cellular automata. GraphToMapConverter.cs connects rooms together by drawing a path in the map. GraphToMapConverter passes map onto MeshGenerator.

- Marching squares 3D room generation
Uses Marching Squares algorithm to generate a floor and wall mesh using the map received from GraphToMapConverter

- Joris Dorman's cycles implementation
- Points based distribution of enemies
- Souls like combat
- Boss battles
- Random weapons
- Lock on targeting
- Multidirectional animation
