from shared.popup import Popup
from shared.config import Config
from shared.connection import Connection
from shared.utils import Utils
from shared.scheduleInfo import ScheduleInfo
from socket import socket
import subprocess
import threading
import datetime
import time
import sys

class Client:
    CONNECTION:socket = None
    WARNING:ScheduleInfo = None
    PROCESS:subprocess.Popen = None

    def abort(self):    
        """
        Requests the user to abort the scheduled shutdown event.
        """

        try:
            # Send data        
            print("     Sending the ABORT request to the server... ", end='')
            self.CONNECTION.sendall("ABORT#{}".format(self.WARNING.id).encode("ascii"))
            print("OK")
                
        except Exception as e:
            self.handleException(e)

        finally:
            print("")

    def warning(self):
        """
        Displays the warning popup to the user, allowing him/her to abort the scheduled shutdown (this depends of the shutdown event type).

        """

        print("Rising the warning event: ")

        if(self.WARNING.popup == Popup.SILENT): print("     No warning message will be prompted so the shutdown event will raise on silent mode.", end='\n\n')
        else:
            print("     Displaying the warning popup, so the user will be able to abort the shutdown on demand (or only warned about it).")
            text = "Aquest ordinador s'apagarà automàticament a les <b>{}</b>.".format(Utils.dateTimeToStr(self.WARNING.time, Utils.TIMEFORMAT))
            noOutput = ">/dev/null 2>&1"

            if(self.WARNING.popup == Popup.INFO): 
                #action = os.system('zenity --notification --no-wrap --text="{}" {}', text, noOutput)
                subprocess.Popen(["zenity", "--notification", "--text={}".format(text)])
            else:
                #action = os.system('zenity --question --no-wrap --text="{}" {}'.format(text + " \nDesitja anul·lar l'aturada automàtica?", noOutput))            
                if(self.PROCESS != None): self.PROCESS.kill()                
                self.PROCESS = subprocess.Popen(["zenity", "--question", "--no-wrap", "--text=\n{}\nDesitja anul·lar l'aturada automàtica?".format(text)], stderr=subprocess.PIPE)
                self.PROCESS.wait()                                
                #self.listen(proc) #hides the dialog if cancelled by another user                                

                if self.PROCESS.returncode == 1: 
                    print("     The user decided to continue with the scheduled shutdown event.", end='\n\n')                                                             
                    subprocess.Popen(["zenity", "--notification", "--text=Apagada automàtica a les {}".format(Utils.dateTimeToStr(self.WARNING.time, Utils.TIMEFORMAT))])                    

                else:                
                    print("     The user decided to abort the scheduled shutdown event.")         
                    #os.system('zenity --notification --text="{}" {}'.format("Recordi apagar l'ordinador manualment. Gràcies.", noOutput))
                    subprocess.Popen(["zenity", "--notification", "--text=Recordi apagar l'ordinador manualment. Gràcies"])
                    self.abort()

    def requestInfo(self):  
        """
        Requests to the server for the current scheduled shutdown info (time and type).
        Return:
            0 if ok (so the global var WARNING will contain the correct info).
            An integer greater than zero if not OK, it can be used in order to wait (using it as seconds)
        """

        try: 
            print("     Requesting for the next shutdown info: ")
            self.CONNECTION.sendall(b"INFO")     
            info = (self.CONNECTION.recv(1024).decode("ascii")).split("#")                        

            #Must avoid setting up repeated info (when continuing with an scheduled shutdown)
            if(self.WARNING == None or self.WARNING.id != info[0]): 
                print("           ID:    {}".format(info[0]))
                print("           Time:  {}".format(info[1]))
                print("           Popup: {}".format(info[2]), end='\n\n')

                self.WARNING = ScheduleInfo(info[0], Utils.strToDateTime(info[1], Utils.DATETIMEFORMAT), None, info[2])
                return 0

            elif(self.WARNING != None and self.WARNING.id == info[0]): 
                print("     The same shutdown info as the current one has been received.")
                return (self.WARNING.time - datetime.datetime.now()).total_seconds()            

        except Exception as e:
            self.handleException(e)            
            self.WARNING = None

            return Config.TIMEOUT

    def setupWarning(self):
        """
        Schedules the warning popup in order to prompt to the user a message with information about the next shutdown.
        """

        print("Setting up the warning event:", end='')

        wrn_time = self.WARNING.time - datetime.timedelta(minutes = Config.WARNING_BEFORE_SHUTDOWN)
        self.WARNING.timer = threading.Timer((wrn_time - datetime.datetime.now()).total_seconds(), self.warning)  
        self.WARNING.timer.start()
        
        print(" OK")      
        print("     The warning popup has been scheduled on {}".format(Utils.dateTimeToStr(wrn_time, Utils.TIMEFORMAT)), end='\n\n')                     

    def listen(self):  
        """
        Continuously polls for data sent from the server using the given connection.
        It can be used to receive refresh events (next shutdown must be shceduled.)
        """

        print("Listening for server messages:")

        while self.WARNING.timer.is_alive():
            try:
                data = self.CONNECTION.recv(1024)             
                if(data == b"REFRESH"):
                    print("     The server requested for a REFRESH:")
                    print("         Cancelling the warning event...", end='')
                    self.WARNING.timer.cancel()
                    print("OK")                                    
                    
                    if(self.PROCESS != None): 
                        print("         Hiding the dialog...", end='')
                        self.PROCESS.kill()
                        print("OK")                

                else:
                    print("     Unexpected message received from the server: {!r}".format(data))          

            except Exception as e:
                #Catching different exceptions does not work...
                if (e.args[0] != "timed out"): 
                    self.handleException(e, "         ")                    

    def handleException(self, e:Exception, prefix:str = ""):
        print("{}EXCEPTION: {}".format(prefix, e))

        try:
            if(e.errno == 10054 or e.errno == 32):
                #Connection closed by the client
                self.WARNING.timer.cancel()
                self.CONNECTION.close() 
        except:
            #do nothing
            return

    def start(self):   
        """
        Starts the client:

        #Step 1: connect with the server
        #Step 2: request for the next scheduled shutdown time
        #Step 3: schedule the related warning event
        #Step 4: listen for messages from the server
            #Step 4.1: REFRESH: abort the current warning event and return to Step 2
        #Step 5: the warning event rised up and the user selected the option to abort
            #Step 5.1: ABORT: send the request to the server
        """

        print("Ubuntu Shutdown Timer (CLIENT) - v1.0.0.0")
        print("Copyright (C) Fernando Porrino Serrano")
        print("Under the GNU General Public License v3.0")
        print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')          

        print("Connecting to the server on {} port {}:".format(Connection.SERVER, Connection.PORT))
        self.CONNECTION = Connection.join()
        self.WARNING = ScheduleInfo(0, None, None, None)
        self.PROCESS = None

        #listen has a loop inside and will remain looping till wrn_timer has been cancelled
        while not self.CONNECTION._closed:          
            t = self.requestInfo()     

            if(t > 0): 
                print("     Waiting for {} senconds till next request...".format(int(t)), end="\n\n")
                time.sleep(t)   
            else:
                self.setupWarning()    
                self.listen()

if __name__ == "__main__":
    c = Client()
    c.start()