#source: https://www.devdungeon.com/content/dialog-boxes-python
#        https://www.geeksforgeeks.org/timer-objects-python/
#        https://realpython.com/intro-to-python-threading/

#dependencies: pip3 install pyautogui
#documentation: https://pyautogui.readthedocs.io/en/latest/

import pyautogui
import threading
import datetime
import os

#TODO: the shutdown times should be in a list
debug = 1
SHUTDOWN_TIMES = ["13:53:00", "13:54:00"]
WARNING_BEFORE_SHUTDOWN = 1 #in minutes

def shutdownTimer():    
    if(debug == 1): print("Shutting down!")
    #os.system('systemctl poweroff')     

def warningTimer(shd_time): 
    if(debug == 1): print("Warning timer rised up, setting up the shutdown timer:")         

    wait = (shd_time - datetime.datetime.now()).total_seconds()
    shd_timer = threading.Timer(wait, shutdownTimer)  
    shd_timer.start()     
    
    if(debug == 1): print("     The shutdown event has been scheduled to rise up at %s" % shd_time.strftime('%H:%M:%S'), end='\n\n')

    action = pyautogui.confirm(text='Aquest ordinador s''apagarà automàticament a les %s' % shd_time.strftime('%H:%M:%S'), title='Apagada automàtica', buttons=['Anul·la l''apagada automàtica'])  # returns "OK" or "Cancel"        
    
    if action != None:
        shd_timer.cancel()
        if(debug == 1): print("The user decided to abort the scheduled shutdown event.")         
        pyautogui.alert(text='Si us plau, recordi apagar l''ordinador manualment quan acabi de fer-lo servir. Gràcies. ', title='Recordatori', button='OK')                

if __name__ == "__main__":
    #TODO: send the shutdown time to the threads, once finished load the next one

    if(debug == 1): 
        print("Ubuntu Shutdown Timer (v0.0.0.1)")
        print("Copyright (C) Fernando Porrino Serrano")
        print("Under the GNU General Public License v3.0")
        print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')        

    if(debug == 1): print("Setting up the warning timers:")
    now = datetime.datetime.now()
    
    for sdt in SHUTDOWN_TIMES:
        shd_time = datetime.datetime.strptime(sdt, '%H:%M:%S').replace(year=now.year, month=now.month, day=now.day)
        wrn_time = shd_time - datetime.timedelta(minutes = WARNING_BEFORE_SHUTDOWN)        

        #wait till warning time for each warning requested (a warning is a shutdown requested time - x minutes)        
        if(wrn_time > datetime.datetime.now()):
            wait = (wrn_time - datetime.datetime.now()).total_seconds()
            wrn_timer = threading.Timer(wait, warningTimer, [shd_time])
            wrn_timer.start()    

            if(debug == 1): print("     A new warning message has been scheduled to popup at %s" % wrn_time.strftime('%H:%M:%S'))
            
        elif(debug == 1): 
            print("     A warning message has been ignored due its schedule time has passed at %s" % wrn_time.strftime('%H:%M:%S'))

    print("")