import datetime

class Utils():
    DATETIMEFORMAT = "%Y/%m/%d %H:%M:%S"
    TIMEFORMAT = "%H:%M:%S"

    @staticmethod
    def getSchedulableDateTime(txt):
        now = datetime.datetime.now()
        dt = Utils.strToDateTime(txt, Utils.TIMEFORMAT)
        
        dt = dt.replace(year=now.year, month=now.month, day=now.day)
        if dt < datetime.datetime.now(): dt = dt + datetime.timedelta(days = 1)

        return dt

    @staticmethod
    def strToDateTime(txt, format=DATETIMEFORMAT):        
        dt = datetime.datetime.strptime(txt, format)        
        return dt

    @staticmethod
    def dateTimeToStr(dt, format=DATETIMEFORMAT):        
        txt = dt.strftime(format)
        return txt