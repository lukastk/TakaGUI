using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TakaGUI.Data;
using TakaGUI.Services;

namespace TakaGUI.Machines
{
	/// <summary>
	/// Summary:
	///		Simulates point keyboard.
	/// </summary>
	public class TextInputMachine
	{
		//Services
		IKeyboardInput keyboardInput;

		public delegate void CursorEvent(object sender, int moveAmount, int oldPosition);
		public delegate bool TextAddedEvent(object sender, int position, string input);
		public delegate bool TextRemovedEvent(object sender, int position);

		public event CursorEvent CursorMoved;
		public event TextAddedEvent TextAdded;
		public event TextRemovedEvent TextRemoved;

		string text = "";
		public string Text
		{
			get { return text; }
			set
			{
				text = value;

				if (text == null)
					text = "";

				if (Cursor > text.Length)
					Cursor = text.Length;
			}
		}
		public MonoFont Font;
		int cursor;
		public int Cursor
		{
			get { return cursor; }
			set
			{
				int oldValue = cursor;
				cursor = value;

				if (cursor < 0)
					cursor = 0;
				else if (cursor > Text.Length)
					cursor = Text.Length;

				if (CursorMoved != null)
					CursorMoved(this, value, oldValue);
			}
		}

		public int MaxChars = -1;

		public bool Push = false;
		double startRapid;
		bool rapidMode = false;
		Keys rapidInput = Keys.None;
		public float TimeToStartRapid = 0.4F;
		public float TimeBetweenRapids = 0.035F;

		public bool Enabled = true;

		public string AllowedChars;

		public TextInputMachine(IKeyboardInput _keyboardInput, MonoFont font)
		{
			keyboardInput = _keyboardInput;

			Font = font;
			AllowedChars = "";
			foreach (string line in font.Characters)
				AllowedChars += line;
		}

		void PushInput(Keys input, GameTime gameTime)
		{
			rapidInput = input;
			Push = true;
			rapidMode = false;
			startRapid = gameTime.TotalGameTime.TotalSeconds;

			handleInput(rapidInput);
		}

		void TryEndInput(Keys input)
		{
			if (input == rapidInput)
			{
				Push = false;
				startRapid = -1;
				rapidMode = false;
				rapidInput = Keys.None;
			}
		}

		public void EndCurrentInput()
		{
			Push = false;
			startRapid = -1;
			rapidMode = false;
			rapidInput = Keys.None;
		}

		void handleInput(Keys input)
		{
			switch (input)
			{
				case Keys.Left:
					Cursor -= 1;
					break;
				case Keys.Right:
					Cursor += 1;
					break;
				case Keys.Back:
					if (Cursor != 0)
					{
						if (TextRemoved != null && !TextRemoved(this, Cursor - 1))
							break;

						text = Text.Remove(Cursor - 1, 1);
						Cursor -= 1;
					}
					break;
				case Keys.Delete:
					if (Cursor != Text.Length)
					{
						if (TextRemoved != null && !TextRemoved(this, Cursor))
							break;

						Text = Text.Remove(Cursor, 1);
					}
					break;
				default:
					char c = GetChar(input);

					if (!AllowedChars.Contains(c))
						break;

					if ((Text.Length < MaxChars || MaxChars == -1) &&
						c != '\0' && c != '\n' && Font.HasChar(c))
					{
						AddToText(Cursor, Convert.ToString(c)); ;
						Cursor += 1;
					}
					break;
			}
		}

		public void AddToText(int position, string input)
		{
			if (TextAdded != null && !TextAdded(this, position, input))
				return;

			Text = Text.Substring(0, position) + input + Text.Substring(position);
		}

		public void Update(GameTime gameTime)
		{
			if (keyboardInput.ClickedKeys.Count != 0 && Enabled)
				PushInput(keyboardInput.ClickedKeys[0], gameTime);

			foreach (Keys releasedKey in keyboardInput.ReleasedKeys)
				TryEndInput(releasedKey);

			if (Push)
			{
				if (!rapidMode)
				{
					if (startRapid + TimeToStartRapid < gameTime.TotalGameTime.TotalSeconds)
					{
						rapidMode = true;
					}
				}

				if (rapidMode)
				{
					if (startRapid + TimeBetweenRapids < gameTime.TotalGameTime.TotalSeconds)
					{
						handleInput(rapidInput);
						startRapid = gameTime.TotalGameTime.TotalSeconds;
					}
				}
			}
		}

		//Returns point char that corresponds to the given key.
		public char GetChar(Keys key)
		{
			string chr = "";

			switch (key)
			{
				case Keys.D1:
					chr = "1";
					break;
				case Keys.D2:
					chr = "2";
					break;
				case Keys.D3:
					chr = "3";
					break;
				case Keys.D4:
					chr = "4";
					break;
				case Keys.D5:
					chr = "5";
					break;
				case Keys.D6:
					chr = "6";
					break;
				case Keys.D7:
					chr = "7";
					break;
				case Keys.D8:
					chr = "8";
					break;
				case Keys.D9:
					chr = "9";
					break;
				case Keys.D0:
					chr = "0";
					break;
				case Keys.Space:
					chr = " ";
					break;
				case Keys.Tab:
					chr = "\t";
					break;
				case Keys.Enter:
					chr = "\n";
					break;
				case Keys.OemComma:
					chr = ",";
					break;
				case Keys.OemPeriod:
					chr = ".";
					break;
				case Keys.OemMinus:
					chr = "-";
					break;
				default:
					if (IsValidChar(key))
						chr = keyboardInput.IsPressed(Keys.LeftShift) || keyboardInput.IsPressed(Keys.RightShift) ? key.ToString().ToUpper() : key.ToString().ToLower();
					break;
			}

			if (chr.Length == 1) //Only allow single length keys be passed on.
				return chr[0];

			return '\0'; //Return null;
		}

		bool IsValidChar(Keys key)
		{
			switch (key)
			{
				case Keys.Up: return false;
				case Keys.Down: return false;
				case Keys.Left: return false;
				case Keys.Right: return false;
				case Keys.LeftShift: return false;
				case Keys.RightShift: return false;

				default: return true;
			}
		}
	}
}