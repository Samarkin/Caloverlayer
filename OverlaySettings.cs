using System;
using System.IO;

namespace Samarkin.Caloverlayer
{
	class OverlaySettings
	{
		public OverlaySettings()
		{
			FontFamily = "Segoe UI Semibold";
			WorkDays = 5;
			Year = DateTime.Now.Year;
			Month = DateTime.Now.Month;
			WholeYear = false;
			VerticalOffset = 0;
			Size = 500;
		}

		public bool WholeYear { get; set; }
		public int Year { get; set; }
		public int? Month { get; set; }
		public int WorkDays { get; set; }
		public int VerticalOffset { get; set; }
		public string Culture { get; set; }
		public string FontFamily { get; set; }

		public int Size { get; set; }
		private string _filename;
		public string Filename
		{
			get { return _filename; }
			set
			{
				_filename = value;
				_outputFileName = Path.GetFileNameWithoutExtension(Filename) + "_overlay.png";
			}
		}

		private string _outputFileName;
		public string OutputFileName
		{
			get
			{
				string filename;
				int n = 0;
				do
				{
					filename = Path.GetFileNameWithoutExtension(_outputFileName)
						+ (n > 0 ? string.Format("({0})", n) : string.Empty) + Path.GetExtension(_outputFileName);
					n++;
				} while (File.Exists(filename));
				return filename;
			}
			set { _outputFileName = value; }
		}
	}
}
