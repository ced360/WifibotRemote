using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows;

namespace WifibotRemote
{
    class Com
    {
        MainForm form;
        public Com(MainForm _form)
        {
            form = _form;
        }

        byte[] bytes = new byte[1024];
        Socket senderSock;

        public bool socketConnected()
        {
            try
            {
                return senderSock.Connected;
            }
            catch (Exception) { return false; }
        }

        public void Connect(string ipToConnect, int port)
        {
            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission 
                    TransportType.Tcp,        // Defines transport types 
                    "",                       // Gets the IP addresses 
                    SocketPermission.AllPorts // All ports 
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Creates a network endpoint 
                IPAddress ipAddr = IPAddress.Parse(ipToConnect);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                // Create one Socket object to setup Tcp connection 
                senderSock = new Socket(
                    ipAddr.AddressFamily,// Specifies the addressing scheme 
                    SocketType.Stream,   // The type of socket  
                    ProtocolType.Tcp     // Specifies the protocols  
                    );

                senderSock.NoDelay = false;   // Using the Nagle algorithm 

                // Establishes a connection to a remote host 
                senderSock.Connect(ipEndPoint);

                //form.connected = true;
                Console.WriteLine("Connected to {0}", ipAddr.ToString());
                //form.log("Connected to " + ipAddr.ToString());
            }
            catch (Exception exc) { Console.WriteLine("Erreur: \n {0}", exc); /*form.msgbox("Erreur : "+exc.ToString());*/ }

        }
        public void Send(byte[] msg)
        {
            try
            {
                // Sends data to a connected Socket. 
                int bytesSend = senderSock.Send(msg);
                Console.WriteLine("Sended {0} bytes to server", bytesSend);
                form.log("Sended " + bytesSend + " bytes to server");
                //Receive();

            }
            catch (Exception exc) { Console.WriteLine("Error send :\n{0}", exc); }
        }
        public byte[] Receive()
        {
            try
            {
                // Receives data from a bound Socket. 
                int bytesRec = 0;// senderSock.Receive(bytes);
                int totalBytesRec = 0;
                byte[] ret = new byte[100];
                //Console.WriteLine("Received {0} bytes", bytesRec);
                //form.log("Received " + bytesRec + " bytes from server");
                // Converts byte array to string 
                String theMessageToReceive = "";// = Encoding.Unicode.GetString(bytes, 0, bytesRec);
                //for (int i = 0; i < bytesRec; i++)
                //{ theMessageToReceive += Convert.ToString(bytes[i]) + " "; }
                // Continues to read the data till data isn't available 
                while (senderSock.Available > 0)
                {
                    bytesRec = senderSock.Receive(bytes);
                    Console.WriteLine("Received {0} bytes", bytesRec);
                    form.log("Received " + bytesRec + " bytes from server");
                    //theMessageToReceive += Encoding.ASCII.GetString(bytes, 0, bytesRec);// Encoding.Unicode.GetString(bytes, 0, bytesRec);
                    //theMessageToReceive += Convert.ToString(bytes[0]);//Encoding.ASCII.GetString(bytes, 0, bytesRec);// Encoding.Unicode.GetString(bytes, 0, bytesRec);
                    for (int i = 0; i < bytesRec; i++)
                    {
                        ret[i + totalBytesRec] = bytes[i];
                        theMessageToReceive += Convert.ToString(bytes[i]) + " ";
                    }

                }

                //tbReceivedMsg.Text = "The server reply: " + theMessageToReceive;
                Console.WriteLine("Server reply: {0}", theMessageToReceive);

                return ret;
            }
            catch (Exception exc) { Console.WriteLine("error rcv: {0}", exc); return null; }
        }
        public void Disconnect()
        {
            try
            {
                // Disables sends and receives on a Socket. 
                senderSock.Shutdown(SocketShutdown.Both);

                //Closes the Socket connection and releases all resources 
                senderSock.Close();
                //form.connected = false;
                Console.WriteLine("Disconnected from server");

            }
            catch (Exception exc) { Console.WriteLine("erreur Disconnect:{0}", exc); }
        }
    }
}
