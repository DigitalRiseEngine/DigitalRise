using System;

namespace DigitalRise.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EditorInfoAttribute : Attribute
	{
		public string Category { get; }
		public Type SubType { get; }

		public EditorInfoAttribute(string category, Type subType)
		{
			Category = category;
			SubType = subType;
		}

		public EditorInfoAttribute(string category) : this(category, null)
		{
		}
	}
}
