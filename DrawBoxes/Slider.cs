using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TakaGUI.Data;
using System.IO;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class Slider : DrawBox
	{
		public event DoubleChangedEvent SliderValueChanged;
		public event DefaultEvent SlideStarted;
		public event DefaultEvent SlideEnded;

		#region Textures
		public static string DefaultCategory = "Slider";

		public ISprite Milestone;
		public ISprite Pointer;
		public ISprite RailEdge;
		public ISprite RailMiddle;

		public int PointerMilestoneMargin = 3;

		#endregion

		double _MaxValue = 1;
		public double MaxValue
		{
			get { return _MaxValue; }
			set
			{
				_MaxValue = value;

				if (_MaxValue < 1)
					_MaxValue = 1;
			}
		}

		double _StepValue;
		public double StepValue
		{
			get { return _StepValue; }
			set
			{
				_StepValue = value;

				if (_StepValue < 0)
					_StepValue = 0;
				if (_StepValue > MaxValue)
					_StepValue = MaxValue;
			}
		}

		double _Value;
		public double Value
		{
			get { return _Value; }
			set
			{
				double oldValue = _Value;
				_Value = value;

				if (_Value < 0)
					_Value = 0;
				else if (_Value > MaxValue)
					_Value = _MaxValue;

				if (SliderValueChanged != null)
					SliderValueChanged(this, oldValue, _Value);
			}
		}

		int PointerX
		{
			get
			{
				return RealX + (int)Math.Round((Width - RailEdge.Width * 2 - Pointer.Width) * (Value / MaxValue), 0) + RailEdge.Width;
			}
			set
			{
				Value = MaxValue * (((double)value - RealX - RailEdge.Width) / (Width - RailEdge.Width * 2 - Pointer.Width));
			}
		}

		public override int MinWidth
		{
			get { return Pointer.Width; }
		}
		public override int MinHeight { get { return Pointer.Height + PointerMilestoneMargin + Milestone.Height; } }
		public override int MaxHeight { get { return MinHeight; } }

		public override int Height
		{
			get { return MinHeight; }
		}

		int mouseOrigin;
		bool isMovingPointer = false;

		public virtual void Initialize(string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			Milestone = GetTexture(file, category, "Milestone");
			Pointer = GetTexture(file, category, "Pointer");
			RailEdge = GetTexture(file, category, "RailEdge");
			RailMiddle = GetTexture(file, category, "RailMiddle");

			base.BaseInitialize();
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			#region Move Scroller
			if (HasFocus && IsUnderMouse &&
				MouseInput.X > PointerX &&
				MouseInput.X < PointerX + Pointer.Width &&
				MouseInput.Y > RealY &&
				MouseInput.Y < RealY + Pointer.Height &
				MouseInput.IsClicked(MouseButtons.Left))
			{
				isMovingPointer = true;
				mouseOrigin = MouseInput.X;

				if (SlideStarted != null)
					SlideStarted(this);
			}

			if (isMovingPointer)
			{
				if (MouseInput.IsPressed(MouseButtons.Left) == false)
				{
					isMovingPointer = false;

					if (SlideEnded != null)
						SlideEnded(this);
				}
				else
				{
					PointerX += MouseInput.X - mouseOrigin;

					if (!(Value <= 0) && !(Value >= MaxValue))
						mouseOrigin = MouseInput.X;
				}
			}

			#endregion
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			int railAddX = Pointer.Width / 2;
			int railY = y + (Pointer.Height / 2) - (RailEdge.Height / 2);

			render.DrawSprite(RailEdge, new Vector2(x + railAddX, railY), Color.White);
			render.DrawSprite(RailEdge, new Vector2(x + railAddX + Width - RailEdge.Width, railY), Color.White);

			render.DrawSprite(RailMiddle, new Rectangle(x + railAddX + RailEdge.Width, railY, Width - RailEdge.Width * 2 - railAddX * 2, RailMiddle.Height), Color.White);

			render.DrawSprite(Pointer, new Vector2(PointerX, y), Color.White);

			//Dras milestones
			double stepPixelWidth = Math.Max((Width - RailEdge.Width * 2 - railAddX) * (StepValue / MaxValue), 1F);
			double stepX = x + railAddX;
			while (stepX < (x + Width - RailEdge.Width * 2 - railAddX))
			{
				render.DrawSprite(Milestone, new Vector2((int)Math.Round(stepX, 0), y + Pointer.Height + PointerMilestoneMargin), Color.White);
				stepX += stepPixelWidth;
			}

			render.End();
		}
	}
}
