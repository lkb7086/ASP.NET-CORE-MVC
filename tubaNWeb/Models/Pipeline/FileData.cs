namespace tubaNWeb.Models.Pipeline
{
    public class FileData
    {
        public ulong Idx { get; set; }
        public string EpisodeNum { get; set; }
        public string SceneNum { get; set; }
        public string CutNum { get; set; }
        public string DirPath { get; set; }
        public string FileName { get; set; }
        public string PolygonType { get; set; }
        public string AssetType { get; set; }
        public string DataType { get; set; }
        public string AssetVer { get; set; }
        public sbyte FrameAddVal { get; set; }
        public string ApplyState { get; set; }

        public long CreateTick { get; set; }
        public long CheckTick { get; set; }

		public string Active { get; set; }
	}




}
