using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteTaskManager
{
    class Program
    {
        //Default web port
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
                            ClientWriter.WriteLine("<html><center><h2><a href=\"/Reboot\">Reboot pc</a><br><a href=\"/Shutdown\">Shutdown pc</a><br><a href=\"/TaskKill\">Task list</a></h2></center></html>"); //Payload
                            ClientWriter.Flush(); //Send data down the pipe
                        }
                        else if(Tokens[1] == "/Reboot")//Requesting a reboot
                        {
                            ClientWriter.WriteLine("HTTP/1.0 520 OK\n"); //Response Header
                            ClientWriter.WriteLine("<html><center><h2><p>Restarting.</p></h2></center></html>"); //Payload
                            ClientWriter.Flush(); //Send data down the pipe
                            //Spawn shutdown.exe process with the arguments to reboot immediately
                            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0 -f");
                        }
                        else if(Tokens[1] == "/Shutdown")//Requesting a power off
                        {
                            ClientWriter.WriteLine("HTTP/1.0 520 OK\n"); //Response Header
                            ClientWriter.WriteLine("<html><center><h2><p>Powering down.</p></h2></center></html>"); //Payload
                            ClientWriter.Flush(); //Send data down the pipe
                            //Spawn shutdown.exe process with the arguments to shutdown immediately
                            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0 -f");
                        }
                        else if(Tokens[1].Split('/')[1] == "TaskKill")// Kill Processes
                        {
                            Process[] ProcArray = Process.GetProcesses();//get list of processes
                            ClientWriter.WriteLine("HTTP/1.0 520 OK\n");//Response Header
                            if (Tokens[1].Split('/').Length == 2 || Tokens[1].EndsWith("/"))//Return a list of procs we can kill
                            {

                                ClientWriter.WriteLine("<html><center><h2><p>Task list:</p></h2></center></html>"); //Payload if task is killed
                                foreach (Process Proc in ProcArray) //Add each task as a link to the payload
                                {
                                    ClientWriter.WriteLine("<center><h3><p><a href=\"/TaskKill/{0}\">" + Proc.ProcessName + "</a></p></h3></center>", Proc.Id); //Build payload
                                }
                            }
                            else//Kill the process
                            {
                                int PIDToKill = int.Parse(Tokens[1].Split('/')[2]);//Get the PID from the url
                                Process ProcToKill = null; //Var to store the process
                                //Check if process exists
                                foreach (Process Proc in ProcArray)
                                {
                                    //If the process ID matches assign the process to ProcToKill
                                    if(Proc.Id == PIDToKill)
                                    {
                                        ProcToKill = Proc;
                                    }
                                }
                                if(ProcToKill != null)//Check process exists
                                {
                                    ProcToKill.Kill();//Kill the process
                                    if (ProcToKill.HasExited)
                                    {
                                        ClientWriter.WriteLine("<html><center><h2><p>Task killed.</p><br><a href=\"/\">Home</a></h2></center></html>"); //Payload if task is killed
                                    }
                                    else
                                    {
                                        ClientWriter.WriteLine("<html><center><h2><p>Failed to kill task.</p><br><a href=\"/\">Home</a></h2></center></html>"); //Payload if task is failed.
                                    }
                                }
                                else
                                {
                                    ClientWriter.WriteLine("<html><center><h2><p>Could not find task.</p><br><a href=\"/\">Home</a></h2></center></html>"); //Payload if task is failed.
                                }

                            }
                            ClientWriter.Flush(); //Send data down the pipe
                        }
                        else //Unknown request
                        {
                            ClientWriter.WriteLine("HTTP/1.0 404 OK\n"); //Response Header. 404 is not ok but we'll lie because it doesn't make a difference.
                            ClientWriter.WriteLine("<html><center><h2></p>404. Unknown request.</p><br><a href=\"/\">Home</a></h2></center></html>"); //Payload.
                            ClientWriter.Flush(); //Send data down the pipe
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
