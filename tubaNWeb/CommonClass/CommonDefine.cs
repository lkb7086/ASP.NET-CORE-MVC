namespace tubaNWeb.CommonClass
{
	public static class CommonDefine
	{
		public enum User
		{
			normal,
			member,
			admin
		}

		public static object PipelineLock = new Object();
		public const string DEFAULT_DIR = "/VideoManagerData/";
	}
}
