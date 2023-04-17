namespace tubaNWeb.Models.Pipeline
{
	public class ProjectData
	{
		public string ProjectName { get; set; }
		public string NetworkIp { get; set; }
		public string NetworkFolder { get; set; }
		public string EpisodePath { get; set; }
		public string FbxFolder { get; set; }
		public sbyte FPS { get; set; }

		public bool CheckNullWithMember()
		{
			if (ProjectName == null || NetworkIp == null || NetworkFolder == null || EpisodePath == null || FbxFolder == null)
			{
				return true;
			}
			else
				return false;
		}

		public bool CheckSmallString()
		{
			if (ProjectName.Contains('\'') || NetworkIp.Contains('\'') || NetworkFolder.Contains('\'') || EpisodePath.Contains('\'') || FbxFolder.Contains('\''))
			{
				return true;
			}
			else
				return false;
		}
	}
}
