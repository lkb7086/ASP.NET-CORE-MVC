using System.Text;

namespace tubaNWeb.CommonClass
{
	public class LogManager : Singleton<LogManager>
	{
		private static object logLock = new Object();

		public void Log(string data)
		{
			lock (logLock)
			{
				System.IO.File.AppendAllText("Log.txt", DateTime.Now + "\n" + data + "\n\n", Encoding.UTF8);
			}
		}
	}
}
