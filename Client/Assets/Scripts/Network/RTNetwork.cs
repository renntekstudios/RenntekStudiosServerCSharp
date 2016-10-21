using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace RennTekNetworking {

	public class StateObject {
		//Client Socket
		public Socket workSocket = null;
		//Message Size
		public const int BufferSize = 256;
		//Message
		public byte[] buffer = new byte[BufferSize];
        //recieved data string
        public StringBuilder sb = new StringBuilder();
	}

	public class RTNetwork : MonoBehaviour {

		private static Socket sendSocket = null;

		public static void Connect(String ip, int port){
			try{
                IPAddress ipAddress = IPAddress.Parse(ip);
				IPEndPoint remoteEP = new IPEndPoint (ipAddress, port);


                Socket client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				client.BeginConnect (remoteEP, new AsyncCallback (ConnectCallBack), client);

        //        Send(sendSocket, "This is a test message");


                Receive(sendSocket);
                
            }
            catch (Exception e){
				Debug.LogError (e.ToString ());
			}
		}

        //connect
        private static void ConnectCallBack(IAsyncResult ar){
			try{
				sendSocket = (Socket) ar.AsyncState;
                sendSocket.EndConnect(ar);

				Debug.LogFormat("Client connected to {0}", sendSocket.RemoteEndPoint.ToString());
               
			} catch (Exception e){
				Debug.LogError (e.ToString ());
			}
		}

        private static void Receive(Socket soc)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = soc;

                // Begin receiving the data from the remote device.
                soc.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private static void ReceiveCallback(IAsyncResult ar)
        {
            Debug.Log("GEtting data from server");
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                sendSocket = client;
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        Debug.Log("GOT MESSAGE FROM SERVER " + state.sb.ToString());
                    }
                    // Signal that all bytes have been received.
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallBack2(IAsyncResult ar){
            Debug.Log("ReceiveCallBack");
			byte[] key = { 5, 9, 0, 4 };

			int messageSize = 0;
			int messageOffset = 0;
			byte[] currentMessage;

			int fullMessageSize = 0;
			byte[] fullMessage = new byte[0];

			try{
				if (isConnected()){
					StateObject state = (StateObject) ar.AsyncState;
					Socket soc = state.workSocket;

                    Debug.Log("isConnected on ReceiveCallBack");
					int buffSize = soc.EndReceive(ar);

					if(buffSize != 0 || fullMessage.Length != 0){
						if(fullMessage.Length != 0 && buffSize != 0){
							Array.Resize(ref fullMessage, fullMessage.Length + buffSize);
							byte[] newMessage = new byte[buffSize];
							newMessage = state.buffer;
							Array.ConstrainedCopy(newMessage, 0, fullMessage, fullMessageSize, newMessage.Length);
							messageSize = 0;
						} else if(buffSize != 0){
							fullMessage = new byte[buffSize];
							fullMessage = state.buffer;
							messageSize = 0;
						} else if(fullMessage.Length != 0){
							messageSize = 0;
						}

						if(messageSize == 0){
							messageSize = BitConverter.ToInt32(RTArray.copyOfRange(fullMessage, 0, 4), 0);
							byte[] messageKey = RTArray.copyOfRange(fullMessage, messageSize - 4, messageSize);
							if(Array.Equals(messageKey, key)){
								currentMessage = new byte[messageSize - 8];
								messageOffset = 4;
								Array.ConstrainedCopy(fullMessage, messageOffset, currentMessage, 0, currentMessage.Length);
								messageOffset = messageSize;

								List<object> sentMessage = RTConverter.ReceiveConversion(currentMessage);
								InvokeMessage(sentMessage);

								if(messageOffset != fullMessage.Length){
									fullMessage = RTArray.copyOfRange(fullMessage, messageOffset, fullMessage.Length);
									fullMessageSize = fullMessage.Length;
								} else {
									fullMessage = new byte[0];
									buffSize = 0;
								}
							} else {
								Debug.Log("Key was not found. Message will try to be completed in next read!");
							}
						}
					}
                    else
                    {
                        Debug.Log("Buffer size is 0");
                    }
				} else {
					Disconnect();
				}

			} catch(Exception e){
				    //Debug.LogError (e.ToString ());
			} finally {
				Disconnect ();
			}
		}

		private static void InvokeMessage(List<object> sentMessage){
			Debug.Log ("got some message");
		}

		public static void Send(String MethodName, params object[] data){
			byte[] messageData = RTConverter.SendConversion (MethodName, data);

			sendSocket.BeginSend (messageData, 0, messageData.Length, 0, new AsyncCallback (SendCallBack), sendSocket);
		}

		private static void SendCallBack(IAsyncResult ar){
			try{
				Socket client = (Socket)ar.AsyncState;

				int bytesSent = client.EndSend (ar);
				Debug.LogFormat ("Sent {0} bytes to server.", bytesSent);
			} catch(Exception e){
				Debug.LogError (e.ToString());
			}
		}

		public static void Disconnect(){
			sendSocket.Disconnect (false);
		}

		public static bool isConnected(){
			return sendSocket.Connected;
		}
	}
}