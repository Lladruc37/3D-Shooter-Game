# 3D-Shooter-Game
 3D shooter game for the online games subject of videogames development & design bachelor's degree.

Link to Drive: https://drive.google.com/file/d/121RNI73HwK11lBslMk71H_kDSeYf5aC6/view?usp=sharing (UPC Login is required)

### Team Contributions:
- Sergi: TCP & General Chat functionality
- Guillem: UDP & UI/Scene Design
- Litos: Bug Fixing & QA
- Nuri: UI Interconnectivity & Functions

### Instructions:

If working with 2 or more separate computers:
        - Load HostJoin scene (or the scene that will
        - Press "Create" in one PC, "Join" in the others
        - Check in all PCs whether the connection will be TCP or UDP
        - Fill-in the input fields and press the corresponding button

    If working on the same computer:
        - Load scenes "ServerHost" and "ClientJoin" at the same time
        - Check if all the canvas in "ClientJoin" scene are targeted for the 2nd display
        - Check in all scenes whether the connection will be TCP or UDP
        - Fill-in the input fields and press the corresponding button

    To chat, simply type whatever you want in the input fields after joining/hosting and press Return to send a message

### Comments:
    - As it's presented, the settings of the project are set as if to be tested in the same computer. If there are errors showcasing the "ClientJoin" scene it is due to it being targeted to the Display 2. To properly see the scene, either change the Target Display in all canvas inside the scene to Display 1 or open a new game tab targeted at the Display 2. (More info in the Demo video)
    - There has been no testing with multiple players or with an executable file. However, we believe that the exercise should perfectly be functional regardless of these conditions.
    - Demo Video showcases things that aren't present in the final deliverable (such as UI sizes and Testing Assets). This is because the ZIP delivered only contains the necessary files to properly operate the Lab exercise with its intended purpose.

### Known bugs:
- Sometimes, when ending to debug, unity logs will start spamming 2 messages simoultaneously, preceded by a socket error not being properly closed. This bug can be solved if starting and ending to debug again and is probably due to an incorrect cleanup of the project. As it is an error regarding the Unity Inspector, we haven't invested time in solving this issue.