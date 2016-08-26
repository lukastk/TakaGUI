using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using System.IO;
using TakaGUI.Cores;

namespace TakaGUI.Services
{
	/// <summary>
	/// Summary:
	///		Used to manage the drawing of cursors. Setting CursorToUse to point specific cursor to draw makes the CursorManager to draw
	///		that specific cursor for that draw-loop.
	///		
	///		Note that this is not a GameComponent, it is a service that provides drawing and updating of cursors.
	/// </summary>
	public class CursorManager : GameComponent, ICursorManager
	{
		public int ResourceGroup = 0;
		public string DefaultCategory = "Cursors";

		ISprite cursor;
		ISprite resizeCursor;

		ISprite cursorInUse;

		Cursors _CursorToUse = Cursors.Cursor;
		public Cursors CursorToUse
		{
			get { return _CursorToUse; }
			set
			{
				_CursorToUse = value;

				switch (_CursorToUse)
				{
					case Cursors.Cursor:
						cursorInUse = cursor;
						break;
					case Cursors.ResizeCursor:
						cursorInUse = resizeCursor;
						break;
				}
			}
		}

		public CursorManager(Game game, ISkinFile skin)
			: base(game)
		{
			game.Components.Add(this);

			Init(DefaultCategory, skin);
		}
		public CursorManager(Game game, string category, ISkinFile skin)
			: base(game)
		{
			game.Components.Add(this);

			Init(category, skin);
		}
		void Init(string category, ISkinFile skin)
		{
			cursor = skin.GetSprite(ResourceGroup, category, "Cursor");
			resizeCursor = skin.GetSprite(ResourceGroup, category, "ResizeCursor");
			cursorInUse = cursor;
		}

		public override void Initialize()
		{
			base.Initialize();

			Enabled = false;
		}

		public void UpdateCursor()
		{
			CursorToUse = Cursors.Cursor;
		}

		public void DrawCursor(IRender render, float mouseX, float mouseY)
		{
			render.Begin();
			render.DrawSprite(cursorInUse, new Vector2(mouseX, mouseY), Color.White);
			render.End();
		}
	}

	public interface ICursorManager
	{
		Cursors CursorToUse { get; set; }

		void UpdateCursor();

		void DrawCursor(IRender render, float mouseX, float mouseY);
	}

	public enum Cursors
	{
		Cursor,
		ResizeCursor
	}
}
