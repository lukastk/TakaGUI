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
	public class Button : DrawBox
	{
		#region Events
		public event DefaultEvent Click;

		#endregion

		#region Textures
		public static string DefaultCategory = "Button";

		public ISprite ButtonTexture;
		public ISprite ButtonTexturePressed;

		ISprite textureInUse;

		#endregion

		bool _Pressed;
		public bool Pressed
		{
			get { return _Pressed; }
			set
			{
				_Pressed = value;

				if (_Pressed)
				{
					textureInUse = ButtonTexturePressed;
				}
				else
				{
					textureInUse = ButtonTexture;
				}
			}
		}

		public override int Width { get { return textureInUse.Width; } set { } }
		public override int Height { get { return textureInUse.Height; } set { } }
		public override int MinWidth { get { return textureInUse.Width; } }
		public override int MinHeight { get { return textureInUse.Height; } }
		public override int MaxWidth { get { return textureInUse.Width; } }
		public override int MaxHeight { get { return textureInUse.Height; } }

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			ButtonTexture = GetTexture(file, category, "Button");
			ButtonTexturePressed = GetTexture(file, category, "ButtonPressed");

			Pressed = false;

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			CheckIfButtonIsPressed();

			if (HasFocus && MouseInput.HasReleased(MouseButtons.Left).X != -1 && IsUnderMouse)
				if (Click != null)
					Click(this);
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			render.DrawSprite(textureInUse, new Vector2(x, y), Color.White);

			render.End();
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
