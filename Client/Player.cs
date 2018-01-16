using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Client
{
    class Player
    {
        #region Arguments
        private string id; // Player id
        private string serverAddress; // Ip adress of the player
        private string serverPort; // Port of the server
        private int listeningPort; // Port to listen server messages

        // Keys parameters 
        private string hight;
        private string low;
        private string right;
        private string left;

        private int score; // Score of the player
        #endregion

        #region GetSet
        public string ID
        {
            get
            {
                return id;
            }
        }

        public int ListeningPort
        {
            get
            {
                return listeningPort;
            }

            set
            {
                listeningPort = value;
            }
        }

        public string Hight
        {
            get
            {
                return hight;
            }
        }

        public string Right
        {
            get
            {
                return right;
            }
        }

        public string Low
        {
            get
            {
                return low;
            }
        }

        public string Left
        {
            get
            {
                return left;
            }
        }

        public int Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;
            }
        }
        #endregion

        #region Constructor
        public Player(string sa, string sp, string hight, string right, string low, string left)// Used to store information about the player using this console
        {
            this.serverAddress = sa;
            this.serverPort = sp;
            this.hight = hight;
            this.right = right;
            this.low = low;
            this.left = left;
            id = GenerateID();
        }

        public Player(string id, int score)// Used to store information on others players
        {
            this.id = id;
            this.score = score;
            // Other arguments are never used
        }
        #endregion

        #region Function
        // Generate an unique ID using the time
        public string GenerateID()
        {
            long ticks = DateTime.Now.Ticks;
            byte[] bytes = BitConverter.GetBytes(ticks);
            string id = Convert.ToBase64String(bytes)
                                    .Replace('+', '_')
                                    .Replace('/', '-')
                                    .TrimEnd('=');
            return id;
        }


        // Send a message to the server to know wich block have to be added on the board
        public string AskServerBlock()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                var address = Dns.GetHostEntry(serverAddress).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetworkV6); // IPV4 to IPV6 address

                IPAddress ipAddress = address;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Int32.Parse(serverPort));

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("<NewBlock>" + this.id);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);

                    string result = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (result.IndexOf("<ReturnNewBlock>") < 0)
                    {
                        Console.WriteLine("Server return error.");
                    }

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    return result.Replace("<ReturnNewBlock>", "");
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }


        // Send a message to the server to make it send penalties rows to others players
        public bool SendNewRow()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                var address = Dns.GetHostEntry(serverAddress).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetworkV6); // IPV4 to IPV6 address

                IPAddress ipAddress = address;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Int32.Parse(serverPort));

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("<RemoveRow>" + this.id);// Tell the server that this player have fnished a row

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);

                    string result = Encoding.ASCII.GetString(bytes, 0, bytesRec);//The answer string from the server

                    // Verify that the server answered
                    if (result.IndexOf("<ReturnRemoveRow>") < 0)
                    {
                        Console.WriteLine("Server return error.");
                    }

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        // Function to get the score of every player from the server
        public List<Player> GetScoreAllPlayers()
        {
            List<Player> players = new List<Player>();
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                var address = Dns.GetHostEntry(serverAddress).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetworkV6); // IPV4 to IPV6 address

                IPAddress ipAddress = address;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Int32.Parse(serverPort));

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("<ScorePlayers>" + this.id);// Ask the score of other players 

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);

                    string result = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (result.IndexOf("<ReturnScorePlayers>") < 0)
                    {
                        Console.WriteLine("Server return error.");
                    }

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    // Get the answer of the server and put it on the List of players score
                    result = result.Replace("<ReturnScorePlayers>", "");

                    string[] result_array = result.Split(';');

                    for (int i = 0; i < result_array.Length; i += 2)
                    {
                        players.Add(new Player(result_array[i], Int16.Parse(result_array[i + 1])));// Create a new player with its id and score and add it to the list of all players
                    }
                    return players;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }

        // Function to tell the server this player have lost
        public bool SendGameOverToServer()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                var address = Dns.GetHostEntry(serverAddress).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetworkV6); // IPV4 to IPV6 address

                IPAddress ipAddress = address;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Int32.Parse(serverPort));

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("<GameOver>" + this.id);// Message telling the game is over

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);

                    // Verifying that the server received the message
                    string result = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (result.IndexOf("<ReturnGameOver>") < 0)
                    {
                        Console.WriteLine("Server return error.");
                    }

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return true;

                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }


        // Client listenning the message from the server to start
        public bool StartListening()
        {
            string data = null;
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, listeningPort);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a players ...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.  
                    while (true)// Listening for the server message
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<Start>") > -1)// If the server send the start message
                        {
                            break;
                        }
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    listener.Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }
    }
    #endregion
}
