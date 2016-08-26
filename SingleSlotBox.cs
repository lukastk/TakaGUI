using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TakaGUI.Data;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Input;
using TakaGUI.Services;

namespace TakaGUI
{
	public delegate void DrawBoxEvent(object sender, DrawBox drawBox);

	/// <summary>
	/// Summary:
	///		IDrawBoxContainer instances contain DrawBoxes and handles focus and input.
	/// </summary>
	public abstract class SingleSlotBox : SlotBox
	{
		#region Events
		public event DrawBoxEvent DrawBoxAdded;
		public event DrawBoxEvent DrawBoxRemoved;

		void ContainerBox_KeyDown(object sender, Keys key)
		{
			if (key == Keys.Tab && tabOrder.Count != 0)
			{
				if (DrawBoxWithFocus != null)
					DrawBoxWithFocus = tabOrder.GetNext(DrawBoxWithFocus);
				else
					DrawBoxWithFocus = tabOrder.First();
			}
		}

		#endregion

		#region Container Fields
		public new ReadOnlyCollection<DrawBox> DrawBoxList
		{
			get { return base.DrawBoxList; }
		}
		protected Origin ChildrenDrawOrigin = new Origin();

		TabOrder tabOrder = new TabOrder(Debug);

		public bool AlwaysWrapWidth = false;
		public bool AlwaysWrapHeight = false;

		#endregion

		#region Container Methods
		protected override ViewRect GenerateSlotBoundaries(SlotBox.SlotHandler slotId, int newWidth, int newHeight)
		{
			return GetDefaultBoundaries(newWidth, newHeight);
		}

		public abstract ViewRect GetDefaultBoundaries(int newWidth, int newHeight);
		public ViewRect GetDefaultBoundaries() { return GetDefaultBoundaries(Width, Height); }

		public DrawBox GetDrawBoxWithFocus()
		{
			return DrawBoxWithFocus;
		}
		public DrawBox GetDrawBoxUnderMouse()
		{
			return DrawBoxUnderMouse;
		}

		public void AddDrawBox(DrawBox drawBox)
		{
			PutDrawBoxInSlot(drawBox, AddNewSlot());

			if (DrawBoxAdded != null)
				DrawBoxAdded(this, drawBox);
		}

		public TabOrder GetTabOrder()
		{
			return tabOrder;
		}

		public void PutAllDrawboxesInRemoveQue()
		{
			base.ClearDrawBoxes();
		}

		#endregion

		#region Control Methods
		public void Expand(int top, int bottom, int left, int right)
		{
			Dictionary<DrawBox, DrawBoxAlignment> alignments = new Dictionary<DrawBox, DrawBoxAlignment>();

			foreach (var d in DrawBoxList)
			{
				alignments.Add(d, d.Alignment);
				d.Alignment = DrawBoxAlignment.GetEmpty();
			}

			Y -= top;
			Height += top;

			Height += bottom;

			X -= left;
			Width += left;

			Width += right;

			foreach (var d in DrawBoxList)
			{
				d.X += top;
				d.Y += bottom;
				d.Alignment = alignments[d];
			}
		}

		#endregion

		protected override void DrawBoxHasBeenRemoved(DrawBox box, SlotHandler handler)
		{
			RemoveSlot(handler);

			if (DrawBoxRemoved != null)
				DrawBoxRemoved(this, box);
		}

		public void Wrap()
		{
			WrapWidth();
			WrapHeight();
		}
		public virtual void WrapWidth()
		{//TODO: FIX THIS!!!
			Dictionary<DrawBox, DrawBoxAlignment> alignments = new Dictionary<DrawBox, DrawBoxAlignment>();

			foreach (var d in DrawBoxList)
			{
				alignments.Add(d, d.Alignment);
				d.Alignment = DrawBoxAlignment.GetEmpty();
			}

			int leastX = 0;
			foreach (DrawBox drawBox in DrawBoxList)
			{
				if (drawBox.X < leastX)
					leastX = drawBox.X;
			}

			foreach (DrawBox drawBox in DrawBoxList)
			{
				drawBox.X += -leastX;
			}

			Width = 0;
			foreach (DrawBox drawBox in DrawBoxList)
			{
				if (drawBox.X + drawBox.Width > Width)
					Width = drawBox.X + drawBox.Width;
			}

			foreach (var d in DrawBoxList)
			{
				d.Alignment = alignments[d];
			}
		}
		public virtual void WrapHeight()
		{
			Dictionary<DrawBox, DrawBoxAlignment> alignments = new Dictionary<DrawBox, DrawBoxAlignment>();

			foreach (var d in DrawBoxList)
			{
				alignments.Add(d, d.Alignment);
				d.Alignment = DrawBoxAlignment.GetEmpty();
			}

			int leastY = 0;
			if (DrawBoxList.Count != 0)
				leastY = DrawBoxList[0].Y;
			foreach (DrawBox drawBox in DrawBoxList)
			{
				if (drawBox.Y < leastY)
					leastY = drawBox.Y;
			}

			foreach (DrawBox drawBox in DrawBoxList)
			{
				drawBox.Y -= leastY;
			}

			Height = MinHeight;
			foreach (DrawBox drawBox in DrawBoxList)
			{
				if (drawBox.Y + drawBox.Height > Height)
					Height = drawBox.Y + drawBox.Height;
			}

			foreach (var d in DrawBoxList)
			{
				d.Alignment = alignments[d];
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (AlwaysWrapWidth)
				WrapWidth();
			if (AlwaysWrapHeight)
				WrapHeight();
		}
	}

	public class TabOrder : IList<DrawBox>
	{
		//Services
		IDebug debug;

		List<DrawBox> order = new List<DrawBox>();

		public TabOrder(IDebug _debug)
		{
			debug = _debug;
		}
		public TabOrder(IDebug _debug, List<DrawBox> _order)
		{
			debug = _debug;
			order.AddRange(_order);
		}

		public DrawBox GetNext(DrawBox current)
		{
			if (order.Contains(current) && order.Count != 0)
			{
				int index = order.IndexOf(current) + 1;
				if (index == order.Count)
					index = 0;
				return order[index];
			}
			else
			{
				debug.AddExceptionInClass(this.GetType(), "GetNext", "GetNext failed.");
				return null;
			}
		}

		public int IndexOf(DrawBox item)
		{
			return order.IndexOf(item);
		}

		public void Insert(int index, DrawBox item)
		{
			order.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			order.RemoveAt(index);
		}

		public DrawBox this[int index]
		{
			get
			{
				return order[index];
			}
			set
			{
				order[index] = value;
			}
		}

		public void Add(DrawBox item)
		{
			if (!order.Contains(item))
				order.Add(item);
			else
				debug.AddExceptionInClass(this.GetType(), "Add", "Can't add the same item twice");
		}

		public void AddRange(IList<DrawBox> list)
		{
			foreach (DrawBox d in list)
				Add(d);
		}

		public void Clear()
		{
			order.Clear();
		}

		public bool Contains(DrawBox item)
		{
			return order.Contains(item);
		}

		public void CopyTo(DrawBox[] array, int arrayIndex)
		{
			order.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return order.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(DrawBox item)
		{
			return order.Remove(item);
		}

		public IEnumerator<DrawBox> GetEnumerator()
		{
			return order.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return order.GetEnumerator();
		}
	}
}