using System;
using System.Collections.Generic;
using System.Reflection;

namespace Samarkin.Caloverlayer
{
	class Program
	{
		static int Main(string[] args)
		{
			if (args.Length < 1)
			{
				Usage();
				return 1;
			}

			var settings = new OverlaySettings();
			var q = new Queue<string>(args);
			while (q.Count > 0)
			{
				var arg = q.Dequeue();
				if (string.IsNullOrWhiteSpace(arg)) continue;

				if (arg[0] == '/')
				{
					if (arg == "/?")
					{
						Usage();
						return 0;
					}

					if (arg.StartsWith("/font:"))
						settings.FontFamily = arg.Substring("/font:".Length);

					else if (arg.StartsWith("/culture:"))
						settings.Culture = arg.Substring("/culture:".Length);

					else if (arg.StartsWith("/workdays:"))
					{
						var t = arg.Substring("/workdays:".Length);
						int x;
						if(int.TryParse(t, out x))
							settings.WorkDays = x;
					}

					else if(arg.StartsWith("/offset:"))
					{
						var t = arg.Substring("/offset:".Length);
						int x;
						if (int.TryParse(t, out x))
							settings.VerticalOffset = x;
					}

					else if (arg.StartsWith("/year:"))
					{
						var t = arg.Substring("/year:".Length);
						settings.WholeYear = true;
						int x;
						if (int.TryParse(t, out x))
							settings.Year = x;
					}

					else if (arg.StartsWith("/month:"))
					{
						var t = arg.Substring("/month:".Length);
						if (t == "*")
							settings.Month = null;
						else
						{
							int x;
							if (int.TryParse(t, out x))
								settings.Month = x;
						}
					}

					else if (arg.StartsWith("/out:"))
					{
						var t = arg.Substring("/out:".Length);
						settings.OutputFileName = t;
					}

					else
					{
						Log.Warning("Unknown flag: {0}", arg);
					}
				}
				else
				{
					if (settings.Filename == null)
						settings.Filename = arg;
				}
			}

			var overlay = new OverlayCalendar(settings);
			try
			{
				return overlay.Apply();
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return 255;
			}
		}

		static void Usage()
		{
			var version = ((AssemblyFileVersionAttribute)(typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0])).Version;
			Console.Error.WriteLine("Caloverlayer v" + version +" (c) 2012 Pavel Samarkin");
			Console.Error.WriteLine("Usage: Caloverlayer.exe [flags] <filename>");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Supported flags:");
			Console.Error.WriteLine("  /out:<FileName>        - output file name");
			Console.Error.WriteLine("  /font:<FontFamilyName> - specify font");
			Console.Error.WriteLine("  /culture:<CultureName> - culture name abbreviation");
			Console.Error.WriteLine("  /workdays:<number>     - number of work (non-red) days in a week (0 - 7)");
			Console.Error.WriteLine("  /offset:<number>       - custom vertical offset relative to image center");
			Console.Error.WriteLine("  /year:<number>         - year to generate calendar for");
			Console.Error.WriteLine("  /month:<number>        - month to generate calendar for (1 - 12)");
			Console.Error.WriteLine();
		}
	}
}
