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
using System.IO; 
using System.Text;    
using System.Net.Sockets;       

namespace UST.Core
{
    class Client: Server
    {       
        public Client(Shutdown[] schedule, int port=261186, bool verbose = false): base(schedule, port, verbose){            
        }

        public override void Start(){
            //Source: https://www.c-sharpcorner.com/article/getting-started-with-remote-procedure-call/
            
            try{     
                using (var client = new TcpClient()){
                    if(this.Verbose) Console.WriteLine("Connecting...");  

                    client.Connect("127.0.0.1", this.Port);    
                    if(this.Verbose) Console.WriteLine("Connected!");    
                    
                    if(this.Verbose) Console.WriteLine("Enter the String you want to send:");    
                    var input = Console.ReadLine();                            
                    var stream = client.GetStream();    
                    var enconding = new ASCIIEncoding();    
                    byte[] message = enconding.GetBytes(input); 

                    if(this.Verbose) Console.WriteLine("Sending...");    
                    stream.Write(message, 0, message.Length);    
                    byte[] buffer = new byte[100];    
                    int length = stream.Read(buffer, 0, 100);    

                    if(this.Verbose){
                        for (int i = 0; i < length; i++)    
                        {    
                            Console.Write(Convert.ToChar(buffer[i]));    
                        }   
                    } 
            
                    client.Close();    
                    if(this.Verbose) Console.ReadLine();    
                }
            }
            catch(Exception ex){
                if(this.Verbose) Console.WriteLine("Error: {0}", ex.StackTrace); 
            }            
        }
    }
}