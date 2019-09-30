#source: https://www.devdungeon.com/content/dialog-boxes-python
#        https://www.geeksforgeeks.org/timer-objects-python/
#        https://realpython.com/intro-to-python-threading/

#dependencies: pip3 install pyautogui
#documentation: https://pyautogui.readthedocs.io/en/latest/

import pyautogui
import threading
import datetime
import time
import os

class Appointment:
  def __init__(self, time, popup):
    self.time = time
    self.popup = popup

debug = 1
SHUTDOWN_TIMES = [Appointment("14:55:00", True), Appointment("21:30:00", False)]
WARNING_BEFORE_SHUTDOWN = 1 #in minutes

def shutdown():    
    if(debug == 1): print("Shutting down!")
    #os.system('systemctl poweroff')     

def warningTimer(shd_time, popup): 
    if(debug == 1): print("Warning timer rised up, setting up the shutdown timer:")         

    wait = (shd_time - datetime.datetime.now()).total_seconds()
    shd_timer = threading.Timer(wait, shutdown)  
    shd_timer.start()     

    if(debug == 1): print("     The shutdown event has been scheduled to rise up at %s" % shd_time.strftime('%H:%M:%S'))
    if(not popup and debug == 1): print("     No warning popup will be displayed.", end='\n\n')
    elif(popup):
        if(debug == 1): print("     Displaying warning popup.")
        action = pyautogui.confirm(text='Aquest ordinador s''apagarà automàticament a les %s' % shd_time.strftime('%H:%M:%S'), title='Apagada automàtica', buttons=['Anul·la l''apagada automàtica'])  # returns "OK" or "Cancel"        

        if action != None:
            shd_timer.cancel()
            if(debug == 1): print("     The user decided to abort the scheduled shutdown event.", end='\n\n')         
            pyautogui.alert(text='Si us plau, recordi apagar l''ordinador manualment quan acabi de fer-lo servir. Gràcies. ', title='Recordatori', button='OK')                         

def main():
    if(debug == 1): 
        print("Ubuntu Shutdown Timer (v0.0.0.1)")
        print("Copyright (C) Fernando Porrino Serrano")
        print("Under the GNU General Public License v3.0")
        print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')        

    if(debug == 1): print("Setting up the warning timers:")
    now = datetime.datetime.now()
    
    for sdt in SHUTDOWN_TIMES:
        shd_time = datetime.datetime.strptime(sdt.time, '%H:%M:%S').replace(year=now.year, month=now.month, day=now.day)
        wrn_time = shd_time - datetime.timedelta(minutes = WARNING_BEFORE_SHUTDOWN)        

        #wait till warning time for each warning requested (a warning is a shutdown requested time - x minutes)        
        if(wrn_time > datetime.datetime.now()):
            if(debug == 1): print("     A new warning message has been scheduled to popup at %s" % wrn_time.strftime('%H:%M:%S'), end='\n\n')
            wait = (wrn_time - datetime.datetime.now()).total_seconds()
            time.sleep(wait)
            warningTimer(shd_time, sdt.popup)
            
        elif(debug == 1): 
            print("     A warning message has been ignored due its schedule time has passed at %s" % wrn_time.strftime('%H:%M:%S')) 

if __name__ == "__main__":
    main()