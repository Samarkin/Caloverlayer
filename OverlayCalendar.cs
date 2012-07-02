using System;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Drawing;

namespace Samarkin.Caloverlayer
{
	class OverlayCalendar
	{
		private readonly OverlaySettings _settings;
		public OverlayCalendar(OverlaySettings settings)
		{
			_settings = settings;
		}

		private void FillRoundedRectangle(Graphics graphics, Pen pen, Brush brush, int radius, int x, int y, int width, int height)
		{
			radius = Math.Min(Math.Min(radius, width / 2), height / 2);

			var path = new GraphicsPath();
			int r = radius * 2;
			path.AddArc(x, y, r, r, 180, 90);
			path.AddLine(x + radius, y, x + width - radius, y);
			path.AddArc(x + width - r, y, r, r, 270, 90);
			path.AddLine(x + width, y + radius, x + width, y + height - radius);
			path.AddArc(x + width - r, y + height - r, r, r, 0, 90);
			path.AddLine(x + radius, y + height, x + width - radius, y + height);
			path.AddArc(x, y + height - r, r, r, 90, 90);
			path.AddLine(x, y + radius, x, y + height - radius);

			graphics.FillPath(brush, path);
			graphics.DrawPath(pen, path);
		}

		private bool IsWeekend(DayOfWeek dayOfWeek)
		{
			if (_settings.WorkDays >= 7) return false;
			if (dayOfWeek == DayOfWeek.Sunday) return true;
			if ((int)dayOfWeek > _settings.WorkDays) return true;
			return false;
		}

		public Bitmap CreateOverlay(CultureInfo culture, int year, int month)
		{
			var bmp = new Bitmap(1000, 1000);
			using (var g = Graphics.FromImage(bmp))
			{
				var now = new DateTime(year, month, 1);
				FillRoundedRectangle(g,
					new Pen(Color.FromArgb(100, 200, 200, 200), 4),
					new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(100, 180, 180, 180)),
					20, 20, 20, 960, 960);

				FillRoundedRectangle(g,
					new Pen(Color.FromArgb(100, 200, 200, 200), 4),
					new SolidBrush(Color.FromArgb(200, 20, 20, 20)),
					20, 40, 40, 920, 820);

				var family = _settings.FontFamily;
				var bigFont = new Font(family, 64);
				var smFont = new Font(family, 48);
				var mnFont = new Font(family, 36);
				var tnFont = new Font(family, 28);
				var brush = Brushes.White;
				var weBrush = Brushes.Red;

				// month name
				var mm = now.ToString("MMMM", culture);
				var sz = g.MeasureString(mm, bigFont);

				g.DrawString(mm, bigFont, brush, (1000 - sz.Width) / 2, 60);

				const int w = (920 - 80) / 7;

				var day = culture.DateTimeFormat.FirstDayOfWeek;
				// day of week names
				for (int i = 0; i < 7; i++)
				{
					var dd = culture.DateTimeFormat.GetAbbreviatedDayName(day);
					int x = 80 + w * i;
					sz = g.MeasureString(dd, tnFont);
					g.DrawString(dd, tnFont, IsWeekend(day) ? weBrush : brush, x + (w - sz.Width) / 2, 200);
					day = (DayOfWeek)(((int)day + 1) % 7);
				}

				// month days
				int dim = DateTime.DaysInMonth(now.Year, now.Month);
				// day of week where Sunday is 0
				var dayOfWeek = new DateTime(now.Year, now.Month, 1).DayOfWeek;
				// day of week where FirstDayOfWeek is 0
				int dow = ((dayOfWeek + 7 - culture.DateTimeFormat.FirstDayOfWeek) % 7);
				// total number of weeks in month
				int weeks = (int)Math.Ceiling((dow + dim) / 7.0);
				// height of one row
				int h = (600 - 20) / weeks;
				int week = 0;
				for (int d = 1; d <= dim; d++)
				{
					var weekend = IsWeekend(dayOfWeek);
					if (dow == 0 && d > 1)
						week++;
					int x = 80 + w * dow;
					int y = 260 + h * week;
					var dd = d.ToString();
					sz = g.MeasureString(dd, smFont);
					g.DrawString(dd, smFont, weekend ? weBrush : brush, x + (w - sz.Width) / 2, y + (h - sz.Height) / 2);
					// next day
					dow = (dow + 1) % 7;
					dayOfWeek = (DayOfWeek)(((int)dayOfWeek + 1) % 7);
				}

				// Year
				var yy = year.ToString();
				sz = g.MeasureString(yy, mnFont);
				g.DrawString(yy, mnFont, brush, 1000 - 60 - sz.Width, 840 + (160 - sz.Height) / 2);
			}
			return bmp;
		}

		public int Apply()
		{
			if (!File.Exists(_settings.Filename))
			{
				Log.Error("File {0} not found", _settings.Filename);
				return 2;
			}

			CultureInfo culture = null;
			try
			{
				if (_settings.Culture != null)
				{
					culture = CultureInfo.CreateSpecificCulture(_settings.Culture);
					Log.Info("Using culture: {0}", culture.DisplayName);
				}
			}
			catch (CultureNotFoundException)
			{
				Log.Warning("Cannot find culture {0}", _settings.Culture);
			}
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
				Log.Info("Using your system culture: {0}", culture.DisplayName);
			}

			using (var bmp = new Bitmap(_settings.Filename))
			{
				Log.Info("Image loaded: {0}, {1}x{2}", _settings.Filename, bmp.Width, bmp.Height);
				if (bmp.Height != 960 || bmp.Width != 640)
				{
					Log.Warning("Image is not 640x960");
				}
				int year = _settings.Year;
				int stMonth, endMonth;
				if (_settings.Month.HasValue)
				{
					stMonth = _settings.Month.Value;
					endMonth = _settings.Month.Value;
				}
				else
				{
					stMonth = _settings.WholeYear ? 1 : DateTime.Now.Month;
					endMonth = 12;
				}
				for (int month = stMonth; month <= endMonth; month++)
				{
					Log.Info(string.Format(culture, "Applying calendar overlay for {0:MMMM yyyy}", new DateTime(year, month, 1)));
					using (var result = new Bitmap(bmp))
					{
						using (var graphics = Graphics.FromImage(result))
						{
							using (var overlay = CreateOverlay(culture, year, month))
							{
								graphics.DrawImage(overlay, (result.Width - _settings.Size) / 2,
									(result.Height - _settings.Size) / 2 + _settings.VerticalOffset,
									_settings.Size, _settings.Size);
							}
						}
						var f = _settings.OutputFileName;
						result.Save(f);
						Log.Info("{0} successfuly saved", f);
					}
				}
			}
			return 0;
		}
	}
}
