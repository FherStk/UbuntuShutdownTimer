from shared.popup import Popup
from shared.config import Config
from shared.scheduleInfo import ScheduleInfo
from shared.connection import Connection
from shared.utils import Utils
from socket import socket
import threading
import datetime
import sys
import os

#TODO:  write logs into a file
#       hide zenity warning if another user tooked action (and inform about it)

class Server():
    CONNECTIONS = [] #array of tuples (socket, str)
    SHUTDOWN:ScheduleInfo = None
    TEST = True #for developers test only

    def getOpenConnections(self):
        return list(filter(lambda x: x[0]._closed == False, self.CONNECTIONS))

    def shutdown(self):
        """
        Closes all the open connections and shuts down the computer.        
        """
        print("\nShutdown event started:")
        print("     Closing open connections...")
        
        
        cons = self.getOpenConnections()
        if(len(cons) == 0): print("\n     No clients connected.")
        else:
            for connection, client_address in cons:                
                print("         Closing open connection with client {}... ".format(client_address), end='')
                
                try:
                    connection.close()            
                    print("OK")
                except Exception as e:
                    print("EXCEPTION: {}.".format(e))

        print("\nShutting down!")
        if not self.TEST: os.system('systemctl poweroff')
        
        #TODO: use dbus for shutting down? its important to protect the apt upgrade process.
        #dbus-send --system --print-reply --dest=org.freedesktop.ConsoleKit /org/freedesktop/ConsoleKit/Manager org.freedesktop.ConsoleKit.Manager.Stop

    def refresh(self):
        """
        Sends a broadcast message to all the active connections, requesting them to ask for the next scheduled event.
        """

        print("\nSending the refresh broadcast message to all the clients:", end='')
        cons = self.getOpenConnections()
        if(len(cons) == 0): print("\n     No clients connected.")
        else:
            for (connection, client_address) in cons:
                #It could be more elegant to removed closed connections, but it will increase the logic complexity. Because a little amount of closed connections are expected (almost zero), those will remain in the list.          
                try:
                    connection.sendall(b"REFRESH")
                    print("\n     Message sent to {}.".format(client_address), end='')

                except Exception as e:
                    print("\n     Error sending message to {}. EXCEPTION: {}.".format(client_address, e), end='')
            
            #For logging purposes
            print("")

    def listen(self, connection, client_address):
        """
        Continuously polls for data sent from the client using the given connection.
        It can be used to ask for the shutdown times or for shutdown abort requests.    

        Keyword arguments:
        connection      --- The current connection to listen.
        client_address  --- The current connection's client's address (for log purposes only).
        """

        print("\n     {} - Reading received data.".format(client_address), end='')

        while not connection._closed:
            try:
                data = connection.recv(1024).decode("ascii")  

                if data:
                    if(data == "INFO"):
                        print("\n     {} - Shutdown info requested, sending back:".format(client_address), end='')
                        print("\n           ID:    {}".format(self.SHUTDOWN.id, Utils.TIMEFORMAT), end='')
                        print("\n           Time:  {}".format(Utils.dateTimeToStr(self.SHUTDOWN.time, Utils.DATETIMEFORMAT)), end='')
                        print("\n           Popup: {}".format(self.SHUTDOWN.popup, Utils.TIMEFORMAT), end='')

                        info = "{}#{}#{}".format(self.SHUTDOWN.id, Utils.dateTimeToStr(self.SHUTDOWN.time, Utils.DATETIMEFORMAT), self.SHUTDOWN.popup)
                        connection.sendall(info.encode("ascii"))                    

                    elif(data.startswith == "ABORT"): 
                        id = data.split("#")[1]                       
                        print("\n     {} - Abort requested for the ID={}:".format(client_address, id), end='')

                        if(id != self.SHUTDOWN.id): print("\n       Unable to abort, ID missmatch ({} != {}).".format(id, self.SHUTDOWN.id), end='')
                        else:
                            self.SHUTDOWN.timer.cancel()
                            print("\n       The abort event has been aborted.", end='')

                    else:
                        print("\n     {} - Unexpected message received: {!r}".format(client_address, data), end='')
            
            except Exception as e:
                #Catching different exceptions does not work...
                if (e.args[0] != "timed out"): 
                    print("\n     {} - EXCEPTION: {}".format(client_address, e), end='')
                    
                    if(e.errno == 10054):
                        #Connection closed by the client
                        connection.close()

    def schedule(self, schedule_idx=0):
        """
        Schedules a new shutdown timer, using the schedule time that has been set in the config.py file.
        The schedule_idx arguments defines wich item must be used (overflows will start again from the begining).

        The scheduled info will be stored into the SHUTDOWN:ScheduleInfo global var.

        Keyword arguments:
        schedule_idx    --- The item that will be used for schedulling a new shutdown event (default 0).
                            The list containing these definition can be found in the shared/config.py file.        
        """ 

        print("\nSetting up the next shutdown event:", end='')
        
        schedule_idx = schedule_idx % len(Config.SHUTDOWN_TIMES)
        sdt = Config.SHUTDOWN_TIMES[schedule_idx]                

        shd_time = Utils.getSchedulableDateTime(sdt["time"])
        if self.TEST: shd_time = datetime.datetime.now() + datetime.timedelta(minutes = 1)
        
        shd_timer = threading.Timer((shd_time - datetime.datetime.now()).total_seconds(), self.shutdown)  
        shd_timer.start()

        print("\n     The shutdown has been scheduled on {}".format(Utils.dateTimeToStr(shd_time, Utils.DATETIMEFORMAT)))

        self.SHUTDOWN = ScheduleInfo(schedule_idx+1, shd_time, shd_timer, sdt["popup"])
        
    def nearest_schedule_idx(self):
        min = None
        idx = 0
        schedule_idx = 0        
        now = datetime.datetime.now()

        for x in Config.SHUTDOWN_TIMES:
            time = Utils.getSchedulableDateTime(x["time"])
            secs = (time - now).total_seconds()

            if(min == None or (secs > 0 and secs < min)):
                min = secs
                idx = schedule_idx
            
            schedule_idx += 1

        return idx

    def handle_connections(self, sock):  
        """
        Each time a new connectin is requested through the socket, a new listen thread is started.

        Keyword arguments:
            sock --- The socket assigned for listening new connections.
        """       
        #The first schedule_idx will be the nearest in time.
        schedule_idx = self.nearest_schedule_idx()

        #Step 1: schedule next shutdown.
        #Step 2: broadcast a refresh message for all connected clients (0 on firs loop)
        #Step 3: listen for connections forever.        
        #Step 4: a new connected client or one who received the refresh message will ask for the next scheduled shutdown time & popup type
        #Step 5: when a a client requests to abort the scheduled shutdown
            #Step 5.1: abort        
            #Step 5.2: return to step 1 

        while True:
            self.schedule(schedule_idx)        
            self.refresh()
            schedule_idx += 1
            
            #Please note the breakline at the beginning, this is done on purpose because the multihreading (otherwise different messages can appear on the same line).
            cons = self.getOpenConnections()
            if(len(cons) == 0):
                print("\nWaiting for connections:", end='')

            while self.SHUTDOWN.timer.isAlive():                
                try:
                    #TODO: sock.timeout not working?
                    connection, client_address = sock.accept()

                    print("\n     {} - New connection stablished.".format(client_address), end='')
                    self.CONNECTIONS.append((connection, client_address))         

                    print("\n     {} - Starting a new listening thread.".format(client_address), end='')
                    thread = threading.Thread(target=self.listen, args=(connection, client_address))
                    thread.start()

                except Exception as e:
                    #Catching different exceptions does not work...
                    if (e.args[0] != "timed out"): print("\nEXCEPTION: {}".format(e), end='')     
                
            #For logging purposes
            print("")

    def start(self):    
        """
        Starts the server:

        #Step 1: schedule next shutdown.
        #Step 2: broadcast a refresh message for all connected clients (0 on firs loop)
        #Step 3: listen for connections forever.        
        #Step 4: a new connected client or one who received the refresh message will ask for the next scheduled shutdown time & popup type
        #Step 5: when a a client requests to abort the scheduled shutdown
            #Step 5.1: abort        
            #Step 5.2: return to step 1 
        """

        print("Ubuntu Shutdown Timer (SERVER) - v1.0.0.0")
        print("Copyright (C) Fernando Porrino Serrano")
        print("Under the GNU General Public License v3.0")
        print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')   
            
        #TODO:  use classes for SRV and CLI in order to use global vars and simplify some arguments. It will also simplify the testing

        print("Starting server:")    
        sock = Connection.create()
        sock.listen(0)
        print("     Server ready and listening on {} port {}".format(Connection.SERVER, Connection.PORT))    
        
        try:
            self.handle_connections(sock)

        except Exception as e:
            print("     EXCEPTION: {}.".format(e))

        finally:        
            sock.close()
            print("     Server stopped")

if __name__ == "__main__":
    srv = Server()
    srv.start()