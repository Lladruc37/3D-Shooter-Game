# **Backfired!:** A 3D Shooter Game
We are developing a 3D shooter game for the Networks and Online Games subject of the Videogame Development & Design bachelor's degree.

[Link to Drive to see the deliveries](https://drive.google.com/drive/folders/1cDQBShDQWu6GQNSsq3fKlyPHRGm_DSiF?usp=sharing) and [link to see the Exercise 3: Serialization](https://drive.google.com/file/d/1XV1GGckFLW_pufWyLg1vFNzTEJiZs8Uc/view?usp=share_link) (UPC Login is required)
[Link to the GiHub Repository](https://github.com/Lladruc37/3D-Shooter-Game)

### About the game
_Backfired!_ is a FPS where the end goal is to get 10 points. The twist is that, when you shoot, you are also move backwards, which can be used to jump and move in a fun and unique way. The intended goal is to have 4 different players in the game.

### Team Contributions:
- [Sergi Colomer](https://github.com/Lladruc37): General Chat functionality, Serialization & Data Dispatch between Server and Client interchangeably
- [Guillem Alava](https://github.com/WillyTrek19): UI/Scene Design, Serialization & Data Dispatch between Server and Client interchangeably
- [Carles López](https://github.com/carlesli): Bug Fixing & QA, Gameplay
- [Núria Lamonja](https://github.com/Needlesslord): UI Interconnectivity & Functions, Gameplay

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
- Do not reuse names nor use the charcters [/>] or [</]
- Fill-in the input fields and press the corresponding button
     - As the Host, you will be asked for your name and the server's name
     - As a Player, you will be asked for your name and the server's IP
     
To play:
 - To unlock the mouse view used to aim, press [F1]. To lock it again, press it again.
 - To chat, simply type whatever you want in the input fields after joining/hosting and press Return to send a message. If you want to use the funcion in-game, you need to press [F1] first and to continue playing, it has to be locked again to continue playing
 - To move, use [WASD] as you would normally ([W] to move forward, [A] to the left, [S] backwards and [D] to the right)
 - To shoot press [M1]. You can use it also to boost yourself up in the air
 - To look backwards press [M2]
 - Use the mouse to control where the camera points

### Known bugs:
- We haven't checked the outcome of using the same name between clients or other special characters common in commands.  We speculate it may interfere with the reception/emission of packets.
- Sometimes, when ending to debug or exiting, unity logs will start spamming 2 messages simoultaneously, preceded by a socket error not being properly closed. This bug can be solved if starting and ending to debug again and is probably due to an incorrect cleanup of the project. As it is an error regarding the Unity Inspector, we haven't invested time in solving this issue.
- There are lag spikes and world state update errors when many players are moving simultaneously in-game. This is due to the high amount of sent messages, which our server is not capable of managing. We tried to mitigate some of it by reducing the amount of packages sent, only emitting when a change has happened. This improved the general game experience, but our intention is to make it better in the following deliveries.

### Credits:
- Environment used by _ZENRIN CO., LTD._, available in the [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/japanese-otaku-city-20359)
