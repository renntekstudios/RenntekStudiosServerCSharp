using System;
using System.Reflection;
using System.Net.Sockets;
using System.Globalization;

namespace RTServer
{
	public class RTConverter
	{
		/// <summary>
		/// Converts the parameters.
		/// </summary>
		public static object[] ConvertParams(object[] startParams, ParameterInfo[] targetParams){
			object[] endParamaters = new object[targetParams.Length];

			if(startParams.Length == targetParams.Length){
				for(int i = 0; i < targetParams.Length; i++){
					try{
						if(targetParams[i].ParameterType == typeof(Int32)){
							endParamaters[i] = System.Convert.ToInt32(startParams[i]);
						}else if(targetParams[i].ParameterType == typeof(float)){
							endParamaters[i] = float.Parse (startParams[i].ToString(), CultureInfo.InvariantCulture.NumberFormat);
						} else if(targetParams[i].ParameterType == typeof(String)){
							endParamaters[i] = startParams[i];
						} else if(targetParams[i].ParameterType == typeof(Socket)){
							endParamaters[i] = startParams[i];
						} else {
							Debug.LogWarning("Paramater is not a valid RPC call type!");
							return null;
						}
					}catch(Exception){
						Debug.LogWarning("Paramater " + i + " does not match target type");
						return null;
					}
				}
				return endParamaters;
			} else {
				Debug.LogWarning("Paramaters count do not match for RPC!");
				return null;
			}
		}
	}
}

