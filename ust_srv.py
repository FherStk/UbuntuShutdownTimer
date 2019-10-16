

from src.popup import Popup
from src.config import Config
from src.connection import Connection
import threading
import datetime
import sys

#TODO:  Share socket creation code
#       Load config from a file

def shutdown():
    print("\nShutting down!")
    #os.system('systemctl poweroff')

def listen(connection, client_address, shutdown_timer):
    print("     {} - Reading received data.".format(client_address))

    while True:
        data = connection.recv(1024)  

        if data:
            if(data == b"ABORT"):
                print("     {} - Abort requested, so the shutdown event will be aborted.".format(client_address))
                shutdown_timer.cancel()
                connection.sendall(b"ACK")

            else:
                print("     {} - Unexpected message received: {!r}".format(client_address, data))
                connection.sendall(b"NACK")

        else:
            print("     {} - No data received, closing the connection.".format(client_address), end='\n\n')
            connection.close()
            break

def main():    
    print("Ubuntu Shutdown Timer (SERVER) - v0.0.0.2")
    print("Copyright (C) Fernando Porrino Serrano")
    print("Under the GNU General Public License v3.0")
    print("https://github.com/FherStk/UbuntuShutdownTimer", end='\n\n')   

    print("Setting up the shutdown event:", end='')
    sdt = Config.SHUTDOWN_TIMES[0]
    now = datetime.datetime.now()
    #TODO: once a shutdown has been cancelled, automatically schedule de next-one...

    shd_time = now + datetime.timedelta(seconds = 15)
    #shd_time = datetime.datetime.strptime(sdt["time"], '%H:%M:%S').replace(year=now.year, month=now.month, day=now.day)
    shd_timer = threading.Timer((shd_time - datetime.datetime.now()).total_seconds(), shutdown)  
    shd_timer.start()
    print(" OK")
    print("     The shutdown has been scheduled on {}".format(shd_time.strftime('%H:%M:%S')), end='\n\n')

    print("Starting server:", end='')    
    sock = Connection.create()
    sock.listen(0)
    print(" OK")
    print("     Starting ready and listening on {} port {}".format(Connection.SERVER, Connection.PORT), end='\n\n')    

    print("Waiting for connections:")
    try:
        while True:
            connection, client_address = sock.accept()

            print("     {} - New connection received.".format(client_address))
            thread = threading.Thread(target=listen, args=(connection, client_address, shd_timer))
            thread.start()

    except Exception as e:
        print("     EXCEPTION: {}.".format(e))

    finally:
        sock.close()

if __name__ == "__main__":
    main()