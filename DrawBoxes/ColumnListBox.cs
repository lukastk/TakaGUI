using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	//TODO: add proper sorting for ints
	public class ColumnListBox : DrawBox
	{
		public delegate void SelectedItemChangedEvent(object sender, ListBoxRow oldItem, ListBoxRow newItem);
		public delegate void ItemClickedEvent(object sender, ListBoxRow item, int index);
		public delegate void ItemDoubleClickedEvent(object sender, ListBoxRow item, int index);

		public event SelectedItemChangedEvent SelectedItemChanged;
		public event ItemClickedEvent ItemClicked;
		public event ItemDoubleClickedEvent ItemDoubleClicked;

		#region Resources
		public static string DefaultCategory = "ColumnListBox";

		public ISprite TopLeftCorner;
		public ISprite TopRightCorner;
		public ISprite BottomLeftCorner;
		public ISprite BottomRightCorner;

		public ISprite TopBorder;
		public ISprite RightBorder;
		public ISprite BottomBorder;
		public ISprite LeftBorder;

		public ISprite Inside;

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
		public Color ColumnButtonColor;
		public Color ListElementColor;

		public Color SelectedElementColor;
		public Color ElementUnderMouseColor;

		#endregion

		public override int MinWidth
		{
			get
			{
				return LeftBorder.Width + RightBorder.Width;
			}
		}
		public override int MinHeight
		{
			get
			{
				return HeaderSize + TopBorder.Height + TopBorder.Height;
			}
		}

		int HeaderSize
		{
			get { return ButtonTopLeftCorner.Height + columnNameMargin + Font.CharHeight + columnNameMargin + ButtonBottomBorder.Height; }
		}

		List<ListBoxRow> values = new List<ListBoxRow>();
		public readonly ReadOnlyCollection<ListBoxRow> Values;

		bool[] sortTypes;
		List<string> columnNames = new List<string>();
		public readonly ReadOnlyCollection<string> ColumnNames;
		List<int> columnSizes = new List<int>();
		public readonly ReadOnlyCollection<int> ColumnSizes;

		public bool AutomaticallySetColumnSizes = true;
		public bool OrderAutomatically = true;
		public bool Ascending = true;
		public int OrderingIndex = 0;

		const int columnNameMargin = 1;
		const int textMargin = 2;
		int pressedColumnButton = -1;

		int rowUnderMouse = -1;
		ListBoxRow selectedRow;
		public ListBoxRow SelectedRow
		{
			get { return selectedRow; }
			set { selectedRow = value; }
		}
		public int SelectedRowIndex
		{
			get
			{
				if (SelectedRow == null)
					return -1;

				return Values.IndexOf(SelectedRow);
			}
			set
			{
				SelectedRow = Values[value];
			}
		}

		public ColumnListBox()
		{
			Values = values.AsReadOnly();
			ColumnNames = columnNames.AsReadOnly();
			ColumnSizes = columnSizes.AsReadOnly();
		}

		public virtual void Initialize(int columns, string category = null, ISkinFile file = null)
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

			Inside = GetTexture(file, category, "Inside");

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

			ColumnButtonColor = file.GetColor(category, "ColumnButtonColor");
			ListElementColor = file.GetColor(category, "ListElementColor");

			SelectedElementColor = file.GetColor(category, "SelectedElementColor");
			ElementUnderMouseColor = file.GetColor(category, "ElementUnderMouseColor");

			Width = 100;
			Height = 100;

			//Set default sizes:
			int totalsize = 0;
			for (int n = 0; n < columns; n++)
			{
				columnNames.Add(Convert.ToString(n));
				totalsize += Font.MeasureString(columnNames[n]).X + ButtonLeftBorder.Width + ButtonRightBorder.Width;
			}

			for (int n = 0; n < columns; n++)
			{
				float fraction = (Font.MeasureString(columnNames[n]).X + ButtonLeftBorder.Width + ButtonRightBorder.Width) / (float)totalsize;
				columnSizes.Add((int)Math.Round(Width * fraction, 0));
			}

			sortTypes = new bool[columns];

			base.BaseInitialize();
		}

		public ListBoxRow AddRow(params object[] rowValues)
		{
			ListBoxRow row = new ListBoxRow(rowValues);

			values.Add(row);

			return row;
		}
		public void RemoveRow(int index)
		{
			values.RemoveAt(index);
		}
		public void SortByColumn(int index, bool ascending)
		{
			OrderingIndex = index;

			IEnumerable<ListBoxRow> ordered;

			if (sortTypes[index])
			{
				if (ascending)
					ordered = from row in values orderby Convert.ToDouble(row.Values[index]) ascending select row;
				else
					ordered = from row in values orderby Convert.ToDouble(row.Values[index]) descending select row;
			}
			else
			{
				if (ascending)
					ordered = from row in values orderby row.Values[index] ascending select row;
				else
					ordered = from row in values orderby row.Values[index] descending select row;
			}

			var copy = new List<ListBoxRow>(ordered);

			values.Clear();
			values.AddRange(copy);
		}

		public void Sort()
		{
			SortByColumn(OrderingIndex, Ascending);
		}

		public void ClearRows()
		{
			values.Clear();
		}

		public void SetColumnName(int index, string name)
		{
			columnNames[index] = name;
		}
		public void SetColumnSize(int index, int size)
		{
			if (size < ButtonLeftBorder.Width + ButtonRightBorder.Width)
			{
				Debug.AddExceptionInClass(this.GetType(), "SetColumnSize", "Size too small.");
			}

			columnSizes[index] = size;
		}

		public void SetIntOrStringSort(params bool[] _sortTypes)
		{
			if (_sortTypes.Length != ColumnNames.Count)
				throw new Exception("The array length has to be the same as the amount of columns.");

			sortTypes = _sortTypes.ToArray();
		}

		const int columnFitMargin = 4;
		public void MakeAllColumnsFit()
		{
			columnSizes.Clear();

			int n = 0;

			foreach (string name in columnNames)
			{
				int size = Font.MeasureString(name).X + ButtonLeftBorder.Width + ButtonRightBorder.Width;

				foreach (ListBoxRow row in values)
				{
					string valStr = row.Values[n] == null ? "NULL" : row.Values[n].ToString();

					size = Math.Max(size, Font.MeasureString(valStr).X);
				}

				columnSizes.Add(size + columnFitMargin);
				n += 1;
			}
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (SelectedRow != null && !Values.Contains(SelectedRow))
				SelectedRow = null;

			if (OrderAutomatically)
				SortByColumn(OrderingIndex, Ascending);

			if (AutomaticallySetColumnSizes)
				MakeAllColumnsFit();

			CheckInput();
			CheckSelectedRow();
		}

		public override void Project(Microsoft.Xna.Framework.GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();
			
			int n = 0;
			int drawX = x + textMargin;

			foreach (string name in columnNames)
			{
				bool pressed = n == pressedColumnButton;

				int size = columnSizes[n];

				bool quit = false;
				if (drawX + columnSizes[n] >= RealX + Width)
				{
					size = RealX + Width - drawX;
					quit = true;
				}

				DrawButton(drawX, y, render, name, size, pressed);

				if (quit)
					break;

				drawX += columnSizes[n];

				n += 1;
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

			render.DrawSprite(Inside, new Rectangle(x + TopLeftCorner.Width, y + TopLeftCorner.Height, Width - BottomLeftCorner.Width - BottomRightCorner.Width, Height - HeaderSize - TopLeftCorner.Height - BottomLeftCorner.Height), Color.White);

			render.End();

			//DrawSprite list items:

			render.AddViewRect(new ViewRect(RealX + TopLeftCorner.Width,
											RealY + TopLeftCorner.Height + HeaderSize,
											Width - TopLeftCorner.Width - TopRightCorner.Width,
											Height - HeaderSize - TopLeftCorner.Height - BottomLeftCorner.Height));

			render.Begin();

			int rowX = RealX + TopLeftCorner.Width + textMargin;
			int rowY = RealY + TopLeftCorner.Height + HeaderSize + textMargin;
			int rowNumber = 0;
			foreach (ListBoxRow row in Values)
			{
				if (rowNumber == rowUnderMouse)
					render.DrawRect(new Rectangle(rowX, rowY, Width - TopLeftCorner.Width - TopRightCorner.Width, Font.CharHeight + textMargin), ElementUnderMouseColor);
				if (row == SelectedRow)
					render.DrawRect(new Rectangle(rowX, rowY, Width - TopLeftCorner.Width - TopRightCorner.Width, Font.CharHeight + textMargin), SelectedElementColor);

				int index = 0;
				foreach (object val in row.Values)
				{
					string valStr = val == null ? "NULL" : val.ToString();

					Font.DrawString(valStr, new Point(rowX, rowY), ListElementColor, render);
					rowX += ColumnSizes[index];
					index += 1;
				}

				rowX = RealX + TopLeftCorner.Width + textMargin;
				rowY += Font.CharHeight + textMargin;

				rowNumber += 1;
			}

			render.End();
		}
		void DrawButton(int x, int y, IRender render, string text, int size, bool pressed)
		{
			render.DrawSprite(ButtonTopLeftCorner, new Vector2(x, y), Color.White);
			render.DrawSprite(ButtonTopRightCorner, new Vector2(x + size - ButtonTopRightCorner.Width, y), Color.White);
			render.DrawSprite(ButtonBottomLeftCorner, new Vector2(x, y + ButtonTopLeftCorner.Height + columnNameMargin + Font.CharHeight + columnNameMargin), Color.White);
			render.DrawSprite(ButtonBottomRightCorner, new Vector2(x + size - ButtonBottomLeftCorner.Width, y + ButtonTopRightCorner.Height + columnNameMargin + Font.CharHeight + columnNameMargin), Color.White);

			render.DrawSprite(ButtonTopBorder, new Rectangle(x + ButtonTopLeftCorner.Width, y, size - ButtonTopLeftCorner.Width - ButtonTopRightCorner.Width, ButtonTopBorder.Height), Color.White);
			render.DrawSprite(ButtonBottomBorder, new Rectangle(x + ButtonBottomLeftCorner.Width, y + ButtonTopLeftCorner.Height + columnNameMargin + Font.CharHeight + columnNameMargin, size - ButtonBottomLeftCorner.Width - ButtonBottomRightCorner.Width, ButtonBottomBorder.Height), Color.White);

			render.DrawSprite(ButtonLeftBorder, new Rectangle(x, y + ButtonTopLeftCorner.Height, ButtonLeftBorder.Width, columnNameMargin + Font.CharHeight + columnNameMargin), Color.White);
			render.DrawSprite(ButtonRightBorder, new Rectangle(x + size - ButtonRightBorder.Width, y + ButtonTopLeftCorner.Height, ButtonLeftBorder.Width, columnNameMargin + Font.CharHeight + columnNameMargin), Color.White);

			ISprite inside = pressed ? ButtonInsidePressed : ButtonInside;
			render.DrawSprite(inside, new Rectangle(x + ButtonTopLeftCorner.Width, y + ButtonTopBorder.Height, size - ButtonLeftBorder.Width - ButtonRightBorder.Width, columnNameMargin + Font.CharHeight + columnNameMargin), Color.White);

			int strX = x + ButtonTopLeftCorner.Width + (size - ButtonTopLeftCorner.Width - ButtonTopRightCorner.Width) / 2 - Font.MeasureString(text).X / 2;
			int strY = y + HeaderSize / 2 - Font.CharHeight / 2;
			Font.DrawString(text, new Point(strX, strY), Color.White, render);
		}

		void CheckInput()
		{
			pressedColumnButton = -1;

			if (!IsUnderMouse)
				return;

			if (!MouseInput.IsPressed(MouseButtons.Left))
				return;

			if (!IsMouseInRect(new Rectangle(
				RealX, RealY,
				Width, HeaderSize)))
				return;

			int n = 0;
			int tabX = RealX + textMargin;
			foreach (int size in ColumnSizes)
			{
				if (IsMouseInRect(new Rectangle(
					tabX, RealY, size, HeaderSize)))
				{
					pressedColumnButton = n;

					if (MouseInput.IsClicked(MouseButtons.Left))
					{
						Ascending = !Ascending;
						SortByColumn(n, Ascending);
					}

					return;
				}

				tabX += size;

				n += 1;
			}
		}
		void CheckSelectedRow()
		{
			rowUnderMouse = -1;

			if (!IsUnderMouse)
				return;

			if (!IsMouseInRect(new Rectangle(RealX + TopLeftCorner.Width,
											RealY + TopLeftCorner.Height + HeaderSize,
											Width - TopLeftCorner.Width - TopRightCorner.Width,
											Height - HeaderSize - TopLeftCorner.Height - BottomLeftCorner.Height)))
				return;

			int rowX = RealX + TopLeftCorner.Width + textMargin;
			int rowY = RealY + TopLeftCorner.Height + HeaderSize + textMargin;

			int n = 0;
			foreach (ListBoxRow row in values)
			{
				if (IsMouseInRect(new Rectangle(
					rowX, rowY, Width - TopLeftCorner.Width - TopRightCorner.Width, Font.CharHeight + textMargin)))
				{
					rowUnderMouse = n;

					if (MouseInput.IsClicked(MouseButtons.Left))
					{
						ListBoxRow oldSelected = SelectedRow;
						SelectedRow = Values[n];

						if (SelectedItemChanged != null)
							SelectedItemChanged(this, oldSelected, SelectedRow);

						if (ItemClicked != null)
							ItemClicked(this, SelectedRow, values.IndexOf(SelectedRow));

						if (!MouseInput.IsDoubleClicked(MouseButtons.Left).IsNegative && ItemDoubleClicked != null)
							ItemDoubleClicked(this, SelectedRow, values.IndexOf(SelectedRow));
					}
				}

				rowY += Font.CharHeight + textMargin;

				n += 1;
			}
		}

		public class ListBoxRow
		{
			public object[] Values { get; private set; }
			public object[] ExtraValues;

			public ListBoxRow(int columns)
			{
				Values = new string[columns];
			}
			public ListBoxRow(object[] values)
			{
				Values = values.ToArray();
			}

			public void SetValue(int index, object value)
			{
				Values[index] = value;
			}
		}
	}
}
