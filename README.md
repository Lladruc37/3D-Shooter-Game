# **Backfired!:** A 3D Shooter Game
We are developing a 3D shooter game for the Networks and Online Games subject of the Videogame Development & Design bachelor's degree.

[Link to Drive to see the deliveries](https://drive.google.com/drive/folders/1cDQBShDQWu6GQNSsq3fKlyPHRGm_DSiF?usp=share_link) and [link to see the Exercise 4: World State Replication](https://drive.google.com/file/d/1Gcl3fany79bvKzFyl4_L0UMVU383DUOp/view?usp=share_link) (UPC Login is required)
[Link to the GiHub Repository](https://github.com/Lladruc37/3D-Shooter-Game)

### About the game
_Backfired!_ is a FPS where the end goal is to get 5 points. The twist is that, when you shoot, you are also move backwards, which can be used to jump and move in a fun and unique way. The intended goal is to have 4 different players in the game.

### Team Contributions:
- [Sergi Colomer](https://github.com/Lladruc37): Bug Fixing & QA, Ping System
- [Guillem Alava](https://github.com/WillyTrek19): Bug Fixing & Optimizations, Ingame Console, Ping System
- [Carles López](https://github.com/carlesli): Bug Fixing & QA, Gameplay
- [Núria Lamonja](https://github.com/Needlesslord): Bug Fixing & Optimizations

### Instructions to start the game:

As of now, Backfired! only works on LAN.

- If working with 2 or more separate computers:
     - Load HostJoin scene
     - Press "Create" in one PC, "Join" in the others

- If working on the same computer (as shown in the demo video):
    - Build the project with the following scenes
        - Scenes/HostJoin -> Scene 0
        - Scenes/ServerHost -> Scene 1
        - Scenes/ClientJoin -> Scene 2
    - Open any instances of the .exe generated in the same computer

#### How to Play
To create a server:
- Fill-in the input fields and press the corresponding button
     - As the Host, you will be asked for your name and the server's name
     - As a Player, you will be asked for your name and the server's IP
     
To play:
 - To unlock the mouse view used to aim, press [F1]. To lock it again, press it again.
 - To chat, simply type whatever you want in the input fields after joining/hosting and press Return to send a message. If you want to use the funcion in-game, you need to press [F1] first and to continue playing, it has to be locked again to continue playing
 - To shoot press [M1]. You can use it also to boost yourself up in the air
 - To look backwards press [M2] or [Alt].
 - Use the mouse to control where the camera points
 - To activate console press [F12]

### Changes/Errors fixed from previous delivery:
- Added an ingame console to showcase the commands "Debug.Log" inbuild. This has made debugging much easier.
- Solved the lag issue. It was due to an overuse of the function "Thread.Sleep()". We have reduced the times and removed unnecessary calls of such functions.
- Removed all usage of strings and special characters from packets. All packets are now sent as raw data from a MemoryStream, and use IDS to identify the type of package sent (declared in GameplayManager.cs). We have also reduced the size of packets send from 1024 bytes to multiples of 256 bytes (256 being the smallest size).
- Players are now instantiated instead of prefixed to a set number of characters and spawn in a random position. Therefore, there's no limit to how many players can join in the same game. Players can now join and leave in the middle of the match, and the server will update the games accordingly.
- Sockets & server now closed properly. No warnings appear regarding aborted threads and aborted connections (to our knowledge).
- Added a ping system that constantly sends and recieves information to ensure that the connection is still active. If a time has passed without recieving pings from a lagging/disconnected user, the player is removed from the server entirely. If a user doesn't recieve pings from a server, they return to the "Join server" screen.
- Reduced the amount of lists/dictionaries used in the application from 6 to 3 (1 for networking & 2 for the game). All info regarding the clients is now stored in the same class (Also in GameplayManager.cs)

### Known bugs:
- POLTERBUG: Player may appear inside of a building when spawning (when starting game or after death) or outside the arena alltogether. The first instance (inside the building spawn) we have tried to fix it by using "Physics.CheckSphere" & "Physics.CheckCapsule" with specific hitboxes and, as of this delivery, it happens only 1 - 5% of the time. The second one (outside the arena spawn) we don't know why it happens, and it's extremely rare. We theorize it is due package sending issues (a package is sent with an erroneous position that overrides the random spawn position update).
- Due to delays between threads, there may be instances when the exception "InvalidOperationException: Collection was modified; enumeration operation may not execute" appears and stops the connection alltogether. This is because there are times in which lists are being modified as other threads are using them. However, from our testing, this is extremely rare, as the code is fast/optimized enough so that they don't coincide often. We believe it happens more when, for example, the server is ran in the Unity Engine directly and the users are ran in builds, when there's jitter/slow internet connections or when too many packages are sent due to the amount of players connected in the same match (as in, for example, 15 - 20 players at the same time).

### Credits:
- Environment used by _ZENRIN CO., LTD._, available in the [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/japanese-otaku-city-20359)
