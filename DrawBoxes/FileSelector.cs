using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;
using Microsoft.Xna.Framework;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Input;
using TakaGUI.Machines;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class FileSelector : SlotBox
	{
		public DefaultEvent UpIconClicked;

		#region Textures
		public static string DefaultCategory = "FileSelector";

		public ISprite TopRightCorner;
		public ISprite BottomLeftCorner;
		public ISprite BottomRightCorner;
		public ISprite UpperFieldBottomRightCorner;

		public ISprite TopBorder;
		public ISprite BottomBorder;
		public ISprite LeftBorder;
		public ISprite MiddleBorder;
		public ISprite RightBorder;

		public ISprite Inside;

		public ISprite FolderSign;

		//Icon
		public ISprite IconTop;
		public ISprite IconBottom;
		public ISprite IconLeft;
		public ISprite IconRight;
		public ISprite IconInside;
		public ISprite IconUpSign;

		public ISprite IconInsidePressed;
		public ISprite IconUpSignPressed;

		#endregion

		#region TextField
		public event DefaultEvent TextChanged;

		TextInputMachine inputMachine;
		public string Text
		{
			get { return inputMachine.Text; }
			set
			{
				inputMachine.Text = value;

				if (TextChanged != null)
					TextChanged(this);
			}
		}
		public string Filename
		{
			get
			{
				if (CurrentDirectory != null)
					return Path.Combine(CurrentDirectory.FullName, Text);

				return Text;
			}
		}
		public int CursorPosition
		{
			get { return inputMachine.Cursor; }
			set { inputMachine.Cursor = value; }
		}

		void FileSelector_FocusChanged(object sender, bool newValue)
		{
			if (!newValue)
			{
				inputMachine.EndCurrentInput();
			}
		}
		void FileSelector_DrawBoxHasFocus(object sender)
		{
			if (MouseInput.IsClicked(MouseButtons.Left) &&
				IsMouseInRect(
				new Rectangle(
					RealX + IconTop.Width + TextMargin,
					RealY + TopBorder.Height + TextMargin,
					Width - IconTop.Width - RightBorder.Width - TextMargin * 2,
					UpperFieldHeight - TopBorder.Height - MiddleBorder.Height - TextMargin * 2)))
							
			{
				CursorPosition = (MouseInput.X - (RealX + IconTop.Width + TextMargin)) / (Font.CharWidth + Font.HorizontalSpace);
			}
		}

		#endregion

		#region Directory
		public DirectoryInfo CurrentDirectory
		{
			get;
			protected set;
		}
		protected List<FileInfo> fileList = new List<FileInfo>();
		protected List<DirectoryInfo> directoryList = new List<DirectoryInfo>();
		public ReadOnlyCollection<FileInfo> FileList
		{
			get;
			private set;
		}
		public ReadOnlyCollection<DirectoryInfo> DirectoryList
		{
			get;
			private set;
		}

		public bool CanSelectSeveralElements = false;
		protected List<FileInfo> selectedFiles = new List<FileInfo>();
		public ReadOnlyCollection<FileInfo> SelectedFiles;
		protected List<DirectoryInfo> selectedDirectories = new List<DirectoryInfo>();
		public ReadOnlyCollection<DirectoryInfo> SelectedDirectories;

		#endregion

		protected HScrollbar HScrollbar
		{
			get
			{
				if (hSlot != null)
					return (HScrollbar)hSlot.DrawBox;
				return null;
			}
		}
		protected VScrollbar VScrollbar
		{
			get
			{
				if (vSlot != null)
					return (VScrollbar)vSlot.DrawBox;
				return null;
			}
		}
		SlotHandler hSlot;
		SlotHandler vSlot;

		public MonoFont Font;
		public Color FontColor = Color.Black;
		public int TextMargin = 3;
		public Color SeparatorColor = Color.Black;
		public Color SelectedColor = Color.CornflowerBlue;

		public bool UpIsPressed
		{
			get;
			private set;
		}

		const int UpperFieldTextMargin = 2;
		int UpperFieldHeight
		{
			get
			{
				int height = 0;
				if (Font != null)
					height = Font.CharHeight + UpperFieldTextMargin * 2 + TopBorder.Height + BottomBorder.Height;

				height = Math.Max(height, IconTop.Height + IconUpSign.Height + IconBottom.Height);

				return height;
			}
		}
		int LowerFieldWidth
		{
			get
			{
				if (VScrollbar != null && VScrollbar.Activated)
					return Width - VScrollbar.MinWidth;
				else
					return Width;
			}
		}
		int LowerFieldHeight
		{
			get
			{
				if (HScrollbar != null && HScrollbar.Activated)
					return Height - UpperFieldHeight - HScrollbar.MinHeight;
				else
					return Height - UpperFieldHeight;
			}
		}
		public override int MinWidth
		{
			get
			{
				int minWidth = IconTop.Width + RightBorder.Width;

				if (VScrollbar != null && VScrollbar.Activated)
					minWidth += VScrollbar.MinWidth;

				if (HScrollbar != null && HScrollbar.Activated)
					minWidth = Math.Max(HScrollbar.MinWidth, minWidth);

				return minWidth;
			}
		}
		public override int MinHeight
		{
			get
			{
				int minHeight = MiddleBorder.Height + BottomBorder.Height + UpperFieldHeight;

				if (VScrollbar != null && VScrollbar.Activated)
					minHeight = Math.Max(minHeight, VScrollbar.MinHeight + UpperFieldHeight);
				if (HScrollbar != null && HScrollbar.Activated)
					minHeight += HScrollbar.MinHeight;

				return minHeight;
			}
		}

		int selectorX
		{
			get { return RealX + LeftBorder.Width + TextMargin; }
		}
		int selectorY
		{
			get { return RealY + UpperFieldHeight + TextMargin; }
		}
		int selectorWidth
		{
			get { return LowerFieldWidth - LeftBorder.Width - RightBorder.Width - TextMargin * 2; }
		}
		int selectorHeight
		{
			get { return LowerFieldHeight - BottomBorder.Height - TextMargin * 2; }
		}
		int textAreaOriginX
		{
			get { return (int)Math.Round((selectorWidth - textAreaWidth) * ((float)HScrollbar.ScrollerPosition / HScrollbar.MaxValue), 0); }
		}
		int textAreaOriginY
		{
			get { return (int)Math.Round((selectorHeight - textAreaHeight) * ((float)VScrollbar.ScrollerPosition / VScrollbar.MaxValue), 0); }
		}
		int textAreaWidth = 0;
		int textAreaHeight = 0;

		public List<Keys> MultipleSelectKeys
		{
			get;
			private set;
		}

		public FileSelector()
		{
			MultipleSelectKeys = new List<Keys>();
			MultipleSelectKeys.Add(Keys.LeftControl);
			MultipleSelectKeys.Add(Keys.RightControl);

			FileList = fileList.AsReadOnly();
			DirectoryList = directoryList.AsReadOnly();

			SelectedFiles = selectedFiles.AsReadOnly();
			SelectedDirectories = selectedDirectories.AsReadOnly();

			//TextField
			FocusChanged += new BooleanChangedEvent(FileSelector_FocusChanged);
			DrawBoxHasFocus += new DefaultEvent(FileSelector_DrawBoxHasFocus);
		}

		void Container_SizeChanged(object sender, Point oldSize, Point newSize)
		{
			LoadSize();
		}

		public virtual void Initialize(string category = null, ISkinFile _file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (_file == null)
				_file = DefaultSkinFile;

			file = _file;

			TopRightCorner = GetTexture(file, category, "TopRightCorner");
			BottomLeftCorner = GetTexture(file, category, "BottomLeftCorner");
			BottomRightCorner = GetTexture(file, category, "BottomRightCorner");
			UpperFieldBottomRightCorner = GetTexture(file, category, "UpperFieldBottomRightCorner");

			TopBorder = GetTexture(file, category, "TopBorder");
			BottomBorder = GetTexture(file, category, "BottomBorder");
			LeftBorder = GetTexture(file, category, "LeftBorder");
			MiddleBorder = GetTexture(file, category, "MiddleBorder");
			RightBorder = GetTexture(file, category, "RightBorder");

			Inside = GetTexture(file, category, "Inside");

			FolderSign = GetTexture(file, category, "FolderSign");

			IconTop = GetTexture(file, category, "IconTop");
			IconBottom = GetTexture(file, category, "IconBottom");
			IconLeft = GetTexture(file, category, "IconLeft");
			IconRight = GetTexture(file, category, "IconRight");
			IconInside = GetTexture(file, category, "IconInside");
			IconUpSign = GetTexture(file, category, "IconUpSign");

			IconInsidePressed = GetTexture(file, category, "IconInsidePressed");
			IconUpSignPressed = GetTexture(file, category, "IconUpSignPressed");

			Font = GetMonoFont(file, category, "Font");

			inputMachine = new TextInputMachine(KeyboardInput, Font);

			base.BaseInitialize();
		}

		ISkinFile file;
		public override void AddedToContainer()
		{
			Container.SizeChanged += new SizeChangedEvent(Container_SizeChanged);

			hSlot = AddNewSlot();
			HScrollbar hScrollbar = new HScrollbar();
			hScrollbar.Initialize(null, file);
			PutDrawBoxInSlot(hScrollbar, hSlot);
			hScrollbar.Activated = false;

			vSlot = AddNewSlot();
			VScrollbar vScrollbar = new VScrollbar();
			vScrollbar.Initialize(null, file);
			PutDrawBoxInSlot(vScrollbar, vSlot);
			vScrollbar.Activated = false;

			GoToDirectory(Environment.CurrentDirectory);
		}

		public List<string> GetNames()
		{
			List<string> nameList = new List<string>();

			foreach (DirectoryInfo d in directoryList)
				nameList.Add(d.Name);
			foreach (FileInfo f in fileList)
				nameList.Add(f.Name);

			return nameList;
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);
			
			HScrollbar.Activated = textAreaWidth > selectorWidth;
			VScrollbar.Activated = textAreaHeight > selectorHeight;

			if (HScrollbar.Activated)
			{
				HScrollbar.Width = LowerFieldWidth;
				HScrollbar.X = 0;
				HScrollbar.Y = Height - HScrollbar.Height;

				HScrollbar.MaxValue = textAreaWidth - selectorWidth;
			}
			if (VScrollbar.Activated)
			{
				VScrollbar.Height = LowerFieldHeight;
				VScrollbar.X = Width - VScrollbar.Width;
				VScrollbar.Y = UpperFieldHeight;

				VScrollbar.MaxValue = textAreaHeight - selectorHeight;
			}

			//Check up-icon mousepress
			if (IsUnderMouse && MouseInput.IsPressed(MouseButtons.Left) && IsMouseInRect(
				new Rectangle(RealX + IconLeft.Width, RealY + IconTop.Height, IconTop.Width - IconLeft.Width - IconRight.Width, UpperFieldHeight - IconTop.Height - IconBottom.Height)))
			{
				if (MouseInput.IsClicked(MouseButtons.Left))
				{
					GoUp();
				}

				UpIsPressed = true;
			}
			else
				UpIsPressed = false;

			UpdateSelectedElements();

			//TextField
			if (HasFocus)
			{
				inputMachine.Update(gameTime);
			}
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			ViewRect orig = render.GetViewRect();

			render.Begin();

			ISprite iconUpSignInUse = UpIsPressed ? IconUpSignPressed : IconUpSign;
			ISprite iconInsideInUse = UpIsPressed ? IconInsidePressed : IconInside;

			//	/-------\
			//  |  /|\  |
			//  | / | \ |
			//  |   |   |
			//  \-------/
			render.DrawSprite(IconTop, new Vector2(x, y), Color.White);
			render.DrawSprite(IconLeft, new Rectangle(x, y + IconTop.Height, IconLeft.Width, UpperFieldHeight - IconTop.Height - IconBottom.Height), Color.White);
			render.DrawSprite(IconRight, new Rectangle(x + IconTop.Width - IconRight.Width, y + IconTop.Height, IconRight.Width, UpperFieldHeight - IconTop.Height - IconBottom.Height), Color.White);
			render.DrawSprite(IconBottom, new Vector2(x, y + UpperFieldHeight - IconBottom.Height), Color.White);
			render.DrawSprite(iconInsideInUse, new Rectangle(x + IconLeft.Width, y + IconTop.Height, IconTop.Width - IconLeft.Width - IconRight.Width, UpperFieldHeight - IconTop.Height - IconBottom.Height), Color.White);
			render.DrawSprite(IconUpSign, new Vector2(x + IconLeft.Width + ((IconTop.Width - IconLeft.Width - IconRight.Width) / 2) - (IconUpSign.Width / 2), y + (UpperFieldHeight / 2) - (IconUpSign.Height / 2)), Color.White);

			//UPPER FIELD:
			//Top
			render.DrawSprite(TopBorder, new Rectangle(x + IconTop.Width, y, Width - IconTop.Width - TopRightCorner.Width, TopBorder.Height), Color.White);
			//TopRight
			render.DrawSprite(TopRightCorner, new Vector2(x + Width - TopRightCorner.Width, y), Color.White);
			//Right
			render.DrawSprite(RightBorder, new Rectangle(x + Width - RightBorder.Width, y + TopRightCorner.Height, RightBorder.Width, UpperFieldHeight - TopRightCorner.Height - UpperFieldBottomRightCorner.Height), Color.White);
			//BottomRight
			render.DrawSprite(UpperFieldBottomRightCorner, new Vector2(x + Width - UpperFieldBottomRightCorner.Width, y + UpperFieldHeight - UpperFieldBottomRightCorner.Height), Color.White);
			//Bottom
			render.DrawSprite(BottomBorder, new Rectangle(x + IconTop.Width, y + UpperFieldHeight - BottomBorder.Height, Width - IconTop.Width - TopRightCorner.Width, BottomBorder.Height), Color.White);
			//Inside
			render.DrawSprite(Inside, new Rectangle(x + IconTop.Width, y + TopBorder.Height, Width - IconTop.Width - RightBorder.Width, UpperFieldHeight - TopBorder.Height - BottomBorder.Height), Color.White);

			//LOWER FIELD:
			//Middle
			render.DrawSprite(MiddleBorder, new Rectangle(x + LeftBorder.Width, y + UpperFieldHeight, LowerFieldWidth - LeftBorder.Width - RightBorder.Width, MiddleBorder.Height), Color.White);
			//Right
			render.DrawSprite(RightBorder, new Rectangle(x + LowerFieldWidth - RightBorder.Width, y + UpperFieldHeight, RightBorder.Width, LowerFieldHeight - BottomRightCorner.Height), Color.White);
			//BottomRight
			render.DrawSprite(BottomRightCorner, new Vector2(x + LowerFieldWidth - BottomRightCorner.Width, y + UpperFieldHeight + LowerFieldHeight - BottomRightCorner.Height), Color.White);
			//Bottom
			render.DrawSprite(BottomBorder, new Rectangle(x + BottomLeftCorner.Width, y + UpperFieldHeight + LowerFieldHeight - BottomBorder.Height, LowerFieldWidth - BottomLeftCorner.Width - BottomRightCorner.Width, BottomBorder.Height), Color.White);
			//BottomLeft
			render.DrawSprite(BottomLeftCorner, new Vector2(x, y + UpperFieldHeight + LowerFieldHeight - BottomLeftCorner.Height), Color.White);
			//Left
			render.DrawSprite(LeftBorder, new Rectangle(x, y + UpperFieldHeight, LeftBorder.Width, LowerFieldHeight - BottomLeftCorner.Height), Color.White);
			//Inside
			render.DrawSprite(Inside, new Rectangle(x + LeftBorder.Width, y + UpperFieldHeight + MiddleBorder.Height, LowerFieldWidth - LeftBorder.Width - RightBorder.Width, LowerFieldHeight - MiddleBorder.Height - BottomBorder.Height), Color.White);

			render.End();

			render.SetViewRect(orig.AddGet(new ViewRect(RealX + IconTop.Width + TextMargin, RealY + TopBorder.Height + TextMargin, Width - IconTop.Width - RightBorder.Width - TextMargin * 2, UpperFieldHeight - TopBorder.Height - MiddleBorder.Height - TextMargin * 2)));
			render.Begin();

			string textToDraw = Text;
			if (HasFocus)
				textToDraw = Text.Insert(CursorPosition, "|");
			Font.DrawString(textToDraw, new Point(RealX + IconTop.Width + TextMargin, RealY + TopBorder.Height + TextMargin), FontColor, render);
			
			render.End();

			render.SetViewRect(orig.AddGet(new ViewRect(selectorX, selectorY, selectorWidth, selectorHeight)));
			render.Begin();

			int drawX = selectorX + textAreaOriginX;
			int drawY = selectorY + textAreaOriginY;

			//DrawSprite filedir
			Font.DrawString(CurrentDirectory.FullName, new Point(drawX, drawY), FontColor, render);
			drawY += TextMargin + Font.CharHeight;
			render.DrawLine(new Vector2(drawX, drawY), new Vector2(selectorX + selectorWidth, drawY), SeparatorColor);
			drawY += 1 + TextMargin;

			foreach (DirectoryInfo d in directoryList)
			{
				if (selectedDirectories.Contains(d))
					render.DrawRect(new Rectangle(selectorX, drawY, selectorWidth, Font.CharHeight + Font.VerticalSpace), SelectedColor);

				render.DrawSprite(FolderSign, new Vector2(drawX, drawY), Color.White);

				Font.DrawString(d.Name, new Point(drawX + FolderSign.Width + TextMargin, drawY), FontColor, render);
				drawY += Font.CharHeight + Font.VerticalSpace;
			}

			foreach (FileInfo f in fileList)
			{
				if (selectedFiles.Contains(f))
					render.DrawRect(new Rectangle(selectorX, drawY, selectorWidth, Font.CharHeight + Font.VerticalSpace), SelectedColor);

				Font.DrawString(f.Name, new Point(drawX, drawY), FontColor, render);
				drawY += Font.CharHeight + Font.VerticalSpace;
			}

			render.End();
		}

		public void GoToDirectory(DirectoryInfo directory)
		{
			GoToDirectory(directory.FullName);
		}
		public void GoToDirectory(string directory)
		{
			DirectoryInfo oldDir = CurrentDirectory;

			CurrentDirectory = new DirectoryInfo(directory);
			if (!LoadCurrentDirectory())
			{
				CurrentDirectory = oldDir;
				LoadCurrentDirectory();
			}
		}
		bool LoadCurrentDirectory()
		{
			fileList.Clear();
			directoryList.Clear();

			try
			{
				foreach (DirectoryInfo d in CurrentDirectory.GetDirectories())
				{
					directoryList.Add(d);
				}
			}
			catch (UnauthorizedAccessException e)
			{
				Debug.AddLine(e);
				return false;
			}

			try
			{
				foreach (FileInfo f in CurrentDirectory.GetFiles())
				{
					fileList.Add(f);
				}
			}
			catch (UnauthorizedAccessException e)
			{
				Debug.AddLine(e);
				return false;
			}

			selectedFiles.Clear();
			selectedDirectories.Clear();

			LoadSize();

			return true;
		}
		public void GoUp()
		{
			if (CurrentDirectory.Parent == null)
				return;

			DirectoryInfo oldDir = CurrentDirectory.Parent;
			CurrentDirectory = CurrentDirectory.Parent;
			if (!LoadCurrentDirectory())
			{
				CurrentDirectory = oldDir;
				LoadCurrentDirectory();
			}
		}

		void LoadSize()
		{
			textAreaWidth = Font.MeasureString(CurrentDirectory.FullName).X;
			textAreaHeight = Font.CharHeight;
			textAreaHeight += TextMargin * 2 + 1;

			foreach (DirectoryInfo d in CurrentDirectory.GetDirectories())
			{
				Point size = Font.MeasureString(d.Name);
				if (size.X > textAreaWidth)
					textAreaWidth = size.X;

				textAreaHeight += Font.CharHeight + Font.VerticalSpace;
			}

			foreach (FileInfo f in fileList)
			{
				Point size = Font.MeasureString(f.Name);
				if (size.X + FolderSign.Width + TextMargin > textAreaWidth)
					textAreaWidth = size.X + FolderSign.Width + TextMargin;

				textAreaHeight += Font.CharHeight + Font.VerticalSpace;
			}

			HScrollbar.Activated = textAreaWidth > selectorWidth;
			VScrollbar.Activated = textAreaHeight > selectorHeight;
			HScrollbar.Value = 0;
			VScrollbar.Value = 0;
		}

		void UpdateSelectedElements()
		{
			if (!HasFocus)
				return;

			if (!MouseInput.IsPressed(MouseButtons.Left))
				return;

			if (!IsMouseInRect(new Rectangle(selectorX, selectorY, selectorWidth, selectorHeight)))
				return;

			int selectedIndex = (int)Math.Round((float)(MouseInput.Y - selectorY - TextMargin * 2 - 1 - Font.CharHeight - textAreaOriginY) / (Font.CharHeight + Font.VerticalSpace), 0);

			bool multipleSelectedKeyIsPressed = false;
			foreach (Keys key in MultipleSelectKeys)
				if (KeyboardInput.IsPressed(key))
					multipleSelectedKeyIsPressed = true;

			if (selectedIndex >= 0 && selectedIndex < directoryList.Count)
			{
				if (!multipleSelectedKeyIsPressed || !CanSelectSeveralElements)
				{
					selectedDirectories.Clear();
					selectedFiles.Clear();
				}
				selectedDirectories.Add(directoryList[selectedIndex]);

				if (!MouseInput.IsDoubleClicked(MouseButtons.Left).IsNegative)
					GoToDirectory(directoryList[selectedIndex].FullName);
			}
			else if (selectedIndex >= directoryList.Count && (selectedIndex - directoryList.Count) < fileList.Count)
			{
				if (!multipleSelectedKeyIsPressed || !CanSelectSeveralElements)
				{
					selectedDirectories.Clear();
					selectedFiles.Clear();
					Text = fileList[selectedIndex - directoryList.Count].Name;
					inputMachine.Cursor = Text.Length;
				}
				selectedFiles.Add(fileList[selectedIndex - directoryList.Count]);
			}
		}
	}
}
