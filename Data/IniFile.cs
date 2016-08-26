using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TakaGUI.IO
{
	public class IniFile
	{
		Dictionary<string, Dictionary<string, string>> _Data;

		public IniFile()
		{
			_Data = new Dictionary<string, Dictionary<string, string>>();
		}
		public IniFile(string fileDir)
		{
			_Data = new Dictionary<string, Dictionary<string, string>>();
			ReadFile(fileDir);
		}

		public Dictionary<string, string> this[string key]
		{
			get
			{
				return _Data[key];
			}
		}

		public void ReadFile(string fileDir)
		{
			//ADDDEBUG when point key and var comes before point section.
			//ADDDEBUG when two sections have the same key.
			//ADDDEBUG when there is two "=" in one line.
			//ADDDEBUG when there is an unneven amount of control characters.
			//ADDDEBUG invalid chars in rowValues, keys.
			//ADDDEBUG if things like this appears "key=423 helloasd".
			//ADDTOGAME escape characters.
			FileStream fs = new FileStream(fileDir, FileMode.Open, FileAccess.Read);
			TextReader tr = new StreamReader(fs);

			string currentSection  = "";
			Dictionary<string, string> currentDict = new Dictionary<string, string>();
			KeyAndValue keyAndVal;
			string line;
			int indexOfFirstQuote;
			int indexOfSecondQuote;
			while ((line = tr.ReadLine()) != null)
			{
				keyAndVal = new KeyAndValue();
				for (int c = 0; c < line.Length; c++)
				{
					if (line[c] == '[')
					{
						if (currentSection != "")
							_Data.Add(currentSection, currentDict);
						currentSection = line.Substring(line.IndexOf('[') + 1, line.IndexOf(']') - line.IndexOf('[') - 1);
						currentDict = new Dictionary<string, string>();
					}
					if (line[c] == ';')
						break;
					if (line[c] == '=')
					{
						keyAndVal.Key = line.Substring(0, c);
						line = line.Substring(c + 1);
						if (line.Contains('"'))
							if (line.Contains(';'))
								if (line.IndexOf(';') > line.IndexOf('"'))
								{
									indexOfFirstQuote = line.IndexOf('"');
									indexOfSecondQuote = line.IndexOf('"', indexOfFirstQuote + 1);
									keyAndVal.Value = AddEscapeSequences(line.Substring(indexOfFirstQuote + 1,
												indexOfSecondQuote - indexOfFirstQuote - 1));
								}
								else
									keyAndVal.Value = AddEscapeSequences(line.Substring(0, line.IndexOf(';')).Split(new char[] { ' ' })[0]);
							else
							{
								indexOfFirstQuote = line.IndexOf('"');
								indexOfSecondQuote = line.IndexOf('"', indexOfFirstQuote + 1);
								keyAndVal.Value = AddEscapeSequences(line.Substring(indexOfFirstQuote + 1,
											indexOfSecondQuote - indexOfFirstQuote - 1));
							}
						else
						{
							keyAndVal.Value = AddEscapeSequences(line.Split(new char[] { ' ' })[0]);
						}
						currentDict.Add(keyAndVal.Key, keyAndVal.Value);
					}
				}
			}
			if (currentSection != "")
			{
				_Data.Add(currentSection, currentDict);
			}
		}

		public Dictionary<string, Dictionary<string, string>> GetConfigData()
		{
			return _Data;
		}

		public T1 SetVal<T1>(T1 id, string section, string key, string convertFunctionName)
		{
			if (_Data.ContainsKey(section) || _Data.ContainsKey(key))
			{
				MethodInfo convertFunction = Type.GetType("System.Convert").GetMethod(convertFunctionName,
																new Type[]{Type.GetType("System.String")});

				id = (T1)convertFunction.Invoke(null, new object[] { _Data[section][key] });
			}

			return id;
		}

		public string AddEscapeSequences(string str)
		{
			string[] slashList = str.Split(new string[] { @"\\" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < slashList.Length; i++)
				slashList[i] = slashList[i].Replace(@"\EXECDIR", Environment.CurrentDirectory);
			str = String.Empty;

			for (int i = 0; i < slashList.Length; i++)
			{
				str += slashList[i];
				if (i != (slashList.Length - 1))
					str += @"\";
			}
			return str;
		}

		public class KeyAndValue
		{
			public string Key;
			public string Value;
		}
	}
}
