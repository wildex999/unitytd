::UnityTD(Temp. name)::

This is a game created with Unity 4. It's a 2D Tower Defence game, with online multiplayer and mazing(Towers and enemies share space, forcing enemies to pathfind).
It's currently unfinished, only having the basic capabilities, but no actual game on top of it.
What is implemented:

- Basic 2D Grid based Tower Defence.
  - You can place towers, with tower taking up anything from 1 to n grid spaces.
  - Towers cost money
  - Spaces on the map can be marked as "noPlace", meaning tower's can not be placed there.
  - The tower will fire at the Enemies as they walk into range.
  - You can sell towers

- Enemies spawn from spawn points
  - Enemies will path find through tower placements, and the player is stopped from placing a tower if it would close of the last path.
  - There are different types of Enemies. Some can fly over the towers, but there are special tower that the other can walk under, but the fliers can not fly under/over
  - The enemies take damage, and can die from the towers, giving money to the player.
  - They will walk up to the "Player Castle" and cause damage to the player.

- Map editor
  - The absolute basics of a map editor is implemented
  - Can place/remove map tiles, Spawn points and end goal("Castle")
  - Can save and load maps

- Online multiplayer
  - You can log in to the main server with a name(No password or registration at this point)
    - Main server is written in NodeJS
  - You can either create or join a named game.(No list of all games yet)
  - When hosting a game you can select the map to host, and will then go into a lobby with a list of player, info about the map, a chat, and ready buttons.
  - Player joining will also go into this lobby.
  - The host can use a custom map from the editor, and will upload the map to any player joining.

- The networking portion is LockStep. That means that the only network data sent is player action(Place tower, sell tower, Upgrade tower)
No actualy network data about Enemy movement or Tower firing is sent. 
Instead each client will be running their own deterministic simulation, taking one simulation step once the host sends a "Step" packet(Indicating end of player actions for this step).
Pathfinding, Collision detection and movement is all written in deterministic fixed-point.
  - Built inn hash check to see if they have fallen out of sync(But no way to correct or join mid-game)
  - Currently it all seems to work without any problems.
  
- How to build:

Game:
- Update mainIp in NetManager.cs if server not running on localhost(Multiplayer only).
- Build game using Unity 4.5(Last tested)

Server:
- cd unitytd_server
- npm install async
- node main.js
