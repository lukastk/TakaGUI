using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakaGUI.DrawBoxes
{
	public class GridPanel : SlotBox
	{
		int[] cellWidths;
		int[] cellHeights;
		SlotHandler[,] SlotArray;

		public Size GridSize
		{
			get;
			private set;
		}

		public int MarginX = 2;
		public int MarginY = 2;

		int minWidth;
		int minHeight;
		public override int Width
		{
			get
			{
				return minWidth;
			}
			set
			{
			}
		}
		public override int Height
		{
			get
			{
				return minHeight;
			}
			set
			{
			}
		}
		public override int MinWidth
		{
			get { return minWidth; }
		}
		public override int MinHeight
		{
			get { return minHeight; }
		}
		public override int MaxWidth
		{
			get
			{
				return MinWidth;
			}
		}
		public override int MaxHeight
		{
			get
			{
				return MinHeight;
			}
		}

		void drawBox_SizeChanged(object sender, Microsoft.Xna.Framework.Point oldSize, Microsoft.Xna.Framework.Point newSize)
		{
			CheckSize();
		}

		public virtual void Initialize(int width, int height)
		{
			GridSize = new Size(width, height);
			cellWidths = new int[width];
			cellHeights = new int[height];
			SlotArray = new SlotHandler[width, height];

			base.BaseInitialize();
		}

		public void AddDrawBox(DrawBox drawBox)
		{
			for (int y = 0; y < SlotArray.GetLength(1); y++)
				for (int x = 0; x < SlotArray.GetLength(0); x++)
					if (SlotArray[x, y] == null)
					{
						SetDrawBoxAt(drawBox, x, y);
						return;
					}
		}

		public void RemoveDrawBox(DrawBox drawBox)
		{
			for (int x = 0; x < SlotArray.GetLength(0); x++)
				for (int y = 0; y < SlotArray.GetLength(1); y++)
					if (SlotArray[x, y] != null && SlotArray[x, y].DrawBox == drawBox)
					{
						RemoveSlotAt(x, y);
						return;
					}
		}

		public void SetNewSize(int width, int height)
		{
			GridSize = new Size(width, height);

			SlotHandler[,] oldArray = SlotArray;
			SlotArray = new SlotHandler[width, height];

			for (int x = 0; x < Math.Min(oldArray.GetLength(0), width); x++)
				for (int y = 0; y < Math.Min(oldArray.GetLength(1), height); y++)
					SlotArray[x, y] = oldArray[x, y];

			if (width != cellWidths.Length)
			{
				int[] oldCellWidths = cellWidths;
				cellWidths = new int[width];
				for (int n = 0; n < Math.Min(oldCellWidths.Length, width); n++)
					cellWidths[n] = oldCellWidths[n];
			}

			if (height != cellHeights.Length)
			{
				int[] oldCellHeights = cellHeights;
				cellHeights = new int[height];
				for (int n = 0; n < Math.Min(oldCellHeights.Length, height); n++)
					cellHeights[n] = oldCellHeights[n];
			}

			for (int x = 0; x < oldArray.GetLength(0); x++)
				for (int y = 0; y < oldArray.GetLength(1); y++)
					if (x > width || y > height)
						RemoveSlot(oldArray[x, y]);
		}

		public void SetDrawBoxAt(DrawBox drawBox, int x, int y)
		{
			if (SlotArray[x, y] != null)
				RemoveDrawBoxInSlot(SlotArray[x, y]);

			SlotArray[x, y] = AddNewSlot();

			if (drawBox != null)
			{
				PutDrawBoxInSlot(drawBox, SlotArray[x, y]);
				drawBox.SizeChanged += new SizeChangedEvent(drawBox_SizeChanged);
			}

			CheckSize();
		}
		public void RemoveSlotAt(int x, int y)
		{
			if (SlotArray[x, y].DrawBox != null)
				SlotArray[x, y].DrawBox.SizeChanged -= drawBox_SizeChanged;

			RemoveSlot(SlotArray[x, y]);
			SlotArray[x, y] = null;

			CheckSize();
		}

		public override void Idle(Microsoft.Xna.Framework.GameTime gameTime)
		{
			//Don't allow the children to have an alignment.
			foreach (DrawBox box in DrawBoxList)
				box.Alignment = DrawBoxAlignment.GetEmpty();

			for (int x = 0; x < SlotArray.GetLength(0); x++)
			{
				for (int y = 0; y < SlotArray.GetLength(1); y++)
				{
					if (SlotArray[x, y] == null || SlotArray[x, y].DrawBox == null)
						continue;

					int w = (from wx in Enumerable.Range(0, x) select cellWidths[wx]).Sum();
					SlotArray[x, y].DrawBox.X = w + x * (MarginX) + (cellWidths[x] / 2) - SlotArray[x, y].DrawBox.Width / 2;

					int h = (from hy in Enumerable.Range(0, y) select cellHeights[hy]).Sum();
					SlotArray[x, y].DrawBox.Y = h + y * (MarginY) + (cellHeights[y] / 2) - SlotArray[x, y].DrawBox.Height / 2;
				}
			}
		}

		public void CheckSize()
		{
			for (int x = 0; x < SlotArray.GetLength(0); x++)
			{
				if ((from y in Enumerable.Range(0, SlotArray.GetLength(1))
					 where (!object.Equals(SlotArray[x, y], null) && !object.Equals(SlotArray[x, y].DrawBox, null))
					 select SlotArray[x, y]).Count() != 0)
				{
					cellWidths[x] = (from y in Enumerable.Range(0, SlotArray.GetLength(1))
									 where (!object.Equals(SlotArray[x, y], null) && !object.Equals(SlotArray[x, y].DrawBox, null))
									 select SlotArray[x, y].DrawBox.Width).Max();
				}
			}

			for (int y = 0; y < SlotArray.GetLength(1); y++)
			{
				if ((from x in Enumerable.Range(0, SlotArray.GetLength(0))
					 where (!object.Equals(SlotArray[x, y], null) && !object.Equals(SlotArray[x, y].DrawBox, null))
					 select SlotArray[x, y]).Count() != 0)
				{
					cellHeights[y] = (from x in Enumerable.Range(0, SlotArray.GetLength(0))
									  where (!object.Equals(SlotArray[x, y], null) && !object.Equals(SlotArray[x, y].DrawBox, null))
									  select SlotArray[x, y].DrawBox.Height).Max();
				}
			}

			minWidth = (from w in cellWidths select w).Sum() + GridSize.Width * MarginX - MarginX;
			minHeight = (from h in cellHeights select h).Sum() + GridSize.Height * MarginY - MarginY;
		}
	}
}
