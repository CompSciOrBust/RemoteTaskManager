using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    class Program
    {
        static int Port = 80;

        static void Main(string[] args)
        {
            //Open new tcp listner for incoming traffic
            TcpListener Listner = new TcpListener(Port);
            Listner.Start();
            //Run forever
            while (true)
            {
                //Wait until a connection is recieved
                TcpClient Client = Listner.AcceptTcpClient();
                //Set up streams for input and output traffic
                StreamReader ClientReader = new StreamReader(Client.GetStream());
                StreamWriter ClientWriter = new StreamWriter(Client.GetStream());
                //Read request and parse tokens
                string Request = ClientReader.ReadLine();
                string[] Tokens = Request.Split(' ');
                switch (Tokens[0])
                {
                    case "GET":
                        if(Tokens[1] == "/")//Default page requested
                        {
                            ClientWriter.WriteLine("HTTP/1.0 520 OK\n"); //Response Header
                            ClientWriter.WriteLine("<html><center><h2><a href=\"/Reboot\">Reboot pc</a><br><a href=\"/Shutdown\">shutdown pc</a></h2></center></html>"); //Payload
                            ClientWriter.Flush(); //Send data down the pipe
                        }
                        if(Tokens[1] == "/Reboot")//Requesting a reboot
                        {
                            ClientWriter.WriteLine("HTTP/1.0 520 OK\n"); //Response Header
                            //Spawn shutdown.exe process with the arguments to reboot immediately
                            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                        }
                        if(Tokens[2] == "/Shutdown")//Requesting a power off
                        {
                            ClientWriter.WriteLine("HTTP/1.0 520 OK\n"); //Response Header
                            //Spawn shutdown.exe process with the arguments to shutdown immediately
                            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
                        }
                        break;
                    default:
                        //Unknown action
                        break;
                }
                //Close client
                Client.Close();
            }
        }
    }
}
