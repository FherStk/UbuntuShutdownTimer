Source: https://pymotw.com/3/socket/tcp.html
        https://www.geeksforgeeks.org/socket-programming-multi-threading-python/


INFO:
How does it works:
   The server starts:
        It setups a new shutdown event from the given schedule times 
        It opens a new communication channel via socket and waits for the clients to connect in.

    The client starts:
        It setups a new warning event from the given schedule times (shared config file with server and client, because client and server shares computer in order to work event without network).
        When a popup prompts, the action chosen by the user will be send to the server.

    A client connects to the server:
        The client shares with the server the action taken by te user (abort or continue with the shudown).
            Abort: the shutdown event will be cancelled and the next-one will be scheduled.
            Ignore: no action will be taken.

Notice that there's other ways to accomplish this task, but the chosen one allows to:
    Simple implementation with the minimal communication and synchronization between server and client.
    Take profit of the single computer architecture (client and server shares machine and configuration files).