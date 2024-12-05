using DigitalRise.Data.Modelling;
using DigitalRise.SceneGraph;

namespace AssetManagementBase
{
	public static class DRGraphicsXNAssetsExt
	{
		private readonly static AssetLoader<DrModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			return loader.Load(manager, assetName);
		};

		private readonly static AssetLoader<SceneNode> _sceneLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);
			return SceneNode.ReadFromString(data, manager);
		};


		public static DrModel LoadGltf(this AssetManager assetManager, string path)
		{
			return assetManager.UseLoader(_gltfLoader, path);
		}

		public static SceneNode LoadSceneNode(this AssetManager assetManager, string path) => assetManager.UseLoader(_sceneLoader, path);
	}
}
