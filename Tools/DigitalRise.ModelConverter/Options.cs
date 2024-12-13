namespace DigitalRise.ModelConverter
{
	internal class Options
	{
		public string InputFile { get; set; }
		public string OutputFile { get; set; }

		public bool GenerateTangentsAndBitangents { get; set; }
		public bool FlipWindingOrder { get; set; }

		public override string ToString() =>
			$"{InputFile}, OutputFolder={OutputFolder}, GenerateTangents={GenerateTangentsAndBitangents}, FlipWindingOrder={FlipWindingOrder}";
	}
}
