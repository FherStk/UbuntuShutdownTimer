import datetime

class Utils():

    @staticmethod
    def getSchedulableDateTime(str):
        now = datetime.datetime.now()
        dt = datetime.datetime.strptime(str, '%H:%M:%S').replace(year=now.year, month=now.month, day=now.day)
        if dt < datetime.datetime.now(): dt = dt + datetime.timedelta(days = 1)

        return dt