using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public abstract class Scrollbar : DrawBox
	{
		public delegate void ScrollEventHandler(object sender, double scroll);

		#region Events
		public event ScrollEventHandler Scroll;
		private double oldValue;

		#endregion

		double maxValue = 1;
		public double MaxValue
		{
			get { return maxValue; }
			set
			{
				maxValue = value;

				if (maxValue < 0)
					maxValue = 0;
				else if (maxValue > (ScrollArea - minScrollerSize))
					maxValue = ScrollArea - minScrollerSize;
			}
		}

		double _value;
		public double Value
		{
			get { return _value; }
			set
			{
				_value = value;

				if (_value < 0)
					_value = 0;
				else if (_value > MaxValue)
					_value = MaxValue;
			}
		}

		double step = 0.1;
		public double Step
		{
			get { return step; }
			set { step = value; }
		}

		public abstract int ScrollArea { get; }

		protected const int minScrollerSize = 10;
		protected int ScrollerSize
		{
			get { return ScrollArea - MaxScrollerPosition; }
		}

		protected int MaxScrollerPosition
		{
			get { return ScrollArea - minScrollerSize; }
		}
		public double ScrollerPosition
		{
			get { return MaxScrollerPosition * (double)(Value / MaxValue); }
		}

		protected bool leftButtonIsPressed;
		protected bool rightButtonIsPressed;
		protected bool rapid;
		protected float rapidStart;
		protected const float timeBeforeRapid = 0.5f;
		public float TimeBetweenRapid = 0.02f;

		#region Textures
		public ISprite BarButton;
		public ISprite BarButtonPressed;
		public ISprite BarInside;
		public ISprite ScrollerEdge;
		public ISprite ScrollerInside;
		public ISprite ScrollerCenter;

		#endregion

		protected virtual void Initialize(string category, ISkinFile file)
		{
			BarButton = GetTexture(file, category, "BarButton");
			BarButtonPressed = GetTexture(file, category, "BarButtonPressed");
			BarInside = GetTexture(file, category, "BarInside");
			ScrollerEdge = GetTexture(file, category, "ScrollerEdge");
			ScrollerInside = GetTexture(file, category, "ScrollerInside");
			ScrollerCenter = GetTexture(file, category, "ScrollerCenter");

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (oldValue != Value)
			{
				if (Scroll != null)
					Scroll(this, Value - oldValue);
			}

			oldValue = Value;
		}
	}

	public class VScrollbar : Scrollbar
	{
		public static string DefaultCategory = "VScrollbar";

		public override int ScrollArea
		{
			get
			{
				return Height - BarButton.Width * 2;
			}
		}

		public override int MinWidth { get { return BarButton.Width; } }
		public override int MaxWidth { get { return BarButton.Width; } }
		public override int MinHeight { get { return BarButton.Height * 2 + minScrollerSize; } }

		public override int Width
		{
			get { return MinWidth; }
		}

		int mouseOrigin;
		bool isMovingScroller = false;

		public new void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			base.Initialize(category, file);
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			#region Move Scroller
			if (IsUnderMouse &&
				MouseInput.X > RealX &&
				MouseInput.X < RealX + ScrollerEdge.Width &&
				MouseInput.Y > RealY + BarButton.Height + ScrollerPosition &&
				MouseInput.Y < RealY + BarButton.Height + ScrollerPosition + ScrollerSize &
				MouseInput.IsClicked(MouseButtons.Left))
			{
				isMovingScroller = true;
				mouseOrigin = MouseInput.Y;
			}

			if (isMovingScroller)
			{
				if (MouseInput.IsPressed(MouseButtons.Left) == false)
				{
					isMovingScroller = false;
				}
				else
				{
					if (MaxScrollerPosition != 0)
						Value += MaxValue * ((MouseInput.Y - mouseOrigin) / (double)MaxScrollerPosition);

					if (!(ScrollerPosition <= 0) && !(ScrollerPosition >= MaxScrollerPosition))
						mouseOrigin = MouseInput.Y;
				}
			}

			#endregion

			#region Button Clicked and Checked
			if (!MouseInput.IsPressed(MouseButtons.Left))
			{
				leftButtonIsPressed = false;
				rightButtonIsPressed = false;
				rapid = false;
			}

			//Top
			if (IsUnderMouse &&
				MouseInput.X > RealX &&
				MouseInput.X < RealX + BarButton.Width &&
				MouseInput.Y > RealY &&
				MouseInput.Y < RealY + BarButton.Height &&
				MouseInput.IsClicked(MouseButtons.Left))
			{
				Value -= Step;
				leftButtonIsPressed = true;
				rapidStart = (float)gameTime.TotalGameTime.TotalSeconds;
			}

			//Bottom
			if (IsUnderMouse &&
				MouseInput.X > RealX &&
				MouseInput.X < RealX + BarButton.Width &&
				MouseInput.Y > RealY + Height - BarButton.Height &&
				MouseInput.Y < RealY + Height &&
				MouseInput.IsClicked(MouseButtons.Left))
			{
				Value += Step;
				rightButtonIsPressed = true;
				rapidStart = (float)gameTime.TotalGameTime.TotalSeconds;
			}

			if (leftButtonIsPressed || rightButtonIsPressed)
			{
				double change = 0;
				if (leftButtonIsPressed)
					change = -Step;
				else if (rightButtonIsPressed)
					change = Step;

				if (gameTime.TotalGameTime.TotalSeconds > rapidStart + timeBeforeRapid)
					rapid = true;

				if (rapid)
				{
					if (gameTime.TotalGameTime.TotalSeconds > rapidStart + TimeBetweenRapid)
					{
						Value += change;
						rapidStart = (float)gameTime.TotalGameTime.TotalSeconds;
					}
				}

			}

			#endregion
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			ISprite barButtonTop = leftButtonIsPressed ? BarButtonPressed : BarButton;
			ISprite barButtonBottom = rightButtonIsPressed ? BarButtonPressed : BarButton;

			render.DrawSprite(barButtonTop, new Vector2(x, y), Color.White);
			render.DrawSprite(BarInside,
				new Rectangle(x, y + barButtonTop.Height, Width, Height - barButtonTop.Height * 2), Color.White);
			render.DrawSprite(barButtonBottom, new Vector2(x, y + Height - barButtonBottom.Height), SpriteEffects.FlipVertically, Color.White);
			

			int scrollerY = (int)Math.Round(y + BarButton.Height + ScrollerPosition, 0);
			render.DrawSprite(ScrollerEdge, new Vector2(x, scrollerY), Color.White);
			render.DrawSprite(ScrollerInside,
				new Rectangle(x, scrollerY + ScrollerEdge.Height, Width, ScrollerSize - ScrollerEdge.Height * 2), Color.White);
			render.DrawSprite(ScrollerEdge, new Vector2(x, scrollerY + ScrollerSize - ScrollerEdge.Height), SpriteEffects.FlipVertically, Color.White);
			render.DrawSprite(ScrollerCenter, new Vector2(x, scrollerY + ScrollerSize / 2 - ScrollerCenter.Height / 2), Color.White);
			
			render.End();
		}
	}

	public class HScrollbar : Scrollbar
	{
		public static string DefaultCategory = "HScrollbar";

		public override int ScrollArea
		{
			get
			{
				return Width - BarButton.Width * 2;
			}
		}

		public override int MinWidth
		{
			get { return BarButton.Width * 2 + MaxScrollerPosition + minScrollerSize; }
		}
		public override int MinHeight { get { return BarButton.Height; } }
		public override int MaxHeight { get { return BarButton.Height; } }

		public override int Height
		{
			get { return MinHeight; }
		}

		int mouseOrigin;
		bool isMovingScroller = false;

		public new void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			base.Initialize(category, file);
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			#region Move Scroller
			if (IsUnderMouse &&
				MouseInput.X > RealX + BarButton.Width + ScrollerPosition &&
				MouseInput.X < RealX + BarButton.Width + ScrollerPosition + ScrollerSize &&
				MouseInput.Y > RealY &&
				MouseInput.Y < RealY + ScrollerEdge.Height &
				MouseInput.IsClicked(MouseButtons.Left))
			{
				isMovingScroller = true;
				mouseOrigin = MouseInput.X;
			}

			if (isMovingScroller)
			{
				if (MouseInput.IsPressed(MouseButtons.Left) == false)
				{
					isMovingScroller = false;
				}
				else
				{
					if (MaxScrollerPosition != 0)
						Value += MaxValue * ((MouseInput.X - mouseOrigin) / (double)MaxScrollerPosition);

					if (!(ScrollerPosition <= 0) && !(ScrollerPosition >= MaxValue))
						mouseOrigin = MouseInput.X;
				}
			}

			#endregion

			#region Button Clicked and Checked
			if (!MouseInput.IsPressed(MouseButtons.Left))
			{
				leftButtonIsPressed = false;
				rightButtonIsPressed = false;
				rapid = false;
			}

			//Left
			if (IsUnderMouse &&
				MouseInput.X > RealX &&
				MouseInput.X < RealX + BarButton.Width &&
				MouseInput.Y > RealY &&
				MouseInput.Y < RealY + BarButton.Height &&
				MouseInput.IsClicked(MouseButtons.Left))
			{
				Value -= Step;
				leftButtonIsPressed = true;
				rapidStart = (float)gameTime.TotalGameTime.TotalSeconds;
			}

			//Right
			if (IsUnderMouse &&
				MouseInput.X > RealX + Width - BarButton.Width &&
				MouseInput.X < RealX + Width &&
				MouseInput.Y > RealY &&
				MouseInput.Y < RealY + BarButton.Height &&
				MouseInput.IsClicked(MouseButtons.Left))
			{
				Value += Step;
				rightButtonIsPressed = true;
				rapidStart = (float)gameTime.TotalGameTime.TotalSeconds;
			}

			if (leftButtonIsPressed || rightButtonIsPressed)
			{
				double change = 0;
				if (leftButtonIsPressed)
					change = -Step;
				else if (rightButtonIsPressed)
					change = Step;

				if (gameTime.TotalGameTime.TotalSeconds > rapidStart + timeBeforeRapid)
					rapid = true;

				if (rapid)
				{
					if (gameTime.TotalGameTime.TotalSeconds > rapidStart + TimeBetweenRapid)
					{
						Value += change;
						rapidStart = (float)gameTime.TotalGameTime.TotalSeconds;
					}
				}

			}

			#endregion
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			ISprite barButtonLeft = leftButtonIsPressed ? BarButtonPressed : BarButton;
			ISprite barButtonRight = rightButtonIsPressed ? BarButtonPressed : BarButton;

			render.DrawSprite(barButtonLeft, new Vector2(x, y), Color.White);
			render.DrawSprite(BarInside,
				new Rectangle(x + BarButton.Width, y, Width - BarButton.Width * 2, BarInside.Height), Color.White);
			render.DrawSprite(barButtonRight, new Vector2(x + Width - BarButton.Width, y), SpriteEffects.FlipHorizontally, Color.White);

			int scrollerX = (int)Math.Round(x + BarButton.Width + ScrollerPosition, 0);
			render.DrawSprite(ScrollerEdge, new Vector2(scrollerX, y), Color.White);
			render.DrawSprite(ScrollerInside,
				new Rectangle(scrollerX + ScrollerEdge.Width, y, ScrollerSize - ScrollerEdge.Width * 2, ScrollerInside.Height), Color.White);
			render.DrawSprite(ScrollerEdge, new Vector2(scrollerX + ScrollerSize - ScrollerEdge.Width, y), SpriteEffects.FlipHorizontally, Color.White);
			render.DrawSprite(ScrollerCenter, new Vector2(scrollerX + ScrollerSize / 2 - ScrollerCenter.Width / 2, y), Color.White);

			render.End();
		}
	}
}
