import threading
from shared.popup import Popup

class ScheduleInfo():
    time = None
    popup = None
    timer = None

    def __init__(self, time:str, timer:threading.Timer, popup: Popup):
        self.time = time
        self.popup = popup
        self.timer = timer