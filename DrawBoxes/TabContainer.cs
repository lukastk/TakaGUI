using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	//TODO: change PAGE to TAB
	public class TabContainer : SingleSlotBox
	{
		public delegate void TabChangedEvent(object sender, TabPage oldTab, TabPage newTab);
		public event TabChangedEvent TabChanged;

		#region Resources
		public static string DefaultCategory = "TabContainer";

		public ISprite TopLeftCorner;
		public ISprite TopRightCorner;
		public ISprite BottomLeftCorner;
		public ISprite BottomRightCorner;

		public ISprite TopBorder;
		public ISprite RightBorder;
		public ISprite BottomBorder;
		public ISprite LeftBorder;

		//Button
		public ISprite ButtonTopLeftCorner;
		public ISprite ButtonTopRightCorner;
		public ISprite ButtonBottomLeftCorner;
		public ISprite ButtonBottomRightCorner;

		public ISprite ButtonTopBorder;
		public ISprite ButtonRightBorder;
		public ISprite ButtonBottomBorder;
		public ISprite ButtonLeftBorder;

		public ISprite ButtonInside;

		public ISprite ButtonInsidePressed;

		public MonoFont Font;
		public Color FontColor;

		#endregion

		List<TabPage> pages = new List<TabPage>();
		List<float> sizePercentages = new List<float>();
		public ReadOnlyCollection<TabPage> Pages
		{
			get;
			private set;
		}
		const int pageNameMargin = 1;
		int pressedPage = -1;

		int minWidth;
		public override int MinWidth
		{
			get
			{
				return minWidth;
			}
		}
		public override int MinHeight
		{
			get
			{
				return HeaderSize + TopLeftCorner.Height + BottomLeftCorner.Height;
			}
		}

		int HeaderSize
		{
			get { return ButtonTopLeftCorner.Height + pageNameMargin + Font.CharHeight + pageNameMargin + ButtonBottomBorder.Height; }
		}

		public TabPage SelectedPage
		{
			get { return (from p in pages where p.Panel.Activated select p).First(); }
			set
			{
				SelectPage(value);
			}
		}

		public TabContainer()
		{
			Pages = pages.AsReadOnly();
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

			//Button
			ButtonTopLeftCorner = GetTexture(file, category, "ButtonTopLeftCorner");
			ButtonTopRightCorner = GetTexture(file, category, "ButtonTopRightCorner");
			ButtonBottomLeftCorner = GetTexture(file, category, "ButtonBottomLeftCorner");
			ButtonBottomRightCorner = GetTexture(file, category, "ButtonBottomRightCorner");

			ButtonTopBorder = GetTexture(file, category, "ButtonTopBorder");
			ButtonBottomBorder = GetTexture(file, category, "ButtonBottomBorder");
			ButtonLeftBorder = GetTexture(file, category, "ButtonLeftBorder");
			ButtonRightBorder = GetTexture(file, category, "ButtonRightBorder");

			ButtonInside = GetTexture(file, category, "ButtonInside");
			ButtonInsidePressed = GetTexture(file, category, "ButtonInsidePressed");

			Font = GetMonoFont(file, category, "Font");

			FontColor = file.GetColor(category, "FontColor");

			ReloadPageSizes();

			base.BaseInitialize();
		}

		public void SelectPage(string name)
		{
			TabPage selected = (from p in pages where p.Name == name select p).First();

			SelectPage(selected);
		}
		public void SelectPage(TabPage selected)
		{
			if (!pages.Contains(selected))
			{
				Debug.AddExceptionInClass(this.GetType(), "SelectPage", "Can't select point nonexisting page");
				return;
			}

			selected.Panel.Activated = true;

			TabPage oldSelected = null;
			foreach (TabPage p in pages)
			{
				if (p.Panel.Activated)
					oldSelected = p;

				if (p != selected)
					p.Panel.Activated = false;
			}

			if (TabChanged != null)
				TabChanged(this, oldSelected, selected);
		}

		public Panel AddTabPage(string name)
		{
			Panel panel = new Panel();
			panel.Initialize();
			AddTabPage(new TabPage(name, panel));

			panel.Fill();
			panel.Alignment = DrawBoxAlignment.GetFull();

			return panel;
		}
		public void AddTabPage(string name, Panel panel)
		{
			AddTabPage(new TabPage(name, panel));
		}
		public void AddTabPage(TabPage page)
		{
			pages.Add(page);
			AddDrawBox(page.Panel);
			SelectPage(page.Name);

			ReloadPageSizes();
		}

		public void RemoveTabPage(string name)
		{
			TabPage page = (from p in pages where p.Name == name select p).First();
			RemoveTabPage(page);
		}
		public void RemoveTabPage(Panel panel)
		{
			TabPage page = (from p in pages where p.Panel == panel select p).First();
			RemoveTabPage(page);
		}
		public void RemoveTabPage(TabPage page)
		{
			pages.Remove(page);
			RemoveDrawBoxFromSlot(page.Panel);

			ReloadPageSizes();
		}

		void ReloadPageSizes()
		{
			minWidth = 0;
			int totalHeaderSize = 0;

			foreach (TabPage page in pages)
			{
				totalHeaderSize += Font.MeasureString(page.Name).X + ButtonLeftBorder.Width + ButtonRightBorder.Width;
				minWidth += ButtonLeftBorder.Width + ButtonRightBorder.Width;
			}

			sizePercentages.Clear();

			foreach (TabPage page in pages)
			{
				if (page.Name.Length == 0)
					sizePercentages.Add(0);

				sizePercentages.Add((Font.MeasureString(page.Name).X + ButtonLeftBorder.Width + ButtonRightBorder.Width) / (float)totalHeaderSize);
			}

			minWidth = Math.Max(minWidth, TopLeftCorner.Width + TopRightCorner.Width);
		}

		public override ViewRect GetDefaultBoundaries(int newWidth, int newHeight)
		{
			return new ViewRect(RealX + TopLeftCorner.Width,
								RealY + TopLeftCorner.Height + HeaderSize,
								newWidth - TopLeftCorner.Width - TopRightCorner.Width,
								newHeight - HeaderSize - TopLeftCorner.Height - BottomLeftCorner.Height);
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			CheckInput();
		}

		void CheckInput()
		{
			pressedPage = -1;

			if (!IsUnderMouse)
				return;

			if (!MouseInput.IsPressed(MouseButtons.Left))
				return;

			if (!IsMouseInRect(new Rectangle(
				RealX, RealY,
				Width, HeaderSize)))
				return;

			int n = 0;
			int tabX = RealX;
			foreach (TabPage page in pages)
			{
				int size = Math.Min(Font.MeasureString(page.Name).X + ButtonLeftBorder.Width + ButtonRightBorder.Width, (int)Math.Round(Width * sizePercentages[n], 0));

				if (IsMouseInRect(new Rectangle(
					tabX, RealY, size, HeaderSize)))
				{
					pressedPage = n;

					if (MouseInput.IsClicked(MouseButtons.Left))
						SelectPage(page);

					return;
				}

				tabX += size;
				n += 1;
			}
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			if (pages.Count == 0)
			{
				render.DrawSprite(ButtonTopLeftCorner, new Vector2(x, y), Color.White);
				render.DrawSprite(ButtonTopRightCorner, new Vector2(x + Width - ButtonTopRightCorner.Width, y), Color.White);
				render.DrawSprite(ButtonBottomLeftCorner, new Vector2(x, y + ButtonTopLeftCorner.Height + pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);
				render.DrawSprite(ButtonBottomRightCorner, new Vector2(x + Width - ButtonBottomLeftCorner.Width, y + ButtonTopRightCorner.Height + pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);

				render.DrawSprite(ButtonTopBorder, new Rectangle(x + ButtonTopLeftCorner.Width, y, Width - ButtonTopLeftCorner.Width - ButtonTopRightCorner.Width, ButtonTopBorder.Height), Color.White);
				render.DrawSprite(ButtonBottomBorder, new Rectangle(x + ButtonBottomLeftCorner.Width, y + ButtonTopLeftCorner.Height + pageNameMargin + Font.CharHeight + pageNameMargin, Width - ButtonBottomLeftCorner.Width - ButtonBottomRightCorner.Width, ButtonBottomBorder.Height), Color.White);

				render.DrawSprite(ButtonLeftBorder, new Rectangle(x, y + ButtonTopLeftCorner.Height, ButtonLeftBorder.Width, pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);
				render.DrawSprite(ButtonRightBorder, new Rectangle(x + Width - ButtonRightBorder.Width, y + ButtonTopLeftCorner.Height, ButtonLeftBorder.Width, pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);

				render.DrawSprite(ButtonInside, new Rectangle(x + ButtonTopLeftCorner.Width, y + ButtonTopBorder.Height, Width - ButtonLeftBorder.Width - ButtonRightBorder.Width, pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);
			}
			else
			{
				int n = 0;
				int drawX = x;

				foreach (TabPage page in pages)
				{
					int size = Math.Min(Font.MeasureString(page.Name).X + ButtonLeftBorder.Width + ButtonRightBorder.Width, (int)Math.Round(Width * sizePercentages[n], 0));

					bool pressed = n == pressedPage;

					DrawTab(drawX, y, render, page.Name, size, pressed);

					drawX += size;
					n += 1;
				}
			}
			
			y += HeaderSize;

			render.DrawSprite(TopLeftCorner, new Vector2(x, y), Color.White);
			render.DrawSprite(TopRightCorner, new Vector2(x + Width - TopRightCorner.Width, y), Color.White);
			render.DrawSprite(BottomLeftCorner, new Vector2(x, y + Height - HeaderSize - BottomLeftCorner.Height), Color.White);
			render.DrawSprite(BottomRightCorner, new Vector2(x + Width - BottomRightCorner.Width, y + Height - HeaderSize - BottomRightCorner.Height), Color.White);

			render.DrawSprite(TopBorder, new Rectangle(x + TopLeftCorner.Width, y, Width - TopLeftCorner.Width - TopRightCorner.Width, TopBorder.Height), Color.White);
			render.DrawSprite(BottomBorder, new Rectangle(x + BottomLeftCorner.Width, y + Height - HeaderSize - BottomBorder.Height, Width - BottomLeftCorner.Width - BottomRightCorner.Width, BottomBorder.Height), Color.White);
			render.DrawSprite(LeftBorder, new Rectangle(x, y + TopLeftCorner.Height, LeftBorder.Width, Height - HeaderSize - TopLeftCorner.Height - BottomLeftCorner.Height), Color.White);
			render.DrawSprite(RightBorder, new Rectangle(x + Width - RightBorder.Width, y + TopLeftCorner.Height, RightBorder.Width, Height - HeaderSize - TopRightCorner.Height - BottomRightCorner.Height), Color.White);

			render.End();
		}
		void DrawTab(int x, int y, IRender render, string text, int size, bool pressed)
		{
			render.DrawSprite(ButtonTopLeftCorner, new Vector2(x, y), Color.White);
			render.DrawSprite(ButtonTopRightCorner, new Vector2(x + size - ButtonTopRightCorner.Width, y), Color.White);
			render.DrawSprite(ButtonBottomLeftCorner, new Vector2(x, y + ButtonTopLeftCorner.Height + pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);
			render.DrawSprite(ButtonBottomRightCorner, new Vector2(x + size - ButtonBottomLeftCorner.Width, y + ButtonTopRightCorner.Height + pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);

			render.DrawSprite(ButtonTopBorder, new Rectangle(x + ButtonTopLeftCorner.Width, y, size - ButtonTopLeftCorner.Width - ButtonTopRightCorner.Width, ButtonTopBorder.Height), Color.White);
			render.DrawSprite(ButtonBottomBorder, new Rectangle(x + ButtonBottomLeftCorner.Width, y + ButtonTopLeftCorner.Height + pageNameMargin + Font.CharHeight + pageNameMargin, size - ButtonBottomLeftCorner.Width - ButtonBottomRightCorner.Width, ButtonBottomBorder.Height), Color.White);

			render.DrawSprite(ButtonLeftBorder, new Rectangle(x, y + ButtonTopLeftCorner.Height, ButtonLeftBorder.Width, pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);
			render.DrawSprite(ButtonRightBorder, new Rectangle(x + size - ButtonRightBorder.Width, y + ButtonTopLeftCorner.Height, ButtonLeftBorder.Width, pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);

			ISprite inside = pressed ? ButtonInsidePressed : ButtonInside;
			render.DrawSprite(inside, new Rectangle(x + ButtonTopLeftCorner.Width, y + ButtonTopBorder.Height, size - ButtonLeftBorder.Width - ButtonRightBorder.Width, pageNameMargin + Font.CharHeight + pageNameMargin), Color.White);

			Font.DrawString(text, new Point(x + ButtonTopLeftCorner.Width, y + HeaderSize / 2 - Font.CharHeight / 2), FontColor, render); 
		}

		public class TabPage
		{
			public string Name;
			public readonly Panel Panel;

			public TabPage(string name, Panel panel)
			{
				Name = name;
				Panel = panel;
			}
		}
	}
}