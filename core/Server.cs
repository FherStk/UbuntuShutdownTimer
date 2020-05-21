/*
    Copyright © 2020 Fernando Porrino Serrano
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
using System.Net;  
using System.Text;    
using System.Net.Sockets;    
using System.Collections.Generic;    

namespace UST.Core
{
    class Server
    {
        public int Port {get; private set;}
        public List<Shutdown> Schedule {get; private set;}
        public bool Verbose {get; private set;}

        public Server(Shutdown[] schedule, int port=261186, bool verbose = false){
            this.Port = port;
            this.Schedule = new List<Shutdown>(schedule);
        }

        public virtual void Start(){
            //Source: https://www.c-sharpcorner.com/article/getting-started-with-remote-procedure-call/

            var address = IPAddress.Parse("127.0.0.1");                    
            var listener = new TcpListener(address, this.Port);

            try{         
                listener.Start();    
                
                if(this.Verbose){
                    Console.WriteLine("Server is running on port: {0}", this.Port);    
                    Console.WriteLine("Local endpoint: {0}", listener.LocalEndpoint);    
                    Console.WriteLine("Waiting for Connections...");    
                }
                
                using(var socket = listener.AcceptSocket()){
                    if(this.Verbose) Console.WriteLine("Connection accepted from: {0}", socket.RemoteEndPoint);    

                    var buffer = new byte[100];    
                    var length = socket.Receive(buffer);    
                    
                    if(this.Verbose){ 
                        Console.WriteLine("Data recieved..");    
                        for (int i = 0; i < length; i++)     
                            Console.Write(Convert.ToChar(buffer[i]));    
                    }

                    var enconding = new ASCIIEncoding();    
                    socket.Send(enconding.GetBytes("Automatic message: " + "String received byte server !"));    
                    
                    if(this.Verbose) Console.WriteLine("\nAutomatic message has been sent");    

                    socket.Close();    
                    if(this.Verbose) Console.ReadLine();  
                }
            }   
            catch(Exception ex){
                if(this.Verbose) Console.WriteLine("Error: {0}", ex.StackTrace); 
            }
            finally{
                listener.Stop();
            }
        }
    }
}
