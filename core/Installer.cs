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
        private string _clientFile = "ust-client.sh";
        private string _clientFolder = "/etc/profile.d/";
        private string _serverFile = "ust-server.service";
        private string _serverFolder = "/lib/systemd/system/";
  
        public void Install(){
            Console.WriteLine("Installation requested: ", DateTime.Now.Year);
            InstallDbusPolicies();
            InstallingServerService();
            InstallingClientApp();
            ReloadDbusConfig();            
        }
        
         public void Uninstall(){
            Console.WriteLine("Uninstallation requested: ", DateTime.Now.Year);
            UninstallDbusPolicies();
            UninstallingServerService();
            UninstallingClientApp();
            ReloadDbusConfig();            
        }
        
        private void InstallDbusPolicies(){
            var source = Path.Combine(Utils.AppFolder, "files", _dbusFile);
            var dest = Path.Combine(_dbusFolder, _dbusFile);

            Console.WriteLine("  Setting up the dbus policies:");            
            if(!File.Exists(dest)){
                Console.Write("    Creating the file {0}... ", dest);
                File.Copy(source, dest);
                Console.WriteLine("OK");
            }
            else{
                Console.Write("    Updating the file {0}... ", dest);
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
        }

        private void UninstallDbusPolicies(){
            var source = Path.Combine(Utils.AppFolder, "files", _dbusFile);
            var dest = Path.Combine(_dbusFolder, _dbusFile);

            Console.WriteLine("  Removing the dbus policies:");            
            if(File.Exists(dest)){                               
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(dest);

                var root = "busconfig";
                var nodeName = "policy";
                var attrName = "context";
                var attrValue = "default";
                
                Console.WriteLine("  Removing the dbus entries... ");            
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
        }

        private void InstallingServerService(){
            var source = Path.Combine(Utils.AppFolder, "files", _serverFile);
            var dest = Path.Combine(_serverFolder, _serverFile);
                        
            if(File.Exists(dest)) UninstallingServerService();
            
            Console.WriteLine("  Setting up the server service:");
            Console.Write("    Creating the new service {0}... ", dest);
            File.WriteAllText(dest, String.Format(File.ReadAllText(source), Utils.AppFolder));               
            Console.WriteLine("OK");

            Console.WriteLine("    Reloading the services daemon... ");
            Utils.RunShellCommand("sudo systemctl daemon-reload");
            Console.WriteLine("OK");

            Console.WriteLine("    Enabling the service... ");
            Utils.RunShellCommand($"sudo systemctl enable {_serverFile}");
            Console.WriteLine("OK");

            Console.WriteLine("    Starting the service... ");
            Utils.RunShellCommand($"sudo systemctl start {_serverFile}");
            Console.WriteLine("OK");            
        }

        private void UninstallingServerService(){
            var source = Path.Combine(Utils.AppFolder, "files", _serverFile);
            var dest = Path.Combine(_serverFolder, _serverFile);
            
            Console.WriteLine("  Removing the server service:");
            if(File.Exists(dest)){                      
                Console.WriteLine("    Stopping the service... ");
                Utils.RunShellCommand($"sudo systemctl stop {_serverFile}");
                Console.WriteLine("OK");

                Console.WriteLine("    Disabling the service... ");
                Utils.RunShellCommand($"sudo systemctl disable {_serverFile}");
                Console.WriteLine("OK");

                Console.Write("    Removing the old service {0}... ", dest);
                File.Delete(dest);
                Console.WriteLine("OK");

                Console.WriteLine("    Reloading the services daemon... ");
                Utils.RunShellCommand("sudo systemctl daemon-reload");
                Console.WriteLine("OK");
            }           
        }

        private void InstallingClientApp(){
            var source = Path.Combine(Utils.AppFolder, "files", _clientFile);
            var dest = Path.Combine(_clientFolder, _clientFile);
                            
            if(File.Exists(dest)) UninstallingClientApp();
            
            Console.WriteLine("  Setting up the client application:");     
            Console.Write("    Creating the new application launcher {0}... ", dest);
            File.WriteAllText(dest, String.Format(File.ReadAllText(source), Utils.AppFolder));               
            Console.WriteLine("OK");
        }
        
        private void UninstallingClientApp(){
            var source = Path.Combine(Utils.AppFolder, "files", _clientFile);
            var dest = Path.Combine(_clientFolder, _clientFile);
                
            Console.WriteLine("  Removing the client application:");     
            if(File.Exists(dest)){                 
                //TODO: find a way to stop all clients.
                // Console.WriteLine("    Stopping the application... ");
                // RunShellCommand("sudo systemctl stop ust.service");
                // Console.WriteLine("OK");

                Console.Write("    Removing the old application launcher {0}... ", dest);
                File.Delete(dest);
                Console.WriteLine("OK");
            }
        }

        private async void ReloadDbusConfig(){
            Console.WriteLine();
            Console.Write("  Reloading dbus configuration...     ");
            var systemConnection = Connection.System;
            var dbusManager = systemConnection.CreateProxy<IDBus>("org.freedesktop.DBus", "/org/freedesktop/DBus");
            await dbusManager.ReloadConfigAsync();
            Console.WriteLine("OK"); 
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