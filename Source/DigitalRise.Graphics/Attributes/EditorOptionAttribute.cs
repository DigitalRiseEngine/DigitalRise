using System;

namespace DigitalRise.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class EditorOptionAttribute: Attribute
	{
		public Type Type { get; }

		public EditorOptionAttribute(Type type)
		{
			this.Type = type;
		}
	}
}
