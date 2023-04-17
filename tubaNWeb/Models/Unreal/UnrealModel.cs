using tubaNWeb.CommonClass;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace tubaNWeb.Models.Unreal
{
	public class UnrealModel
	{
		//private static object lockObject = new Object();

		const int MAX_SPLIT_NUM = 7;

		//Impersonation functionality
		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		//Disconnection after file operations
		[DllImport("kernel32.dll")]
		private static extern Boolean CloseHandle(IntPtr hObject);

		private enum ApplyStateType
		{
			Fbx,
			Uasset,
			Sequence,
			MAX
		};

		public JObject? ImportEpisode(JObject recvJObject)
		{
			JObject sendJObject = new JObject();
			string tableName;
			int time;
			int updateResult;

			try
			{
				tableName = recvJObject["TableName"].ToString();
				bool fbxResult = false;
				time = UpdateManager.Instance.GetUpdateTime(tableName);
				if (time < UpdateManager.DEFAULT_UPDATE_TIME)
				{
					updateResult = 0;
				}
				else
				{
					fbxResult = FindFbxDirectory2(tableName, SearchOption.AllDirectories);
					updateResult = 1;
				}

				time = UpdateManager.DEFAULT_UPDATE_TIME - time;

				if (fbxResult == false && updateResult == 1)
				{
					LogManager.Instance.Log("ImportEpisode / 전체갱신 실패");
					sendJObject.Add("Type", "ImportEpisode");
					sendJObject.Add("Result", "Fail");
					return sendJObject;
				}
			}
			catch (Exception ex)
			{
				LogManager.Instance.Log(ex.ToString());

				sendJObject.Add("Type", "ImportEpisode");
				sendJObject.Add("Result", "Fail");
				return sendJObject;
			}

			string query = string.Format("SELECT episodeNum FROM {0} WHERE episodeNum != '' GROUP BY episodeNum ORDER BY episodeNum", tableName);
			JArray sendJArray = new JArray();
			using (MySqlConnection conn = DBManager.Instance.PipelineConnection)
			{
				MySqlCommand cmd = new MySqlCommand(query, conn);
				try
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							JObject jObject = new JObject();
							jObject.Add("EpisodeNum", reader.GetString(0));
							sendJArray.Add(jObject);
						}
					}
				}
				catch (MySqlException ex)
				{
					LogManager.Instance.Log(ex.ToString());

					sendJObject.Add("Type", "ImportEpisode");
					sendJObject.Add("Result", "Fail");
					return sendJObject;
				}
			}

			sendJObject.Add("Type", "ImportEpisode");
			sendJObject.Add("Result", "Success");
			sendJObject.Add("UpdateResult", updateResult);
			if (updateResult == 0)
				sendJObject.Add("UpdatedTime", time);
			sendJObject.Add("EpisodeNumArray", sendJArray);
			return sendJObject;
		}

		public JObject? ImportSceneCut(JObject recvJObject)
		{
			return null;
		}

		public JObject? ChangeState(JObject recvJObject)
		{
			JObject sendJObject = new JObject();
			string tableName;
			string assetArray;
			JArray jArray;
			try
			{
				tableName = recvJObject["TableName"].ToString();
				assetArray = recvJObject["AssetArray"].ToString();
				jArray = JArray.Parse(assetArray);
			}
			catch (Exception ex)
			{
				LogManager.Instance.Log(ex.ToString());

				sendJObject.Add("Type", "ChangeState");
				sendJObject.Add("Result", "Fail");
				return sendJObject;
			}

			foreach (JObject item in jArray)
			{
				string assetName;
				string fieldName;
				int hasExcuted;
				try
				{
					assetName = item["AssetName"].ToString();
					fieldName = item["FieldName"].ToString();
					hasExcuted = int.Parse(item["HasExcuted"].ToString());
				}
				catch (Exception ex)
				{
					LogManager.Instance.Log(ex.ToString());

					sendJObject.Add("Type", "ChangeState");
					sendJObject.Add("Result", "Fail");
					return sendJObject;
				}

				string applyState;
				switch ((ApplyStateType)hasExcuted)
				{
					case ApplyStateType.Fbx:
						applyState = "Fbx";
						break;
					case ApplyStateType.Uasset:
						applyState = "Uasset";
						break;
					case ApplyStateType.Sequence:
						applyState = "Sequence";
						break;
					case ApplyStateType.MAX:
						applyState = "MAX";
						break;
					default:
						applyState = "?";
						break;
				}

				string query = string.Format("UPDATE {0} SET {1} = '{2}' WHERE FileName = '{3}'", tableName, fieldName, applyState, assetName);
				using (MySqlConnection conn = DBManager.Instance.PipelineConnection)
				{
					MySqlCommand cmd = new MySqlCommand(query, conn);
					try
					{
						int row = cmd.ExecuteNonQuery();
					}
					catch (MySqlException ex)
					{
						LogManager.Instance.Log(ex.ToString());
						sendJObject.Add("Type", "ChangeState");
						sendJObject.Add("Result", "Fail");
						return sendJObject;
					}
				}
			}

			sendJObject.Add("Type", "ChangeState");
			sendJObject.Add("Result", "Success");
			return sendJObject;
		}

		public JObject? UnrealLog(JObject recvJObject)
		{
			JObject sendJObject = new JObject();
			string project;
			string name;
			string verbosity;
			string log;
			try
			{
				project = recvJObject["Project"].ToString();
				verbosity = recvJObject["Verbosity"].ToString();
				log = recvJObject["Log"].ToString();
				name = recvJObject["Name"].ToString();
			}
			catch (Exception ex)
			{
				LogManager.Instance.Log(ex.ToString());

				sendJObject.Add("Type", "UnrealLog");
				sendJObject.Add("Result", "Fail");
				return sendJObject;
			}

			project += "_log";
			string query = string.Format("INSERT INTO {0} VALUES (null, '{1}', '{2}', '{3}', now())", project, name, verbosity, log);
			using (MySqlConnection conn = DBManager.Instance.PipelineConnection)
			{
				MySqlCommand cmd = new MySqlCommand(query, conn);
				try
				{
					int row = cmd.ExecuteNonQuery();
				}
				catch (MySqlException ex)
				{
					LogManager.Instance.Log(ex.ToString());

					sendJObject.Add("Type", "UnrealLog");
					sendJObject.Add("Result", "Fail");
					return sendJObject;
				}
			}

			sendJObject.Add("Type", "UnrealLog");
			sendJObject.Add("Result", "Success");
			return sendJObject;
		}

		public JObject? TableName()
		{
			JObject sendJObject = new JObject();
			JArray tableJArray = new JArray();
			JArray driveJArray = new JArray();
			JArray fpsJArray = new JArray();

			int i = 0;
			using (MySqlConnection conn = DBManager.Instance.PipelineConnection)
			{
				string query = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'pipeline' ORDER BY CREATE_TIME");
				MySqlCommand cmd = new MySqlCommand(query, conn);
				try
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							if (reader.GetString(0).Equals("project") || reader.GetString(0).Contains("_log"))
								continue;
							JObject jObject = new JObject();
							jObject.Add("Table", reader.GetString(0));
							tableJArray.Add(jObject);
							i++;
						}
					}
				}
				catch (MySqlException ex)
				{
					LogManager.Instance.Log(ex.ToString());
					sendJObject.Add("Type", "TableName");
					sendJObject.Add("Result", "Fail");
					return sendJObject;
				}

				if (i == 0)
				{
					LogManager.Instance.Log("table is empty");
					sendJObject.Add("Type", "TableName");
					sendJObject.Add("Result", "Fail");
					return sendJObject;
				}

				query = string.Format("SELECT episodePath, fps FROM project ORDER BY date");
				cmd.CommandText = query;
				try
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							JObject jObject = new JObject();
							string drive = reader.GetString("episodePath");
							drive = drive.Substring(0, drive.IndexOf('/') + 1);
							jObject.Add("Drive", drive);
							driveJArray.Add(jObject);

							JObject jObject2 = new JObject();
							string fps = reader.GetString("fps");
							jObject2.Add("FPS", fps);
							fpsJArray.Add(jObject2);
						}
					}
				}
				catch (MySqlException ex)
				{
					LogManager.Instance.Log(ex.ToString());
					sendJObject.Add("Type", "TableName");
					sendJObject.Add("Result", "Fail");
					return sendJObject;
				}
			}

			sendJObject.Add("Type", "TableName");
			sendJObject.Add("Result", "Success");
			sendJObject.Add("TableArray", tableJArray);
			sendJObject.Add("DriveArray", driveJArray);
			sendJObject.Add("FPSArray", fpsJArray);
			return sendJObject;
		}

		public bool FindFbxDirectory2(string tableName, SearchOption searchOption)
		{			
			return true;
		}









	}
}
