from shared.popup import Popup
from shared.config import Config
from shared.connection import Connection
import threading
import datetime
import sys
import os

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
        text = "Aquest ordinador s'apagarà automàticament a les <b>{}</b>.".format(shd_time.strftime('%H:%M:%S'))
        noOutput = ">/dev/null 2>&1"

        if(popup == popup.INFO): action = os.system('zenity --notification --no-wrap --text="{}" {}', text, noOutput)
        else:
            action = os.system('zenity --question --no-wrap --text="{}" {}'.format(text + " \nDesitja anul·lar l'aturada automàtica?", noOutput))            
            if action == 256: 
                print("     The user decided to continue with the scheduled shutdown event.", end='\n\n')                     
                os.system('zenity --notification --text="{}" {}'.format("Apagada automàtica a les {}".format(shd_time.strftime('%H:%M:%S')), noOutput))

            else:                
                print("     The user decided to abort the scheduled shutdown event.", end='\n\n')         
                os.system('zenity --notification --text="{}" {}'.format("Recordi apagar l'ordinador manualment. Gràcies.", noOutput))
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