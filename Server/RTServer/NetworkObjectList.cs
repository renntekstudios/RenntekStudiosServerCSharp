using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace RTServer
{
	public class NetworkObjectsList
	{
		public int actorId = -1;
		public Socket soc;
		public String objectName = "";
		public String roomName = "";
		public int clientId = 0;
		public Vector3 Position;
		public Quaternion Rotation;
		public NetworkObjectsList next;

		public NetworkObjectsList (int id, Socket cliSoc, string room, int cliId, string name, Vector3 position, Quaternion rotation)
		{
			actorId = id;
			soc = cliSoc;
			objectName = name;
			roomName = room;
			clientId = cliId;
			this.Position = position;
			this.Rotation = rotation;
		}
	}

	public class NetworkObject
	{
		private NetworkObjectsList first;
		private int numActors = 0;
		private int count = 0;

		public NetworkObject()
		{
			first = null;
		}

		public bool isEmpty()
		{
			return (first == null);
		}

		public int Length
		{
			get{
				return count;
			}
		}

		public int AssignActorId(){
			return numActors + 1;
		}

		public void add(int id, Socket cliSoc, string room, int clientId, string name, Vector3 Position, Quaternion Rotation){
			numActors++;
			count++;
			NetworkObjectsList newObject = new NetworkObjectsList (id, cliSoc, room, clientId, name, Position, Rotation);
			newObject.next = first;
			first = newObject;
		}

		public void remove(int actorId){
			NetworkObjectsList current = first;
			NetworkObjectsList previous = null;

			while (current.actorId != actorId) {
				if (current.next == null) {
					return;
				} else {
					previous = current;
					current = current.next;
				}
			}

			if (current == first) {
				first = first.next;
				count--;
			} else {
				previous.next = current.next;
			}
		}

		public void remove(Socket soc)
		{
			NetworkObjectsList current = first;
			NetworkObjectsList previous = null;
			while (current.soc != soc) {
				if (current.next == null) {
					return;
				} else {
					previous = current;
					current = current.next;
				}
			}

			if (current == first) {
				first = first.next;
				count--;

			} else {
				previous.next = current.next;
			}
		}

		public string SendString(int actorId)
		{
			NetworkObjectsList current = first;
			while (current.actorId != actorId) {
				if (current.next == null) {
					return null;
				} else {
					current = current.next;
				}
			}

			return current.actorId + " " + current.objectName + " " + current.Position.x + " " + current.Position.y + " " + current.Position.z + " " + current.Rotation.x + " " + current.Rotation.y + " " + current.Rotation.z + " " + current.Rotation.w;
		}

		public void SetPosition(int actorId, Vector3 Position)
		{
			NetworkObjectsList current = first;
			while (current.actorId != actorId) {
				if (current.next == null) {
					return;
				} else {
					current = current.next;
				}
			}

			current.Position = Position;
		}

		public void SetRotation(int actorId, Quaternion Rotation){
			NetworkObjectsList current = first;
			while (current.actorId != actorId) {
				if (current.next == null) {
					return;
				} else {
					current = current.next;
				}
			}

			current.Rotation = Rotation;
		}

		public List<NetworkObjectsList> GetItemsInRoom(string roomName)
		{
			List<NetworkObjectsList> temp = new List<NetworkObjectsList>();
			NetworkObjectsList current = first;

			while(current != null)
			{
				if (current.roomName == roomName) {
					temp.Add (current);
				}

				current = current.next;
			} 

			return temp;
		}

		public void DeleteRoomItems(string roomName)
		{
			Debug.Log (roomName);
			NetworkObjectsList current = first;
			int i = 0;
			while (current != null) {
				Debug.Log (current.roomName);
				if (current.roomName == roomName) {
					remove (current.actorId);
					i++;
				}
					current = current.next;
			}

			Debug.Log (i + " network objects have been removed!");
			Debug.Log (Length);
		}
	}
}

