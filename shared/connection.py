import socket
from shared.config import Config

class Connection():

    SERVER:str = Config.SERVER
    PORT:str = Config.PORT
    TIMEOUT:int = Config.TIMEOUT

    @staticmethod
    def create():
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.bind((Connection.SERVER, Connection.PORT))
        sock.settimeout(Connection.TIMEOUT)
        return sock

    @staticmethod
    def join():
        sock = socket.create_connection((Connection.SERVER, Connection.PORT))
        sock.settimeout(Connection.TIMEOUT)
        return sock
