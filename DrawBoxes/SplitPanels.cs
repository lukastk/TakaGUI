using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class HorizontalSplitPanel : SlotBox
	{
		public Color InnerBorderColor;
		public Color OuterBorderColor;

		public string DefaultCategory = "SplitPanel";

		public Panel TopPanel
		{
			get
			{
				if (topSlot != null)
					return (Panel)topSlot.DrawBox;
				return null;
			}
		}
		public Panel BottomPanel
		{
			get
			{
				if (bottomSlot != null)
					return (Panel)bottomSlot.DrawBox;
				return null;
			}
		}
		SlotHandler topSlot;
		SlotHandler bottomSlot;

		public bool CanMoveBorder = true;
		public bool DrawBorder = true;

		int borderSize;
		public int BorderSize
		{
			get { return borderSize; }
			set
			{
				borderSize = value;

				if (borderSize < 3)
					borderSize = 3;
				if (borderSize > Height)
					borderSize = Height;

				BorderPositon = BorderPositon;
			}
		}
		int borderPosition;
		public int BorderPositon
		{
			get { return borderPosition; }
			set
			{
				borderPosition = value;

				if (borderPosition < 0)
					borderPosition = 0;
				if (borderPosition > (Height - BorderSize))
					borderPosition = Height - BorderSize;
			}
		}
		public int ReverseBorderPosition
		{
			get { return Height - BorderPositon - BorderSize; }
			set
			{
				BorderPositon = Height - value - borderSize;
			}
		}
		public override int MinHeight
		{
			get
			{
				return BorderSize;
			}
		}

		bool isHoldingBorder;

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			InnerBorderColor = file.GetColor(category, "InnerBorderColor");
			OuterBorderColor = file.GetColor(category, "OuterBorderColor");

			Width = 100;
			Height = 100;
			BorderSize = 5;
			BorderPositon = 50;

			base.BaseInitialize();
		}

		public override void AddedToContainer()
		{
			topSlot = AddNewSlot();
			Panel topPanel = new Panel();
			topPanel.Initialize();
			PutDrawBoxInSlot(topPanel, topSlot);
			topPanel.X = 0;
			topPanel.Y = 0;
			topPanel.Fill();
			topPanel.Alignment = DrawBoxAlignment.GetFull();

			bottomSlot = AddNewSlot();
			Panel bottomPanel = new Panel();
			bottomPanel.Initialize();
			PutDrawBoxInSlot(bottomPanel, bottomSlot);
			bottomPanel.X = 0;
			bottomPanel.Y = 0;
			bottomPanel.Fill();
			bottomPanel.Alignment = DrawBoxAlignment.GetFull();
		}

		protected override ViewRect GenerateSlotBoundaries(SlotHandler handler, int newWidth, int newHeight)
		{
			if (handler == topSlot)
			{
				return new ViewRect(RealX, RealY, newWidth, BorderPositon - 1);
			}
			if (handler == bottomSlot)
			{
				return new ViewRect(RealX, RealY + BorderPositon + BorderSize - 1, newWidth, newHeight - BorderPositon);
			}

			return base.GenerateSlotBoundaries(handler, newWidth, newHeight);
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			if (!DrawBorder)
				return;

			render.Begin();

			render.DrawLine(new Vector2(x, y + borderPosition), new Vector2(x + Width, y + borderPosition), OuterBorderColor);
			render.DrawLine(new Vector2(x, y + borderPosition + BorderSize - 1), new Vector2(x + Width, y + borderPosition + BorderSize - 1), OuterBorderColor);
			render.DrawRect(new Rectangle(x, y + borderPosition, Width, BorderSize - 2), InnerBorderColor);

			render.End();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (HasFocus && MouseInput.IsPressed(MouseButtons.Left) && CanMoveBorder && DrawBorder)
			{
				if (MouseInput.IsClicked(MouseButtons.Left) &&
					IsMouseInRect(new Rectangle(
						RealX, RealY + BorderPositon, Width, BorderSize)))
				{
					isHoldingBorder = true;
				}
				else if (isHoldingBorder)
				{
					int oldPositon = BorderPositon;
					BorderPositon += MouseInput.DeltaY;

					if (MouseInput.DeltaY != 0 && BorderPositon == oldPositon)
						isHoldingBorder = false;
				}
			}
			else
				isHoldingBorder = false;
		}
	}

	public class VerticalSplitPanel : SlotBox
	{
		public Color InnerBorderColor;
		public Color OuterBorderColor;

		public string DefaultCategory = "SplitPanel";

		public Panel LeftPanel
		{
			get
			{
				if (leftSlot != null)
					return (Panel)leftSlot.DrawBox;
				return null;
			}
		}
		public Panel RightPanel
		{
			get
			{
				if (rightSlot != null)
					return (Panel)rightSlot.DrawBox;
				return null;
			}
		}
		SlotHandler leftSlot;
		SlotHandler rightSlot;

		public bool CanMoveBorder = true;
		public bool DrawBorder = true;

		int borderSize;
		public int BorderSize
		{
			get { return borderSize; }
			set
			{
				borderSize = value;

				if (borderSize < 3)
					borderSize = 3;
				if (borderSize > Width)
					borderSize = Width;

				BorderPositon = BorderPositon;
			}
		}
		int borderPosition;
		public int BorderPositon
		{
			get { return borderPosition; }
			set
			{
				borderPosition = value;

				if (borderPosition < 0)
					borderPosition = 0;
				if (borderPosition > (Width - BorderSize))
					borderPosition = Width - BorderSize;
			}
		}
		public int ReverseBorderPosition
		{
			get { return Width - BorderPositon - BorderSize; }
			set
			{
				BorderPositon = Width - value - borderSize;
			}
		}
		public override int MinWidth
		{
			get
			{
				return BorderSize;
			}
		}

		bool isHoldingBorder;

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			InnerBorderColor = file.GetColor(category, "InnerBorderColor");
			OuterBorderColor = file.GetColor(category, "OuterBorderColor");

			Width = 100;
			Height = 100;
			BorderSize = 5;
			BorderPositon = 50;

			base.BaseInitialize();
		}

		public override void AddedToContainer()
		{
			leftSlot = AddNewSlot();
			Panel leftPanel = new Panel();
			leftPanel.Initialize();
			PutDrawBoxInSlot(leftPanel, leftSlot);
			leftPanel.X = 0;
			leftPanel.Y = 0;
			leftPanel.Fill();
			leftPanel.Alignment = DrawBoxAlignment.GetFull();

			rightSlot = AddNewSlot();
			Panel rightPanel = new Panel();
			rightPanel.Initialize();
			PutDrawBoxInSlot(rightPanel, rightSlot);
			rightPanel.X = 0;
			rightPanel.Y = 0;
			rightPanel.Fill();
			rightPanel.Alignment = DrawBoxAlignment.GetFull();
		}

		protected override ViewRect GenerateSlotBoundaries(SlotBox.SlotHandler handler, int newWidth, int newHeight)
		{
			if (handler == leftSlot)
			{
				return new ViewRect(RealX, RealY, BorderPositon - 1, newHeight);
			}
			if (handler == rightSlot)
			{
				return new ViewRect(RealX + BorderPositon + BorderSize - 1, RealY, newWidth - BorderPositon, newHeight);
			}

			return base.GenerateSlotBoundaries(handler, newWidth, newHeight);
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			if (!DrawBorder)
				return;

			render.Begin();

			render.DrawLine(new Vector2(x + borderPosition, y), new Vector2(x + borderPosition, y + Height), OuterBorderColor);
			render.DrawLine(new Vector2(x + borderPosition + BorderSize - 1, y), new Vector2(x + borderPosition + BorderSize - 1, y + Height), OuterBorderColor);
			render.DrawRect(new Rectangle(x + borderPosition, y,  BorderSize - 2, Height), InnerBorderColor);

			render.End();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (HasFocus && MouseInput.IsPressed(MouseButtons.Left) && CanMoveBorder)
			{
				if (MouseInput.IsClicked(MouseButtons.Left) &&
					IsMouseInRect(new Rectangle(
						RealX + BorderPositon, RealY, BorderSize, Height)))
				{
					isHoldingBorder = true;
				}
				else if (isHoldingBorder)
				{
					int oldPositon = BorderPositon;
					BorderPositon += MouseInput.DeltaX;

					if (MouseInput.DeltaX != 0 && BorderPositon == oldPositon)
						isHoldingBorder = false;
				}
			}
			else
				isHoldingBorder = false;
		}
	}
}
