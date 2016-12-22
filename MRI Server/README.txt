Open a terminal and navigate to the folder which contains DemoServer.cs.
You can navigate through the file system using the cd command. If you are
not familiar with this at all, make a new folder on your desktop titled
"DemoServer", and place the DemoServer.cs file inside it. Then execute the
following command:

    cd ~/Desktop/DemoServer

Once you are in the proper directory, execute the following commands:

    mcs DemoServer.cs
    sudo mono DemoServer.exe (you will be asked for the password, which is "a")

The server program should prompt you for another command.
Enter the following command:

	run

At this point, your terminal should look something like this:

    user@computer:$ cd ~/DemoServer
    user@computer:$ mcs DemoServer.cs
    user@computer:$ sudo mono DemoServer.exe 
    Enter a command (use "commands" to see a list of commands
    > run
    Waiting for client to establish connection...

If the run command fails, you will get an error message which will *hopefully*
help you solve the problem.

**Important note**
If the server is failing, try closing both the server and mri game, or even rebooting
the computer with the server on it.
