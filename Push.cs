using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Services;

namespace TakaGUI
{
	public static class Push
	{
		//Services
		public static IDebug Debug;

		public static void InitializeServices(IDebug debug)
		{
			Debug = debug;
		}

		public static void ToHorizontalCenter(DrawBox box)
		{
			box.X = (box.Boundaries.Width / 2) - box.Width / 2;
		}
		public static void ToVerticalCenter(DrawBox box)
		{
			box.Y = (box.Boundaries.Height / 2) - box.Height / 2;
		}

		public enum Side { Top, Bottom, Left, Right}
		public static void FromSide(DrawBox box, int distance, Side edge)
		{
			ViewRect bounds = box.Boundaries;

			switch (edge)
			{
				case Side.Top:
					box.Y = distance;
					break;
				case Side.Bottom:
					box.Y = bounds.Height - box.Height - distance;
					break;
				case Side.Left:
					box.X = distance;
					break;
				case Side.Right:
					box.X = bounds.Width - box.Width - distance;
					break;
			}
		}

		public static int Up(int yBorder, int margin, params DrawBox[] drawBoxes)
		{
			if (drawBoxes.Length == 0)
				Debug.AddExceptionInClass(typeof(Push), "Up", "Can't push 0 drawboxes.");

			int largestHeight = (from box in drawBoxes select box.Height).Max();

			foreach (DrawBox box in drawBoxes)
				box.Y = yBorder - (int)Math.Round((float)largestHeight / 2, 0) - (int)Math.Round((float)box.Height / 2, 0) - margin;

			return yBorder - largestHeight - margin;
		}
		public static int Down(int yBorder, int margin, params DrawBox[] drawBoxes)
		{
			if (drawBoxes.Length == 0)
				Debug.AddExceptionInClass(typeof(Push), "Below", "Can't push 0 drawboxes.");

			int largestHeight = (from box in drawBoxes select box.Height).Max();

			foreach (DrawBox box in drawBoxes)
				box.Y = yBorder + (int)Math.Round((float)largestHeight / 2, 0) - (int)Math.Round((float)box.Height / 2, 0) + margin;

			return yBorder + largestHeight + margin;
		}
		public static int Left(int xBorder, int margin, params DrawBox[] drawBoxes)
		{
			if (drawBoxes.Length == 0)
				Debug.AddExceptionInClass(typeof(Push), "Left", "Can't push 0 drawboxes.");

			int largestWidth = (from box in drawBoxes select box.Width).Max();

			foreach (DrawBox box in drawBoxes)
				box.X = xBorder - (int)Math.Round((float)largestWidth / 2, 0) - (int)Math.Round((float)box.Width / 2, 0) - margin;

			return xBorder - largestWidth - margin;
		}
		public static int Right(int xBorder, int margin, params DrawBox[] drawBoxes)
		{
			if (drawBoxes.Length == 0)
				Debug.AddExceptionInClass(typeof(Push), "Right", "Can't push 0 drawboxes.");

			int largestWidth = (from box in drawBoxes select box.Width).Max();

			foreach (DrawBox box in drawBoxes)
				box.X = xBorder + (int)Math.Round((float)largestWidth / 2, 0) - (int)Math.Round((float)box.Width / 2, 0) + margin;

			return xBorder + largestWidth;
		}

		public static void ToTheTopSideOf(DrawBox box, DrawBox alignWith, int margin, VerticalAlign align)
		{
			VerticallyAlignWith(box, alignWith, align);

			box.Y = alignWith.Y - box.Height - margin;
		}
		public static void ToTheBottomSideOf(DrawBox box, DrawBox alignWith, int margin, VerticalAlign align)
		{
			VerticallyAlignWith(box, alignWith, align);

			box.Y = alignWith.Y + alignWith.Height + margin;
		}

		public static void ToTheLeftSideOf(DrawBox box, DrawBox alignWith, int margin, HorizontalAlign align)
		{
			HorizontallyAlignWith(box, alignWith, align);

			box.X = alignWith.X - box.Width - margin;
		}
		public static void ToTheRightSideOf(DrawBox box, DrawBox alignWith, int margin, HorizontalAlign align)
		{
			HorizontallyAlignWith(box, alignWith, align);

			box.X = alignWith.X + alignWith.Width + margin;
		}

		public enum HorizontalAlign { Top, Center, Bottom }
		public static void HorizontallyAlignWith(DrawBox box, DrawBox alignWith, HorizontalAlign align)
		{
			switch (align)
			{
				case HorizontalAlign.Top:
					box.Y = alignWith.Y;
					break;
				case HorizontalAlign.Center:
					box.Y = alignWith.Y + (alignWith.Height / 2) - (box.Height / 2);
					break;
				case HorizontalAlign.Bottom:
					box.Y = alignWith.Y + alignWith.Height - box.Height;
					break;
			}
		}

		public enum VerticalAlign { Left, Center, Right }
		public static void VerticallyAlignWith(DrawBox box, DrawBox alignWith, VerticalAlign align)
		{
			switch (align)
			{
				case VerticalAlign.Left:
					box.X = alignWith.X;
					break;
				case VerticalAlign.Center:
					box.X = alignWith.X + (alignWith.Width / 2) - (box.Width / 2);
					break;
				case VerticalAlign.Right:
					box.X = alignWith.X + alignWith.Width - box.Width;
					break;
			}
		}

		public static int GetTopSide(params DrawBox[] drawBoxes)
		{
			return (from box in drawBoxes select box.Y).Min();
		}
		public static int GetBottomSide(params DrawBox[] drawBoxes)
		{
			return (from box in drawBoxes select box.Y + box.Height).Max();
		}
		public static int GetLeftSide(params DrawBox[] drawBoxes)
		{
			return (from box in drawBoxes select box.X).Min();
		}
		public static int GetRightSide(params DrawBox[] drawBoxes)
		{
			return (from box in drawBoxes select box.X + box.Width).Max();
		}
	}
}
