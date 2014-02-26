using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using Lidgren.Message;

namespace Chat
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Application start...");

            string app_identifier = "chat";
            string server_ip = "127.0.0.1";
            int server_port = 11515;
            int max_client = 100;


            ChatClient client = new ChatClient();
            client.Init(app_identifier);

            ChatServer server = new ChatServer();
            server.Init(app_identifier, server_port, max_client);
            
            while (true)
            {
                Console.Write("command> ");
                string line = Console.ReadLine();
                if (line == "quit")
                {
                    client.Close();
                    server.Stop();
                    break;
                }
                else if (line == "start server")
                {
                    server.Start();
                }
                else if (line == "connect")
                {
                    client.Connect(server_ip, server_port);
                }
                else if (line.IndexOf("login") >= 0)
                {
                    string[] token = line.Split(' ');
                    if (token.Length >= 2)
                    {
                        client.Login(token[1]);
                    }
                    else
                    {
                        Console.WriteLine("Invalid command.");
                    }
                }
                else if (line.IndexOf("logout") >= 0)
                {
                    client.Logout();
                }
                else if (line.IndexOf("send") >= 0)
                {
                    string[] token = line.Split(' ');
                    if (token.Length == 3)
                    {
                        client.Send(token[1], token[2]);
                    }
                    else
                    {
                        Console.WriteLine("Invalid command.");
                    }
                }
                else
                {
                    if (line.Length > 0 && client.IsLogin())
                    {
                        client.SendToAll(line);
                    }
                }
            }

            Console.WriteLine("Application is quitted!");
            Console.Read();
        }
    }
}
