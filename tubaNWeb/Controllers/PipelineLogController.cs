using tubaNWeb.CommonClass;
using tubaNWeb.Models.Pipeline;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace tubaNWeb.Controllers
{
	public class PipelineLogController : Controller
	{
		private static readonly int PAGE_LIMIT = 5000;

		public IActionResult LogInformation(UnrealLogParameter unrealLogParameter)
		{
			if (HttpContext.Session.GetInt32("User") == null || HttpContext.Session.GetInt32("User") == 0)
			{
				return RedirectToAction("Login", "Account");
			}

			List<UnrealLogData> unrealLogDataList = new List<UnrealLogData>();
			List<TableData> tableList = new List<TableData>();
			int rowCount = 0;
			bool isPagination = false;

			if (unrealLogParameter.Page == 0)
			{
				unrealLogParameter.Page = 1;
			}

			int offset = (unrealLogParameter.Page - 1) * PAGE_LIMIT;

			// SearchDate가 null이면 전체 날짜
			if (unrealLogParameter.SearchDate1 == null)
				unrealLogParameter.SearchDate1 = string.Empty;
			if (unrealLogParameter.SearchDate2 == null)
				unrealLogParameter.SearchDate2 = string.Empty;

			using (MySqlConnection conn = DBManager.Instance.PipelineConnection)
			{
				lock (CommonDefine.PipelineLock)
				{
					string query = string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'pipeline' ORDER BY CREATE_TIME");
					MySqlCommand cmd = new MySqlCommand(query, conn);
					try
					{
						using (MySqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								if (!reader.GetString(0).Contains("_log"))
									continue;
								TableData tableData = new TableData();
								tableData.TableName = reader.GetString(0);
								if (unrealLogParameter.TableName != null && tableData.TableName.Equals(unrealLogParameter.TableName))
									tableData.IsSelected = true;
								else
									tableData.IsSelected = false;
								tableList.Add(tableData);
							}
						}
					}
					catch (MySqlException ex)
					{
						LogManager.Instance.Log(ex.ToString());
						return View(ex.ToString());
					}

					if (tableList.Count == 0)
					{
						LogManager.Instance.Log("table is empty");
						return View("table is empty");
					}

					// 처음 접속시
					if (unrealLogParameter.TableName == null)
					{
						unrealLogParameter.TableName = tableList[0].TableName;
						tableList[0].IsSelected = true;
					}

					switch (unrealLogParameter.SearchType)
					{
						case 0: // 오늘 날짜 검색
							query = string.Format("SELECT * FROM {0} WHERE DATE(time) = '{1}' LIMIT {2}", unrealLogParameter.TableName, DateTime.Today, PAGE_LIMIT);
							break;
						case 1: // 날짜 검색
							if (unrealLogParameter.SearchDate1.Equals(string.Empty))
							{
								query = string.Format("SELECT * FROM {0} LIMIT {1} OFFSET {2}", unrealLogParameter.TableName, PAGE_LIMIT, offset);
								isPagination = true;
							}
							else
								query = string.Format("SELECT * FROM {0} WHERE DATE(time) = '{1}' LIMIT {2}", unrealLogParameter.TableName, unrealLogParameter.SearchDate1, PAGE_LIMIT);
							break;
						case 2: // 이름 검색
							if (unrealLogParameter.SearchDate2.Equals(string.Empty))
							{
								if (unrealLogParameter.SearchName == null)
									unrealLogParameter.SearchName = string.Empty;

								query = string.Format("SELECT * FROM {0} WHERE name = '{1}' LIMIT {2}", unrealLogParameter.TableName, unrealLogParameter.SearchName, PAGE_LIMIT);
							}
							else
								query = string.Format("SELECT * FROM {0} WHERE name = '{1}' AND DATE(time) = '{2}' LIMIT {3}", unrealLogParameter.TableName, unrealLogParameter.SearchName, unrealLogParameter.SearchDate2, PAGE_LIMIT);
							break;
						default:
							break;
					}

					cmd = new MySqlCommand(query, conn);
					try
					{
						using (MySqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								UnrealLogData unrealLogData = new UnrealLogData();
								unrealLogData.Idx = reader.GetUInt64("idx");
								unrealLogData.Name = reader.GetString("name");
								unrealLogData.Verbosity = reader.GetString("type");
								unrealLogData.Log = reader.GetString("log");
								unrealLogData.Time = reader.GetString("time");
								unrealLogData.Type = unrealLogData.Verbosity.ToLower();
								unrealLogDataList.Add(unrealLogData);
							}
						}
					}
					catch (MySqlException ex)
					{
						LogManager.Instance.Log(ex.ToString());
						return View(ex.ToString());
					}

					if (isPagination)
					{
						query = string.Format("SELECT COUNT(*) FROM {0}", unrealLogParameter.TableName);
						cmd.CommandText = query;
						try
						{
							rowCount = Convert.ToInt32(cmd.ExecuteScalar());
						}
						catch (MySqlException ex)
						{
							LogManager.Instance.Log(ex.ToString());
							return View(ex.ToString());
						}
					}
				}
			}

			ViewData["IsPagination"] = isPagination;
			ViewData["SearchType"] = unrealLogParameter.SearchType;
			ViewData["SearchDate1"] = unrealLogParameter.SearchDate1;
			ViewData["SearchDate2"] = unrealLogParameter.SearchDate2;
			ViewData["SearchName"] = unrealLogParameter.SearchName;
			ViewData["Page"] = unrealLogParameter.Page;
			ViewData["MaxPage"] = (rowCount / PAGE_LIMIT) + 1;
			ViewBag.unrealLogDataList = unrealLogDataList;
			ViewBag.tableList = tableList;

			return View();
		}


	}
}