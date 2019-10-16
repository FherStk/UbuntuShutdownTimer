from enum import Enum

class Popup(Enum):
    ABORT = 2   #includes a button allowing to abort the shudown
    INFO=1      #only informative, no abort option
    SILENT=0    #no warning will be displayed