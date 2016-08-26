using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TakaGUI.DrawBoxes
{
	public class VScrollPanel : Panel
	{
		public VScrollbar Scrollbar { get; private set; }

		int highestY;

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			Scrollbar = new VScrollbar();
			Scrollbar.Initialize();
			Scrollbar.Width = 15;

			var slot = AddNewSlot();
			PutDrawBoxInSlot(Scrollbar, slot);
			SizeChanged += delegate(object sender, Point oldSize, Point newSize)
			{
				Scrollbar.Height = newSize.Y;
				Scrollbar.MaxValue = highestY - Height;
			};
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);
		}

		protected override void DrawBoxHasBeenAdded(DrawBox box, SlotBox.SlotHandler handler)
		{
			base.DrawBoxHasBeenAdded(box, handler);

			foreach (var d in DrawBoxList)
			{
				if (d.Y + d.Height > highestY)
					highestY = d.Y + d.Height;
			}

			Scrollbar.Height = Height;
			Scrollbar.MaxValue = highestY - Height;
		}

		protected override Origin GenerateChildOrigin(SlotBox.SlotHandler id)
		{
			if (id.DrawBox != Scrollbar && Scrollbar.MaxValue != 0)
				return new Origin(0, (int)Math.Round(-(highestY - Height) * ((float)Scrollbar.ScrollerPosition / Scrollbar.MaxValue), 0));

			return base.GenerateChildOrigin(id);
		}

		protected override Services.ViewRect GenerateSlotBoundaries(SlotBox.SlotHandler slotId, int newWidth, int newHeight)
		{
			if (slotId.DrawBox == Scrollbar)
				return new Services.ViewRect(RealX + Width - Scrollbar.Width, RealY, Scrollbar.Width, Scrollbar.Height);

			return base.GenerateSlotBoundaries(slotId, newWidth - Scrollbar.Width, newHeight);
		}
	}
}
