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
	public class ResizableButton : DrawBox
	{
		#region Events
		public event DefaultEvent Click;

		#endregion

		#region Textures
		public static string DefaultCategory = "ResizableButton";

		public ISprite TopLeftCorner;
		public ISprite TopRightCorner;
		public ISprite BottomLeftCorner;
		public ISprite BottomRightCorner;

		public ISprite TopBorder;
		public ISprite BottomBorder;
		public ISprite LeftBorder;
		public ISprite RightBorder;

		public ISprite Inside;

		public ISprite TopLeftCornerPressed;
		public ISprite TopRightCornerPressed;
		public ISprite BottomLeftCornerPressed;
		public ISprite BottomRightCornerPressed;

		public ISprite TopBorderPressed;
		public ISprite BottomBorderPressed;
		public ISprite LeftBorderPressed;
		public ISprite RightBorderPressed;

		public ISprite InsidePressed;

		ISprite topLeftInUse;
		ISprite topRightInUse;
		ISprite bottomLeftInUse;
		ISprite bottomRightInUse;

		ISprite topInUse;
		ISprite bottomInUse;
		ISprite leftInUse;
		ISprite rightInUse;

		ISprite insideInUse;

		#endregion

		bool _Pressed;
		public virtual bool Pressed
		{
			get { return _Pressed; }
			set
			{
				_Pressed = value;

				if (_Pressed)
				{
					topLeftInUse = TopLeftCornerPressed;
					topRightInUse = TopRightCornerPressed;
					bottomLeftInUse = BottomLeftCornerPressed;
					bottomRightInUse = BottomRightCornerPressed;

					topInUse = TopBorderPressed;
					bottomInUse = BottomBorderPressed;
					leftInUse = LeftBorderPressed;
					rightInUse = RightBorderPressed;

					insideInUse = InsidePressed;
				}
				else
				{
					topLeftInUse = TopLeftCorner;
					topRightInUse = TopRightCorner;
					bottomLeftInUse = BottomLeftCorner;
					bottomRightInUse = BottomRightCorner;

					topInUse = TopBorder;
					bottomInUse = BottomBorder;
					leftInUse = LeftBorder;
					rightInUse = RightBorder;

					insideInUse = Inside;
				}
			}
		}

		public string Title = String.Empty;
		public MonoFont Font;

		public override int MinWidth { get { return Math.Max(leftInUse.Width, topLeftInUse.Width) + Math.Max(rightInUse.Width, topRightInUse.Width); } }
		public override int MinHeight { get { return Math.Max(topInUse.Height, topLeftInUse.Height) + Math.Max(bottomInUse.Height, bottomLeftInUse.Height); } }

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			TopLeftCorner = GetTexture(file, category, "TopLeftCorner");
			TopRightCorner = GetTexture(file, category, "TopRightCorner");
			BottomLeftCorner = GetTexture(file, category, "BottomLeftCorner");
			BottomRightCorner = GetTexture(file, category, "BottomRightCorner");

			TopBorder = GetTexture(file, category, "TopBorder");
			BottomBorder = GetTexture(file, category, "BottomBorder");
			LeftBorder = GetTexture(file, category, "LeftBorder");
			RightBorder = GetTexture(file, category, "RightBorder");

			Inside = GetTexture(file, category, "Inside");

			TopLeftCornerPressed = GetTexture(file, category, "TopLeftCornerPressed");
			TopRightCornerPressed = GetTexture(file, category, "TopRightCornerPressed");
			BottomLeftCornerPressed = GetTexture(file, category, "BottomLeftCornerPressed");
			BottomRightCornerPressed = GetTexture(file, category, "BottomRightCornerPressed");

			TopBorderPressed = GetTexture(file, category, "TopBorderPressed");
			BottomBorderPressed = GetTexture(file, category, "BottomBorderPressed");
			LeftBorderPressed = GetTexture(file, category, "LeftBorderPressed");
			RightBorderPressed = GetTexture(file, category, "RightBorderPressed");

			InsidePressed = GetTexture(file, category, "InsidePressed");

			Font = GetMonoFont(file, category, "Font");

			Pressed = false;

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			CheckIfButtonIsPressed();

			//if (Pressed)
			//	return;
			if (HasFocus && MouseInput.HasReleased(MouseButtons.Left).X != -1 && IsUnderMouse)
				if (Click != null)
					Click(this);
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			render.DrawBody(new Rectangle(x, y, Width, Height), Color.White,
				topInUse,
				topRightInUse,
				rightInUse,
				bottomRightInUse,
				bottomInUse,
				bottomLeftInUse,
				leftInUse,
				topLeftInUse,
				insideInUse);

			Font.DrawString(Title, new Point(x + (Width / 2), y + (Height / 2)), new Vector2(0.5F, 0.5F), Color.White, render);

			render.End();
		}

		public void FitToText()
		{
			Point v = Font.MeasureString(Title);
			Width = MinWidth + v.X;
			Height = MinHeight + v.Y;
		}

		void CheckIfButtonIsPressed()
		{
			if (MouseInput.IsPressed(MouseButtons.Left) && IsUnderMouse)
				Pressed = true;
			else
				Pressed = false;
		}
	}
}
