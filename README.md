# 3D-Shooter-Game
 3D shooter game for the online games subject of videogames development & design bachelor's degree.

Link to Drive: https://drive.google.com/file/d/1BySvb7YwGbk0EMkxc23JQBSrW9cTGM3F/view?usp=sharing (UPC Login is required)

### Team Contributions:
- Sergi: TCP & General Chat functionality
- Guillem: UDP & UI/Scene Design
- Litos: Bug Fixing & QA
- Nuri: UI Interconnectivity & Functions

### Instructions:

    If working with 2 or more separate computers:
        - Load HostJoin scene
        - Press "Create" in one PC, "Join" in the others
        - Alternatively you can just load "ServerHost" and "ClientJoin" at each of the PCs
        - Check in all PCs whether the connection will be TCP or UDP
        - Fill-in the input fields and press the corresponding button

    If working on the same computer (as shown in the demo video):
        - Load scenes "ServerHost" and "ClientJoin" at the same time
        - Check if all the canvas game objects in "ClientJoin" scene are targeted for the 2nd display as well as the camera
        - Open a second game tab targeted at the Display 2
        - Check in all scenes whether the connection will be TCP or UDP
        - Fill-in the input fields and press the corresponding button

    To chat, simply type whatever you want in the input fields after joining/hosting and press Return to send a message

### Comments:
    - As it's presented, the settings of the project are set as if to be tested in two different computers. If it were to be tried using the same computer, a message error regarding the existance of two EventSystems at the same time would create an exception error and both UIs would appear overlayed one on top of the other. The first problem is solved by simply removing one of the EventSystems and the 2nd error has been solved in the previous lines.
    - Demo Video showcases things that aren't present in the final deliverable (such as UI sizes and Testing Assets). This is because the Unity package delivered only contains the necessary files to properly operate the Lab exercise with its intended purpose and it was done before some of the latest modifications/bug fixes.

### Known bugs:
    - Sometimes, when ending to debug, unity logs will start spamming 2 messages simoultaneously, preceded by a socket error not being properly closed. This bug can be solved if starting and ending to debug again and is probably due to an incorrect cleanup of the project. As it is an error regarding the Unity Inspector, we haven't invested time in solving this issue.
    - If you were to start a server in TCP and enter a user with an UDP method (or viceversa), you will trigger an infinite loop. Since we will procede with the usage of UDP in following deliveries, we haven't put much effort in solving this issue. Just don't forget to double check the checkboxes.
