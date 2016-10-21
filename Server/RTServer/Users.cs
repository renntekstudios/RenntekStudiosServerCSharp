using System;
using System.Net.Sockets;

namespace RTServer
{
	public class User
	{
		public int actorId = -1;
		public Socket Soc;
		public String username = "";
		public String room;
		public User next;

		public User (int id, Socket soc, String user, String roomName)
		{
			actorId = id;
			Soc = soc;
			username = user;
			room = roomName;
		}
	}

	public class UserList
	{
		private User first;
		public int numActors = 0;
		public int num = 0;

		public UserList()
		{
			first = null;
		}

		public int Length
		{
			get{
				return num;
			}
		}

		public bool isEmpty()
		{
			return (first == null);
		}

		public void Add(Socket socket, String userName, String roomName)
		{
			numActors++;
			num++;
			User newUser = new User(numActors, socket, userName, roomName);
			newUser.next = first;
			first = newUser;
			Debug.Log ("New user created!");
		}


		//Remove the user using their actor ID
		public void Remove(int actorId)
		{
			User current = first;
			User previous = first;
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
				num--;
			} else {
				previous.next = current.next;
			}
		}

		//Remove the user using their socket
		public void Remove(Socket soc)
		{
			User current = first;
			User previous = null;
			while (current.Soc != soc) {
				if (current.next == null) {
					return;
				} else {
					previous = current;
					current = current.next;
				}
			}

			if (current == first) {
				first = first.next;
				num--;
			} else {
				previous.next = current.next;
			}
		}

		public String GetUserRoom(Socket soc){
			User current = first;
			while (current.Soc != soc) {
				if (current.next == null) {
					return null;
				} else {
					current = current.next;
				}
			}
			return current.room;
		}

		public void SetUserRoom(Socket soc, String roomName){
			User current = first;
			while (current.Soc != soc) {
				if (current.next == null) {
					return;
				} else {
					current = current.next;
				}
			}
			current.room = roomName;
		}

		public void SetUserName(Socket soc, String userName){
			User current = first;
			while (current.Soc != soc) {
				if (current.next == null) {
					return;
				} else {
					current = current.next;
				}

				current.username = userName;
			}
		}

		public String GetUserRoom(String username){
			User current = first;
			while (current.username != username) {
				if (current.next == null) {
					return null;
				} else {
					current = current.next;
				}
			}
			return current.room;
		}

		public int GetUserId(Socket soc)
		{
			User current = first;
			while (current.Soc != soc) {
				Debug.Log (current.actorId);
				if (current.next == null) {
					return -1;
				} else {
					current = current.next;
				}
			}
			return current.actorId;
		}
	}
}

