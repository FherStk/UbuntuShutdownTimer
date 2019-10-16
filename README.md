# Ubuntu Shutdown Timer

## WARNING
Under developement, it wont work properly till this warning dissapears :p

## What does it do?
Ubuntu Shutdown Timer (UST from now on) is a python3 program that allows scheduling a set of automated shutdowns for computers running Ubuntu OS (18.04 and above). With an in advance configurable warning, a popup message can be displayed to the current user in order to inform about the shutdown event and even allowing him/she to abort it.

## How does it work?
It has been built on a client-server architecture that uses sockets for communication, because it must works properly when a user is logged in (or a set of users are using the same machine) and even when there's no user logged in. 

* The server starts:
** A new shutdown event is scheduled from the given schedule times (inside the `config.py' file).
** A new communication channel via socket is created and the server waits for the clients to connect in.

* The client starts:
** A new warning event is scheduled from the given schedule times (inside the `config.py' file).
** There are three warning types:
*** *SILENT*: No warning will be displayed, so the shutdown event will proceed as scheduled.
*** *INFO*: A warning will be displayed in order to inform the user about the scheduled shutdown time, but its read-only.
*** *ABORT*: The warning includes an option that allows the user to abort the scheduled shutdown event. If used, the client will communicate the server in order to abort the shudown.

Notice that there's other ways to accomplish this task, but the chosen one allows to:
* Simple implementation with the minimal communication and synchronization between server and client.
* Take profit of the single computer architecture (client and server shares machine and configuration files).

## Greatings and sources:
https://pymotw.com/3/socket/tcp.html
https://www.geeksforgeeks.org/socket-programming-multi-threading-python/
