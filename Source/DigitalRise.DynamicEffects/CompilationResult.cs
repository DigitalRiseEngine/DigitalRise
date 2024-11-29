using System;
using System.Collections.Generic;

namespace DigitalRise
{
	internal class CompilationResult
	{
		public byte[] Data { get; private set; }
		public Dictionary<string, string> Defines { get; private set; }

		internal CompilationResult(byte[] data, Dictionary<string, string> defines)
		{
			Data = data ?? throw new ArgumentNullException(nameof(data));
			Defines = defines;
		}
	}
}
