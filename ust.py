#source: https://www.devdungeon.com/content/dialog-boxes-python
#        https://www.geeksforgeeks.org/timer-objects-pythonç/
#        https://realpython.com/intro-to-python-threading/

#dependencies: pip3 install pyautogui
#documentation: https://pyautogui.readthedocs.io/en/latest/

import pyautogui
import threading
import datetime

#TODO: the shutdown times should be in a list
#TODO: config files with schedule times, temeouts, etc.
now = datetime.datetime.now()
shd_time = datetime.datetime(now.year, now.month, now.day, 17,26,0)
wrn_time = shd_time - datetime.timedelta(minutes=5)
polling = 5 #the timers will poll every x sencds.

shd_thread = None
wrn_thread = None

def shutdownTimer():
    now = datetime.datetime.now()
    shd_timer = threading.Timer(polling, shutdownTimer) #every minute

    if now.hour == shd_time.hour and now.minute == shd_time.minute:        
        wrn_thread._stop()
        print("Shutting down! \n") 

    else:            
       shd_timer.start()   


def warningTimer(): 
    now = datetime.datetime.now()
    wrn_timer = threading.Timer(polling, warningTimer) #every minute

    if now >= wrn_time and now <= shd_time:
        action = pyautogui.confirm(text='Aquest ordinador s''apagarà automàticament a les %s' % shd_time.strftime('%H:%M'), title='Apagada automàtica', buttons=['Anul·la l''apagada automàtica'])  # returns "OK" or "Cancel"
        
        if action != None:
            pyautogui.alert(text='Si us plau, recordi apagar l''ordinador manualment quan acabi de fer-lo servir. Gràcies. ', title='', button='OK')
            shd_thread._stop()

    else:            
        wrn_timer.start()       

if __name__ == "__main__":
    #TODO: send the shutdown time to the threads, once finished load the next one
    shd_thread = threading.Thread(target=shutdownTimer)
    shd_thread.start()

    wrn_thread = threading.Thread(target=warningTimer)    
    wrn_thread.start()

    