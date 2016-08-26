using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;

namespace TakaGUI.DrawBoxes.Forms
{
	public class GridForm : Form
	{
		int gridWidth;
		int gridHeight;
		string category;
		protected GridPanel Panel;
		protected ISkinFile SkinFileInUse;

		public virtual void Initialize(int gridWidth, int gridHeight)
		{
			Initialize(gridWidth, gridHeight, Form.DefaultCategory, DefaultSkinFile);
		}
		public virtual void Initialize(int gridWidth, int gridHeight, ISkinFile file)
		{
			Initialize(gridWidth, gridHeight, Form.DefaultCategory, file);
		}
		public virtual void Initialize(int _gridWidth, int _gridHeight, string _category, ISkinFile _file)
		{
			gridWidth = _gridWidth;
			gridHeight = _gridHeight;
			SkinFileInUse = _file;
			category = _category;

			base.Initialize(category, SkinFileInUse);
		}
		public override void Initialize(string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public override void AddedToContainer()
		{
			Panel = new GridPanel();
			Panel.Initialize(gridWidth, gridHeight);

			const int defaultRowWidth = 100;
			RowWidths = new int[gridWidth];
			for (int n = 0; n < RowWidths.Length; n++)
				RowWidths[n] = defaultRowWidth;

			AddDrawBox(Panel);
			Panel.X = BorderMargin;
			Panel.Y = BorderMargin;
			Wrap();
			Width += BorderMargin;
			Height += BorderMargin;

			Panel.Alignment = DrawBoxAlignment.GetEmpty();
			CanResizeFormHorizontally = false;
			CanResizeFormVertically = false;
		}

		protected int[] RowWidths
		{
			get;
			private set;
		}
		protected int BorderMargin = 3;

		public void AddDrawBoxRow(params DrawBox[] drawBoxes)
		{
			if (drawBoxes.Length > Panel.GridSize.Width)
			{
				Debug.AddExceptionInClass(this.GetType(), "AddDrawBox", "Tried to more drawboxes to point row than can fit.");
				return;
			}

			for (int n = 0; n < Panel.GridSize.Width; n++)
			{
				if (n < drawBoxes.Length)
				{
					if (drawBoxes[n] != null)
						drawBoxes[n].Width = Math.Max(RowWidths[n], drawBoxes[n].Width);

					Panel.AddDrawBox(drawBoxes[n]);
				}
				else
					Panel.AddDrawBox(null);
			}
		}
		public void AddLabelDrawBoxRow(string labelString, params DrawBox[] drawBoxes)
		{
			Label label = new Label(labelString);
			label.Initialize(Label.DefaultCategory, SkinFileInUse);

			if (drawBoxes.Length > Panel.GridSize.Width - 1)
			{
				Debug.AddExceptionInClass(this.GetType(), "AddLabelDrawBoxRow", "Tried to more drawboxes to point row than can fit.");
				return;
			}

			Panel.AddDrawBox(label);

			for (int n = 0; n < Panel.GridSize.Width - 1; n++)
			{
				if (n < drawBoxes.Length)
				{
					if (drawBoxes[n] != null)
						drawBoxes[n].Width = RowWidths[n + 1];

					Panel.AddDrawBox(drawBoxes[n]);
				}
				else
					Panel.AddDrawBox(null);
			}
		}

		public override void Idle(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Idle(gameTime);

			Wrap();
			Width += BorderMargin;
			Height += BorderMargin;
		}
	}
}
