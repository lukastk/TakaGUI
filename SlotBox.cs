using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using TakaGUI.DrawBoxes;
using TakaGUI.Services;

namespace TakaGUI
{
	/// <summary>
	/// Acts as containers for DrawBoxes, they provide them with "boundaries", the spaces that the drawboxes
	/// can draw in. Different drawboxes within a slotbox can have different bounds.
	/// </summary>
	public class SlotBox : DrawBox
	{
		/// <summary>
		/// If null, no drawbox has focus. Otherwise the field shows the drawbox with focus.
		/// </summary>
		protected DrawBox DrawBoxWithFocus;
		/// <summary>
		/// If null, no drawbox is under the mouse. Otherwise the field shows the drawbox under the mouse.
		/// </summary>
		protected DrawBox DrawBoxUnderMouse;

		/// <summary>
		/// Holds all slots, with the SlotHandlers as the keys and the slots as values.
		/// </summary>
		private Dictionary<SlotHandler, Slot> Slots = new Dictionary<SlotHandler, Slot>();
		/// <summary>
		/// Holds all drawboxes.
		/// </summary>
		private List<DrawBox> drawBoxList = new List<DrawBox>();
		/// <summary>
		/// Whenever RequestRemoval(DrawBox) is called, the drawbox is added to the removeQue, awaiting removal.
		/// </summary>
		private List<DrawBox> removeQue = new List<DrawBox>();
		protected ReadOnlyCollection<DrawBox> DrawBoxList;
		protected ReadOnlyCollection<SlotHandler> SlotHandlerList
		{
			get { return Slots.Keys.ToList().AsReadOnly(); }
		}

		public bool DialoguesAreHidden = false;
		public bool DarkenWhenDialogueExists = true;
		public Color DarkeningMask = new Color(0f, 0f, 0f, 0.3f);
		protected List<DrawBox> dialogues = new List<DrawBox>();
		public DrawBox GetCurrentDialogue()
		{
			if (dialogues.Count != 0)
				return dialogues.Last();

			return null;
		}
		public void PutDialogOnStack(DrawBox drawBox)
		{
			dialogues.Add(drawBox);
		}
		public void RemoveDialogFromStack(DrawBox drawBox)
		{
			dialogues.Remove(drawBox);
		}

		public SlotBox()
		{
			DrawBoxList = drawBoxList.AsReadOnly();
			SizeChanged += SlotBox_SizeChanged;
		}

		#region Control Methods
		/// <summary>
		/// Queries the slotbox whether the child-drawbox is the first to draw.
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public bool IsChildFirstToDraw(DrawBox child)
		{
			return child == DrawBoxList.First();
		}
		/// <summary>
		/// Queries the slotbox whether the child-drawbox is the last to draw.
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public bool IsChildLastToDraw(DrawBox child)
		{
			return child == DrawBoxList.Last();
		}
		/// <summary>
		/// Queries the slotbox whether the child-drawbox is the drawbox with focus.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public bool TestDrawBoxWithFocus(DrawBox box)
		{
			return DrawBoxWithFocus == box;
		}
		/// <summary>
		/// Queries the slotbox whether the child-drawbox is the drawbox under the mouse.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public bool TestDrawBoxUnderMouse(DrawBox box)
		{
			return DrawBoxUnderMouse == box;
		}
		/// <summary>
		/// Queries the slotbox whether the child-drawbox is inside the slotbox.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public bool TestIsDrawBoxInContainer(DrawBox box)
		{
			return DrawBoxList.Contains(box);
		}

		/// <summary>
		/// Tells the drawbox to put the provided child-drawbox in front of the drawing and update que.
		/// </summary>
		/// <param name="drawBox"></param>
		public void PutChildInFront(DrawBox drawBox)
		{
			if (!DrawBoxList.Contains(drawBox))
			{
				Debug.AddExceptionInClass(this.GetType(), "ContainerBox.PutChildInBack", "The DrawBox instance is not one of the alignWith-objects.");
				return;
			}

			drawBoxList.Remove(drawBox);
			drawBoxList.Add(drawBox);
		}
		/// <summary>
		/// Tells the drawbox to put the provided child-drawbox in the back of the drawing and update que.
		/// </summary>
		/// <param name="drawBox"></param>
		public void PutChildInBack(DrawBox drawBox)
		{
			if (!DrawBoxList.Contains(drawBox))
			{
				Debug.AddExceptionInClass(this.GetType(), "ContainerBox.PutChildInBack", "The DrawBox instance is not one of the alignWith-objects.");
				return;
			}

			drawBoxList.Remove(drawBox);
			drawBoxList.Insert(0, drawBox);
		}

		#endregion

		#region Boundaries
		protected virtual Origin GenerateChildOrigin(SlotHandler id)
		{
			return new Origin(0, 0);
		}
		protected Origin GenerateChildOrigin(DrawBox box)
		{
			return GenerateChildOrigin(box.Handler);
		}
		protected virtual ViewRect GetMasterBoundaries()
		{
			return Boundaries;
		}

		//TODO: make point GetChildDrawBoundary, something that doesnt affect size.
		protected ViewRect GenerateChildBoundaries(DrawBox drawBox, int newWidth, int newHeight)
		{
			if (drawBox.Container != this)
			{
				Debug.AddExceptionInClass(this.GetType(), "GenerateChildBoundaries", "Tried to get boundaries for point drawbox with point different container");
				return ViewRect.Empty;
			}

			return GenerateSlotBoundaries(drawBox.Handler);
		}
		protected ViewRect GenerateChildBoundaries(DrawBox drawBox)
		{
			return GenerateChildBoundaries(drawBox, Width, Height);
		}
		protected virtual ViewRect GenerateSlotBoundaries(SlotHandler slotId, int newWidth, int newHeight)
		{
			return new ViewRect(RealX, RealY, newWidth, newHeight);
		}
		protected ViewRect GenerateSlotBoundaries(SlotHandler slotId)
		{
			return GenerateSlotBoundaries(slotId, Width, Height);
		}

		void MakeContainerWidthValid(int oldWidth)
		{
			//TODO: check for eternal loop.

			bool widthIsValid = false;

			while (!widthIsValid)
			{
				widthIsValid = true;

				foreach (DrawBox box in DrawBoxList)
				{
					if (!box.IsContainerWidthValid(GenerateChildBoundaries(box).Width))
					{
						widthIsValid = false;
						break;
					}
				}

				if (!widthIsValid)
				{
					if (oldWidth < Width)
						Width -= 1;
					else if (oldWidth > Width)
						Width += 1;
					else
						return;
				}
			}
		}
		void MakeContainerHeightValid(int oldHeight)
		{
			//TODO: check for eternal loop.
			bool heightIsValid = false;

			while (!heightIsValid)
			{
				heightIsValid = true;

				foreach (DrawBox box in DrawBoxList)
				{
					if (!box.IsContainerHeightValid(GenerateChildBoundaries(box).Height))
					{
						heightIsValid = false;
						break;
					}
				}

				if (!heightIsValid)
				{
					if (oldHeight < Height)
						Height -= 1;
					else if (oldHeight > Height)
						Height += 1;
					else
						return;
				}
			}
		}
		internal override bool IsValidWidth(int newWidth)
		{
			foreach (DrawBox box in DrawBoxList)
			{
				if (!box.IsContainerWidthValid(GenerateChildBoundaries(box, newWidth, Height).Width))
					return false;
			}

			return base.IsValidWidth(newWidth);
		}
		internal override bool IsValidHeight(int newHeight)
		{
			foreach (DrawBox box in DrawBoxList)
			{
				if (!box.IsContainerHeightValid(GenerateChildBoundaries(box, Width, newHeight).Height))
					return false;
			}

			return base.IsValidHeight(newHeight);
		}

		void SlotBox_SizeChanged(object sender, Point oldSize, Point newSize)
		{
			UpdateChildSizes(oldSize, newSize);
		}
		void UpdateChildSizes(Point oldSize, Point newSize)
		{
			SetChildParameters();

			if (oldSize.X != newSize.X)
				MakeContainerWidthValid(oldSize.X);

			if (oldSize.Y != newSize.Y)
				MakeContainerHeightValid(oldSize.Y);
		}

		public override void UpdateSize()
		{
			base.UpdateSize();

			MakeContainerSizeValid();

			foreach (DrawBox box in DrawBoxList)
				box.UpdateSize();
		}
		public void MakeContainerSizeValid()
		{
			UpdateChildSizes(oldSize, new Point(Width, Height));
		}

		//TODO: better name
		public void SetChildParameters()
		{
			foreach (SlotHandler childHandlers in Slots.Keys)
			{
				if (childHandlers.DrawBox == null)
					continue;

				childHandlers.DrawBox.LocationOrigin = GenerateChildOrigin(childHandlers);
				childHandlers.DrawBox.Boundaries = GenerateSlotBoundaries(childHandlers);
				childHandlers.DrawBox.MasterBoundaries = GetMasterBoundaries();
			}
		}

		#endregion

		public override void Close()
		{
			base.Close();

			ClearDrawBoxes();
		}

		/// <summary>
		/// Adds a new slot to the slotbox, and also returns its handler.
		/// </summary>
		/// <returns></returns>
		protected SlotHandler AddNewSlot()
		{
			Slot slot = new Slot();
			Slots.Add(slot.Handler, slot);

			return slot.Handler;
		}
		/// <summary>
		/// Removes the slot with the provided handler.
		/// </summary>
		/// <param name="handler"></param>
		public void RemoveSlot(SlotHandler handler)
		{
			Slot slot = Slots[handler];

			if (slot.DrawBox != null)
				RemoveDrawBoxFromSlot(slot.DrawBox);

			Slots.Remove(handler);
		}

		/// <summary>
		/// Inserts the drawbox to the slot with the provided handler.
		/// </summary>
		/// <param name="box">The box to insert into the slot.</param>
		/// <param name="handler">The handler of the slot.</param>
		protected void PutDrawBoxInSlot(DrawBox box, SlotHandler handler)
		{
			if (box == this)
			{
				Debug.AddExceptionInClass(this.GetType(), "PutDrawBoxInSlot", "Can't add this to itself.");
				return;
			}

			if (handler.DrawBox != null)
			{
				Debug.AddExceptionInClass(this.GetType(), "PutDrawBoxInSlot", "Can't add point DrawBox to an occupied slot.");
				return;
			}

			if (DrawBoxList.Contains(box))
			{
				Debug.AddExceptionInClass(this.GetType(), "PutDrawBoxInSlot", "Can't add point DrawBox instance twice to the same container");
				return;
			}

			if (box.Container != null)
			{
				Debug.AddExceptionInClass(this.GetType(), "PutDrawBoxInSlot", "Can't add point DrawBox instance that already has point container.");
				return;
			}

			if (!box.IsInitialized)
			{
				Debug.AddExceptionInClass(this.GetType(), "PutDrawBoxInSlot", "Can't add point DrawBox that is not initialized.");
				return;
			}

			if (!Slots.ContainsKey(handler))
			{
				Debug.AddExceptionInClass(this.GetType(), "PutDrawBoxInSlot", "Tried to put drawbox in non-existing slot.");
				return;
			}

			Slots[handler].DrawBox = box;
			drawBoxList.Add(box);

			box.Parent = Parent;
			box.Container = this;
			box.Handler = handler;

			UpdateSize();

			box.ReloadAlignment();
			box.AddedToContainer();
			DrawBoxHasBeenAdded(box, handler);
		}
		/// <summary>
		/// Removes the specified drawbox from its slot.
		/// </summary>
		/// <param name="box"></param>
		protected void RemoveDrawBoxFromSlot(DrawBox box)
		{
			var slotList = (from s in Slots.Values where s.DrawBox == box select s);
			if (slotList.Count() != 0)
			{
				RemoveDrawBoxInSlot(slotList.First().Handler);
			}
			else
				Debug.AddExceptionInClass(this.GetType(), "RemoveDrawBoxFromSlot", "Tried to remove non-existing drawbox.");
		}
		/// <summary>
		/// Removes the drawbox in the slot of the provided handler.
		/// </summary>
		/// <param name="handler"></param>
		protected void RemoveDrawBoxInSlot(SlotHandler handler)
		{
			DrawBox drawBox = Slots[handler].DrawBox;

			if (drawBox.Container != this)
			{
				Debug.AddExceptionInClass(this.GetType(), "RemoveDrawBoxFromSlot", "DrawBox does not have correct container.");
				return;
			}

			if (!drawBox.IsClosed)
				drawBox.Close();

			drawBox.Container = null;
			drawBox.Handler = null;

			Slots[handler].DrawBox = null;
			drawBoxList.Remove(drawBox);

			DrawBoxHasBeenRemoved(drawBox, handler);

			UpdateSize();
		}

		/// <summary>
		/// Is called whenever a new drawbox has been added.
		/// </summary>
		/// <param name="box"></param>
		/// <param name="handler"></param>
		protected virtual void DrawBoxHasBeenAdded(DrawBox box, SlotHandler handler) { }
		/// <summary>
		/// Is called whenever a drawbox has been removed.
		/// </summary>
		/// <param name="box"></param>
		/// <param name="handler"></param>
		protected virtual void DrawBoxHasBeenRemoved(DrawBox box, SlotHandler handler) { }

		public void RequestRemoval(DrawBox box)
		{
			if (!removeQue.Contains(box))
				removeQue.Add(box);
		}

		protected DrawBox GetDrawBoxInSlot(SlotHandler slotId)
		{
			return Slots[slotId].DrawBox;
		}

		protected virtual void HandleFocus()
		{
			if (DrawBoxWithFocus != null && DrawBoxWithFocus.Suspended)
				DrawBoxWithFocus = null;

			if (!HasFocus)
			{
				DrawBoxWithFocus = null;
				return;
			}
			
			DrawBox oldDrawBoxWithFocus = DrawBoxWithFocus;

			DrawBoxWithFocus = null;

			foreach (DrawBox d in DrawBoxList)
			{
				if (DialoguesAreHidden && dialogues.Contains(d))
					continue;

				if (d.CheckFocus())
				{
					if (DrawBoxWithFocus == null)
						DrawBoxWithFocus = d;
					else if (DrawBoxList.IndexOf(d) > DrawBoxList.IndexOf(DrawBoxWithFocus))
						DrawBoxWithFocus = d;
				}
			}
			
			if (DrawBoxWithFocus == null)
				DrawBoxWithFocus = oldDrawBoxWithFocus;
		}
		protected virtual void HandleMouseOver()
		{
			if (!IsUnderMouse)
			{
				DrawBoxUnderMouse = null;
				return;
			}

			DrawBoxUnderMouse = null;

			foreach (DrawBox child in DrawBoxList)
			{
				if (DialoguesAreHidden && dialogues.Contains(child))
					continue;

				if (child.CheckMouseOver())
				{
					if (DrawBoxUnderMouse == null)
						DrawBoxUnderMouse = child;
					else if (DrawBoxList.IndexOf(child) > DrawBoxList.IndexOf(DrawBoxUnderMouse))
						DrawBoxUnderMouse = child;
				}
			}
		}

		protected void ClearDrawBoxes()
		{
			foreach (DrawBox d in new List<DrawBox>(DrawBoxList))
				RemoveDrawBoxFromSlot(d);
		}

		public override void Update(GameTime gameTime)
		{
			SetChildParameters();

			base.Update(gameTime);

			if (IsClosed || !IsInitialized || !Activated)
				return;

			HandleFocus();
			HandleMouseOver();
			while (dialogues.Count != 0 && dialogues.Last().IsClosed)
				dialogues.RemoveAt(dialogues.Count - 1);

			if (!DialoguesAreHidden && dialogues.Count != 0)
			{
				var dialog = dialogues.Last();
				if (dialog != DrawBoxWithFocus)
					DrawBoxWithFocus = null;
				if (dialog != DrawBoxUnderMouse)
					DrawBoxUnderMouse = null;
			}

			if (removeQue.Count != 0)
			{
				foreach (DrawBox d in removeQue)
					RemoveDrawBoxFromSlot(d);

				removeQue.Clear();
			}

			foreach (DrawBox d in new List<DrawBox>(drawBoxList))
			{
				if (DialoguesAreHidden && dialogues.Contains(d))
					continue;

				d.Update(gameTime);
			}
		}

		public override void Draw(GameTime gameTime, ViewRect viewRect)
		{
			base.Draw(gameTime, viewRect);

			if (!IsInitialized || !Activated || Hidden)
				return;

			viewRect.Add(Boundaries);
			viewRect.Add(GetMasterBoundaries());

			if (!DialoguesAreHidden && DarkenWhenDialogueExists && GetCurrentDialogue() != null)
			{
				DrawBox currentDialogue = GetCurrentDialogue();

				foreach (DrawBox d in DrawBoxList)
					if (currentDialogue != d)
						d.Draw(gameTime, viewRect);

				var render = GraphicsManager.GetRender();
				render.Begin();
				render.Clear(DarkeningMask);
				render.End();

				currentDialogue.Draw(gameTime, viewRect);
			}
			else
			{
				foreach (DrawBox d in DrawBoxList)
				{
					if (DialoguesAreHidden && dialogues.Contains(d))
						continue;

					d.Draw(gameTime, viewRect);
				}
			}
		}

		internal class Slot
		{
			public readonly SlotHandler Handler;
			public DrawBox DrawBox;

			public Slot()
			{
				Handler = new SlotHandler(this);
			}
		}

		public class SlotHandler
		{
			Slot slot;

			public DrawBox DrawBox
			{
				get { return slot.DrawBox; }
			}

			internal SlotHandler(Slot _slot)
			{
				slot = _slot;
			}
		}
	}
}
 