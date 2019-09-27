#source: https://www.devdungeon.com/content/dialog-boxes-python
#        https://www.geeksforgeeks.org/timer-objects-python/

#dependencies: pip3 install pyautogui
#documentation: https://pyautogui.readthedocs.io/en/latest/

import pyautogui
import threading
import datetime

def chek(): 
    now = datetime.datetime.now()
    if now.hour == 16 and now.minute == 0:
        print("Shut down! \n") 
    else:            
        timer = threading.Timer(60.0, chek) #every minute
        timer.start()     

chek()
#action = pyautogui.confirm(text='Aquest ordinador s''apagarà automàticament en 5 minuts', title='Apagada automàtica', buttons=['Anul·la l''apagada automàtica'])  # returns "OK" or "Cancel"
#
#if action != None:
#    print('Abort')