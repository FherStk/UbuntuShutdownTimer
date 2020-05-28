/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses: 
      - Tmds.DBus by Tom Deseyn: under the MIT License (https://www.nuget.org/packages/Tmds.DBus/)

    This file is part of Ubuntu Shutdown Timer (UST from now on).

    UST is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    UST is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with UST.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Xml;
using Tmds.DBus;
using DBus.DBus;

namespace UST
{ 
    public class Installer{
        private string _dbusFile = "system-local.conf";
        private string _dbusFolder = "/etc/dbus-1";
        private string _notifierFile = "notify.sh";
        private string _clientFile = "ust-client.sh";
        private string _clientFolder = "/etc/profile.d/";
        private string _serverFile = "ust-server.service";
        private string _serverFolder = "/lib/systemd/system/";
  
        public void Install(){
            Console.WriteLine("Installation requested: ");
            InstallDbusPolicies();
            Console.WriteLine();

            InstallingServerService();
            Console.WriteLine();

            InstallingClientApp();
            Console.WriteLine();

            ReloadDbusConfig();            
        }
        
         public void Uninstall(){
            Console.WriteLine("Uninstallation requested: ");
            UninstallDbusPolicies();
            Console.WriteLine();

            UninstallingServerService();
            Console.WriteLine();

            UninstallingClientApp();
            Console.WriteLine();

            ReloadDbusConfig();            
        }

        private void InstallDbusPolicies(){
            var dest = Path.Combine(_dbusFolder, _dbusFile);

            Console.WriteLine("  Setting up the D-Bus policies ({0}):", dest);            
            if(!File.Exists(dest)){
                Console.Write("    Creating file.......................");
                File.Copy(Utils.GetFilePath(_dbusFile), dest);
                Console.WriteLine("OK");
            }
            else{
                Console.Write("    Updating entries....................");
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(dest);

                var root = "busconfig";
                var nodeName = "policy";
                var attrName = "context";
                var attrValue = "default";
                
                var def = doc.DocumentElement.SelectSingleNode($"/{root}/{nodeName}[@{attrName}='{attrValue}']");
                if(def == null) def = CreateXmlNode(doc, doc.DocumentElement.SelectSingleNode($"/{root}"), nodeName, attrName, attrValue);
                
                nodeName = "allow";
                attrName = "own";
                attrValue = "net.xeill.elpuig.UST1";
                var allow = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                if(allow == null) CreateXmlNode(doc, def, nodeName, attrName, attrValue);

                attrName = "send_destination";
                var send = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                if(send == null) CreateXmlNode(doc, def, nodeName, attrName, attrValue);

                attrName = "receive_sender";
                var receive = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                if(receive == null) CreateXmlNode(doc, def, nodeName, attrName, attrValue);

                doc.Save(dest);
                Console.WriteLine("OK");
            } 
            Console.WriteLine("    Done!");
        }

        private void UninstallDbusPolicies(){
            var dest = Path.Combine(_dbusFolder, _dbusFile);

            Console.WriteLine("  Removing the D-Bus policies ({0}):", dest);            
            if(File.Exists(dest)){                               
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(dest);

                var root = "busconfig";
                var nodeName = "policy";
                var attrName = "context";
                var attrValue = "default";
                
                Console.Write("    Removing entries....................");            
                var def = doc.DocumentElement.SelectSingleNode($"/{root}/{nodeName}[@{attrName}='{attrValue}']");
                if(def != null){
                    nodeName = "allow";
                    attrName = "own";
                    attrValue = "net.xeill.elpuig.UST1";
                    var allow = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                    if(allow != null) def.RemoveChild(allow);
 
                    attrName = "send_destination";
                    var send = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                    if(send != null) def.RemoveChild(send);

                    attrName = "receive_sender";
                    var receive = def.SelectSingleNode($"{nodeName} [@{attrName}='{attrValue}']");
                    if(receive != null) def.RemoveChild(receive);

                    doc.Save(dest);                    
                }
                Console.WriteLine("OK");
            } 
            Console.WriteLine("    Done!");
        }

        private void InstallingServerService(){
            var dest = Path.Combine(_serverFolder, _serverFile);
                        
            if(File.Exists(dest)) UninstallingServerService();
            Console.WriteLine();
            
            Console.WriteLine($"  Setting up the server service ({dest}):");
            Console.Write("    Creating the new service............");
            File.WriteAllText(dest, String.Format(File.ReadAllText(Utils.GetFilePath(_serverFile)), AppContext.BaseDirectory));               
            Console.WriteLine("OK");

            Console.Write("    Reloading the services daemon.......");
            Utils.RunShellCommand("sudo systemctl daemon-reload", true);
            Console.WriteLine("OK");

            Console.Write("    Enabling the service................");
            Utils.RunShellCommand($"sudo systemctl enable {_serverFile}", true);
            Console.WriteLine("OK");

            Console.Write("    Starting the service................");
            Utils.RunShellCommand($"sudo systemctl start {_serverFile}", true);
            Console.WriteLine("OK"); 
            Console.WriteLine("    Done!");           
        }

        private void UninstallingServerService(){
            var dest = Path.Combine(_serverFolder, _serverFile);
            
            Console.WriteLine($"  Removing the server service ({dest}):");
            if(File.Exists(dest)){                      
                Console.Write("    Stopping the service................");
                Utils.RunShellCommand($"sudo systemctl stop {_serverFile}", true);
                Console.WriteLine("OK");

                Console.Write("    Disabling the service...............");
                Utils.RunShellCommand($"sudo systemctl disable {_serverFile}", true);
                Console.WriteLine("OK");

                Console.Write("    Removing the service................");
                File.Delete(dest);
                Console.WriteLine("OK");

                Console.Write("    Reloading the services daemon.......");
                Utils.RunShellCommand("sudo systemctl daemon-reload", true);
                Console.WriteLine("OK");
            } 
            Console.WriteLine("    Done!");          
        }

        private void InstallingClientApp(){
            var dest = Path.Combine(_clientFolder, _clientFile);
                            
            if(File.Exists(dest)) UninstallingClientApp();
            Console.WriteLine();
            
            Console.WriteLine("  Setting up the client application ({0}): ", dest);
            Console.Write("    Creating logon launcher.............");
            File.WriteAllText(dest, String.Format(File.ReadAllText(Utils.GetFilePath(_clientFile)), AppContext.BaseDirectory));               
            Console.WriteLine("OK");

            Console.Write("    Setting permissions.................");
            Utils.RunShellCommand($"chmod +x {Utils.GetFilePath(_notifierFile)}", true);
            Console.WriteLine("OK");

            Console.WriteLine("    Done!");
        }
        
        private void UninstallingClientApp(){
            var dest = Path.Combine(_clientFolder, _clientFile);
                
            Console.WriteLine("  Removing the client application ({0}): ", dest);
            if(File.Exists(dest)){                 
                //TODO: find a way to stop all clients.
                // Console.WriteLine("    Stopping the application... ");
                // RunShellCommand("sudo systemctl stop ust.service");
                // Console.WriteLine("OK");

                Console.Write("    Removing application launcher.......");
                File.Delete(dest);
                Console.WriteLine("OK");
            }
            Console.WriteLine("    Done!");
        }

        private void ReloadDbusConfig(){      
            /*
                http://manpages.ubuntu.com/manpages/bionic/man1/dbus-daemon.1.html

                SIGHUP will cause the D-Bus daemon to PARTIALLY reload its configuration file and to flush
                its user/group information caches. Some configuration changes would require kicking all
                apps off the bus; so they will only take effect if you restart the daemon. Policy changes
                should take effect with SIGHUP.
            */    
            Console.WriteLine("  Updating D-Bus: ");
            Console.Write("    Reloading configuration.............");
            var systemConnection = Connection.System;
            var dbusManager = systemConnection.CreateProxy<IDBus>("org.freedesktop.DBus", "/org/freedesktop/DBus");
            dbusManager.ReloadConfigAsync();
            Console.WriteLine("OK"); 

            Console.Write("    Reloading daemon....................");
            Utils.RunShellCommand("pkill -HUP dbus-daemon", true);
            Console.WriteLine("OK"); 
            
            Console.WriteLine("    Done!");
        }
        
        private XmlNode CreateXmlNode(XmlDocument doc, XmlNode parent, string nodeName, string attrName, string attrValue){
            var node = doc.CreateElement(nodeName);                    
            var attr = doc.CreateAttribute(attrName);
            attr.Value = attrValue;
            node.Attributes.Append(attr);
            parent.AppendChild(node);

            return node;
        }
    }
}