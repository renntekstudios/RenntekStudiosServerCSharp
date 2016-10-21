using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTServer
{
    class Debug
    {
		String currentLogDate = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
        public static void Log(object output)
        {
			string logText = "<" + DateTime.Now.ToString () + "> " + output;
			//WriteLog (logText);
			Console.WriteLine(logText);
        }

		public static void LogWarning(object output)
		{
			string logText = "<" + DateTime.Now.ToString () + "> " + output;
			//WriteLog (logText);
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(logText);
			Console.ResetColor ();
		}

		public static void LogError(object output)
		{
			string logText = "<" + DateTime.Now.ToString () + "> " + output;
			//WriteLog (logText);
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(logText);
			Console.ResetColor ();
		}

		private void WriteLog(string logText){
			using (StreamWriter file = new StreamWriter (Directory.GetCurrentDirectory () + "/logs/log-" + currentLogDate + ".txt", true)) {
				file.WriteLine(logText);
			}
		}
    }
}
