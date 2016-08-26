using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TakaGUI.Data;
using System.IO;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class IconButton : ResizableButton
	{
		public new static string DefaultCategory = "IconButton";
		public ISprite Icon;
		public ISprite IconPressed;
		ISprite iconInUse;

		public bool BeSquare = true;

		public override bool Pressed
		{
			get { return base.Pressed; }
			set
			{
				base.Pressed = value;

				if (Pressed)
					iconInUse = IconPressed;
				else
					iconInUse = Icon;
			}
		}
		
		public override void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			Icon = GetTexture(file, category, "Icon");
			IconPressed = GetTexture(file, category, "Icon");

			base.Initialize(category, file);

			reloadSize();
		}

		int minWidth, minHeight;
		public override int MinWidth { get { return minWidth; } }
		public override int MinHeight { get { return minHeight; } }

		void reloadSize()
		{
			minWidth = base.MinWidth + iconInUse.Width;
			minHeight = base.MinHeight + iconInUse.Height;
			if (BeSquare)
			{
				minWidth = Math.Max(minWidth, minHeight);
				minHeight = minWidth;
				Width = Math.Max(Width, Height);
				Height = Width;
			}
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			reloadSize();
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			base.Project(gameTime, x, y, render);

			render.Begin();
			render.DrawSprite(iconInUse, new Vector2(x + (Width / 2) - iconInUse.Width / 2, y + (Height / 2) - iconInUse.Height / 2), Color.White);
			render.End();
		}
	}
}
