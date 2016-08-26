using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class Panel : SingleSlotBox
	{
		public Panel()
		{
		}

		public Color BackgroundColor = Color.Transparent;

		public virtual void Initialize()
		{
			base.BaseInitialize();
		}

		public override ViewRect GetDefaultBoundaries(int newWidth, int newHeight)
		{
			return new ViewRect(RealX, RealY, newWidth, newHeight);
		}

		public override void Idle(GameTime gameTime)
		{
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();
			render.DrawRect(new Rectangle(x, y, Width, Height), BackgroundColor);
			render.End();
		}
	}
}
