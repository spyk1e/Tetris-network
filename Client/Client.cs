using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Client
    {
        #region Function
        // Function to send a message to the server
        static string SendMessage(string IP, int port, string message, string resultid)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                var address = Dns.GetHostEntry(IP).AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetworkV6); // IPV4 to IPV6 address

                IPAddress ipAddress = address;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes(message);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

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
        #endregion

        #region Main
        public static void Main(String[] args)
        {
            Console.CursorVisible = false;// Invisible cursor
            Player player = new Player(args[0], args[1], args[2], args[3], args[4], args[5]);// Create the player with informations from arguments typed during the cmd start
            Console.Title = player.ID;// Change the name of the console by the ID of the player

            while (true)
            {
                string initialize = SendMessage(args[0], Int32.Parse(args[1]), "<Initialize>" + player.ID, "<ReturnInitialize>");

                string[] Initialize = initialize.Split(';');

                player.ListeningPort = Int32.Parse(Initialize[3]);
                Platform platform = new Platform(Int32.Parse(Initialize[0]), Int32.Parse(Initialize[1]), Int32.Parse(Initialize[2]), player);

                if (Int64.Parse(Initialize[4]) == 1) // Game can start
                {
                    platform.StartGame();
                }
                else
                {
                    if (player.StartListening()) // Game can start
                    {
                        platform.StartGame();
                    }
                }
            }
        }
        #endregion
    }
}
