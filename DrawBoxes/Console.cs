using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TakaGUI.Machines;
using TakaGUI.Data;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class Console : DrawBox
	{
		public static string DefaultCategory = "Console";

		public Color BackColor = Color.White;
		public Color FontColor = Color.Black;

		char[,] display;
		public MonoFont Font;

		public int BufferWidth
		{
			get { return display.GetLength(0); }
		}
		public int BufferHeight
		{
			get { return display.GetLength(1); }
		}

		public int DrawSpaceWidth
		{
			get { return Width; }
		}
		public int DrawSpaceHeight
		{
			get { return Height; }
		}

		public bool TerminalMode = true;
		public List<string> Lines = new List<string>();
		public string CurrentLine
		{
			get { return inputMachine.Text; }
			set { inputMachine.Text = value; }
		}

		int _CursorBlinkTime;
		public int CursorBlinkTime
		{
			get { return _CursorBlinkTime; }
			set
			{
				_CursorBlinkTime = value;

				if (cursorBlinkTimer != null)
					cursorBlinkTimer.Tick -= cursorBlinkTimer_Tick;

				cursorBlinkTimer = new Timer(_CursorBlinkTime, Timer.TimeUnits.Milisecond);

				cursorBlinkTimer.Tick += new TimerEvent(cursorBlinkTimer_Tick);
			}
		}
		bool drawCursor = true;
		Timer cursorBlinkTimer;
		public float CursorSize = 0.3F;

		int _OriginX;
		public int OriginX
		{
			get { return _OriginX; }
			set
			{
				_OriginX = value;
			}
		}
		int _OriginY;
		public int OriginY
		{
			get { return _OriginY; }
			set
			{
				_OriginY = value;
			}
		}

		TextInputMachine inputMachine;

		public Console()
		{
			CursorBlinkTime = 500;
			FocusChanged += new BooleanChangedEvent(Console_FocusChanged);
		}

		public virtual void Initialize(int bufferWidth, int bufferHeight)
		{
			Initialize(bufferWidth, bufferHeight, DefaultSkinFile);
		}
		public virtual void Initialize(int bufferWidth, int bufferHeight, ISkinFile file)
		{
			Initialize(bufferWidth, bufferHeight, DefaultCategory, file);
		}
		public virtual void Initialize(int bufferWidth, int bufferHeight, string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;
			display = new char[bufferWidth, bufferHeight];
			Font = GetMonoFont(file, category, "Font");

			Width = bufferWidth * (Font.CharWidth + Font.HorizontalSpace);
			Height = bufferHeight * (Font.CharHeight + Font.VerticalSpace);

			inputMachine = new TextInputMachine(KeyboardInput, Font);
			inputMachine.CursorMoved += new TextInputMachine.CursorEvent(machine_CursorMoved);
			inputMachine.Cursor = inputMachine.Cursor; //This will call machine_CursorMoved()

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (!cursorBlinkTimer.Started)
				cursorBlinkTimer.Start(gameTime);

			cursorBlinkTimer.Update(gameTime);

			if (TerminalMode && HasFocus)
			{
				if (KeyboardInput.ClickedKeys.Count != 0 && inputMachine.GetChar(KeyboardInput.ClickedKeys[0]) == '\n')
				{
					AddCurrentLineParts(Lines);
					CurrentLine = "";
					updateOriginToCursor();
				}

				List<string> allLines = GetAllLines();

				inputMachine.Update(gameTime);

				int lineEndX = OriginX + BufferWidth;
				int lineEndY = OriginY + BufferHeight;

				//Clearing the display
				for (int bufferX = 0; bufferX < display.GetLength(0); bufferX++)
					for (int bufferY = 0; bufferY < display.GetLength(1); bufferY++)
						display[bufferX, bufferY] = '\0';

				for (int y = OriginY; y < lineEndY; y++)
				{
					if (y < 0)
						continue;
					if (y >= allLines.Count)
						break;

					string line = allLines[y];

					for (int x = OriginX; x < Math.Min(lineEndX, OriginX + line.Length); x++)
						display[x, y - OriginY] = line[x];
				}
			}
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			render.DrawRect(new Rectangle(x, y, DrawSpaceWidth, DrawSpaceHeight), BackColor);

			int drawX = x;
			int drawY = y;
			for (int iy = 0; iy < BufferHeight; iy++)
			{
				for (int ix = 0; ix < BufferWidth; ix++)
				{
					if (display[ix, iy] != '\0')
						Font.DrawChar(display[ix, iy], new Point(drawX, drawY), FontColor, render);
					drawX += Font.CharWidth + Font.HorizontalSpace;
				}

				drawX = x - OriginX;
				drawY += Font.CharHeight + Font.VerticalSpace;
			}

			if (TerminalMode && drawCursor)
			{
				int sum = 0;
				int cursorY = Lines.Count - OriginY;
				int cursorX = OriginX;

				List<string> currentLineParts = new List<string>();
				AddCurrentLineParts(currentLineParts);

				string lastPart = "";
				bool partIsFullSize = false;
				foreach (string part in currentLineParts)
				{
					lastPart = part;

					sum += part.Length;

					if (inputMachine.Cursor <= sum)
					{
						partIsFullSize = part.Length == BufferWidth && sum == inputMachine.Cursor;
						break;
					}

					cursorY += 1;
				}

				if (partIsFullSize)
				{
					cursorX = OriginX;
					cursorY += 1;
				}
				else
					cursorX = inputMachine.Cursor - sum + lastPart.Length;

				int cursorHeight = (int)Math.Round(Font.CharHeight * CursorSize, 0);
				render.DrawRect(new Rectangle(cursorX * (Font.CharWidth + Font.HorizontalSpace) + x,
												cursorY * (Font.CharHeight + Font.VerticalSpace) + Font.CharHeight - cursorHeight + y,
												Font.CharWidth,
												cursorHeight), FontColor);
			}

			render.End();
		}

		List<string> GetAllLines()
		{
			List<string> allLines = new List<string>(Lines);

			AddCurrentLineParts(allLines);

			return allLines;
		}
		void AddCurrentLineParts(List<string> lines)
		{
			foreach (string line in CurrentLine.Split('\n'))
			{
				string lineCopy = line;
				while (BufferWidth <= lineCopy.Length)
				{
					lines.Add(lineCopy.Substring(0, BufferWidth));
					lineCopy = lineCopy.Substring(BufferWidth);
				}

				if (lineCopy != "" || line == "")
					lines.Add(lineCopy);
			}
		}

		void cursorBlinkTimer_Tick(int ticks)
		{
			drawCursor = !drawCursor;
		}

		void Console_FocusChanged(object sender, bool newValue)
		{
			if (!newValue)
			{
				inputMachine.EndCurrentInput();
			}
		}

		void machine_CursorMoved(object sender, int moveAmount, int oldPosition)
		{
			updateOriginToCursor();
		}
		void updateOriginToCursor()
		{
			if (!TerminalMode)
				return;

			int sum = 0;
			int cursorY = Lines.Count;

			List<string> currentLineParts = new List<string>();
			AddCurrentLineParts(currentLineParts);

			string lastPart = "";
			bool partIsFullSize = false;
			foreach (string part in currentLineParts)
			{
				lastPart = part;

				sum += part.Length;

				if (inputMachine.Cursor <= sum)
				{
					partIsFullSize = part.Length == BufferWidth && sum == inputMachine.Cursor;
					break;
				}

				cursorY += 1;
			}

			if (partIsFullSize)
				cursorY += 1;

			int h = (int)Math.Round((float)DrawSpaceHeight / (Font.CharHeight + Font.VerticalSpace), 0);
			if (cursorY >= OriginY + h)
			{
				OriginY = cursorY - h + 1;
			}

			if (cursorY < OriginY)
			{
				OriginY = cursorY;
			}
		}
	}
}
