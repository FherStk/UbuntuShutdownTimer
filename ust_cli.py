from shared.popup import Popup
from shared.config import Config
from shared.connection import Connection
from shared.utils import Utils
import threading
import datetime
import sys
import os

def abort(connection):    
    try:
        # Send data        
        print("     Sending the ABORT request to the server... ", end='')
        connection.sendall(b"ABORT")
        print("OK")
              
    except Exception as e:
        print("EXCEPTION: {}".format(e))            

def warning(connection, shd_time, popup):
    print("Rising the warning event: ")

    if(popup == popup.SILENT): print("     No warning message will be prompted so the shutdown event will raise on silent mode.", end='\n\n')
    else:
        print("     Displaying the warning popup, so the user will be able to abort the shutdown on demand (or only warned about it).")
        text = "Aquest ordinador s'apagarà automàticament a les <b>{}</b>.".format(Utils.dateTimeToStr(shd_time, Utils.TIMEFORMAT))
        noOutput = ">/dev/null 2>&1"

        if(popup == popup.INFO): action = os.system('zenity --notification --no-wrap --text="{}" {}', text, noOutput)
        else:
            action = os.system('zenity --question --no-wrap --text="{}" {}'.format(text + " \nDesitja anul·lar l'aturada automàtica?", noOutput))            
            if action == 256: 
                print("     The user decided to continue with the scheduled shutdown event.", end='\n\n')                     
                os.system('zenity --notification --text="{}" {}'.format("Apagada automàtica a les {}".format(Utils.dateTimeToStr(shd_time, Utils.TIMEFORMAT)), noOutput))

            else:                
                print("     The user decided to abort the scheduled shutdown event.", end='\n\n')         
                os.system('zenity --notification --text="{}" {}'.format("Recordi apagar l'ordinador manualment. Gràcies.", noOutput))
                abort(connection)

def requestInfo(connection):  
    try: 
        print("     Requesting for the next shutdown time... ", end='')
        connection.sendall(b"TIME")        
        shd_time = Utils.strToDateTime(connection.recv(1024).decode("ascii"))
        print("OK: {}".format(Utils.dateTimeToStr(shd_time, Utils.TIMEFORMAT)))   

        print("     Requesting for the next warning popup type... ", end='')
        connection.sendall(b"POPUP")        
        popup = connection.recv(1024).decode("ascii")
        print("OK: {}".format(popup), end='\n\n')        

        return shd_time, popup

    except Exception as e:
        print("EXCEPTION: {}".format(e))     
        return "" 

def setupWarning(connection, shd_time, popup):
    print("Setting up the warning event:", end='')

    wrn_time = shd_time - datetime.timedelta(minutes = Config.WARNING_BEFORE_SHUTDOWN)
    wrn_timer = threading.Timer((wrn_time - datetime.datetime.now()).total_seconds(), warning,  [connection, shd_time, popup])  
    wrn_timer.start()
    
    print(" OK")      
    print("     The warning popup has been scheduled on {}".format(Utils.dateTimeToStr(wrn_time, Utils.TIMEFORMAT)), end='\n\n')                     

    return wrn_timer

def listen(connection, wrn_timer):  
    print("Listening for server messages")

    while wrn_timer.is_alive():
        data = connection.recv(1024)  

        if data:
            if(data == b"REFRESH"):
                print("The server requested for a REFRESH, cancelling the warning event... ", end='')                
                wrn_timer.cancel()
                print("OK")                

            else:
                print("Unexpected message received from the server: {!r}".format(data))

def main():    
    print("Ubuntu Shutdown Timer (CLIENT) - v0.2.0.0")
    print("Copyright (C) Fernando Porrino Serrano")
    print("Under the GNU General Public License v3.0")
    print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')   

    #Step 1: connect with the server
    #Step 2: request for the next scheduled shutdown time
    #Step 3: schedule the related warning event
    #Step 4: listen for messages from the server
        #Step 4.1: REFRESH: abort the current warning event and return to Step 2
    #Step 5: the warning event rised up and the user selected the option to abort
        #Step 5.1: ABORT: send the request to the server

    print("Connecting to the server on {} port {}:".format(Connection.SERVER, Connection.PORT))
    connection = Connection.join()

    #listen has a loop inside and will remain looping till wrn_timer has been cancelled
    while True:
        shd_time, popup = requestInfo(connection)    
        wrn_timer = setupWarning(connection, shd_time, popup)    
        listen(connection, wrn_timer)

if __name__ == "__main__":
    main()