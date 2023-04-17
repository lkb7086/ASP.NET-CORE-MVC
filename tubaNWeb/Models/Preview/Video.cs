namespace tubaNWeb.Models.Preview
{
	public class Video
	{
		public uint UID { get; set; }
        public uint SortNumber { get; set; }
        public string Name { get; set; }
        public string ImageLink { get; set; }
		public string VideoLink { get; set; }
        public float Rating { get; set; }
		public string Date { get; set; }
	}
}
