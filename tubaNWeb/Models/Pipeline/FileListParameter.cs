namespace tubaNWeb.Models.Pipeline
{
	public class FileListParameter
	{
		public int Page { get; set; }
		public string Episode { get; set; }
		public string Scene { get; set; }
		public string Cut { get; set; }
		public string TableName { get; set; }
		public int SearchType { get; set; }
		public string SearchFilter { get; set; }
		public string SearchString { get; set; }
	}
}
