using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RTServer
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }


    public class MainClass
	{
		private static byte[] _buffer = new byte[1024]; //buffer when getting data from the users
		private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Socket creation

		//private static List<Socket> _clientSocket = new List<Socket> ();//this is only temparory way to hold users connecting
		private static UserList _clientSocket = new UserList(); //this holds all user data 

		//private static Log Debug = new Log(); //This is for the time stamp logging
		private static Commander commandObject = new Commander(); //This contains all the functions for commands sent by the clients

		public static void Main (string[] args)
		{
			Console.Title = "RennTek Studios Server - MMO";
			SetupServer (); //start the server

			Console.ReadLine ();
		}

		/// <summary>
		/// Setups the server.
		/// </summary>
		private static void SetupServer()
		{
			Console.WriteLine("Setting up server..."); //Tell the admin you are starting
			_serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024));
			_serverSocket.Listen(50);
			Console.WriteLine("==================================== \n" +
							  "=====    RennTek Studios - SERVER - MMO  ===== \n" +
							  "==================================== \n"); //lets just do something fancy to show its ready
			_serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null); //Lets accept users to connect
			Console.WriteLine("Ready for clients... On IP=127.0.0.1:Port=1024");
		}

        //clients connected
		private static void AcceptCallback(IAsyncResult AR)
		{
			Socket socket = _serverSocket.EndAccept(AR); 

			Debug.Log("Client Connected..." + socket.AddressFamily); //tell the admin someone connected
			_clientSocket.Add(socket, "", "");
			Debug.Log ("Clients connected: " + _clientSocket.Length);
			socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReadCallback), socket);
           // _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Debug.Log("User ID = " + GetClientId(socket));


            //this is how we send data to client, client doesnt currently get it
            SendToClient(socket, "Help");

        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.
                    SendToClient(client, content);
                }
                else
                {
                    // Not all data received. Get more.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        //messages from client to server
        private static void ReceiveCallback(IAsyncResult AR)
		{
            Debug.Log("ReceiveCallBack");
			Socket socket = (Socket)AR.AsyncState;

			int received = 0;

			try{
				received = socket.EndReceive(AR);
			}
			catch{
				DisconnectUser (socket);
				return;
			}
				
			if (socket.Connected) {
				byte[] dataBuf = new byte[received];

				Array.Copy (_buffer, dataBuf, received);

				string text = Encoding.ASCII.GetString (dataBuf);

				if (text != "") {
					Debug.Log ("Client has sent message: " + text);

					string[] commands = text.Split ('/');

					for (int i = 0; i < commands.Length; i++) {
						if (commands [i] != "") {	
							commands [i] = commands [i].Replace ("/", "");
							DoCommand (commands [i], socket);
						}
					}

                    if (commands.Contains("bc"))
                    {
         
                    }
                } else {
                    //when we disconnect we send a blank message saying we want to disconnect
                    DisconnectUser (socket);
					return;
				}

			} else {
				DisconnectUser (socket);
				return;
			}

			socket.BeginReceive (_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback (ReceiveCallback), socket);
		}

		private static void DoCommand(string msg, Socket soc){
			string[] temp = msg.Split (' ');
			string command = temp [0];

			Type type = commandObject.GetType ();
			MethodBase commandFunction = type.GetMethod (command);

			if (commandFunction != null) {
				object[] pars = new object[temp.Length - 1];
				for (int i = 0; i < pars.Length; i++) {
					pars [i] = temp [i + 1];
				}
					
				if ((string)pars [pars.Length - 1] == "") {
					pars [pars.Length - 1] = soc;
				}


				ParameterInfo[] paramaters = commandFunction.GetParameters ();
				object[] endParamaters = RTConverter.ConvertParams (pars, paramaters);

				if (commandFunction != null) {
					try {
						commandFunction.Invoke (commandObject, endParamaters);
					} catch (Exception e) {
						Debug.Log (e);
					}
				} else {
					Debug.Log ("commandFunction is null");
				}
			}
		}

        private static void SendToClient(Socket soc, String data)
        {
            Debug.Log("SendToClient");
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            soc.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), soc);
        }

        public static void SendCallBack(IAsyncResult AR)
		{
            Debug.Log("Send CallBack");
			Socket socket = (Socket)AR.AsyncState;

			try
			{
                int bytesSent = socket.EndSend(AR);
                Debug.Log("Sent = " + bytesSent);
			}

			catch (ArgumentException e)
			{
				Debug.Log(e);
			}
		}

		public static void ChangeUserRoom(Socket user, String roomName)
		{
			_clientSocket.SetUserRoom(user, roomName);
		}

		public static String GetPlayerRoom(Socket user)
		{
            Debug.Log("GetPlayerRoom();");
			String playersRoom = _clientSocket.GetUserRoom(user);

			return playersRoom;
		}

		public static int GetClientId(Socket soc)
		{
			return _clientSocket.GetUserId(soc);
		}

        //client has been disconnected
		private static void DisconnectUser(Socket soc)
		{
			string roomToLeave = _clientSocket.GetUserRoom (soc);
			if (roomToLeave != "" || roomToLeave != null) {
				commandObject.LeaveRoom(roomToLeave, soc);
			}

			_clientSocket.Remove (soc);
			soc.Close ();

			Debug.Log ("Client disconnected...");
			Debug.Log ("Clients still connected: " + _clientSocket.Length);
		}
	}
}
