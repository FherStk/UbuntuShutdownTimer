from shared.popup import Popup
from shared.config import Config
from shared.connection import Connection
import threading
import datetime
import sys
import os

def shutdown(connections):
    """
    Just shuts down the machine    

    Keyword arguments:
    connections --- All the open connections.
    """
    print("\nClosing open connections...")
    for connection, client_address in connections:
        print("     Closing open connection with client {}".format(client_address))
        #TODO: try/catch on each close, the sutdown must prevail! muahahaha!
        connection.close()

    print("\nShutting down!")
    #os.system('systemctl poweroff')

def listen(connections, connection, client_address, shutdown_time, shutdown_timer):
    """
    Continuous polling for data sent from the client using the given connection.
    It can be used to ask for the shutdown times or for shutdown abort requests.    

    Keyword arguments:
    connections     --- All the open connections.
    connection      --- The current connection to listen.
    client_address  --- The current connection's client's address (for log purposes only).
    shd_time        --- The scheduled shutdown time.
    shd_timer       --- The shutdown timer used for schedulling.
    """

    print("     {} - Reading received data.".format(client_address))

    while True:
        data = connection.recv(1024)  

        if data:
            if(data == b"TIME"):
                print("     {} -  Shutdown time requested, sending back the current scheduled shutdown time.".format(client_address))
                shutdown_timer.cancel()
                connection.sendall(shutdown_time.encode("ascii"))

            elif(data == b"ABORT"):
                print("     {} - Abort requested, so the shutdown event will be aborted.".format(client_address))
                shutdown_timer.cancel()
                connection.sendall(b"ACK")

                #TODO: send the new info to all the clients in order to update (and remove the aborted scheduled events).

            else:
                print("     {} - Unexpected message received: {!r}".format(client_address, data))
                connection.sendall(b"NACK")


def schedule(connections, schedule_idx=0):  
    """
    Schedules a new shutdown timer, using the schedule time that has been set in the config.py file.
    The schedule_idx arguments defines wich item must be used (overflows will start again from the begining).

    Keyword arguments:
    connections     --- All the open connections.
    schedule_idx    --- The item that will be used for schedulling a new shutdown event (default 0).
                        The list containing these definition can be found in the shared/config.py file.

    Return:
    shd_time        ---  The scheduled shutdown time.
    shd_timer       ---  The shutdown timer used for schedulling.
    """ 

    print("Setting up the shutdown event:", end='')

    schedule_idx = schedule_idx % len(Config.SHUTDOWN_TIMES)
    sdt = Config.SHUTDOWN_TIMES[schedule_idx]
    now = datetime.datetime.now()

    #Next line for testing only purposes
    #shd_time = now + datetime.timedelta(minutes = Config.WARNING_BEFORE_SHUTDOWN)
    shd_time = datetime.datetime.strptime(sdt["time"], '%H:%M:%S').replace(year=now.year, month=now.month, day=now.day)

    if shd_time < datetime.datetime.now(): shd_time = shd_time + datetime.timedelta(days = 1)
    shd_timer = threading.Timer((shd_time - datetime.datetime.now()).total_seconds(), shutdown, [connections])  
    shd_timer.start()

    print(" OK")
    print("     The shutdown has been scheduled on {}".format(shd_time.strftime('%H:%M:%S')), end='\n\n')

    return shd_time, shd_timer

def handle_connections(sock):
    connections = []
    schedule_idx = -1

    while True:
        shd_time, shd_timer = schedule(connections, schedule_idx+1)

        print("Waiting for connections:")
        while shd_time > datetime.datetime.now():
            connection, client_address = sock.accept()

            print("     {} - New connection received.".format(client_address))
            connections.append((connection, client_address))

            print("     {} - Starting a new listening thread.".format(client_address))
            thread = threading.Thread(target=listen, args=(connections, connection, client_address, shd_time, shd_timer))
            thread.start()

def main():    
    print("Ubuntu Shutdown Timer (SERVER) - v0.2.0.0")
    print("Copyright (C) Fernando Porrino Serrano")
    print("Under the GNU General Public License v3.0")
    print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')   
        
    #TODO:  multi-user support: if one cancels the shutdown, it wont be warned for other users logged in.    

    print("Starting server:", end='')    
    sock = Connection.create()
    sock.listen(0)
    print(" OK")
    print("     Starting ready and listening on {} port {}".format(Connection.SERVER, Connection.PORT), end='\n\n')    

    
    try:
        handle_connections(sock)

    except Exception as e:
        print("     EXCEPTION: {}.".format(e))

    finally:        
        sock.close()
        print("     Server stopped")

if __name__ == "__main__":
    main()