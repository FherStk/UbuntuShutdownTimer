#Source: https://pymotw.com/3/socket/tcp.html
#        https://www.geeksforgeeks.org/socket-programming-multi-threading-python/

'''
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
'''
from shared.popup import Popup
from shared.config import Config
from shared.connection import Connection
import threading
import datetime
import sys
import os

#TODO:  Share socket creation code
#       Load config from a file
#       Use create_connection

def cancel():
    print("Connecting to the server on {} port {}:".format(Connection.SERVER, Connection.PORT))
    sock = Connection.join()

    try:
        # Send data        
        print("     Sending the ABORT request to the server... ", end='')
        sock.sendall(b"ABORT")

        # Look for the response
        data = sock.recv(1024)
        if(data == b"ACK"):
            print("OK")

        elif (data == b"NACK"):
            print("ERROR! The server responded with a NON-ACK")
            #TODO: retry sistem

        else:
            print("ERROR! Unexpected response received: {}".format(data))

    except Exception as e:
        print("EXCEPTION: {}".format(e))
        
    finally:
        print("     Closing the connetion with the server... ", end='')
        sock.close()        
        print("OK", end='\n\n')

def warning(shd_time, popup):
    print("Rising the warning event: ")

    if(popup == popup.SILENT): print("     No warning message will be prompted so the shutdown event will raise on silent mode.", end='\n\n')
    else:
        print("     Displaying the warning popup, so the user will be able to abort the shutdown on demand (or only warned about it).")
        text = "Aquest ordinador s'apagara automaticament a les {}.".format(shd_time.strftime('%H:%M:%S'))#.encode('ascii')
        noOutput = ">/dev/null 2>&1"

        if(popup == popup.INFO): action = os.system('zenity --notification --text="{}" {}', text, noOutput)
        else:
            text = text + " \n\nDessitga anul.lar laturada automatica?"
            action = os.system('zenity --question --text="{}" {}'.format(text, noOutput))
            #For testing only
            action = 1
            if action == 256: print("     The user decided to continue with the scheduled shutdown event.", end='\n\n')         
            else:                
                print("     The user decided to abort the scheduled shutdown event.", end='\n\n')         
                os.system('zenity --info --text="Si us plau, recordi apagar l\'ordinador manualment quan acabi de fer-lo servir. Gracies." {}'.format(noOutput))
                cancel()

def main():    
    print("Ubuntu Shutdown Timer (CLIENT) - v0.0.0.2")
    print("Copyright (C) Fernando Porrino Serrano")
    print("Under the GNU General Public License v3.0")
    print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')   

    print("Setting up the warning event:", end='')
    sdt = Config.SHUTDOWN_TIMES[0]
    now = datetime.datetime.now()   
    #TODO: once a shutdown has been cancelled, automatically schedule de next-one...

    shd_time = now + datetime.timedelta(minutes = Config.WARNING_BEFORE_SHUTDOWN)
    #shd_time = datetime.datetime.strptime(sdt["time"], '%H:%M:%S').replace(year=now.year, month=now.month, day=now.day)
    wrn_time = shd_time - datetime.timedelta(minutes = Config.WARNING_BEFORE_SHUTDOWN)
    wrn_timer = threading.Timer((wrn_time - datetime.datetime.now()).total_seconds(), warning, [shd_time, Config.SHUTDOWN_TIMES[0]["popup"]])  
    print(" OK")      
    print("     The warning popup has been scheduled on {}".format(wrn_time.strftime('%H:%M:%S')), end='\n\n')                    
    wrn_timer.start()  

if __name__ == "__main__":
    main()