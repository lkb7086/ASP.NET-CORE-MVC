using tubaNWeb.CommonClass;
using tubaNWeb.Models.Preview;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;

namespace tubaNWeb.Controllers
{
	public class AccountController : Controller
	{
		[HttpGet]
		public IActionResult Login()
		{
			if (HttpContext.Session.GetString("UserId") != null)
			{
                return RedirectToAction("Title", "PreviewAlbum", new { sort = 1 });
            }

            ViewData["LoginID"] = Request.Cookies["LoginID"];
			ViewData["LoginPassword"] = Request.Cookies["LoginPassword"];
			ViewData["CurProcessNumber"] = 1;			

			// 공지사항
			if (HttpContext.Session.GetInt32("Notice") == null)
			{
				string notice = System.IO.File.ReadAllText("D:/VideoManagerData/_data/notice.txt");
				notice = notice.Replace("\r", string.Empty);
				notice = notice.Replace("\n", "<br>");
				//ViewData["NoticeAlert"] = string.Format("<script>alert('{0}');</script>", notice);
				ViewData["NoticeAlert"] = string.Format("<script>Swal.fire('공지', '{0}', 'info');</script>", notice);
				HttpContext.Session.SetInt32("Notice", 1);
			}

			return View();
		}

		[HttpPost]
		public IActionResult Login(User model)
		{
			string loginID = Request.Cookies["LoginID"];
			string loginPassword = Request.Cookies["LoginPassword"];

			if (loginID == null)
			{
				ViewData["LoginID"] = model.UserId;
			}
			else
			{
				ViewData["LoginID"] = Request.Cookies["LoginID"];
			}

			if (loginPassword == null)
			{
				ViewData["LoginPassword"] = model.Password;
			}
			else
			{
				ViewData["LoginPassword"] = Request.Cookies["LoginPassword"];
			}

			ViewData["CurProcessNumber"] = model.ProcessNumber;

			// 접근권한 검사
			string remoteIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
			if (remoteIpAddress != "::1") // 로컬이 아닐때만 검사
			{
				LogManager.Instance.Log("아이피 확인: " + remoteIpAddress);
				if (remoteIpAddress.Contains("192.168") == false)
				{
					ViewData["Alert"] = "<script>Swal.fire('', '유효하지 않은 접속입니다.', 'error');</script>";
					return View();
				}
			}

			// 로그인 상태면 바로 보내기
			if (HttpContext.Session.GetString("UserId") != null)
			{
				return RedirectToAction("Title", "PreviewAlbum", new { sort = 1 });
			}
			
			if (model.ProcessNumber == 1) // 로그인이면(1번)
			{
				if (ModelState.IsValid) // 모든 입력값이 입력되었는지
				{
					using (MySqlConnection conn = DBManager.Instance.PreviewConnection)
					{
						string query = string.Format("SELECT COUNT(*), user FROM videomanager.account WHERE id = '{0}' AND pass = '{1}'", model.UserId, model.Password);
						MySqlCommand cmd = new MySqlCommand(query, conn);
						try
						{
							int row = 0;
							sbyte user = 0;
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    row = reader.GetInt32(0);
                                    user = reader.GetSByte(1);
                                }
                            }

							if (row > 0)
							{
								// 세션
								HttpContext.Session.SetString("UserId", model.UserId);
								HttpContext.Session.SetInt32("User", user);

								// 쿠키
								CookieOptions options = new CookieOptions();
								options.Expires = DateTime.Now.AddDays(365);
								Response.Cookies.Append("LoginID", model.UserId, options);
								Response.Cookies.Append("LoginPassword", model.Password, options);

                                TempData.Clear();

                                return RedirectToAction("Title", "PreviewAlbum", new { sort = 1 });
							}
							else
							{
								//ModelState.AddModelError(string.Empty, "사용자 ID 혹은 비밀번호가 올바르지 않습니다.");
								//return Content("<script language='javascript' type='text/javascript'> alert('에러다.'); </script>");
								//return Content("<script language='javascript' type='text/javascript'>alert('Thanks for Feedback!');</script>");
								ViewData["Alert"] = "<script>Swal.fire('', '사용자 ID 혹은 비밀번호가 올바르지 않습니다.', 'warning');</script>";
							}
						}
						catch (MySqlException ex)
						{
							LogManager.Instance.Log(ex.ToString());
						}
					}
				}
			}
			else if (model.ProcessNumber == 2)
			{
				if (ModelState.IsValid)
				{
					// "select id from videomanager.account where id = '%s' and pass = '%s'"
					using (MySqlConnection conn = DBManager.Instance.PreviewConnection)
					{
						string query = string.Format("SELECT COUNT(*) FROM videomanager.account WHERE id = '{0}' AND pass = '{1}'", model.UserId, model.Password);
						MySqlCommand cmd = new MySqlCommand(query, conn);
						try
						{
							int row = Convert.ToInt32(cmd.ExecuteScalar());
							if (row > 0)
							{
								if (model.NewPassword.Equals(model.CheckNewPassword))
								{
									query = string.Format("UPDATE videomanager.account SET pass = '{0}' WHERE id = '{1}'", model.NewPassword, model.UserId);
									cmd.CommandText = query;
									cmd.ExecuteNonQuery();
									ViewData["Alert"] = "<script>Swal.fire('', '비밀번호가 변경 되었습니다.', 'success');</script>";
								}
								else
								{
									ViewData["Alert"] = "<script>Swal.fire('', '새로운 비밀번호가 일치하지 않습니다. 입력하신 내용을 다시 확인해 주세요.', 'warning');</script>";
								}
							}
							else
							{
								ViewData["Alert"] = "<script>Swal.fire('', '계정정보가 일치하지 않습니다. 입력하신 내용을 다시 확인해 주세요.', 'warning');</script>";
							}
						}
						catch (MySqlException ex)
						{
							LogManager.Instance.Log(ex.ToString());
						}
					}
				}
				else
				{
					ViewData["Alert"] = "<script>Swal.fire('', '비밀번호 변경패널의 모든 사항을 기입해주세요.', 'warning');</script>";
				}
			}

			return View();
		}

		public IActionResult Logout()
		{
			bool isNotice = false;
			if (HttpContext.Session.GetInt32("Notice") != null)
			{
				isNotice = true;
			}

			bool isAccess = false;
			if (HttpContext.Session.GetInt32("Access") != null)
			{
				if (HttpContext.Session.GetInt32("Access") == 1)
					isAccess = true;
			}

			HttpContext.Session.Clear();

			if (isNotice)
			{
				HttpContext.Session.SetInt32("Notice", 1);
			}

			if (isAccess)
			{
				HttpContext.Session.SetInt32("Access", 1);
			}

			return RedirectToAction("Login", "Account");
		}









	}
}
