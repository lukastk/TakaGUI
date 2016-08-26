using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TakaGUI.IO
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Union
	{
		[FieldOffset(6)]
		public byte ByteData;
		[FieldOffset(0)]
		public string StringText;
		[FieldOffset(4)]
		public short UnionShort;
		[FieldOffset(4)]
		public byte LowByte;
		[FieldOffset(5)]
		public byte HighByte;
	}
}
