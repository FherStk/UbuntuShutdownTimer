import socket
from src.config import Config

class Connection():

    SERVER = Config.SERVER
    PORT = Config.PORT

    @staticmethod
    def create():
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.bind((Config.SERVER, Config.PORT))
        return sock

    @staticmethod
    def join():
        return socket.create_connection((Config.SERVER, Config.PORT))
