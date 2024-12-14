using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;

namespace DigitalRise.ModelStorage.Binary
{
	public abstract class SerializableCollection<T>
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int BufferId { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public List<T> Data { get; } = new List<T>();

		internal void LoadBinaryData(ReadContext ctx)
		{
			ctx.ReadCollection(BufferId, LoadItem, Data);
		}

		internal void SaveBinaryData(WriteContext ctx)
		{
			BufferId = ctx.WriteCollection(Data, SaveItem);
		}

		protected abstract T LoadItem(BinaryReader reader);

		protected abstract void SaveItem(BinaryWriter writer, T item);
	}
}
