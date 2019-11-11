import threading
from shared.popup import Popup

class ScheduleInfo():
    id = 0
    time = None
    popup = None
    timer = None

    def __init__(self, id:int, time:str, timer:threading.Timer, popup: Popup):
        self.id = id
        self.time = time
        self.popup = popup
        self.timer = timer