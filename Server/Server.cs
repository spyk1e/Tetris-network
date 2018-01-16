using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    class Server
    {
        #region Arguments
        public static int ClientNumber;// Number of clients whom have to connect
        public static List<Client> clients;// List of clients with their infos in the client object
        public static Random rd = new Random();// Initialize the random seed
        #endregion

        #region Function
        public static bool StartListening(int port, int column, int row, int delay)
        {
            string data = null;
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the host running the application.  

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  

                clients = new List<Client>(); // Initialize list of clients
                Console.WriteLine("Waiting for a connection...");

                while (true)
                {

                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();

                    data = null;


                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (data.IndexOf("<Initialize>") > -1)
                        {
                            break;
                        }
                    }

                    data = data.Replace("<Initialize>", ""); // Contain ID of Player

                    // Add the new client to the list of clients
                    clients.Add(new Client(data, IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()), ((IPEndPoint)handler.RemoteEndPoint).Port.ToString()));

                    // Write on the console of the server the ID of the new client
                    Client current_client = clients.Find(x => x.ID == data);
                    Console.WriteLine("Client find : " + data);
                    byte[] msg;

                    // Game can start
                    if (ClientNumber == clients.Count)
                    {
                        msg = Encoding.ASCII.GetBytes("<ReturnInitialize>" + column + ";" + row + ";" + delay + ";" + current_client.ClientPort + ";" + "1"); // column;row;delay;port;start
                        foreach (Client client in clients)
                        {
                            if (client != current_client)
                                client.SendMessage("<Start>", null);
                        }
                        handler.Send(msg);
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        listener.Close();
                        break;
                    }
                    else// If some player are missing, only send the parameters of the board
                    {
                        msg = Encoding.ASCII.GetBytes("<ReturnInitialize>" + column + ";" + row + ";" + delay + ";" + current_client.ClientPort + ";" + "0");// column;row;delay;port;start
                    }

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
                // Close the connection
                listener.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;

        }

        public static bool GameListening(int port)
        {
            string data = null;
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the host running the application.  

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                int GameOver = 0;
                bool ServerRestart = false;
                while (true)
                {
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();

                    data = null;

                    byte[] msg;
                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (data.IndexOf("<NewBlock>") > -1)
                        {
                            data = data.Replace("<NewBlock>", ""); // Contain ID of Player
                            Client current_client = clients.Find(x => x.ID == data);

                            msg = Encoding.ASCII.GetBytes("<ReturnNewBlock>" + rd.Next(4) + ";" + current_client.newRow + ";" + GameOver); // BlockID;NewRow;GameOver
                            clients.Find(x => x.ID == data).ResetRow();

                            break;
                        }
                        else if (data.IndexOf("<ScorePlayers>") > -1) // Check if it's a scoreplayer information
                        {
                            data = data.Replace("<ScorePlayers>", ""); // Contain ID of Player

                            List<Client> clients_rank = clients.OrderByDescending(o => o.Score).ToList();
                            string score = "";
                            foreach (Client client in clients_rank)
                            {
                                score += client.ID + ";" + client.Score + ";";
                            }

                            score = score.Remove(score.Length - 1, 1);

                            msg = Encoding.ASCII.GetBytes("<ReturnScorePlayers>" + score); // Score of all player : ID1;Score1;ID2;Score2 ...

                            if (GameOver == 1 && !clients.Find(x => x.ID == data).Lost)
                            {
                                ServerRestart = true;
                            }

                            break;
                        }
                        else if (data.IndexOf("<RemoveRow>") > -1)// Check if it's a remove row information
                        {
                            data = data.Replace("<RemoveRow>", ""); // Contain ID of Player
                            Client current_client = clients.Find(x => x.ID == data);
                            foreach (Client client in clients)
                            {
                                if (client.ID != current_client.ID)
                                    client.AddRow();
                                else
                                    client.Score += 1;
                            }
                            msg = Encoding.ASCII.GetBytes("<ReturnRemoveRow");
                            break;
                        }
                        else if (data.IndexOf("<GameOver>") > -1)// Check if it's a gameover information
                        {
                            data = data.Replace("<GameOver>", ""); // Contain ID of player

                            clients.Find(x => x.ID == data).Lost = true;// Change the Lost argument of the corresponding client
                            int notlostcount = 0;
                            foreach (Client client in clients)// Count the number of client still playing
                            {
                                if (!client.Lost)
                                {
                                    notlostcount++;
                                }
                            }
                            if (notlostcount > 1)// If at least one client playing
                            {
                                GameOver = 0;
                            }
                            else// If every clients have lost
                            {
                                GameOver = 1;// End of the game
                            }

                            msg = Encoding.ASCII.GetBytes("<ReturnGameOver>");
                            break;
                        }
                    }

                    // Send the corresponding answer to the client depending of the message sended
                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    if (ServerRestart)// Restart the server if needed
                    {
                        listener.Close();
                        return true;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }
        #endregion

        #region Main
        public static void Main(String[] args)
        {
            Console.CursorVisible = false;// Invisible cursor
            ClientNumber = Int16.Parse(args[4]);
            while (true)
            {
                // Game start
                if (StartListening(Int16.Parse(args[0]), Int16.Parse(args[1]), Int16.Parse(args[2]), Int16.Parse(args[3])))
                {
                    Console.WriteLine("Game starting !");
                    GameListening(Int32.Parse(args[0]));
                }
            }
        }
        #endregion
    }
}
