# **Backfired!:** A 3D Shooter Game
We are developing a 3D shooter game for the Networks and Online Games subject of the Videogame Development & Design bachelor's degree. The intended goal is to have 4 different players in the game.

[Link to Drive to see the deliveries](https://drive.google.com/drive/folders/1cDQBShDQWu6GQNSsq3fKlyPHRGm_DSiF?usp=share_link) and [link to see the final delivery](https://drive.google.com/drive/folders/1qEkj_JnQXaL9l3SodOoZhM0aKbG4hBWC?usp=share_link) (UPC Login is required)
[Link to the GitHub Repository](https://github.com/Lladruc37/3D-Shooter-Game)

### About the game
_Backfired!_ is an FPS free-for-all where the end goal is to get 5 points. Here's the twist, when you shoot you are also moved backwards, which can be used to jump and move in a fun and unique way. All the players can't move by any other means they would expect.

### Networking Aspects
- Connectivity method: We use _**UDP**_ since our game is a FPS we want a fast connectivity even if we lose some packets.
- Syncronization model: We use _**snapshot interpolation**_ in combination with _**state interpolation**_.
- Network architecture: Our game has a _**client-server architecture**_ and the server is not authorative since the server is listen-based.
- Object replication: We have the _**4 steps**_ that are recomended (mark, uniquely identify, indicate class, serialize) however we don't send multiple objects per packet since each client only sends their _**own player object**_.
- Object serialization: We use _**streams**_ to send the data. The structure we use is:
	- Header (information about the packet type and who is sending it).
	- Data (depending on the packet type).
- Snapshot compression: We use _**fixed point**_ to send less data losing very little precision. We also use _**geometry compression**_ to use 3 numbers instead of 4 for the rotation.
- Latency hiding: We use _**client side prediction**_ with _**client side interpolation**_ to have both the smoothness of interpolating between packets and have less frames of delay.
- Scalability: We use _**instanciating**_ both for the players and the healthpacks.

### Team Contributions:
- [Sergi Colomer](https://github.com/Lladruc37): UI, Gameplay, Ping System, Sockets & Ports, Server to Client comunication, Chat, UIDs, Lock protection, Lerping, Closing Connections, Bug Fixing & Optimizations, QA
- [Guillem Alava](https://github.com/WillyTrek19): UI, Gameplay, Ping System, Scenes Creation & Structure, Chat, UIDs, Lock protection, Lerping, Music & SFX, Closing Connections, Ingame Console, Bug Fixing & Optimizations, QA
- [Carles López](https://github.com/carlesli): Gameplay, Bug Fixing, QA
- [Núria Lamonja](https://github.com/Needlesslord): Bug Fixing, QA

### Instructions to start the game:

As of now, Backfired! only works on LAN.

- If working with 2 or more separate computers:
     - Load HostJoin scene
     - Press "Create" on one PC and "Join" on the others

- If working on the same computer (as shown in the demo video):
    - Build the project with the following scenes
        - Scenes/HostJoin -> Scene 0
        - Scenes/ServerHost -> Scene 1
        - Scenes/ClientJoin -> Scene 2
    - Open any instances of the .exe generated in the same computer

#### How to Play
Creating/Joining a lobby:
- Fill in the input fields and press the corresponding button
     - As the Host, you will be asked for your name and the server's name
     - As a Player, you will be asked for your name and the server's IP
     
In-game Features:
 - To toggle locking & unlocking the mouse, press [F1].
 - To chat, click on the input field and simply write whatever you want and press Return to send the message.
 - To shoot press [M1]. This is used both for shooting at your enemy and for movement.
 - To look behind you press [M2] or [Left Alt].
 - Use the mouse to control where the camera points.
 - To mute all sound press [F11].
 - To activate the in-game console press [F12].

### Changes/Errors fixed from previous delivery:
- When a player joins, information about all players is sent, even if they aren't moving.
- The players now spawn in a random spawnpoint of a few. By doing this, we don't have to worry about the problem of spawning inside a building or too close to another player.
- We added lock protection to the lists that could be modified and read simultaniously.

### Known bugs:
- If the server is completely still when a new client joins mid-game, they won't see this player until they move themselves or even the mouse.
- Sometimes lerping and correcting the error makes the world desync a little bit (very little from our testing).

### Credits:
- Environment used by _ZENRIN CO., LTD._, available in the [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/japanese-otaku-city-20359)
