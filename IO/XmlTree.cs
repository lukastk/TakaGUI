using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace TakaGUI.IO
{
	[DebuggerDisplay("Name={Name}, Value={Value}")]
	[DebuggerTypeProxy(typeof(XmlTreeDebugView))]
	public class XmlTree : List<XmlTree>
	{
		public Dictionary<string, string> Attributes = new Dictionary<string, string>();
		public string Name;
		public string Value;

		public XmlTree()
		{
		}
		public XmlTree(string xmlText)
		{
			TextReader textReader = new StringReader(xmlText);
			using (XmlReader xmlReader = XmlReader.Create(textReader))
			{
				Load(xmlReader);
			}
			textReader.Close();
		}
		public XmlTree(Stream fs)
		{
			using (XmlReader xmlReader = XmlReader.Create(fs))
			{
				Load(xmlReader);
			}
		}
		public XmlTree(XmlReader reader)
		{
			Load(reader);
		}

		public void Load(XmlReader reader)
		{
			reader.MoveToContent();
			Name = reader.Name;

			while (reader.MoveToNextAttribute())
			{
				if (reader.NodeType == XmlNodeType.Attribute)
					Attributes.Add(reader.Name, reader.Value);
			}

			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						Add(new XmlTree(reader.ReadOuterXml()));
						break;
					case XmlNodeType.Text:
						Value = reader.Value;
						break;
				}
			}

			reader.Close();
		}

		public bool ContainsKey(string key)
		{
			foreach (var elem in this)
			{
				if (elem.Name == key)
					return true;
			}

			return false;
		}

		public XmlTree GetXmlTree(string key)
		{
			foreach (var elem in this)
			{
				if (elem.Name == key)
					return elem;
			}

			return null;
		}
		public bool GetBool()
		{
			return Convert.ToBoolean(Value);
		}
		public int GetInt()
		{
			return Convert.ToInt32(Value);
		}
		public double GetDouble()
		{
			return Convert.ToDouble(Value);
		}

		public XmlTree this[string key]
		{
			get { return GetXmlTree(key); }
		}

		internal class XmlTreeDebugView
		{
			private XmlTree XmlTree;

			public string Name;
			public string Value;
			public List<XmlTree> Elements;
			public Dictionary<string, string> Attributes;

			public XmlTreeDebugView(XmlTree xmlTree)
			{
				XmlTree = xmlTree;

				Name = XmlTree.Name;
				Value = XmlTree.Value;
				Elements = xmlTree.ToList();
				Attributes = xmlTree.Attributes;
			}
		}
	}
}
