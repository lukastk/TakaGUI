using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using TakaGUI.Data;
using System.IO;
using TakaGUI.Machines;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class ComboBox : SlotBox
	{
		public delegate void SelectedItemChangedEvent(object sender, int newItemIndex, int oldItemIndex);

		public event SelectedItemChangedEvent SelectedItemChanged;

		SlotHandler iconSlot;
		SlotHandler scrollbarSlot;

		#region Textures
		public static string DefaultCategory = "ComboBox";

		public ISprite TopLeftCorner;
		public ISprite TopRightCorner;
		public ISprite BottomLeftCorner;
		public ISprite BottomRightCorner;

		public ISprite TopBorder;
		public ISprite BottomBorder;
		public ISprite LeftBorder;
		public ISprite RightBorder;

		public ISprite Inside;

		#endregion

		public string CurrentLine
		{
			get
			{
				if (Index == -1)
					return "";

				return Items[Index];
			}
			set
			{
				Items[Index] = value;
			}
		}
		int _Index = -1;
		public int Index
		{
			get { return _Index; }
			set
			{
				int oldValue = _Index;
				_Index = value;

				if (_Index < 0)
					_Index = -1;

				if (_Index >= Items.Count)
					_Index = Items.Count - 1;

				if (oldValue != _Index && SelectedItemChanged != null)
					SelectedItemChanged(this, _Index, oldValue);
			}
		}
		public MonoFont Font;
		public Color FontColor = Color.White;
		public bool MaxCharsIsWidth = true;

		const int edgeSize = 1;

		const int textMargin = 2;
		public override int MinWidth
		{
			get
			{
				int minWidth = edgeSize * 2;

				if (icon != null)
					minWidth = Math.Max(icon.MinWidth, minWidth);

				return minWidth;
			}
		}
		public override int MinHeight
		{
			get
			{
				if (_Open)
					return TextFieldHeight + scrollbar.MinHeight;
				else
					return TextFieldHeight;
			}
		}
		public override int MaxHeight
		{
			get
			{
				if (_Open)
				{
					return -1;
				}

				return TextFieldHeight;
			}
		}
		int TextFieldWidth
		{
			get { return Width - icon.Width; }
		}
		int TextFieldHeight
		{
			get
			{
				int minHeight = Font != null ? Font.CharHeight : 0;

				if (icon != null)
					minHeight = Math.Max(icon.MinHeight, minHeight);

				return minHeight + textMargin * 2;
			}
		}
		int _OpenHeight;
		public int OpenHeight
		{
			get { return _OpenHeight; }
			set
			{
				_OpenHeight = value;

				if (_OpenHeight < TextFieldHeight + scrollbar.MinHeight)
					_OpenHeight = TextFieldHeight + scrollbar.MinHeight;
			}
		}

		int ListHeight
		{
			get
			{
				int listHeight = itemMargin;

				foreach (string line in Items)
					listHeight += (int)Font.MeasureString(line).Y + Font.VerticalSpace;

				return listHeight;
			}
		}

		public override int Height
		{
			get
			{
				if (_Open)
					return OpenHeight;
				else
					return TextFieldHeight;
			}
			set { }
		}

		IconButton icon;
		VScrollbar scrollbar;

		const int itemMargin = 3;
		public List<string> Items
		{
			get;
			protected set;
		}	

		#region Events
		void icon_Click(object sender)
		{
			Open = !Open;
		}

		#endregion

		bool _Open = false;
		public bool Open
		{
			get { return _Open; }
			set
			{
				_Open = value;

				ReloadElements();
			}
		}
		public Color OpenBackgroundColor = Color.White;
		public Color OpenFontColor = Color.Black;
		public Color MouseOverElementColor = Color.CornflowerBlue;
		int ItemPositionY
		{
			get
			{
				if (scrollbar.MaxValue != 0)
					return (int)Math.Round( (ListHeight - (OpenHeight - TextFieldHeight)) * ((float)scrollbar.Value), 0);
				return 0;
			}
		}

		public ComboBox()
		{
			Items = new List<string>();
		}

		public virtual void Initialize(string _category = null, ISkinFile _file = null)
		{
			if (category == null)
				_category = DefaultCategory;
			if (file == null)
				_file = DefaultSkinFile;

			file = _file;
			category = _category;

			TopLeftCorner = GetTexture(file, category, "TopLeftCorner");
			TopRightCorner = GetTexture(file, category, "TopRightCorner");
			BottomLeftCorner = GetTexture(file, category, "BottomLeftCorner");
			BottomRightCorner = GetTexture(file, category, "BottomRightCorner");

			TopBorder = GetTexture(file, category, "TopBorder");
			BottomBorder = GetTexture(file, category, "BottomBorder");
			LeftBorder = GetTexture(file, category, "LeftBorder");
			RightBorder = GetTexture(file, category, "RightBorder");

			Inside = GetTexture(file, category, "Inside");

			Font = GetMonoFont(file, category, "Font");

			base.BaseInitialize();
		}

		ISkinFile file;
		string category;
		public override void AddedToContainer()
		{
			iconSlot = AddNewSlot();
			icon = new IconButton();
			icon.Initialize(category + "." + IconButton.DefaultCategory, file);
			PutDrawBoxInSlot(icon, iconSlot);
			icon.SetSize(Height, Height);
			icon.X = Width - icon.Width;
			icon.Y = 0;
			icon.Click += new DefaultEvent(icon_Click);

			scrollbarSlot = AddNewSlot();
			scrollbar = new VScrollbar();
			scrollbar.Initialize(category + "." + VScrollbar.DefaultCategory, file);
			PutDrawBoxInSlot(scrollbar, scrollbarSlot);
			scrollbar.X = Width - scrollbar.Width;
			scrollbar.Y = TextFieldHeight;
			scrollbar.Activated = false;
		}

		void ReloadElements()
		{
			icon.SetSize(TextFieldHeight, TextFieldHeight);
			icon.X = Width - icon.Width;
			icon.Y = 0;

			OpenHeight = OpenHeight;

			if (Open && ListHeight > OpenHeight - TextFieldHeight)
			{
				scrollbar.Activated = true;
				scrollbar.X = Width - scrollbar.Width;
				scrollbar.Y = TextFieldHeight;
				scrollbar.Height = Height - TextFieldHeight;
			}
			else
				scrollbar.Activated = false;
		}

		public void SetWidthInCharacters(int characterAmount)
		{
			Width = characterAmount * (Font.CharWidth + Font.HorizontalSpace) - Font.HorizontalSpace;
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);
			
			ReloadElements();

			//Check for clicking on an element
			if (Open &&
				IsMouseInRect(new Rectangle(RealX, RealY + TextFieldHeight + itemMargin, TextFieldWidth, OpenHeight - TextFieldHeight)) &&
				MouseInput.IsClicked(MouseButtons.Left))
			{
				int selectedIndex = (MouseInput.Y + ItemPositionY - (RealY + TextFieldHeight + itemMargin)) / (Font.CharHeight + Font.VerticalSpace);

				if (selectedIndex >= 0 && selectedIndex < Items.Count)
				{
					Index = selectedIndex;
					Open = false;
				}
			}

			if (!HasFocus && Open)
				Open = false;
		}
		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			render.DrawBody(new Rectangle(x, y, TextFieldWidth, TextFieldHeight), Color.White,
				TopBorder,
				TopRightCorner,
				RightBorder,
				BottomRightCorner,
				BottomBorder,
				BottomLeftCorner,
				LeftBorder,
				TopLeftCorner,
				Inside);

			Font.DrawString(CurrentLine, new Point(x + LeftBorder.Width + textMargin, y + TextFieldHeight / 2 - Font.CharHeight / 2), FontColor, render);

			render.End();

			if (Open)
				Parent.AddDrawDelegate(DrawOpen);
		}

		void DrawOpen(GameTime gameTime, ViewRect viewRect)
		{
			if (IsClosed || !IsInitialized || !Activated || Hidden)
				return;

			viewRect.Add(MasterBoundaries);
			IRender render = GraphicsManager.GetRender();

			int x = RealX;
			int y = RealY;

			render.AddViewRect(new ViewRect(x, y + TextFieldHeight, TextFieldWidth, OpenHeight - TextFieldHeight));

			render.Begin();

			render.DrawRect(new Rectangle(x, y + TextFieldHeight, TextFieldWidth, OpenHeight - TextFieldHeight), OpenBackgroundColor);

			//DrawSprite items
			int drawY = y + TextFieldHeight - ItemPositionY + itemMargin;
			foreach (string line in Items)
			{
				Rectangle area = new Rectangle(x, drawY, TextFieldWidth, Font.CharHeight + Font.VerticalSpace);
				if (IsMouseInRect(area))
				{
					render.DrawRect(area, MouseOverElementColor);
				}

				Font.DrawString(line, new Point(x + itemMargin, drawY), OpenFontColor, render);
				drawY += Font.CharHeight + Font.VerticalSpace;
			}

			render.End();
		}
	}
}
