using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Client
    {
        #region Argument
        private string id;// Id of the client
        private IPAddress clientAdress;// Ip address of the client
        private int clientPort;// Port of the client
        public int newRow;// Number of penalties rows to add to the client
        private int score;// Score of the client
        private bool lost = false;// If the client can play or alredy lost
        #endregion

        #region GetSet
        public int ClientPort
        {
            get
            {
                return clientPort;
            }
        }

        public IPAddress ClientAdress
        {
            get
            {
                return clientAdress;
            }
        }

        public string ID
        {
            get
            {
                return id;
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

        public bool Lost
        {
            get
            {
                return lost;
            }

            set
            {
                lost = value;
            }
        }
        #endregion

        #region Constructor
        public Client(string id, IPAddress ca, string cp)
        {
            this.id = id;
            this.clientAdress = ca;
            this.clientPort = Int32.Parse(cp);
            this.newRow = 0;
        }

        public Client(string id)
        {
            this.id = id;
            newRow = 0;
        }
        #endregion

        #region Function
        public void AddRow()
        {
            newRow += 1;
        }
        public void ResetRow()
        {
            newRow = 0;
        }


        // Function to send message to the server
        public string SendMessage(string message, string resultid)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(this.clientAdress, this.ClientPort);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(this.clientAdress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes(message);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    if (resultid != null)
                    {
                        // Receive the response from the remote device.  
                        int bytesRec = sender.Receive(bytes);

                        string result = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (result.IndexOf(resultid) < 0)
                        {
                            Console.WriteLine("Server return error.");
                            return null;
                        }

                        // Release the socket.  
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();

                        return result.Replace(resultid, "");
                    }

                    return null;
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
    }
    #endregion
}
