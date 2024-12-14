using DigitalRise.TextureConverter.Content;
using DigitalRise.TextureConverter.Textures;
using DigitalRise.TextureConverter.Pipeline;
using System;
using System.IO;
using System.Reflection;

namespace DigitalRise.TextureConverter
{
	internal class Program
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

		static void ShowUsage()
		{
			Console.WriteLine($"drtexconv {Version}");
			Console.WriteLine("Usage: drtexconv <input> [options]");
			Console.WriteLine();
			Console.WriteLine("Options:");

			var grid = new AsciiGrid();

			grid.SetMaximumWidth(0, 30);

			grid.SetValue(0, 0, "-o, -output <path>");
			grid.SetValue(1, 0, "Specifies the output DDS file.");
			grid.SetValue(0, 1, "-n, --noMipmaps");
			grid.SetValue(1, 1, "Prevents the generation of the mipmaps.");
			grid.SetValue(0, 2, "--inputGamma <floatNumber>");
			grid.SetValue(1, 2, "Specifies the gamma of the input texture. Default value is 2.2f.");
			grid.SetValue(0, 3, "--outputGamma <floatNumber>");
			grid.SetValue(1, 3, "Specifies the gamma of the output texture. Default value is 2.2f.");
			grid.SetValue(0, 4, "-a, --noPremultiplyAlpha");
			grid.SetValue(1, 4, "Prevents the premultiply of the alpha.");
			grid.SetValue(0, 5, "-r, --resizeToPowerOfTwo");
			grid.SetValue(1, 5, "If enabled, the texture is resized to the next largest power of two, maximizing compatibility. Many graphics cards do not support textures sizes that are not a power of two.");
			grid.SetValue(0, 6, "--format (noChange|color|dxt|normal|normalInvertY)");
			grid.SetValue(1, 6, "Specifies the SurfaceFormat type of processed texture. Textures can either remain unchanged the source asset, converted to the Color format, DXT compressed, or DXT5nm compressed. Default value is 'color'.");
			grid.SetValue(0, 7, "--referenceAlpha <floatNumber>");
			grid.SetValue(1, 7, "Specifies the reference alpha value, which is used in the alpha test. Default value is 0.9f.");
			grid.SetValue(0, 8, "-s, --scaleAlphaToCoverage");
			grid.SetValue(1, 8, "Specifies whether the alpha of the lower mipmap levels should be scaled to achieve the same alpha test coverage as in the source image.");

			Console.WriteLine(grid.ToString());
		}

		static string ParseString(string name, string[] args, ref int i)
		{
			++i;
			if (i >= args.Length)
			{
				throw new Exception($"Value isn't provided for '{name}'");
			}

			return args[i];
		}

		static float ParseFloat(string name, string[] args, ref int i)
		{
			var value = ParseString(name, args, ref i);

			float result;
			if (!float.TryParse(value, out result))
			{
				throw new Exception($"Unable to parse float value '{args[i]}' for the argument '{name}'.");
			}

			return result;
		}

		static T ParseEnum<T>(string name, string[] args, ref int i) where T : struct
		{
			var value = ParseString(name, args, ref i);

			T result;
			if (!Enum.TryParse(value, true, out result))
			{
				throw new Exception($"Unable to parse enum value '{args[i]}' for the argument '{name}'.");
			}

			return result;
		}

		static void Process(string[] args)
		{
			if (args.Length == 0 ||
				(args.Length == 1 && (args[0] == "-h" || args[0] == "--help")))
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
						options.OutputFile = ParseString("output", args, ref i);
						break;

					case "-n":
					case "-noMipmaps":
						options.GenerateMipmaps = false;
						break;

					case "--inputGamma":
						options.InputGamma = ParseFloat("inputGamma", args, ref i);
						break;

					case "--outputGamma":
						options.OutputGamma = ParseFloat("outputGamma", args, ref i);
						break;

					case "-a":
					case "-noPremultiplyAlpha":
						options.PremultiplyAlpha = false;
						break;

					case "-r":
					case "--resizeToPowerOfTwo":
						options.ResizeToPowerOfTwo = true;
						break;

					case "--format":
						options.Format = ParseEnum<DRTextureFormat>("format", args, ref i);
						break;

					case "--referenceAlpha":
						options.ReferenceAlpha = ParseFloat("referenceAlpha", args, ref i);
						break;

					case "-s":
					case "--scaleAlphaToCoverage":
						options.ScaleAlphaToCoverage = true;
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

			if (!File.Exists(options.InputFile))
			{
				throw new Exception($"Unable to find file '{options.InputFile}'");
			}

			var time = DateTime.Now;

			var importerContext = new ImporterContext();

			var importer = new DRTextureImporter();
			var textureContent = importer.Import(options.InputFile, importerContext);

			var processor = new DRTextureProcessor
			{
				GenerateMipmaps = options.GenerateMipmaps,
				InputGamma = options.InputGamma,
				OutputGamma = options.OutputGamma,
				PremultiplyAlpha = options.PremultiplyAlpha,
				ResizeToPowerOfTwo = options.ResizeToPowerOfTwo,
				Format = options.Format,
				ReferenceAlpha = options.ReferenceAlpha,
				ScaleAlphaToCoverage = options.ScaleAlphaToCoverage,
			};

			var texture = processor.Process(textureContent);

			var outputFile = options.OutputFile;
			if (string.IsNullOrEmpty(options.OutputFile))
			{
				outputFile = options.InputFile;
			}

			outputFile = Path.ChangeExtension(outputFile, "dds");

			Log($"Writing to '{outputFile}'");
			using (var output = File.OpenWrite(outputFile))
			{
				DdsHelper.Save(texture, output, DdsFlags.None);
			}

			var passed = DateTime.Now - time;
			Log($"{passed.TotalMilliseconds} ms");
		}

		static int Main(string[] args)
		{
			try
			{
				Process(args);
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
				return 1;
			}

			return 0;
		}
	}
}

