using MySql.Data.MySqlClient;

namespace tubaNWeb.CommonClass
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        public static int DEFAULT_UPDATE_TIME = 60;
        private static object updateLock = new Object();

        public int GetUpdateTime(string tableName)
        {
            int time = 0;
            lock (updateLock)
            {
                DateTime updateTime = DateTime.MinValue;
                string query = string.Format("SELECT updateTime FROM project WHERE name = '{0}'", tableName);
                using (MySqlConnection conn = DBManager.Instance.PipelineConnection)
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    try
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                updateTime = reader.GetDateTime("updateTime");
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        LogManager.Instance.Log(ex.ToString());
                        return 0;
                    }

                    TimeSpan ts = DateTime.Now - updateTime;
                    time = (int)ts.TotalSeconds;
                    if (time >= DEFAULT_UPDATE_TIME)
                    {
                        query = string.Format("UPDATE project SET updateTime = now() WHERE name = '{0}'", tableName);
                        cmd.CommandText = query;
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (MySqlException ex)
                        {
                            LogManager.Instance.Log(ex.ToString());
                            return 0;
                        }
                    }
                }
            }

            return time;
        }
    }


}
