using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TakaGUI.Data;
using TakaGUI.Services;

namespace TakaGUI
{
	/// <summary>
	/// Summary:
	///		A MonoFont instance is used to draw text, where the outputted text is drawn with the MonoFontData the instance
	///		is provided.
	/// </summary>
	public class MonoFont : IDisposable
	{
		public ISprite FontSprite;
		public string[] Characters;
		public int CharHeight;
		public int CharWidth;
		public int GridSize;
		public int HorizontalSpace;
		public int VerticalSpace;

		Dictionary<char, Rectangle> charRects = new Dictionary<char, Rectangle>();

		public MonoFont(ISprite fontSprite, string[] characters, int charWidth,
			int charHeight, int gridSize, int horizontalSpace, int verticalSpace)
		{
			FontSprite = fontSprite;
			Characters = characters;
			CharHeight = charHeight;
			CharWidth = charWidth;
			GridSize = gridSize;
			HorizontalSpace = horizontalSpace;
			VerticalSpace = verticalSpace;

			foreach (string line in Characters)
			{
				foreach (char c in line)
					charRects.Add(c, GenerateCharRect(c));
			}
		}

		public void DrawString(string text, Point position, Color color, IRender render)
		{
			DrawString(text, position, new Vector2(0, 0), color, render);
		}
		public void DrawString(string text, Point position, Vector2 origin, Color color, IRender render)
		{
			float addX = 0;
			float addY = 0;

			if (origin.X != 0 || origin.Y != 0)
			{
				Point stringSize = MeasureString(text);
				addX = -stringSize.X * origin.X;
				addY = -stringSize.Y * origin.Y;
			}

			foreach (char c in text)
			{
				if (c == '\n')
				{
					addY += VerticalSpace + CharHeight;
					addX = 0;
				}
				else
				{
					DrawChar(c, new Point((int)(position.X + addX),
											(int)(position.Y + addY)), color, render);

					addX += HorizontalSpace + CharWidth;
				}
			}
		}

		public void DrawChar(char c, Point position, Color color, IRender render)
		{
			Rectangle sourceRect = GetCharRect(c);
			if (sourceRect.IsEmpty)
				return;

			render.DrawSprite(FontSprite,
				new Rectangle(position.X,
							   position.Y,
							   CharWidth,
							   CharHeight),
							   sourceRect, color);
		}
		public void DrawChar(char c, Point position, Vector2 origin, Color color, IRender render)
		{
			int addX = 0;
			int addY = 0;

			if (origin.X != 0 && origin.Y != 0)
			{
				addX = -(int)Math.Round(CharWidth * origin.X, 0);
				addY = -(int)Math.Round(CharHeight * origin.Y, 0);
			}

			Rectangle sourceRect = GetCharRect(c);
			if (sourceRect.IsEmpty)
				return;

			render.DrawSprite(FontSprite,
				new Rectangle(position.X + addX,
							   position.Y + addY,
							   CharWidth,
							   CharHeight),
							   sourceRect, color);
		}

		public Point MeasureString(string text)
		{
			var textSize = new Point();
			string[] lines = text.Split(new[] { '\n' });
			foreach (string line in lines)
			{
				textSize.X = Math.Max(line.Length, textSize.X);
			}

			textSize.X = textSize.X * HorizontalSpace + textSize.X * CharWidth;
			textSize.Y = (lines.Length - 1) * VerticalSpace + lines.Length * CharHeight;

			return textSize;
		}

		Rectangle GenerateCharRect(char chr)
		{
			int posX = 0;
			int posY = 0;
			//Get the character position in the character matrix:
			bool flag = true;
			foreach (string line in Characters)
			{
				foreach (char c in line)
				{
					if (c == chr)
					{
						flag = false;
						break;
					}

					posX += 1;

				}

				if (!flag)
					break;

				posY += 1;
				posX = 0;
			}

			posX = GridSize * posX + CharWidth * posX;
			posY = GridSize * posY + CharHeight * posY;

			if (!flag)
				return new Rectangle(posX, posY, CharWidth, CharHeight);

			return new Rectangle();
		}

		public Rectangle GetCharRect(char c)
		{
			if (charRects.ContainsKey(c))
				return charRects[c];
			else
				return new Rectangle(0, 0, 0, 0);
		}

		public bool HasChar(char chr)
		{
			foreach (string line in Characters)
				if (line.Contains(chr))
					return true;

			//Other characters
			switch (chr)
			{
				case '\n':
					return true;
			}

			return false;
		}

		#region IDisposable
		private bool disposed = false;

		public void Dispose()
		{
			if (!disposed)
			{
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}

		#endregion
	}
}