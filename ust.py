#source: https://www.devdungeon.com/content/dialog-boxes-python
#        https://www.geeksforgeeks.org/timer-objects-python/
#        https://realpython.com/intro-to-python-threading/

#dependencies: pip3 install pyautogui
#documentation: https://pyautogui.readthedocs.io/en/latest/

from enum import Enum
import threading
import datetime
import time
import os

class Popup(Enum):
    ABORT = 2   #includes a button allowing to abort the shudown
    INFO=1      #only informative, no abort option
    SILENT=0    #no warning will be displayed

SHUTDOWN_TIMES = [{"time": "17:00:00", "popup": Popup.ABORT}]
WARNING_BEFORE_SHUTDOWN = 1 #in minutes

def shutdown():
    print("Shutting down!")
    #os.system('systemctl poweroff')

def warningTimer(shd_time, popup):
    GUI = True
    GUIException = ""
    
    try:                
        #The pyautogui library only loads correctly once the GUI becomes ready, otherwise it will rise an exception
        #TODO: remove the pyautogui library
        import pyautogui        
    except Exception as e:
        GUI = False
        GUIException = e
    
    wait = (shd_time - datetime.datetime.now()).total_seconds()
    shd_timer = threading.Timer(wait, shutdown)  
    shd_timer.start()     

    print("     The warning event raised up, so a new shutdown event will be scheduled:")
    print("         Time:       {}".format(shd_time.strftime('%H:%M:%S')))
    print("         Popup:      {}".format(popup))
    print("         GUI loaded: {}".format(GUI), end='')
    if(GUI): print("", end='\n\n')
    else: print(" ({})".format(GUIException), end='\n\n')
        
    #TODO: fix accents for Zenity
    if(popup == Popup.SILENT or not GUI): print("     No warning message will be prompted so the shutdown event will raise on silent mode.", end='\n\n')
    else:
        print("     Displaying the warning popup, so the user will be able to abort the shutdown on demand (or not).")
        text = "Aquest ordinador s'apagara automaticament a les {}.".format(shd_time.strftime('%H:%M:%S'))#.encode('ascii')
        noOutput = ">/dev/null 2>&1"

        if(popup == Popup.INFO): action = os.system('zenity --notification --text="{}" {}', text, noOutput)
        else:
            text = text + " Si vol anul.lar laturada automatica premi el boto 'No'"
            action = os.system('zenity --question --text="{}" {}'.format(text, noOutput))
            
            if action != 256: print("     The user decided to continue with the scheduled shutdown event.", end='\n\n')         
            else:
                shd_timer.cancel()
                print("     The user decided to abort the scheduled shutdown event.", end='\n\n')         
                action = os.system('zenity --info --text="Si us plau, recordi apagar l\'ordinador manualment quan acabi de fer-lo servir. Gracies." {}'.format(noOutput))

def main():    
    print("Ubuntu Shutdown Timer (v0.0.0.1)")
    print("Copyright (C) Fernando Porrino Serrano")
    print("Under the GNU General Public License v3.0")
    print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')        

    print("Setting up the warning timers:")
    now = datetime.datetime.now()   

    for sdt in SHUTDOWN_TIMES:
        #for testing purposes
        warningTimer(now + datetime.timedelta(seconds = 30), sdt["popup"])
        #end testing purposes

        shd_time = datetime.datetime.strptime(sdt["time"], '%H:%M:%S').replace(year=now.year, month=now.month, day=now.day)
        wrn_time = shd_time - datetime.timedelta(minutes = WARNING_BEFORE_SHUTDOWN)        

        #wait till warning time for each warning requested (a warning is a shutdown requested time - x minutes)        
        if(wrn_time > datetime.datetime.now()):
            print("     A new warning event has been scheduled to rise up at %s" % wrn_time.strftime('%H:%M:%S'), end='\n\n')
            wait = (wrn_time - datetime.datetime.now()).total_seconds()
            time.sleep(wait)
            warningTimer(shd_time, sdt["popup"])
                    
        print("     A warning event has been ignored due its schedule time has passed at %s" % wrn_time.strftime('%H:%M:%S')) 

if __name__ == "__main__":
    main()