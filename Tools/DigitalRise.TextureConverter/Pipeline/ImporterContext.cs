using Microsoft.Xna.Framework.Content.Pipeline;

namespace DigitalRise.TextureConverter.Pipeline
{
	internal class ImporterContext : ContentImporterContext
	{
		public override string IntermediateDirectory => throw new System.NotImplementedException();

		public override ContentBuildLogger Logger => throw new System.NotImplementedException();

		public override string OutputDirectory => throw new System.NotImplementedException();

		public override void AddDependency(string filename)
		{
			throw new System.NotImplementedException();
		}
	}
}
