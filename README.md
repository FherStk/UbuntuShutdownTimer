# Ubuntu Shutdown Timer

## WARNING
Under development, it won't work properly till this warning disappears :p

## What does it do?
Ubuntu Shutdown Timer (UST from now on) is a C# (over the .NET Core Framework 3.1) program that allows scheduling a set of automated shutdowns for computers running Ubuntu Desktop OS (20.04 and above), including a set of configurable user-oriented warnings that can be used to cancel the scheduled shutdowns (or just to warn about an imminent power off).

Please, note that event it has been developed over the .NET Core Framework 3.1, the application has been published as self-contained so there's no need to install the .NET Core SDK or runtime; it can just be executed as a regular binary!

## Third party software:
* Tmds.DBus by Tom Deseyn: under the MIT License (https://www.nuget.org/packages/Tmds.DBus/)

## Install guide
### How to install (as root)
1. Download the latest release wherever you want to install the application (for example in `/usr/local/ust`).
2. Update the `settings/settings.json` settings file to fit up your needs.
4. Set execution permissions to the application main file with `chmod +x ust`.
5. Install the application with `./ust --install` which will setup the server and client instances.
6. Congrats! The application is ready and working :)

### Settings
Update the `settings/settings.json` settings file to fit up your needs.

- PopupThreshold (in minutes): Set the time frame of anticipation with which a user must be warned about a shutdown event.
- IgnoreThreshold (in minutes): Set the time frame of anticipation elapsed needed to schedule a shutdown event, so the ones that must happen before that will be ignored (not scheduled).
- AutocancelThreshold (in minutes): Set the time frame of anticipation elapsed needed to ask for user interaction (rise a pop-up), so the ones that must happen before that will be automatically cancelled (the next shutdown event will be scheduled).
- Schedule: Array of shutdown events, the nearest shutdown will be the first one to be scheduled (obviously, the past ones will be ignored); if the user cancels one, the next one will be scheduled.
    - Shutdown (local time): When the shutdown event will be fired.
    - Mode (pop-up behaviour):
        - SILENT: No pop-up or warning will be displayed to the user.
        - INFORMATIVE: The user will be warned about a scheduled shutdown, but no interaction is allowed.
        - CANCELLABLE: The user will be warned about a scheduled shutdown, and is allowed to cancel the scheduled event (the server will schedule the next one in the list).
        
### How to uninstall
1. As **root**, uninstall the application with `ust --uninstall` and all the settings and changes will be reverted.
2. Remove the application folder.
3. That's it! It's sad, but was fun :)

## How does it work?
It has been built as a client-server application that uses D-Bus for communication, because it must work properly when a set of users are logged (sharing a computer) and also when there's no user logged at all. 

### Architecture
The application can be split into two main parts:
1. The server: Works with root permissions as a system.d service defined into `/lib/systemd/system/ust-server.service` launching a self-contained .NET Core 3.1 server-instance application.
2. The client: Works with user permissions as a regular application defined into `/etc/profile.d/ust-client.sh` launching a self-contained .NET Core 3.1 client-instance application.

### Communication process
- The server starts:
    - Registers and exposes the interface within D-Bus.    
    - The nearest (in the future) shutdown event is scheduled from the given schedule times (inside the `files/settings.json` file), if there's no more schedule times for today, it will be scheduled for tomorrow.
    - Listens for requests through D-Bus using events (no polling needed).

- The client starts:
    - Connects to the server using the D-Bus interface and asks for the current scheduled shutdown event.
    - Schedules the warning pop-up using the given settings.
    - Listens for cancellations over the scheduled event on server-side through D-Bus using events (no polling needed).

- Scenario 1 - An info pop-up rises on client-side:
    - An scheduled info pop-up rises on client-side (the user cannot cancel it).
    - No interaction is performed over the server.
    - The shutdown event rises on server and the computer poweroffs.

- Scenario 2 - A cancellable pop-up rises on client-side:
    - An scheduled cancellable pop-up rises on client-side and the user requests for cancellation.
    - A cancellation request is performed over the server.
    - The server accepts the cancellation (validation over scheduled IDs is done to avoid repeated requests from different user sessions).
    - The server aborts the current shutdown and schedules the next one.
    - The server sends a signal to all the client to warn them about a cancellation.
    - The clients (all the connected ones) requests for the new scheduled event.
    - The client cancels their current pop-ups and schedules the new ones.

### The installation process
1. D-Bus policies: 
    1. If needed, a new file will be added to `/etc/dbus-1/system-local.conf`.
    2. The previous file will be modified to allow communication through the `net.xeill.elpuig.UST1` interface for all users.

2. Server service:
    1. A new service will be added to `/lib/systemd/system/ust-server.service`.
    2. The service will run the application in server mode with root permissions on startup, so all users will find a unique running instance to connect with.

3. Client application:
    1. A new launcher will be added to `/etc/profile.d/ust-client.sh`.
    2. The application will run on client mode with user permissions on logon, so all users will connect with the server in order to display pop-ups if needed.

4. Reload D-Bus:
    1. A call to ReloadConfig() will be performed over D-Bus.
    2. The D-Bus daemon will be forced to reload its config with a HUP signal (as the official documentation suggests).

### The uninstallation process
1. D-Bus policies: 
    1. The file `/etc/dbus-1/system-local.conf` won't be removed in order to preserve other configurations.
    2. The policy entries for the `net.xeill.elpuig.UST1` interface will be removed.

2. Server service:
    1. The service `/lib/systemd/system/ust-server.service` will be disabled and removed.

3. Client application:
    1. The launcher `/etc/profile.d/ust-client.sh` will be removed.

4. Reload D-Bus:
    1. A call to ReloadConfig() will be performed over D-Bus.
    2. The D-Bus daemon will be forced to reload its config with a HUP signal (as the official documentation suggests).
