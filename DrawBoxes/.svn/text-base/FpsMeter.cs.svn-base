using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class FpsMeter : DrawBox
	{
		public static string DefaultCategory = "FpsMeter";

		public MonoFont Font;
		public Color FontColor = Color.Black;
		public Color BackgroundColor = Color.White;
		public Color BumpColor = Color.Red;

		public override int MinHeight
		{
			get
			{
				if (Font != null)
				{
					int minHeight = 0;
					if (ShowUpdateFps)
						minHeight += Font.CharHeight;
					if (ShowDrawFps)
						minHeight += Font.CharHeight;
					if (ShowDrawFps && ShowDrawFps)
						minHeight += margin;
					if (ShowBump)
						minHeight += 2;

					return minHeight;
				}

				return 0;
			}
		}
		public override int MaxHeight { get { return MinHeight; } }

		public bool ShowUpdateFps = true;
		public bool ShowDrawFps = true;
		public bool ShowBump = true;

		public double UpdateFps;
		public double DrawFps;
		List<double> updateFpsList = new List<double>();
		List<double> drawFpsList = new List<double>();
		uint meanFloor = 100;

		int bumpX = 0;
		int speed = 1;

		const int margin = 3;

		public FpsMeter()
		{
			HasToBeInFront = true;
		}

		public virtual void Initialize()
		{
			Initialize(DefaultSkinFile);
		}
		public virtual void Initialize(ISkinFile file)
		{
			Initialize(DefaultCategory, file);
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

		public override void Idle(Microsoft.Xna.Framework.GameTime gameTime)
		{
			if (updateFpsList.Count == meanFloor)
				updateFpsList.RemoveAt(0);
			double add = 1 / gameTime.ElapsedGameTime.TotalSeconds;
			if (!double.IsInfinity(add))
				updateFpsList.Add(1 / gameTime.ElapsedGameTime.TotalSeconds);

			UpdateFps = 0;
			foreach (double fps in updateFpsList)
				UpdateFps += fps;
			UpdateFps /= updateFpsList.Count;

			Width = Math.Max(Font.MeasureString("DrawFps: " + DrawFps.ToString()).X, Font.MeasureString("UpdateFps: " + UpdateFps.ToString()).X);
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			if (drawFpsList.Count == meanFloor)
				drawFpsList.RemoveAt(0);
			double add = 1 / gameTime.ElapsedGameTime.TotalSeconds;
			if (!double.IsInfinity(add))
				drawFpsList.Add(1 / gameTime.ElapsedGameTime.TotalSeconds);

			DrawFps = 0;
			foreach (double fps in drawFpsList)
				DrawFps += fps;
			DrawFps /= drawFpsList.Count;

			render.Begin();

			Color background = BackgroundColor;
			Color fontColor = FontColor;
			Color bumpColor = BumpColor;

			const float transparencyFactor = 0.3F;

			if (CheckIfUnderMouse())
			{
				background.R = Convert.ToByte(BackgroundColor.R * transparencyFactor);
				background.G = Convert.ToByte(BackgroundColor.G * transparencyFactor);
				background.B = Convert.ToByte(BackgroundColor.B * transparencyFactor);
				background.A = Convert.ToByte(BackgroundColor.A * transparencyFactor);

				fontColor.R = Convert.ToByte(FontColor.R * transparencyFactor);
				fontColor.G = Convert.ToByte(FontColor.G * transparencyFactor);
				fontColor.B = Convert.ToByte(FontColor.B * transparencyFactor);
				fontColor.A = Convert.ToByte(FontColor.A * transparencyFactor);

				bumpColor.R = Convert.ToByte(bumpColor.R * transparencyFactor);
				bumpColor.G = Convert.ToByte(bumpColor.G * transparencyFactor);
				bumpColor.B = Convert.ToByte(bumpColor.B * transparencyFactor);
				bumpColor.A = Convert.ToByte(bumpColor.A * transparencyFactor);
			}

			render.DrawRect(new Rectangle(x, y, Width, Height), background);

			//Draw bump
			render.DrawLine(new Vector2(x + bumpX, Height), new Vector2(x + bumpX, 0), bumpColor);
			//Move bump
			if (bumpX + speed > Width || bumpX + speed < 0)
				speed = -speed;
			bumpX += speed;

			if (ShowUpdateFps)
				Font.DrawString("DrawFps: " + UpdateFps.ToString(), new Point(x, y), fontColor, render);
			if (ShowDrawFps)
				Font.DrawString("UpdateFps: " + DrawFps.ToString(), new Point(x, y + Font.CharHeight + margin), fontColor, render);

			render.End();
		}

		public override bool CheckFocus()
		{
			return false;
		}
		public override bool CheckMouseOver()
		{
			return false;
		}

		bool CheckIfUnderMouse()
		{
			return base.CheckMouseOver();
		}
	}
}