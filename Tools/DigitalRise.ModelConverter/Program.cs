using DigitalRise.TextureConverter;
using System;
using System.Reflection;

namespace DigitalRise.ModelConverter
{
	internal static class Program
	{
		public static string Version
		{
			get
			{
				var assembly = typeof(Program).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		static void Log(string message) => Console.WriteLine(message);

		static string ParseString(string name, string[] args, ref int i)
		{
			++i;
			if (i >= args.Length)
			{
				throw new Exception($"Value isn't provided for '{name}'");
			}

			return args[i];
		}

		static void ShowUsage()
		{
			Console.WriteLine($"drmodconv {Version}");
			Console.WriteLine("Usage: drmodconv <input> [options]");
			Console.WriteLine();
			Console.WriteLine("Options:");

			var grid = new AsciiGrid();

			grid.SetMaximumWidth(0, 30);

			grid.SetValue(0, 0, "-o, -output <folder>");
			grid.SetValue(1, 0, "Specifies the output folder.");
			grid.SetValue(0, 1, "-t, --generateTangents");
			grid.SetValue(1, 1, "If enabled, then the tangents and bitangents are generated.");
			grid.SetValue(0, 2, "-f, --flipWindingOrder");
			grid.SetValue(1, 2, "If enabled, then the winding order is flipped.");

			Console.WriteLine(grid.ToString());
		}

		static void Process(string[] args)
		{
			if (args.Length == 0 || args.Length == 1 && (args[0] == "-h" || args[0] == "--help"))
			{
				ShowUsage();
				return;
			}

			var options = new Options();

			for (var i = 0; i < args.Length; ++i)
			{
				var arg = args[i];

				switch (arg)
				{
					case "-o":
					case "--output":
						options.OutputFolder = ParseString("output", args, ref i);
						break;

					case "-t":
					case "--generateTangents":
						options.GenerateTangentsAndBitangents = true;
						break;

					case "-f":
					case "--flipWindingOrder":
						options.FlipWindingOrder = true;
						break;

					default:
						if (arg.StartsWith("-"))
						{
							throw new Exception($"Unknown argument '{arg}'");
						}

						options.InputFile = arg;
						break;

				}
			}

			if (string.IsNullOrEmpty(options.InputFile))
			{
				throw new Exception("Input file is not specified");
			}

			Log(options.ToString());

			var converter = new Converter();
			converter.Convert(options);
		}


		static void Main(string[] args)
		{
			try
			{
				Process(args);
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
			}
		}
	}
}

