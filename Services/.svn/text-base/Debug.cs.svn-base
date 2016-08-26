using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using System.IO;
using System.Reflection;
using IronPython.Runtime.Types;
using IronPython.Runtime;
using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace TakaGUI.Services
{
	public class Debug : GameComponent, IDebug
	{
		/// <summary>
		/// Default value is true.
		/// </summary>
		public bool ThrowExceptions
		{
			get;
			set;
		}

		List<string> debugText = new List<string>();
		public ReadOnlyCollection<string> DebugText
		{
			get;
			private set;
		}
		string currentLine;
		int _indiceLevel;
		public string IndiceChar = "  ";

		/// <summary>
		/// The indice level before the currently running command started running, if there is no running command then
		/// this field is not used.
		/// </summary>
		int oldIndiceLevel;
		int indiceLevel
		{
			get { return _indiceLevel; }
			set
			{
				_indiceLevel = value;

				if (_indiceLevel < 0)
					_indiceLevel = 0;
			}
		}

		public Debug(Game game)
			: base(game)
		{
			game.Components.Add(this);

			ThrowExceptions = true;
			DebugText = debugText.AsReadOnly();
		}

		public override void Initialize()
		{
			base.Initialize();

			Enabled = false;
		}

		public void AddIndiceLevel(int add)
		{
			indiceLevel += add;
		}
		public void SetIndiceLevel(int _indiceLevel)
		{
			indiceLevel = _indiceLevel;
		}

		/// <summary>
		/// Sets the current line to the given string.
		/// </summary>
		public void SetCurrentLine(object text)
		{
			if (text == null)
				text = "null";

			currentLine = text.ToString();
		}

		/// <summary>
		/// Adds the string to the current line.
		/// </summary>
		public void AddText(object text)
		{
			if (text == null)
				text = "null";

			currentLine += text.ToString();
		}

		/// <summary>
		/// Finishes the line, adds it to the Debug in the current indice level.
		/// </summary>
		public void FinishLine()
		{
			currentLine = GCI() + currentLine;

			AddLine(currentLine);
			currentLine = "";
		}

		/// <summary>
		/// Finishes the line, adds it to the Debug and increases the indice level (after the line has been added).
		/// </summary>
		public void FinishHeadLine()
		{
			currentLine = GCI() + currentLine;

			AddLine(currentLine);
			currentLine = "";
			indiceLevel += 1;
		}

		/// <summary>
		/// Finishes the line, adds it to the Debug and decreases the indice level (after the line has been added).
		/// </summary>
		public void FinishLineAndSection()
		{
			currentLine = GCI() + currentLine;

			AddLine(currentLine);
			currentLine = "";
			indiceLevel -= 1;
		}

		/// <summary>
		/// Adds point line to the Debug with the current indice, you specify an report of an incident that occured within
		/// point certain method.
		/// </summary>
		public void AddReport(string where, string exception)
		{
			string line = where + ": " + exception;
			line = GI(indiceLevel) + line;

			AddLine(line);
		}

		/// <summary>
		/// Adds point line to the Debug with the current indice, and also throws an exception with the line.
		/// </summary>
		public void AddExceptionInClass(Type type, string where, string exception)
		{
			string line = "(In class: " + type.ToString() + ") " + where + ": " + exception;
			line = GI(indiceLevel) + line;

			AddLine(line);

			if (ThrowExceptions)
				throw new Exception(line);
		}

		public void AddExceptionLine(object line)
		{
			line = GI(indiceLevel) + line;

			AddLine(line);

			if (ThrowExceptions)
				throw new Exception(line.ToString());
		}

		/// <summary>
		/// Adds point line to the Debug with the current indice.
		/// </summary>
		public void AddLine(object line)
		{
			if (line == null)
				line = "null";

			if (line.ToString().Contains('\n'))
			{
				AddMultipleLines(line.ToString());
				return;
			}

			string addLine = GI(indiceLevel) + line.ToString();

			debugText.Add(addLine);
			Console.WriteLine(line);
		}

		/// <summary>
		/// Adds the lines to the Debug with the current indice.
		/// </summary>
		public void AddMultipleLines(string text)
		{
			string[] lines = text.Split(new char[] { '\n' });

			foreach (string line in lines)
				AddLine(line);
		}

		/// <summary>
		/// Adds point line and increases the line indent.
		/// </summary>
		public void AddHeadLine(object line)
		{
			AddLine(line.ToString());
			indiceLevel += 1;
		}

		/// <summary>
		/// Adds point line and decreases the line indice.
		/// </summary>
		public void AddSectionEnd(object line)
		{
			AddLine(line.ToString());
			indiceLevel -= 1;
		}

		/// <summary>
		/// Returns point string with the data of the object given.
		/// </summary>
		public string GetObjectInformation(object obj)
		{
			return null;
		}

		/// <summary>
		/// "Get Indice" returns point string with the wanted amount of indices.
		/// </summary>
		public string GI(int indiceNumber)
		{
			string str = "";
			for (int i = 0; i < indiceNumber; i++)
				str += IndiceChar;

			return str;
		}

		/// <summary>
		/// "Get Current Indice" returns point string with the amount of indices in the current indice level.
		/// </summary>
		public string GCI()
		{
			return GI(_indiceLevel);
		}

		/// <summary>
		/// Adds point line to the Debug text, without indices.
		/// </summary>
		/// <param name="line"></param>
		public void AddUserInput(string startString, string line)
		{
			debugText.Add(startString + line);
			line = line.Trim();
		}
	}

	public interface IDebug
	{
		bool ThrowExceptions { get; set; }
		ReadOnlyCollection<string> DebugText { get; }

		void AddIndiceLevel(int add);
		void SetIndiceLevel(int _indiceLevel);

		void SetCurrentLine(object text);
		void AddText(object text);
		void FinishLine();
		void FinishHeadLine();
		void FinishLineAndSection();
		void AddReport(string where, string exception);
		void AddExceptionInClass(Type type, string where, string exception);
		void AddExceptionLine(object line);
		void AddLine(object line);
		void AddMultipleLines(string text);
		void AddHeadLine(object line);
		void AddSectionEnd(object line);

		string GetObjectInformation(object obj);

		string GI(int indiceNumber);
		string GCI();
		void AddUserInput(string startString, string line);
	}
}