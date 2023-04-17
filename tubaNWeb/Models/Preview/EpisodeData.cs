namespace tubaNWeb.Models.Preview
{
    public class EpisodeData
    {
        public string EpisodeName { get; set; }
        public uint SortNumber { get; set; }
        public int Whatwork1 { get; set; }
        public int Whatwork2 { get; set; }
        public int Whatwork3 { get; set; }
        public string ImagePath { get; set; }
        public string AnimeticPath { get; set; }
        public string AnimationPath { get; set; }
        public string FinalPath { get; set; }
        public uint Uid { get; set; }
		public float Rating { get; set; }
		public string Date { get; set; }

        public void CheckNull()
        {
            EpisodeName = (EpisodeName == null) ? string.Empty : EpisodeName;
            ImagePath = (ImagePath == null) ? string.Empty : ImagePath;
            AnimeticPath = (AnimeticPath == null) ? string.Empty : AnimeticPath;
            AnimationPath = (AnimationPath == null) ? string.Empty : AnimationPath;
            FinalPath = (FinalPath == null) ? string.Empty : FinalPath;
            Date = (Date == null) ? string.Empty : Date;
        }
	}
}
