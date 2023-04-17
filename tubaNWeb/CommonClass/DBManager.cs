using MySql.Data.MySqlClient;

namespace tubaNWeb.CommonClass
{
	public class DBManager : Singleton<DBManager>
	{
		public MySqlConnection PreviewConnection
		{
			get
			{
				MySqlConnection connection = new MySqlConnection
				{
					ConnectionString = "server=127.0.0.1;UserID=root;Password=1234;Database=videomanager;SslMode=none;allowPublicKeyRetrieval=true"
				};
				connection.Open();
				return connection;
			}
		}

		public MySqlConnection PipelineConnection
		{
			get
			{
				MySqlConnection connection = new MySqlConnection
				{
					ConnectionString = "server=127.0.0.1;UserID=root;Password=1234;Database=pipeline;SslMode=none;allowPublicKeyRetrieval=true"
				};
				connection.Open();
				return connection;
			}
		}
	}




}
