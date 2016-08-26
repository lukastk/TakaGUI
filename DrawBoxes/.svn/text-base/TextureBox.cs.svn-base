using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TakaGUI.DrawBoxes
{
	public class TextureBox : DrawBox
	{
		public ISprite Texture;

		public void Initialize()
		{
			base.BaseInitialize();
		}

		public void WrapTexture()
		{
			Width = Texture.Width;
			Height = Texture.Height;
		}

		public override void Project(Microsoft.Xna.Framework.GameTime gameTime, int x, int y, Services.IRender render)
		{
			base.Project(gameTime, x, y, render);

			render.Begin();

			render.DrawSprite(Texture, new Rectangle(x, y, Width, Height), Color.White);

			render.End();
		}
	}
}
