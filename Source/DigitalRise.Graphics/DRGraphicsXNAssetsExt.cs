using System.IO;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Modelling;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Scenes;

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


		private readonly static AssetLoader<IMaterial> _drMaterialLoader = (manager, assetName, settings, tag) =>
		{
			var xml = manager.ReadAsString(assetName);

			var result = DefaultMaterial.FromXml(manager, xml);

			if (string.IsNullOrEmpty(result.Name))
			{
				result.Name = Path.GetFileNameWithoutExtension(assetName);
			}

			return result;
		};

		public static DrModel LoadGltf(this AssetManager assetManager, string path)
		{
			return assetManager.UseLoader(_gltfLoader, path);
		}

		public static IMaterial LoadDRMaterial(this AssetManager assetManager, string path)
		{
			return assetManager.UseLoader(_drMaterialLoader, path);
		}

		public static SceneNode LoadSceneNode(this AssetManager assetManager, string path) => assetManager.UseLoader(_sceneLoader, path);
	}
}
