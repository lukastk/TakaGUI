using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;
using System.IO;
using Microsoft.Xna.Framework;
using TakaGUI.Cores;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes.Forms
{
	/// <summary>
	/// Summary:
	///		Basically contains
	/// </summary>
	public class Form : SingleSlotBox
	{
		#region Textures
		public static string DefaultCategory = "Form";

		bool _Header;
		public bool Header
		{
			get { return _Header; }
			set
			{
				_Header = value;

				if (_Header)
				{
					topLeftCornerInUse = LeftHeader;
					topRightCornerInUse = RightHeader;
					topInUse = TopHeader;
				}
				else
				{
					topLeftCornerInUse = TopLeftCorner;
					topRightCornerInUse = TopRightCorner;
					topInUse = TopBorder;
				}
			}
		}

		public ISprite TopLeftCorner;
		public ISprite TopRightCorner;
		public ISprite BottomLeftCorner;
		public ISprite BottomRightCorner;

		public ISprite TopBorder;
		public ISprite BottomBorder;
		public ISprite LeftBorder;
		public ISprite RightBorder;

		public ISprite LeftHeader;
		public ISprite RightHeader;
		public ISprite TopHeader;

		public ISprite Inside;

		protected ISprite topLeftCornerInUse;
		protected ISprite topRightCornerInUse;
		protected ISprite topInUse;

		public ISprite CloseButton;
		int buttonMargin = 5;
		List<ISprite> buttons = new List<ISprite>();

		#endregion

		#region Data Fields
		public string Title = "";
		string titleToPrint = "";
		public MonoFont Font;
		public Color FontColor = Color.White;

		public override int MinWidth
		{
			get
			{
				if (topLeftCornerInUse != null && topRightCornerInUse != null)
				{
					int addWidth = 0;

					if (Header)
					{
						foreach (ISprite button in buttons)
							addWidth += button.Width + buttonMargin;

						if (buttons.Count != 0)
							addWidth -= buttonMargin;

						addWidth += Font.MeasureString("...").X;
					}

					return topLeftCornerInUse.Width + topRightCornerInUse.Width + addWidth;
				}
				else
					return -1;
			}
		}
		public override int MinHeight
		{
			get
			{
				if (topLeftCornerInUse != null && BottomRightCorner != null)
					return topLeftCornerInUse.Height + BottomRightCorner.Height;
				else
					return -1;
			}
		}

		#endregion

		#region Control Fields
		public bool CanMoveForm = true;
		bool formIsMoving = false;

		public bool CanResizeFormHorizontally = true;
		public bool CanResizeFormVertically = true;
		bool formIsResizing = false;
		Point formResizeOrigin;

		public bool DisposeWhenCloseButtonIsPressed = true;
		public bool DeactivateCloseButtonIsPressed = true;

		bool _CloseButtonOn = true;
		public bool CloseButtonOn
		{
			get { return _CloseButtonOn; }
			set
			{
				_CloseButtonOn = value;

				if (_CloseButtonOn && !buttons.Contains(CloseButton))
				{
					buttons.Add(CloseButton);
				}
				else if (!_CloseButtonOn && buttons.Contains(CloseButton))
				{
					buttons.Remove(CloseButton);
				}
			}
		}

		#endregion

		#region Container Fields
		public Point ContainerLeftTopMargin = new Point(0, 0);
		public Point ContainerBottomRightMargin = new Point(0, 0);

		#endregion

		#region Container Methods
		public override ViewRect GetDefaultBoundaries(int newWidth, int newHeight)
		{
			ViewRect bounds = new ViewRect();
			bounds.X = RealX + topLeftCornerInUse.Width;
			bounds.Y = RealY + topLeftCornerInUse.Height;
			bounds.Width = newWidth - topLeftCornerInUse.Width - topRightCornerInUse.Width;
			bounds.Height = newHeight - topLeftCornerInUse.Height - BottomRightCorner.Height;

			return bounds;
		}

		#endregion

		#region Events Methods
		void Form_FocusChanged(object sender, bool focus)
		{
			if (focus && !Suspended)
				Container.PutChildInFront(this);
		}

		#endregion

		#region Control Methods
		void moveForm()
		{
			if (HasFocus && (IsUnderMouse || formIsMoving))
			{
				if (!MouseInput.IsPressed(MouseButtons.Left))
				{
					formIsMoving = false;

					if (FormMoveEnd != null)
						FormMoveEnd(this);
				}

				if (CanMoveForm && !formIsMoving)
				{
					if (MouseInput.IsClicked(MouseButtons.Left) &&
						MouseInput.X >= RealX &&
						MouseInput.Y >= RealY &&
						MouseInput.X <= RealX + Width &&
						MouseInput.Y <= RealY + topLeftCornerInUse.Height)
					{
						formIsMoving = true;

						if (FormMoveStart != null)
							FormMoveStart(this);
					}
				}
				else if (formIsMoving) //Has to be an "else if" conditional in case MouseInput.DeltaX or Y isnt 0.
				{
					X += MouseInput.DeltaX;
					Y += MouseInput.DeltaY;
				}
			}

			if (formIsMoving && FormIsMoving != null)
				FormIsMoving(this);
		}

		void resizeForm()
		{
			if (IsUnderMouse || formIsResizing)
			{
				if (!MouseInput.IsPressed(MouseButtons.Left))
				{
					formIsResizing = false;

					if (FormResizeEnd != null)
						FormResizeEnd(this);
				}

				if ((CanResizeFormHorizontally || CanResizeFormVertically) && !formIsResizing)
				{
					if (MouseInput.X >= RealX + Width - BottomRightCorner.Width &&
						MouseInput.Y >= RealY + Height - BottomRightCorner.Height &&
						MouseInput.X <= RealX + Width &&
						MouseInput.Y <= RealY + Height)
					{
						CursorManager.CursorToUse = Cursors.ResizeCursor;

						if (MouseInput.IsClicked(MouseButtons.Left))
						{
							formIsResizing = true;
							formResizeOrigin = new Point(MouseInput.X, MouseInput.Y);

							if (FormResizeStart != null)
								FormResizeStart(this);
						}
					}
				}
				else if (formIsResizing) //Has to be an "else if" conditional in case MouseInput.DeltaX or Y isnt 0.
				{
					if (CanResizeFormHorizontally)
						Width += MouseInput.X - formResizeOrigin.X;

					if (CanResizeFormVertically)
						Height += MouseInput.Y - formResizeOrigin.Y;

					formResizeOrigin = new Point(MouseInput.X, MouseInput.Y);

					if (CanResizeFormHorizontally && Width == MinWidth)
						formResizeOrigin.X = RealX + Width;

					if (CanResizeFormVertically && Height == MinHeight)
						formResizeOrigin.Y = RealY + Height;
				}
			}

			if (formIsResizing)
			{
				if (FormIsResizing != null)
					FormIsResizing(this);

				CursorManager.CursorToUse = Cursors.ResizeCursor;
			}
		}

		public override void WrapWidth()
		{
			base.WrapWidth();

			Width += topLeftCornerInUse.Width + topRightCornerInUse.Width;
		}
		public override void WrapHeight()
		{
			base.WrapHeight();

			Height += topLeftCornerInUse.Height + BottomRightCorner.Height;
		}

		#endregion

		#region Events
		public event DefaultEvent FormMoveStart;
		public event DefaultEvent FormIsMoving;
		public event DefaultEvent FormMoveEnd;

		public event DefaultEvent FormResizeStart;
		public event DefaultEvent FormIsResizing;
		public event DefaultEvent FormResizeEnd;

		public event DefaultEvent CloseButtonClicked;

		#endregion

		public Form()
		{
			FocusChanged += new BooleanChangedEvent(Form_FocusChanged);
		}

		public override void AddedToContainer()
		{
			if (!Container.GetType().IsEquivalentTo(typeof(Window)) && !Container.GetType().IsSubclassOf(typeof(Window)))
				Debug.AddExceptionInClass(this.GetType(), "AddedToContainer", "Can't add point container to point container with point type that's not derived from Window");
		}

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

			LeftHeader = GetTexture(file, category, "LeftHeader");
			RightHeader = GetTexture(file, category, "RightHeader");
			TopHeader = GetTexture(file, category, "TopHeader");

			Inside = GetTexture(file, category, "Inside");

			Header = true;

			CloseButton = GetTexture(file, category, "CloseButton");
			CloseButtonOn = true;

			Font = GetMonoFont(file, category, "Font");

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			moveForm();
			resizeForm();

			if (Header)
			{
				UpdateButtons();

				int buttonSize = 0;
				foreach (ISprite button in buttons)
					buttonSize += button.Width + buttonMargin;
				if (buttons.Count != 0)
					buttonSize -= buttonMargin;

				Point titleSize = Font.MeasureString(Title);
				int minTitleSize = Width - topLeftCornerInUse.Width - buttonSize - topRightCornerInUse.Width;
				if (titleSize.X > minTitleSize)
				{
					titleToPrint = "";
					foreach (char c in Title)
					{
						if (Font.MeasureString(titleToPrint + c + "...").X > minTitleSize)
						{
							titleToPrint += "...";
							break;
						}
						else
							titleToPrint += c;
					}
				}
				else
					titleToPrint = Title;
			}
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();
			
			render.DrawBody(new Rectangle(x, y, Width, Height), Color.White,
				topInUse,
				topRightCornerInUse,
				RightBorder,
				BottomRightCorner,
				BottomBorder,
				BottomLeftCorner,
				LeftBorder,
				topLeftCornerInUse,
				Inside);

			if (Header)
			{
				Font.DrawString(titleToPrint, new Point(x + topLeftCornerInUse.Width, y + topLeftCornerInUse.Height / 2), new Vector2(0F, 0.5F), FontColor, render);
				DrawButtons(render, x, y);
			}

			render.End();
		}

		protected void UpdateButtons()
		{
			if (buttons.Count == 0 || !IsUnderMouse || !MouseInput.IsClicked(MouseButtons.Left))
				return;

			int buttonX = RealX + Width - topRightCornerInUse.Width;
			int buttonClicked = -1;

			foreach (ISprite button in buttons)
			{
				buttonX -= button.Width + buttonMargin;
			}

			buttonX += buttonMargin;

			foreach (ISprite button in buttons)
			{
				if (IsMouseInRect(
					new Rectangle(buttonX, RealY + (topInUse.Height / 2) - (button.Height / 2),button.Width, button.Height)))
				{
					buttonClicked = buttons.IndexOf(button);
					break;
				}
			}

			if (buttonClicked == buttons.IndexOf(CloseButton))
			{
				if (CloseButtonClicked != null)
					CloseButtonClicked(this);

				if (DisposeWhenCloseButtonIsPressed)
					Close();
				else if (DeactivateCloseButtonIsPressed)
					Activated = false;
			}
		}
		protected void DrawButtons(IRender render, int x, int y)
		{
			if (buttons.Count == 0)
				return;

			int startX = x + Width - topRightCornerInUse.Width;

			foreach (ISprite button in buttons)
			{
				startX -= button.Width + buttonMargin;
			}

			startX += buttonMargin;

			foreach (ISprite button in buttons)
			{
				render.DrawSprite(button, new Vector2(startX, y + (topLeftCornerInUse.Height / 2) - (button.Height / 2)), Color.White);
				startX += button.Width + buttonMargin;
			}
		}
	}

	public enum DialogResult
	{
		NotFinished, None, OK, Cancel, Abort, Retry, Ignore, Yes, No
	}
}