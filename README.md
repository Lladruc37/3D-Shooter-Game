# **Backfired!:** A 3D Shooter Game
We are developing a 3D shooter game for the Networks and Online Games subject of the Videogame Development & Design bachelor's degree.

[Link to Drive to see the deliveries](https://drive.google.com/drive/folders/1cDQBShDQWu6GQNSsq3fKlyPHRGm_DSiF?usp=sharing) (UPC Login is required)

### About the game
_Backfired!_ is a FPS where the end goal is to get 25 points. The twist is that, when you shoot, you are also move backwards, which can be used to jump and move in a fun and unique way. The intended goal is to have 4 different players in the game.

### Team Contributions:
- [Sergi Colomer](https://github.com/Lladruc37): TCP & General Chat functionality, Serialization & Data Dispatch between Server and Client interchangeably
- [Guillem Alava](https://github.com/WillyTrek19): UDP & UI/Scene Design, Serialization & Data Dispatch between Server and Client interchangeably
- [Carles López](https://github.com/carlesli): Bug Fixing & QA, Gameplay
- [Núria Lamonja](https://github.com/Needlesslord): UI Interconnectivity & Functions, Gameplay

### Instructions to start the game:

As of now, Backfired only works on LAN.

- If working with 2 or more separate computers:
     - Load HostJoin scene
     - Press "Create" in one PC, "Join" in the others
     - Alternatively you can just load "ServerHost" and "ClientJoin" at each of the PCs
     - Check in all PCs whether the connection will be TCP or UDP
     - Fill-in the input fields and press the corresponding button
          - As the Host, you will be asked for your name and the server's name
          - As a Player, you will be asked for your name and the server's IP

- If working on the same computer (as shown in the demo video):
     - Load scenes "ServerHost" and "ClientJoin" at the same time
     - Check if all the canvas game objects in "ClientJoin" scene are targeted for the 2nd display as well as the camera
     - Open a second game tab targeted at the Display 2
     - Check in all scenes whether the connection will be TCP or UDP
     - Fill-in the input fields and press the corresponding button
          - As the Host, you will be asked for your name and the server's name
          - As a Player, you will be asked for your name and the server's IP
        
#### How to Play
 - To unlock the mouse view used to aim, press [F1]. To lock it again, press it again.
 - To chat, simply type whatever you want in the input fields after joining/hosting and press Return to send a message. If you want to use the funcion in-game, you need to press [F1] first and to continue playing, it has to be locked again to continue playing
 - To move, use [WASD] as you would normally ([W] to move forward, [A] to the left, [S] backwards and [D] to the right)
 - To shoot press [M1]. You can use it also to boost yourself up in the air
 - To look backwards press [M2]
 - Use the mouse to control where the camera points

### Comments:
- As it's presented, the settings of the project are set as if to be tested in two different computers. If it were to be tried using the same computer, a message error regarding the existance of two EventSystems at the same time would create an exception error and both UIs would appear overlayed one on top of the other. The first problem is solved by simply removing one of the EventSystems and the 2nd error has been solved in the previous lines.
- Demo Video showcases things that aren't present in the final deliverable (such as UI sizes and Testing Assets). This is because the Unity package delivered only contains the necessary files to properly operate the Lab exercise with its intended purpose and it was done before some of the latest modifications/bug fixes.
- There is lag spike when two players send a message in chat, it has to be improved

### Known bugs:
- Sometimes, when ending to debug, unity logs will start spamming 2 messages simoultaneously, preceded by a socket error not being properly closed. This bug can be solved if starting and ending to debug again and is probably due to an incorrect cleanup of the project. As it is an error regarding the Unity Inspector, we haven't invested time in solving this issue.
- If you were to start a server in TCP and enter a user with an UDP method (or viceversa), you will trigger an infinite loop. Since we will procede with the usage of UDP in following deliveries, we haven't put much effort in solving this issue. Just don't forget to double check the checkboxes.

### Credits:
- Environment used by _ZENRIN CO., LTD._, available in the [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/urban/japanese-otaku-city-20359)
