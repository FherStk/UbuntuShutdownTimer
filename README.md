# Ubuntu Shutdown Timer v1.0.0.0 (alpha-1)

## WARNING
Under developement, it wont work properly till this warning dissapears :p

## What does it do?
Ubuntu Shutdown Timer (UST from now on) is a C# (over the .NET Core Framework 3.1) program that allows scheduling a set of automated shutdowns for computers running Ubuntu Desktop OS (20.04 and above), including a set of configurable user-oriented warnings that can be used to cancel the scheduled shutdowns (or just to warn about an inminent poweroff).

## Third party software:
* Tmds.DBus by Tom Deseyn: under the MIT License (https://www.nuget.org/packages/Tmds.DBus/)

## Install guide
### How to install
1. Download the lastest release wherever you want to install the application (for example in `/usr/local/ust`).
2. Update the `files/settings.json` settings file to fit up your needs.
3. As **root**, install the application with `ust --install` which will setup the server and client instances.
4. Congrats! The application is ready and working :)

### Settings
Update the `files/settings.json` settings file to fit up your needs.

- PopupTimeframe (in minutes):      Set with how much time of anticipation, a user must be warned about a shutdown event.
- Schedule:                         Array of shutdown events, the nearest shutdown will be the first one to be scheduled (obviously, the past ones will be ignored); if the user cancels one, the next one will be scheduled.
    - Shutdown (local datetime):    Stored as is due serialization comapibility, only the time part will be used to schedule an event.
    - Mode     (popup behaviour):
        - SILENT:                   No popup or warning will be displayed to the user.
        - INFORMATIVE:              The user will be warned about an scheduled shutdown, but no interaction is allowed.
        - CANCELLABLE:              The user will be warned about an scheduled shutdown, and is allowed to cancel the scheduled event (the server will schedule the next one in the list).
        
### How to uninstall
1. As **root**, uninstall the application with `ust --uninstall` and all the settings and changes will be reverted.
2. Remove the application folder.
3. That's it! It's sad, but was fun :)

## How does it work?
It has been built as a client-server application that uses D-Bus for communication, because it must works properly when a set of users are logged (sharing a computer) and also when there's no user logged at all. 

### Architecture
The application can be splitted into two main parts:
1. The server: Works with root permissions as a system.d service defined into `/lib/systemd/system/ust-server.service` launching a self-contained .NET Core 3.1 server-instance application.
2. The client: Works with user permissions as a regular aplication defined into `/etc/profile.d/ust-client.sh` launching a self-contained .NET Core 3.1 client-instance application.

### Communication process
- The server starts:
    - Registers and exposes the interface within D-Bus.    
    - The nearest (in the future) shutdown event is scheduled from the given schedule times (inside the `files/settings.json` file), if there's no more schedule times for today, it will be scheduled for tomorrow.
    - Listens for requests through D-Bus using events (no polling needed).

- The client starts:
    - Connects to the server using the D-Bus interface and asks for the current scheduled shutdown event.
    - Schedules the warning popup using the given settings.
    - Listents for cancellations over the scheduled event on server-side through D-Bus using events (no polling needed).

- Scenario 1 - An info popup rises on client-side:
    - An scheduled info popup rises on client-side (the user cannot cancel it).
    - No interaction is performed over the server.
    - The shutdown event rises on server and the computer poweroffs.

- Scenario 2 - A cancellable popup rises on client-side:
    - An scheduled cancellable popup rises on client-side and the user requests for cancellation.
    - A cancellation request is performed over the server.
    - The server accepts the cancellation (validation over scheduled IDs is done to avoid repeated requests from different user sessions).
    - The server aborts the current shutdown and schedules the next one.
    - The server sends a signal to all the client to warn them about a cancellation.
    - The clients (all the connected ones) requests for the new scheduled event.
    - The client cancels their current popups and schedules the new ones.

### The installation process
Comming soon!

### The uninstallation process
Comming soon!