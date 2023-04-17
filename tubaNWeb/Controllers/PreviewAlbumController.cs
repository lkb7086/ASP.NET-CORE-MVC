using tubaNWeb.CommonClass;
using tubaNWeb.Models.Preview;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace tubaNWeb.Controllers
{
	public class PreviewAlbumController : Controller
    {
		[HttpGet]
		public IActionResult Title(int sort)
        {
			HttpContext.Session.SetString("MainMenu", "Preview");

			if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

			if (sort <= 0)
			{
				return RedirectToAction("Login", "Account");
			}

			List<Video> titleList = new List<Video>();
            using (MySqlConnection conn = DBManager.Instance.PreviewConnection)
			{
				string query = string.Empty;

				switch (sort)
				{
					case 1:
						query = string.Format("SELECT uid, name, imagelink, modifydate, sortnumber FROM videomanager.title ORDER BY modifydate ASC");
						ViewData["SortName"] = "수정일 (오름차순)";
						break;
					case 2:
						query = string.Format("SELECT uid, name, imagelink, modifydate, sortnumber FROM videomanager.title ORDER BY modifydate DESC");
						ViewData["SortName"] = "수정일 (내림차순)";
						break;
					case 3:
						query = string.Format("SELECT uid, name, imagelink, modifydate, sortnumber FROM videomanager.title ORDER BY sortnumber ASC");
						ViewData["SortName"] = "번호 (오름차순)";
						break;
					case 4:
						query = string.Format("SELECT uid, name, imagelink, modifydate, sortnumber FROM videomanager.title ORDER BY sortnumber DESC");
						ViewData["SortName"] = "번호 (내림차순)";
						break;

					default:
						query = string.Format("SELECT uid, name, imagelink, modifydate, sortnumber FROM videomanager.title ORDER BY modifydate ASC");
						ViewData["SortName"] = "수정일 (오름차순)";
						break;
				}

				MySqlCommand cmd = new MySqlCommand(query, conn);
				try
				{
					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							Video video= new Video();
                            video.UID = reader.GetUInt32("uid");
                            video.SortNumber = reader.GetUInt32("sortnumber");
                            video.Name = reader.GetString("name");
                            video.ImageLink = reader.GetString("imagelink");
							video.Date = reader.GetString("modifydate");
							titleList.Add(video);
                        }
					}
				}
				catch (MySqlException ex)
				{
					LogManager.Instance.Log(ex.ToString());
				}
			}

			ViewBag.TitleList = titleList;

            return View();
        }

		[HttpGet]
		public IActionResult Episode(uint uid, int sort)
		{
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

			if (uid <= 0 || sort <= 0)
			{
				return RedirectToAction("Login", "Account");
			}

			HttpContext.Session.SetInt32("CurTitle", (int)uid);			
			List<EpisodeData> episodeList = new List<EpisodeData>();
            using (MySqlConnection conn = DBManager.Instance.PreviewConnection)
            {
                string query = string.Empty;

				switch (sort)
                {
                    case 1:
						query = string.Format("SELECT * FROM videomanager.episode WHERE titleuid = {0} ORDER BY modifydate ASC", uid);
						ViewData["SortName"] = "수정일 (오름차순)";
						break;
					case 2:
						query = string.Format("SELECT * FROM videomanager.episode WHERE titleuid = {0} ORDER BY modifydate DESC", uid);
						ViewData["SortName"] = "수정일 (내림차순)";
						break;
					case 3:
						query = string.Format("SELECT * FROM videomanager.episode WHERE titleuid = {0} ORDER BY sortnumber ASC", uid);
						ViewData["SortName"] = "번호 (오름차순)";
						break;
					case 4:
						query = string.Format("SELECT * FROM videomanager.episode WHERE titleuid = {0} ORDER BY sortnumber DESC", uid);
						ViewData["SortName"] = "번호 (내림차순)";
						break;

					default:
						query = string.Format("SELECT * FROM videomanager.episode WHERE titleuid = {0} ORDER BY modifydate ASC", uid);
						ViewData["SortName"] = "수정일 (오름차순)";
						break;
                }

                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    List<uint> uidList = new List<uint>();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {                        
                        while (reader.Read())
                        {
                            EpisodeData episodeData = new EpisodeData();
							episodeData.EpisodeName = reader.GetString("name");
							episodeData.SortNumber = reader.GetUInt32("sortnumber");
							episodeData.Whatwork1 = reader.GetByte("whatwork1");
							episodeData.Whatwork2 = reader.GetByte("whatwork2");
							episodeData.Whatwork3 = reader.GetByte("whatwork3");

							episodeData.ImagePath = reader.GetString("imagelink");
							episodeData.AnimeticPath = reader.GetString("videolink1");
							episodeData.AnimationPath = reader.GetString("videolink2");
							episodeData.FinalPath = reader.GetString("videolink3");

							episodeData.Date = reader.GetString("modifydate");
							episodeData.Uid = reader.GetUInt32("uid");
							episodeList.Add(episodeData);
                            uidList.Add(episodeData.Uid);
                        }  
                    }

                    for (int i = 0; i < uidList.Count; i++)
                    {
                        query = string.Format("SELECT avg(star) FROM videomanager.comment WHERE episodeuid = {0}", uidList[i]);
                        cmd.CommandText = query;

                        object scalar = cmd.ExecuteScalar();
						if (scalar is DBNull)
						{
                            episodeList[i].Rating = 0f;
                            continue;
						}

                        float rating = (float)Convert.ToDouble(scalar);
                        float fixedRating = (float)Math.Round(rating * 2, MidpointRounding.ToPositiveInfinity) / 2; // MidpointRounding.AwayFromZero
						episodeList[i].Rating = fixedRating;
                    }
                }
                catch (MySqlException ex)
                {
                    LogManager.Instance.Log(ex.ToString());
					return View(ex.ToString());
				}
            }

            ViewBag.EpisodeList = episodeList;
			ViewBag.titleUID = uid;


			return View();
		}
	}
}
