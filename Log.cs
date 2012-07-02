using System;

namespace Samarkin.Caloverlayer
{
	static class Log
	{
		private static void Print()
		{
			Console.Error.WriteLine();
		}

		private static void Print(string s)
		{
			Console.Error.WriteLine(s);
		}

		private static void Print(string prefix, string message)
		{
			Print(string.Format("{0}: {1}", prefix, message));
		}

		public static void Error(Exception ex)
		{
			Print(ex.GetType().ToString());
			Print();
			Print(ex.Message);
		}

		public static void Error(string format, params object[] arg)
		{
			Print("ERROR", string.Format(format, arg));
		}

		public static void Warning(string format, params object[] arg)
		{
			Print("WARNING", string.Format(format, arg));
		}

		public static void Info(string format, params object[] arg)
		{
			Print("INFO", string.Format(format, arg));
		}


	}
}
