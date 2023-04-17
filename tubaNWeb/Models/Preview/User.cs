using System.ComponentModel.DataAnnotations;

namespace tubaNWeb.Models.Preview
{
    public class User
    {
        [Required(ErrorMessage = "아이디를 입력해주세요.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
        public string Password { get; set; }

		[Required(ErrorMessage = "새 비밀번호를 입력해주세요.")]
		public string NewPassword { get; set; }
		[Required(ErrorMessage = "새 비밀번호를 확인해주세요.")]
		public string CheckNewPassword { get; set; }
        public int ProcessNumber { get; set; }
	}
}
