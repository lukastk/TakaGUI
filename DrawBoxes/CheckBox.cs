using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TakaGUI;
using System.IO;
using TakaGUI.Data;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class CheckBox : DrawBox
	{
		#region Events
		public event DefaultEvent Click;
		public event BooleanChangedEvent CheckedChanged;

		#endregion

		#region Textures
		public static string DefaultCategory = "CheckBox";

		public ISprite UncheckedTexture;
		public ISprite CheckedTexture;

		ISprite textureInUse;

		#endregion

		bool _Checked;
		public bool Checked
		{
			get { return _Checked; }
			set
			{
				_Checked = value;

				if (_Checked)
				{
					textureInUse = CheckedTexture;
				}
				else
				{
					textureInUse = UncheckedTexture;
				}
			}
		}

		public bool CanChangeValue = true;

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

			UncheckedTexture = GetTexture(file, category, "Unchecked");
			CheckedTexture = GetTexture(file, category, "Checked");

			Checked = false;

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (CanChangeValue && HasFocus && MouseInput.IsClicked(MouseButtons.Left) && IsUnderMouse)
			{
				if (Click != null)
					Click(this);

				Checked = !Checked;

				if (CheckedChanged != null)
					CheckedChanged(this, Checked);
			}
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			render.DrawSprite(textureInUse, new Vector2(x, y), Color.White);

			render.End();
		}
	}
}
