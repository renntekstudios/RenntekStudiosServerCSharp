using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace RTServer
{
	public class Room
	{
		public int roomId = -1;
		public Socket creatorSoc;
		public String roomName;
		public List<Socket> userSocs;
		public List<string> bufferedRPC;
		public int max;
		public Room next;

		public Room (int roomId, Socket creatorSoc, string roomName, List<Socket> userSocs, int max)
		{
			this.roomId = roomId;
			this.creatorSoc = creatorSoc;
			this.roomName = roomName;
			this.userSocs = userSocs;
			this.max = max;
		}
	}

	public class RoomList
	{
		private Room first;
		public int numRooms = 0;
		public int num = 0
			;

		public RoomList(){
			first = null;
		}

		public int Length{
			get{
				return num;
			}
		}

		public bool isEmpty {
			get{
				return (first == null);
			}
		}

		public void Add(Socket creatorSoc, string roomName, int max){
			numRooms++;
			num++;
			List<Socket> userSocs = new List<Socket> ();
			userSocs.Add (creatorSoc);
			Room newRoom = new Room (numRooms * 1000, creatorSoc, roomName, userSocs, max);
			newRoom.next = first;
			first = newRoom;
			Debug.Log ("New room has been created!");
		}

		public bool RoomExist(string roomName){
			Room current = first;
			if (current != null) {
				while (current.roomName != roomName) {
					if (current.next == null) {
						return false;
					} else {
						current = current.next;
					}
				}
				return true;
			}

			return false;
		}

		public void AddUserToRoom(string roomName, Socket newUserSoc){
			Room current = first;
			while (current.roomName != roomName) {
				if (current.next == null) {
					return;
				} else {
					current = current.next;
				}
			}

			current.userSocs.Add (newUserSoc);
		}

		public bool ContainsUser(string roomName, Socket userSoc){
			Room current = first;
			while (current.roomName != roomName) {
				if (current.next == null) {
					return false;
				} else {
					current = current.next;
				}
			}

			if(current.userSocs.Contains(userSoc)){
				return true;
			} else{
				return false;
			}
		}

		public int UserCount(string roomName){
			Room current = first;
			while (current.roomName != roomName) {
				if (current.next == null) {
					return -1;
				} else {
					current = current.next;
				}
			}
			return current.userSocs.Count;
		}

		/// <summary>
		/// Remove the specified room.
		/// </summary>
		public bool Remove(String roomName){
			Room current = first;
			Room previous = null;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return false;
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

			return true;
		}

		/// <summary>
		/// Removes the user from a room.
		/// </summary>
		public bool Remove(string roomName, Socket userSoc){
			Room current = first;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return false;
				} else {
					current = current.next;
				}
			}

			current.userSocs.Remove (userSoc);

			if (current.userSocs.Count!= 0) {
				if (current.creatorSoc == userSoc) {
					Random rnd = new Random ();
					current.creatorSoc = current.userSocs [rnd.Next (0, current.userSocs.Count)];
				}
			}

			return true;
		}

		public bool isFull(string roomName) {
			Room current = first;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return false;
				} else {
					current = current.next;
				}
			}

			Debug.Log (current.userSocs.Count + " " + current.max);
			if (current.userSocs.Count == current.max) {
				
				return true;
			}

			return false;
		}

		public List<Socket> GetRoomUsers(string roomName){
			Room current = first;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return null;
				} else {
					current = current.next;
				}
			}

			return current.userSocs;
		}

		public int GetRoomId(string roomName){
			Room current = first;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return -1000;
				} else {
					current = current.next;
				}
			}

			return current.roomId;
		}

		public Socket GetRoomMaster(string roomName){
			Room current = first;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return null;
				} else {
					current = current.next;
				}
			}

			return current.creatorSoc;
		}

		public void AddBufferedRPC(string roomName, string RPC){
			Room current = first;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return;
				} else {
					current = current.next;
				}

				current.bufferedRPC.Add (RPC);
			}
		}

		public List<string> GetBufferedRPC(string roomName){
			Room current = first;

			while (current.roomName != roomName) {
				if (current.next == null) {
					return null;
				} else {
					current = current.next;
				}
			}

			return current.bufferedRPC;
		}
	}
}

