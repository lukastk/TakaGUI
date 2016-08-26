using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TakaGUI.Data;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class Label : DrawBox
	{
		public static string DefaultCategory = "Label";

		public MonoFont Font;
		public Color FontColor = Color.White;
		public Color BackgroundColor = Color.Transparent;

		public string Text = "";

		public override int Width
		{
			get
			{
				if (Font != null)
					return Font.MeasureString(Text).X;
				
				return 0;
			}
			set
			{
			}
		}
		public override int Height
		{
			get
			{
				if (Font != null)
					return Font.MeasureString(Text).Y;
				
				return 0;
			}
			set
			{
			}
		}

		public override int MinWidth
		{
			get
			{
				return Width;
			}
		}
		public override int MaxWidth
		{
			get
			{
				return Width;
			}
		}
		public override int MinHeight
		{
			get
			{
				return Height;
			}
		}
		public override int MaxHeight
		{
			get
			{
				return Height;
			}
		}

		public Label()
		{
		}

		public Label(string text)
		{
			Text = text;
		}

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			Font = GetMonoFont(file, category, "Font");

			base.BaseInitialize();
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			render.DrawRect(new Rectangle(x, y, Width, Height), BackgroundColor);
			Font.DrawString(Text, new Point(x, y), FontColor, render);

			render.End();
		}
	}
}
