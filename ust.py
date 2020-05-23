#!/usr/bin/env python3
#TODO: install dependencies automatically

from gi.repository import GObject
import dbus
import dbus.service

from dbus.mainloop.glib import DBusGMainLoop
DBusGMainLoop(set_as_default=True)


OPATH = "/com/example/HelloWorld"
IFACE = "com.example.HelloWorld"
BUS_NAME = "com.example.HelloWorld"

#check /etc/dbus-1/system-local.conf  try to allow only the necessary

# <!DOCTYPE busconfig PUBLIC
# "-//freedesktop//DTD D-Bus Bus Configuration 1.0//EN"
# "http://www.freedesktop.org/standards/dbus/1.0/busconfig.dtd">
# <busconfig>
#     <policy user="root">  #is this needed with python? was created for C#
#         <allow eavesdrop="true"/>
#         <allow eavesdrop="true" send_destination="*"/>
#     </policy>
#     <policy context="default">
#         <allow own="com.example.HelloWorld"/>
#         <allow send_destination="com.example.HelloWorld"/>
#         <allow receive_sender="com.example.HelloWorld"/>
#     </policy>
# </busconfig>

#dbus-send --system --print-reply --type=method_call --dest=com.example.HelloWorld /com/example/HelloWorld com.example.HelloWorld.SayHello.SayHello

class Example(dbus.service.Object):
    def __init__(self):
        #bus = dbus.SessionBus()
        bus = dbus.SystemBus()
        bus.request_name(BUS_NAME)
        bus_name = dbus.service.BusName(BUS_NAME, bus=bus)
        dbus.service.Object.__init__(self, bus_name, OPATH)

    @dbus.service.method(dbus_interface=IFACE + ".SayHello", in_signature="", out_signature="")
    def SayHello(self):
        print ("hello, world")


if __name__ == "__main__":
    a = Example()
    loop = GObject.MainLoop()
    loop.run()