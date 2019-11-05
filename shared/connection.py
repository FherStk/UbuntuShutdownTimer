import socket
from shared.config import Config

class Connection():

    SERVER:str = Config.SERVER
    PORT:str = Config.PORT

    @staticmethod
    def create():
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.bind((Config.SERVER, Config.PORT))
        sock.settimeout(5)
        return sock

    @staticmethod
    def join():
        return socket.create_connection((Config.SERVER, Config.PORT))
