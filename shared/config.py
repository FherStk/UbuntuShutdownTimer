from shared.popup import Popup

class Config():
    SHUTDOWN_TIMES = [{"time": "16:00:00", "popup": Popup.ABORT}]
    WARNING_BEFORE_SHUTDOWN = 1 #in minutes    
    SERVER = "localhost"
    PORT = 65002