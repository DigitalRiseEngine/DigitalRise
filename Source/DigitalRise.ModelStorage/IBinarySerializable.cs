using System.IO;

namespace DigitalRise.ModelStorage
{
	internal interface IBinarySerializable
	{
		void LoadFromBinary(BinaryReader br);
		void SaveToBinary(BinaryWriter bw);
	}
}
