﻿using Assimp;
using DigitalRise.ConverterBase.SceneGraph;
using DigitalRise.ModelConverter.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DigitalRise.ModelConverter
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
			var inputModel = @"D:\Projects\DigitalRune\Samples\Content\Dude\Dude.drmdl";

			var modelDescription = ModelDescription.Load(inputModel, Log);

			var folder = Path.GetDirectoryName(inputModel);
			var inputFile = Path.Combine(folder, modelDescription.FileName);

			var importerContext = new ImporterContext();

			var importer = new OpenAssetImporter();
			var modelContent = importer.Import(inputFile, importerContext);

			var processor = new DRModelProcessor
			{
				Logger = Log
			};

			var drModelContent = processor.Process(modelContent, modelDescription);

			drModelContent.Save(@"D:\", "Barrel");
			var k = 5;
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

