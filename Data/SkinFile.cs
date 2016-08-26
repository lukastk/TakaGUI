using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using TakaGUI.Services;

namespace TakaGUI.Data
{
	public class SkinFile : ISkinFile
	{
		//Services
		IResourceManager resourceManager;
		IDebug debug;

		DoubleDictionary<MonoFontDataVariables> fonts = new DoubleDictionary<MonoFontDataVariables>();
		DoubleDictionary<SpriteLoadData> textures = new DoubleDictionary<SpriteLoadData>();
		DoubleDictionary<Color> colors = new DoubleDictionary<Color>();
		DoubleDictionary<string> values = new DoubleDictionary<string>();
		public ReadOnlyDictionary<string, ReadOnlyDictionary<string, MonoFontDataVariables>> Fonts
		{
			get { return fonts.ReadOnlyDictionary; }
		}
		public ReadOnlyDictionary<string, ReadOnlyDictionary<string, SpriteLoadData>> Sprites
		{
			get { return textures.ReadOnlyDictionary; }
		}
		public ReadOnlyDictionary<string, ReadOnlyDictionary<string, Color>> Colors
		{
			get { return colors.ReadOnlyDictionary; }
		}
		public ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> Values
		{
			get { return values.ReadOnlyDictionary; }
		}

		List<Element> elementList = new List<Element>();

		public SkinFile(IResourceManager _resourceManager, IDebug _debug = null)
		{
			resourceManager = _resourceManager;
			debug = _debug;
		}
		public SkinFile(IResourceManager _resourceManager, string dir, IDebug _debug = null)
		{
			resourceManager = _resourceManager;
			debug = _debug;
			LoadFile(dir);
		}

		#region Load
		public void LoadFile(string dir)
		{
			Stage1(dir);
			Stage2();

			fonts.LoadReadOnly();
			textures.LoadReadOnly();
			colors.LoadReadOnly();
			values.LoadReadOnly();

			elementList.Clear();
		}

		void LoadFileRec(string dir)
		{
			Stage1(dir);
			Stage2();
		}

		void AddSkinFileException(int lineNum, string file, string line)
		{
			if (debug != null)
				debug.AddExceptionLine("At line " + lineNum + " in skinFile: " + file + ". Error: " + line);
		}

		void Stage1(string dir)
		{
			FileStream fs;

			try
			{
				fs = new FileStream(dir, FileMode.Open);
			}
			catch (FileNotFoundException)
			{
				if (elementList.Count != 0)
					AddSkinFileException(elementList.Last().LineNumber, elementList.Last().File, "Can't find skinFile \"" + dir + "\"");
				else
					AddSkinFileException(-1, "(no skinFile)", "Can't find skinFile \"" + dir + "\"");
				
				return;
			}
			TextReader tr = new StreamReader(fs);

			int lineNumber = 0;
			string contentDir = "";

			string line = null;
			while ((line = tr.ReadLine()) != null)
			{
				Element element = ProcessLine(line);

				// Include files
				if (element != null)
				{
					element.LineNumber = lineNumber;
					element.File = dir;
					elementList.Add(element);
					element.ContentDir = contentDir;

					if (element.CommandType == CommandType.FunUseContentDir)
						contentDir = element.Parameters[0];
					else if (element.CommandType == CommandType.FunIncludeFile)
					{
						if (element.Parameters.Length != 1)
						{
							AddSkinFileException(lineNumber, dir, "INCLUDE_FILE must have one parameter.");
						}
						else
							Stage1(Path.Combine(Path.GetDirectoryName(dir), element.Parameters[0]));
					}
				}

				lineNumber += 1;
			}

			fs.Close();
		}

		void Stage2()
		{
			HandleVariables();

			CategorizeVariables();

			HandleAdresses();

			//Convert \@ and \$ to normal.
			foreach (Element element in elementList)
				for (int i = 0; i < element.Parameters.Length; i++)
				{
					bool lastWasBackslash = false;
					string line = "";
					foreach (char c in element.Parameters[i])
					{
						if (lastWasBackslash)
						{
						}
						if (lastWasBackslash && c == '\\')
						{
							line += '\\';
						}
						else if (c != '\\')
							line += c;
						lastWasBackslash = c == '\\' && !lastWasBackslash;
					}
					element.Parameters[i] = line;
				}

			//Sort the elements into the groups.
			foreach (Element element in elementList)
			{
				switch (element.CategoryType)
				{
					case CommandType.CatValue:
						if (!values.ContainsKey(element.Category))
							values.Add(element.Category, new Dictionary<string, string>());

						values[element.Category].Add(element.DataName, element.Parameters[0]);
						break;
					case CommandType.CatTexture:
						if (!textures.ContainsKey(element.Category))
							textures.Add(element.Category, new Dictionary<string, SpriteLoadData>());

						if (element.ContentDir != "")
						{
							if (element.Spritesheet == "")
								element.Parameters[0] = Path.Combine(element.ContentDir, element.Parameters[0]);
							else
								element.Spritesheet = Path.Combine(element.ContentDir, element.Spritesheet);
						}

						element.Spritesheet = element.Spritesheet.Replace('/', '\\');
						element.Parameters[0] = element.Parameters[0].Replace('/', '\\');
						textures[element.Category].Add(element.DataName, new SpriteLoadData(element.Spritesheet, element.Parameters[0]));
						break;
					case CommandType.CatColor:
						if (!colors.ContainsKey(element.Category))
							colors.Add(element.Category, new Dictionary<string, Color>());

						Color color;
						if (element.Parameters.Length == 3)
						{
							color = new Color(Convert.ToInt32(element.Parameters[0]),
											Convert.ToInt32(element.Parameters[1]),
											Convert.ToInt32(element.Parameters[2]));
						}
						else
						{
							color = new Color(Convert.ToInt32(element.Parameters[0]),
											Convert.ToInt32(element.Parameters[1]),
											Convert.ToInt32(element.Parameters[2]),
											Convert.ToInt32(element.Parameters[3]));
						}

						colors[element.Category].Add(element.DataName, color);
						break;
					case CommandType.CatFont:
						if (!fonts.ContainsKey(element.Category))
							fonts.Add(element.Category, new Dictionary<string, MonoFontDataVariables>());

						MonoFontDataVariables data = new MonoFontDataVariables();
						data.Texture = element.Parameters[0].Replace('/', '\\');
						if (element.ContentDir != "")
							data.Texture = Path.Combine(element.ContentDir, data.Texture);
						data.Characters = element.Parameters[1].Split(new [] {"|#|"}, StringSplitOptions.RemoveEmptyEntries);
						data.CharWidth = Convert.ToInt32(element.Parameters[2]);
						data.CharHeight = Convert.ToInt32(element.Parameters[3]);
						data.GridSize = Convert.ToInt32(element.Parameters[4]);
						data.HorizontalSpace = Convert.ToInt32(element.Parameters[5]);
						data.VerticalSpace = Convert.ToInt32(element.Parameters[6]);

						fonts[element.Category].Add(element.DataName, data);

						break;
				}
			}
		}

		void HandleVariables()
		{
			// Get all values.
			DoubleDictionary<string> variables = new DoubleDictionary<string>();
			foreach (Element elem in elementList)
			{
				if (!variables.ContainsKey(elem.File))
					variables.Add(elem.File, new Dictionary<string, string>());

				if (elem.CommandType == CommandType.Variable)
				{
					if (elem.Parameters.Length != 2) // VAR varName varValue
					{
						AddSkinFileException(elem.LineNumber, elem.File, "VAR must have one parameter.");
						continue;
					}
					else
						variables[elem.File].Add(elem.Parameters[0], elem.Parameters[1]);
				}
			}

			// Remove the variable declarations.
			foreach (Element elem in new List<Element>(elementList))
				if (elem.CommandType == CommandType.Variable)
					elementList.Remove(elem);

			// Set all variable-references.
			foreach (Element elem in elementList)
			{
				for (int i = 0; i < elem.Parameters.Length; i++)
				{
					if (elem.Parameters[i].Length != 0 && elem.Parameters[i][0] == '$')
					{
						string key = elem.Parameters[i].Substring(1);
						if (!variables[elem.File].ContainsKey(key))
						{
							AddSkinFileException(elem.LineNumber, elem.File, "Can't find variable \"" + key + "\"");
							continue;
						}
						else
						{
							elem.Parameters[i] = variables[elem.File][key];
						}
					}
				}
			}
		}

		void CategorizeVariables()
		{
			CommandType currentCategoryType = CommandType.CatValue;
			string currentCategory = "General";

			string currentSpritesheet = "";

			foreach (Element element in new List<Element>(elementList))
			{
				switch (element.CommandType)
				{
					case CommandType.CatValue:
						currentCategoryType = CommandType.CatValue;
						currentCategory = "General";
						currentSpritesheet = "";
						break;
					case CommandType.CatTexture:
						currentCategoryType = CommandType.CatTexture;
						currentCategory = "General";
						currentSpritesheet = "";
						break;
					case CommandType.CatColor:
						currentCategoryType = CommandType.CatColor;
						currentCategory = "General";
						currentSpritesheet = "";
						break;
					case CommandType.CatFont:
						currentCategoryType = CommandType.CatFont;
						currentCategory = "General";
						currentSpritesheet = "";
						break;
					case CommandType.FunCategory:
						currentCategory = element.Parameters[0];
						currentSpritesheet = "";
						break;
					case CommandType.FunUseSpritesheet:
						currentSpritesheet = element.Parameters[0];
						break;
				}

				if (element.CommandType != CommandType.Data && element.CommandType != CommandType.FunImportCategory)
				{
					elementList.Remove(element);
					continue;
				}

				element.CategoryType = currentCategoryType;
				element.Category = currentCategory;
				element.Spritesheet = currentSpritesheet;
			}

			//Import categories
			List<Element> copy = new List<Element>();
			copy.AddRange(elementList);
			foreach (Element element in copy)
			{
				if (element.CommandType == CommandType.FunImportCategory)
				{
					string importFrom = element.Parameters[0];
					string importTo = element.Category;
					CommandType categoryType = element.CategoryType;

					foreach (Element subElem in copy)
					{
						if (subElem.Category == importFrom && subElem.CategoryType == categoryType)
						{
							Element insert = subElem.Copy();
							insert.Category = importTo;
							insert.LineNumber = -1;
							insert.File = "Category:" + subElem.Category;

							elementList.Add(insert);
						}
					}

					//Remove the import function from element-list.
					elementList.Remove(element);
				}
			}
		}

		void HandleAdresses()
		{
			bool adressesExist = true;

			while (adressesExist)
			{
				adressesExist = false;

				foreach (Element elem in elementList)
				{
					for (int i = 0; i < elem.Parameters.Length; i++)
					{
						if (elem.Parameters[i].Length != 0 && elem.Parameters[i][0] == '@')
						{
							adressesExist = true;

							string[] insertValues = null;

							if (!elem.Parameters[i].Substring(1).Contains(';'))
							{
								AddSkinFileException(elem.LineNumber, elem.File, "Format of adress is wrong");
								elem.Parameters[i] = "";
								continue;
							}

							string adressCategory = elem.Parameters[i].Substring(1).Split(';')[0];
							string adressName = elem.Parameters[i].Substring(1).Split(';')[1];

							foreach (Element subElem in elementList)
							{
								if (subElem == elem || subElem.CommandType != CommandType.Data)
									continue;

								if (subElem.DataName == adressName && subElem.Category == adressCategory && subElem.CategoryType == elem.CategoryType)
									insertValues = subElem.Parameters;
							}

							if (insertValues == null)
							{
								AddSkinFileException(elem.LineNumber, elem.File, "Can't find value \"" + elem.Parameters[i] + "\"");
								continue;
							}

							List<string> Values = new List<string>();
							for (int n = 0; n < elem.Parameters.Length; n++)
							{
								if (n == i)
									Values.AddRange(insertValues);
								else
									Values.Add(elem.Parameters[n]);
							}

							elem.Parameters = Values.ToArray();
						}
					}
				}
			}


		}

		Element ProcessLine(string line)
		{
			bool lastWasWhitespace = true;
			bool lastWasBackslash = false;
			bool inQuotes = false;

			string currentPart = "";
			List<StringValue> lineParts = new List<StringValue>();
			for (int n = 0; n < line.Length; n++)
			{
				if (line[n] == '"' && !lastWasBackslash)
				{
					if (lastWasWhitespace && !inQuotes)
					{
						lineParts.Add(new StringValue(currentPart, false));
						currentPart = "";
						inQuotes = true;
					}
					else if (inQuotes)
					{
						lineParts.Add(new StringValue(currentPart, true));
						currentPart = "";
						inQuotes = false;
					}
					else
						currentPart += line[n];
				}
				else if ((line[n] == ' ' || line[n] == '\t') && !inQuotes)
				{
					lineParts.Add(new StringValue(currentPart, false));
					currentPart = "";
				}
				else if (line[n] == '#' && !inQuotes)
				{
					if (lastWasBackslash)
						currentPart += '#';
					else
						break;
				}
				else if (lastWasBackslash)
				{
					if (line[n] == 't')
						currentPart += '\t';
					else if (line[n] == '"')
						currentPart += '"';
					else if (line[n] == 'n')
						currentPart += '\n';
					else if (line[n] == '@')
						currentPart += "\\@"; // If @ is not the first char in a part, it is not an adress.
					else if (line[n] == '$')
						currentPart += "\\$"; // If $ is not the first char in a part, it is not a var-reference.
					else if (line[n] == '\\')
						currentPart += "\\\\";
				}
				else if (line[n] != '\\')
				{
					currentPart += line[n];
				}

				lastWasWhitespace = line[n] == ' ' || line[n] == '	' && !inQuotes;
				lastWasBackslash = !lastWasBackslash && line[n] == '\\';
			}

			lineParts.Add(new StringValue(currentPart, inQuotes)); //Add last currentLine.

			// Remove empty values.
			List<StringValue> copy = new List<StringValue>();
			copy.AddRange(lineParts);
			lineParts.Clear();
			foreach (StringValue part in copy)
			{
				if (part.Value != "" || part.IsQuote)
					lineParts.Add(part);
			}

			if (lineParts.Count == 0)
				return null;

			// Add command type to new Element instance.
			Element element = new Element();
			switch (lineParts.First().Value)
			{
				case "[VALUES]":
					element.CommandType = CommandType.CatValue;
					break;
				case "[TEXTURES]":
					element.CommandType = CommandType.CatTexture;
					break;
				case "[FONTS]":
					element.CommandType = CommandType.CatFont;
					break;
				case "[COLORS]":
					element.CommandType = CommandType.CatColor;
					break;

				case "CATEGORY":
					element.CommandType = CommandType.FunCategory;
					break;
				case "USE_SPRITESHEET":
					element.CommandType = CommandType.FunUseSpritesheet;
					break;
				case "INCLUDE_FILE":
					element.CommandType = CommandType.FunIncludeFile;
					break;
				case "IMPORT_CATEGORY":
					element.CommandType = CommandType.FunImportCategory;
					break;
				case "USE_CONTENTDIR":
					element.CommandType = CommandType.FunUseContentDir;
					break;

				case "VAR":
					element.CommandType = CommandType.Variable;
					break;

				default:
					element.CommandType = CommandType.Data;
					element.DataName = lineParts.First().Value;
					break;
			}

			// Add Element values
			element.Parameters = new string[lineParts.Count - 1];
			int index = -1;
			foreach (StringValue value in lineParts)
			{
				if (index != -1)
					element.Parameters[index] = value.Value;

				index += 1;
			}

			return element;
		}

		public enum CommandType
		{
			CatValue,
			CatTexture,
			CatFont,
			CatColor,

			FunCategory,
			FunUseSpritesheet,
			FunIncludeFile,
			FunImportCategory,
			FunUseContentDir,

			Variable,
			Data
		}

		struct StringValue
		{
			public string Value;
			public bool IsQuote;

			public StringValue(string value, bool isQuote)
			{
				Value = value;
				IsQuote = isQuote;
			}
		}

		class Element
		{
			public CommandType CommandType;
			public string DataName = null;
			public string[] Parameters; //TODO: check for value validity here.

			public string Category;
			public CommandType CategoryType;

			public string Spritesheet;

			public string ContentDir;
			public int LineNumber;
			public string File;

			public Element Copy()
			{
				Element element = new Element();

				element.CommandType = CommandType;
				element.DataName = DataName;
				element.Parameters = (string[])Parameters.Clone();
				element.Category = Category;
				element.CategoryType = CategoryType;
				element.Spritesheet = Spritesheet;
				element.ContentDir = ContentDir;
				element.LineNumber = LineNumber;
				element.File = File;

				return element;
			}
		}

		class DoubleDictionary<T> : Dictionary<string, Dictionary<string, T>>
		{
			public ReadOnlyDictionary<string, ReadOnlyDictionary<string, T>> ReadOnlyDictionary
			{
				get;
				private set;
			}

			public void LoadReadOnly()
			{
				var internalDict = new Dictionary<string, ReadOnlyDictionary<string, T>>();

				foreach (string key in Keys)
					internalDict.Add(key, new ReadOnlyDictionary<string, T>(this[key]));

				ReadOnlyDictionary = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, T>>(internalDict);
			}
		}

		#endregion

		public MonoFont GetFont(int resourceGroup, string category, string name)
		{
			MonoFontDataVariables vars = fonts[category][name];

			MonoFont font = new MonoFont(
						SpriteSheet.GetSingleSprite(resourceManager, vars.Texture, resourceGroup),
						vars.Characters,
						vars.CharWidth,
						vars.CharHeight,
						vars.GridSize,
						vars.HorizontalSpace,
						vars.VerticalSpace);

			return font;
		}
		public ISprite GetSprite(int resourceGroup, string category, string name)
		{
			SpriteLoadData vars = textures[category][name];

			if (vars.Spritesheet == "")
				return SpriteSheet.GetSingleSprite(resourceManager, vars.TextureName, resourceGroup);
			else
			{
				ISpriteSheet spriteSheet = resourceManager.Load<SpriteSheet>(vars.Spritesheet, resourceGroup);
				ISprite sprite = new Sprite(spriteSheet.GetIndex(vars.TextureName), spriteSheet);
				return sprite;
			}
		}
		public Color GetColor(string category, string name)
		{
			return colors[category][name];
		}
		public string GetValues(string category, string name)
		{
			return values[category][name];
		}

		public struct MonoFontDataVariables
		{
			public string Texture;
			public string[] Characters;
			public int CharHeight;
			public int CharWidth;
			public int GridSize;
			public int HorizontalSpace;
			public int VerticalSpace;
		}

		public struct SpriteLoadData
		{
			public string Spritesheet; //If empty, no spritebatch.
			public string TextureName;

			public SpriteLoadData(string spritesheet, string name)
			{
				Spritesheet = spritesheet;
				TextureName = name;
			}
		}
	}

	public interface ISkinFile
	{
		ReadOnlyDictionary<string, ReadOnlyDictionary<string, SkinFile.SpriteLoadData>> Sprites { get; }
		ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> Values { get; }
		ReadOnlyDictionary<string, ReadOnlyDictionary<string, Microsoft.Xna.Framework.Color>> Colors { get; }
		ReadOnlyDictionary<string, ReadOnlyDictionary<string, SkinFile.MonoFontDataVariables>> Fonts { get; }

		Color GetColor(string category, string name);
		MonoFont GetFont(int resourceGroup, string category, string name);
		ISprite GetSprite(int resourceGroup, string category, string name);
		string GetValues(string category, string name);
		void LoadFile(string dir);
	}
}
