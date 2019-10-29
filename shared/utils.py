from shared.config import Config
import datetime

class Utils():
    DATETIMEFORMAT = "%Y/%m/%d %H:%M:%S"
    TIMEFORMAT = "%H:%M:%S"

    @staticmethod
    def getSchedulableDateTime(str):
        now = datetime.datetime.now()
        dt = Utils.strToDateTime(str, Utils.TIMEFORMAT)
        
        dt = dt.replace(year=now.year, month=now.month, day=now.day)
        if dt < datetime.datetime.now(): dt = dt + datetime.timedelta(days = 1)

        return dt

    @staticmethod
    def strToDateTime(str, format=Utils.DATETIMEFORMAT):        
        dt = datetime.datetime.strptime(str, format)        
        return dt

    @staticmethod
    def dateTimeToStr(dt, format=Utils.DATETIMEFORMAT):        
        dt = dt.strftime(format)
        return dt