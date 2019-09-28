#source: https://www.devdungeon.com/content/dialog-boxes-python
#        https://www.geeksforgeeks.org/timer-objects-pythonç/
#        https://realpython.com/intro-to-python-threading/

#dependencies: pip3 install pyautogui
#documentation: https://pyautogui.readthedocs.io/en/latest/

import pyautogui
import threading
import datetime
import os

#TODO: the shutdown times should be in a list
#TODO: config files with schedule times, temeouts, etc.
now = datetime.datetime.now()
shd_time = datetime.datetime(now.year, now.month, now.day, 13,23,0)
wrn_time = shd_time - datetime.timedelta(minutes=1)
debug = 1

polling = 5 #the timers will poll every x sencds.

def shutdownTimer():    
    if(debug == 1): print("Shutting down!")
    #os.system('systemctl poweroff')     

def warningTimer(): 
    if(debug == 1): print("Warning timer rised up, setting up the shutdown timer:", end=' ')         

    wait = (shd_time - datetime.datetime.now()).total_seconds()
    shd_timer = threading.Timer(wait, shutdownTimer)  
    shd_timer.start()     
    
    if(debug == 1): 
        print("OK")
        print("     The shutdown event has been scheduled to rise up at %s" % shd_time.strftime('%H:%M'), end='\n\n')

    action = pyautogui.confirm(text='Aquest ordinador s''apagarà automàticament a les %s' % shd_time.strftime('%H:%M'), title='Apagada automàtica', buttons=['Anul·la l''apagada automàtica'])  # returns "OK" or "Cancel"        
    
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

    #wait till warning time
    if(debug == 1): print("Setting up the warning timer:", end=' ')
    if(wrn_time > datetime.datetime.now()):
        wait = (wrn_time - datetime.datetime.now()).total_seconds()
        wrn_timer = threading.Timer(wait, warningTimer)
        wrn_timer.start()    

        if(debug == 1): 
            print("OK")
            print("     The warning message has been scheduled to popup at %s" % wrn_time.strftime('%H:%M'), end='\n\n')
        
    elif(debug == 1): 
        print("ERROR")
        print("     The warning time has passed, so no automated shutdown will be scheduled.", end='\n\n')

    