using UnityEngine;
using System.Collections;

namespace RennTekNetworking
{
	public class RTArray : MonoBehaviour {

		public static byte[] copyOfRange(byte[] sourceArray, int start, int stop){
			int size = stop - start;
			byte[] temp = new byte[size];

			for (int i = 0; start < stop; i++, start++) {
				temp [i] = sourceArray [start];
			}

			return temp;
		}
	}
}