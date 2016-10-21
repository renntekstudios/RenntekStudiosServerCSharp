using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace RTServer
{
	public class Commander
	{
		//Object made over the network
		NetworkObject NetList = new NetworkObject();
		public static RoomList rooms = new RoomList();

		//Rooms
		//public static Dictionary<String, List<Socket>> rooms = new Dictionary<String, List<Socket>>();

		//-------------------------------//
		//----Object related commands----//
		//-------------------------------//

		//This is used to determain the and start the tracking of new network objects
		public void Instantiate(string objectName, float x, float y, float z, float rx, float ry, float rz, float rw, Socket soc){

			int clientId = MainClass.GetClientId (soc); //get the id for the person who sent it
			string room = MainClass.GetPlayerRoom (soc); //get the room name of the sender
			int newActorId = Commander.rooms.GetRoomId(room) + NetList.AssignActorId(); //new id for the object
			Vector3 position = new Vector3(x, y, z);
			Quaternion rotation = new Quaternion (rx, ry, rz, rw);

			NetList.add (newActorId, soc, room, clientId, objectName, position, rotation); //add the object to the the network object list

			if (clientId != -1) {
				string sendMsg = "/Instantiate " + objectName + " " + x + " " + y + " " + z + " " + rx + " " + ry + " " + rz + " " + rw +  " " + newActorId + " " + clientId; //the string to send

				MsgInRoom (room, sendMsg, null); //send the message to everyone in the room
				Debug.Log ("Netowrk object created...");
				Debug.Log ("Network objects in list: " + NetList.Length);
			}
		}

		//This is used to update the stored netowrking objects data and send the update to all the cleints in the room.
		public void UpdatePosition(int actorId, float x, float y, float z, float rx, float ry, float rz, float rw, Socket soc){

			int id = System.Convert.ToInt32 (actorId); //this id is for the object
			Vector3 position = new Vector3(x, y, z);
			Quaternion rotation = new Quaternion(rx, ry, rz, rw);

			NetList.SetPosition (id, position); //change the saved data for the network object
			NetList.SetRotation(id, rotation);

			string sendMsg = "/UpdatePosition " + id + " " + x + " " + y + " " + z + " " + rz + " " + ry + " " + rz + " " + rw; //the string to send
			string room = MainClass.GetPlayerRoom (soc);//get the room name of the sender

			MsgInRoom (room, sendMsg, soc);//send the message to everyone in the room
		}

		//this retrieves the userid for 
		public void GetUserId (Socket soc){
			int id = MainClass.GetClientId(soc); //get the id of the client that is wanting its id

			if (id != -1) {
				string sendMsg = "/SetUserId " + id;//string to send

				MsgToClient (sendMsg, soc);//send the string back to the client
				Debug.Log ("Joined client has been given its id");
			} else {
				Debug.Log ("User does not exist!");
			}
		}

		public void RemoveNetObject(string actorId, Socket soc){
			int id = System.Convert.ToInt32 (actorId);
			NetList.remove (id);
		}

		//-------------------------------//
		//-----Room related commands-----//
		//-------------------------------//

		public void CreateRoom(string roomName, Socket makerSocket, int maxUsers)
		{
			if (!rooms.RoomExist(roomName))
			{
				rooms.Add (makerSocket, roomName, maxUsers);
				MainClass.ChangeUserRoom(makerSocket, roomName);

				string msg = "/JoinRoom " + roomName + " true";

				MsgToClient (msg, makerSocket);
				Debug.Log("Room Created: " + roomName);
			}
		}

		public void JoinRoom(string roomName, int maxUsers, Socket joinerSocket)
		{
			if (MainClass.GetPlayerRoom(joinerSocket) == "")
			{
				if (rooms.RoomExist(roomName))
				{
					if (!rooms.isFull (roomName))
					{
						if (!rooms.ContainsUser(roomName, joinerSocket)) {
							rooms.AddUserToRoom (roomName, joinerSocket);
							MainClass.ChangeUserRoom (joinerSocket, roomName);

							string msg = "/JoinRoom " + roomName + " false";
							string msg2 = "/NewUser " + rooms.UserCount (roomName);

							MsgToClient (msg, joinerSocket);
							MsgInRoom (roomName, msg2, joinerSocket);

							SendRoomObjects (roomName, joinerSocket);

							Debug.Log ("User joined " + roomName + ". Users in room: " + rooms.UserCount (roomName));
						} else {
							return;
						}
					}
					else
					{
						return;
					}
				}
				else
				{
					int max = System.Convert.ToInt32(maxUsers);
					CreateRoom(roomName, joinerSocket, max);
				}
			}
			else
			{
				this.LeaveRoom(MainClass.GetPlayerRoom(joinerSocket), joinerSocket);
				JoinRoom(roomName, maxUsers, joinerSocket);
			}
		}

		public void SendRoomObjects(string roomName, Socket joinerSocket)
		{
			List<NetworkObjectsList> temp = NetList.GetItemsInRoom (roomName);

			if (temp != null) {
				for (int i = 0; i < temp.Count; i++) {
					string sendMsg = "/Instantiate " + temp [i].objectName + " " + temp [i].Position.x + " " + temp [i].Position.y + " " + temp [i].Position.z + " " + temp[i].Rotation.x + " " + temp[i].Rotation.y + " " + temp[i].Rotation.z + " " + temp[i].Rotation.w + " " + temp [i].actorId + " " + temp[i].clientId;
					MsgToClient (sendMsg, joinerSocket);
				}
			}
		}

		public void SendBufferedRPC(string RoomName, Socket joinerSocket){
			List<string> temp = rooms.GetBufferedRPC (RoomName);

			if (temp != null) {
				for (int i = 0; i < temp.Count; i++) {
					string sendMsg = temp[i];
					MsgToClient (sendMsg, joinerSocket);
				}
			}
		}

		public void LeaveRoom(string roomName, Socket leaverSocket)
		{
			if (rooms.RoomExist(roomName))
			{
				if (rooms.ContainsUser(roomName, leaverSocket))
				{
					rooms.Remove(roomName, leaverSocket);
					MainClass.ChangeUserRoom(leaverSocket, "");
				}
				else
				{
					return;
				}

				if (rooms.UserCount(roomName) != 0)
				{
					Debug.Log("User left " + roomName + ". Users in room: " + rooms.UserCount(roomName));
				}
				else
				{
					DeleteRoom(roomName);
				}
			}
		}

		//this is used to remove anything about a room
		public void DeleteRoom(string roomName)
		{
			//make sure the room does exsist if not then log it
			if (rooms.RoomExist (roomName)) {
				//delete the room and delete any network items that deal with the room
				rooms.Remove (roomName);
				NetList.DeleteRoomItems (roomName);
				Debug.Log ("Room " + roomName + " has been deleted");
			} else {
				Debug.Log ("User was not in room but tried to send a message!");
			}
		}

		//----------------------------------//
		//----Messaging related commands----//
		//----------------------------------//

		public void RPC(string functionName, string target, string pars, Socket senderSocket){
			object[] startParams = pars.Split (',');
			string roomName = MainClass.GetPlayerRoom (senderSocket);

			string msg = "/RPC " + functionName + " " + pars;

			if (target == "All") {
				ServerRPC (functionName, startParams);
				MsgInRoom (roomName, msg, null);
			} else if (target == "Others") {
				ServerRPC (functionName, startParams);
				MsgInRoom (roomName, msg, senderSocket);
			} else if (target == "MasterClient") {
				Socket sendTo = rooms.GetRoomMaster(roomName);
				if(sendTo != null){
					MsgToClient (msg, sendTo);
				}
			} else if (target == "Server") { 
				ServerRPC (functionName, startParams);
			} else if (target == "AllBuffered") {
				ServerRPC (functionName, startParams);
				MsgInRoom (roomName, msg, null);
				rooms.AddBufferedRPC (roomName, msg);
			} else if (target == "OthersBuffered") {
				ServerRPC (functionName, startParams);
				MsgInRoom (roomName, msg, senderSocket);
				rooms.AddBufferedRPC (roomName, msg);
			} else {
				Debug.LogError ("RPC target was not a valid type! RPC will not be ran!");
			}
		}
	
		public void ServerRPC(string function, params object[] startParams){
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				if(a.FullName != "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"){
					foreach (Type t in a.GetTypes()) {
						if (Attribute.GetCustomAttribute (t, typeof(RTRPC)) != null) {
							object instance = Activator.CreateInstance (t);
							MethodBase method = t.GetMethod (function);

							if (method != null) {
								ParameterInfo[] paramaters = method.GetParameters();
								object[] invokeParams = RTConverter.ConvertParams(startParams, paramaters);
								if(invokeParams != null){
									method.Invoke(instance, invokeParams);
								}
							}
						}
					}
				}
			}
			Console.WriteLine ("Function (" + function + ") is not in an rpc class or does not exist!");
		}

		//Send message to everyone in the room
		public void MsgInRoom(string roomName, String msg, Socket soc)
		{
			List<Socket> roomList = rooms.GetRoomUsers(roomName); //create a local instance of the sockets in the room

			byte[] data = Encoding.ASCII.GetBytes(msg);

			//Go through the data and send the messages to each client in the room
			for (int i = 0; i < roomList.Count; i++)
			{
				if (roomList [i] != soc || soc == null) {
					roomList [i].BeginSend (data, 0, data.Length, SocketFlags.None, new AsyncCallback (SendCallBack), roomList [i]);
				}
			}
		}

		//Send message to just a spacific client
		public void MsgToClient(string msg, Socket soc)
		{
			byte[] data = Encoding.ASCII.GetBytes(msg);
			soc.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallBack), soc);
		}

		//this is the actual sending function
		private static void SendCallBack(IAsyncResult AR)
		{
			//get the socket 
			Socket socket = (Socket)AR.AsyncState;

			//attempt to send the message unless there is an issue then log the reason
			try
			{
				socket.EndSend(AR);
			}
			catch (ArgumentException e)
			{
				Debug.Log(e);
			}
		}
	}
}

