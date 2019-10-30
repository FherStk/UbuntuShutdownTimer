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

class Server():
    CONNECTIONS = [] #array of tuples (socket, str)
    SHUTDOWN:ScheduleInfo = None

    def shutdown(self):
        """
        Closes all the open connections and shuts down the computer.        
        """

        print("\nClosing open connections...")
        for connection, client_address in self.CONNECTIONS:
            print("     Closing open connection with client {}... ".format(client_address), end='')
            
            try:
                connection.close()            
                print("OK")
            except Exception as e:
                print("     EXCEPTION: {}.".format(e))

        print("\nShutting down!")
        #os.system('systemctl poweroff')

    def refresh(self):
        """
        Sends a broadcast message to all the active connections, requesting them to ask for the next scheduled event.
        """

        print("\nSending the refresh broadcast message to all the clients:")
        for (connection, client_address) in self.CONNECTIONS:
            print("     Sending message to {}... ".format(client_address), end='')
            
            try:
                connection.sendall(b"REFRESH")
                print("OK")
            except Exception as e:
                print("     EXCEPTION: {}.".format(e))

    def listen(self, connection, client_address):
        """
        Continuously polls for data sent from the client using the given connection.
        It can be used to ask for the shutdown times or for shutdown abort requests.    

        Keyword arguments:
        connection      --- The current connection to listen.
        client_address  --- The current connection's client's address (for log purposes only).
        """

        print("     {} - Reading received data.".format(client_address))

        while True:
            data = connection.recv(1024)  

            if data:
                if(data == b"TIME"):
                    print("     {} -  Shutdown time requested, sending back the current scheduled shutdown time.".format(client_address))
                    connection.sendall(Utils.dateTimeToStr(self.SHUTDOWN.time).encode("ascii"))

                if(data == b"POPUP"):
                    print("     {} -  Popup type requested, sending back the current warning popup type.".format(client_address))
                    connection.sendall("{}".format(self.SHUTDOWN.popup).encode("ascii"))

                elif(data == b"ABORT"):
                    print("     {} - Abort requested, so the shutdown event will be aborted.".format(client_address))
                    self.SHUTDOWN.timer.cancel()

                else:
                    print("     {} - Unexpected message received: {!r}".format(client_address, data))

    def schedule(self, schedule_idx=0):
        """
        Schedules a new shutdown timer, using the schedule time that has been set in the config.py file.
        The schedule_idx arguments defines wich item must be used (overflows will start again from the begining).

        The scheduled info will be stored into the SHUTDOWN:ScheduleInfo global var.

        Keyword arguments:
        schedule_idx    --- The item that will be used for schedulling a new shutdown event (default 0).
                            The list containing these definition can be found in the shared/config.py file.        
        """ 

        print("Setting up the shutdown event:", end='')
        
        #Next 2 lines for testing only purposes
        #now = datetime.datetime.now()
        #shd_time = now + datetime.timedelta(minutes = Config.WARNING_BEFORE_SHUTDOWN)
        schedule_idx = schedule_idx % len(Config.SHUTDOWN_TIMES)
        sdt = Config.SHUTDOWN_TIMES[schedule_idx]    
        shd_time = Utils.getSchedulableDateTime(sdt["time"])
        shd_timer = threading.Timer((shd_time - datetime.datetime.now()).total_seconds(), self.shutdown)  
        shd_timer.start()

        print(" OK")
        print("     The shutdown has been scheduled on {}".format(Utils.dateTimeToStr(shd_time, Utils.TIMEFORMAT)), end='\n\n')

        self.SHUTDOWN = ScheduleInfo(shd_time, shd_timer, sdt["popup"])
        

    def handle_connections(self, sock):  
        """
        Each time a new connectin is requested through the socket, a new listen thread is started.

        Keyword arguments:
            sock --- The socket assigned for listening new connections.
        """       
        schedule_idx = -1

        #Step 1: schedule next shutdown.
        #Step 2: broadcast a refresh message for all connected clients (0 on firs loop)
        #Step 3: listen for connections forever.        
        #Step 4: a new connected client or one who received the refresh message will ask for the next scheduled shutdown time & popup type
        #Step 5: when a a client requests to abort the scheduled shutdown
            #Step 5.1: abort        
            #Step 5.2: return to step 1 

        while True:
            self.schedule(schedule_idx+1)        
            self.refresh()

            print("Waiting for connections:")
            while self.SHUTDOWN.timer.isAlive():
                connection, client_address = sock.accept()

                print("     {} - New connection stablished.".format(client_address))
                self.CONNECTIONS.append((connection, client_address))         

                print("     {} - Starting a new listening thread.".format(client_address))
                thread = threading.Thread(target=self.listen, args=(connection, client_address))
                thread.start()

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

        print("Ubuntu Shutdown Timer (SERVER) - v0.2.0.0")
        print("Copyright (C) Fernando Porrino Serrano")
        print("Under the GNU General Public License v3.0")
        print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')   
            
        #TODO:  use classes for SRV and CLI in order to use global vars and simplify some arguments. It will also simplify the testing

        print("Starting server:", end='')    
        sock = Connection.create()
        sock.listen(0)
        print(" OK")
        print("     Starting ready and listening on {} port {}".format(Connection.SERVER, Connection.PORT), end='\n\n')    
        
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