using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using TakaGUI.Data;

namespace TakaGUI.DrawBoxes.Forms
{
	public class FileForm : Dialogue
	{
		public DialogResult Result = DialogResult.NotFinished;

		public ReadOnlyCollection<FileInfo> SelectedFiles
		{
			get { return selector.SelectedFiles; }
		}
		public ReadOnlyCollection<DirectoryInfo> SelectedDirectories
		{
			get { return selector.SelectedDirectories; }
		}
		public FileInfo SelectedFile
		{
			get
			{
				if (SelectedFiles.Count != 0)
					return SelectedFiles[0];

				return null;
			}
		}
		public DirectoryInfo SelectedFolder
		{
			get
			{
				if (SelectedDirectories.Count != 0)
					return SelectedDirectories[0];

				return null;
			}
		}

		public string SaveDirectory
		{
			get
			{
				if (selector.Text == "" || selector.Text == null)
					return null;

				return Path.Combine(selector.CurrentDirectory.FullName, selector.Text);
			}
		}

		FileSelector selector;
		ResizableButton okButton;
		ResizableButton cancelButton;

		public FileFormTypes FileFormType { get; private set; }
		public OperationTypes OperationType { get; private set; }

		public enum FileFormTypes { File, Folder }
		public enum OperationTypes { Open, Save, Select }

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = true, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}
		public void Initialize(FileFormTypes fileFormType, OperationTypes operationType, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile _file = null)
		{
			FileFormType = fileFormType;
			OperationType = operationType;

			if (title == null)
				switch (fileFormType)
				{
					case FileFormTypes.File:
						switch (OperationType)
						{
							case OperationTypes.Open:
								title = "Open File";
								break;
							case OperationTypes.Save:
								title = "Save File";
								break;
							case OperationTypes.Select:
								title = "Select File";
								break;
						}
						break;
					case FileFormTypes.Folder:
						switch (OperationType)
						{
							case OperationTypes.Open:
								title = "Open Folder";
								break;
							case OperationTypes.Save:
								title = "Save Folder";
								break;
							case OperationTypes.Select:
								title = "Select Folder";
								break;
						}
						break;
				}


			if (_file == null)
				_file = DefaultSkinFile;
			file = _file;

			base.Initialize(closeFunction, title, resizable, isDialog, category, _file);
		}

		ISkinFile file;
		public override void AddedToContainer()
		{
			const int buttonMinSize = 50;

			selector = new FileSelector();
			selector.Initialize(null, file);
			AddDrawBox(selector);
			selector.Width = 200;
			selector.Height = 100;

			cancelButton = new ResizableButton();
			cancelButton.Initialize(null, file);
			AddDrawBox(cancelButton);
			cancelButton.Title = "Cancel";
			cancelButton.FitToText();
			cancelButton.Width = Math.Max(cancelButton.Width, buttonMinSize);
			cancelButton.X = 0;
			cancelButton.Y = selector.Height + 3;

			okButton = new ResizableButton();
			okButton.Initialize(null, file);
			AddDrawBox(okButton);
			switch (OperationType)
			{
				case OperationTypes.Open:
					okButton.Title = "Open";
					break;
				case OperationTypes.Save:
					okButton.Title = "Save";
					break;
				case OperationTypes.Select:
					okButton.Title = "Select";
					break;
			}
			okButton.FitToText();
			okButton.Width = Math.Max(okButton.Width, buttonMinSize);
			okButton.X = selector.Width - cancelButton.Width;
			okButton.Y = selector.Height + 3;

			Wrap();
			
			selector.Alignment = DrawBoxAlignment.GetFull();
			okButton.Alignment = DrawBoxAlignment.GetRightBottom();
			cancelButton.Alignment = DrawBoxAlignment.GetLeftBottom();

			okButton.Click += new DefaultEvent(okButton_Click);
			cancelButton.Click += new DefaultEvent(cancelButton_Click);
		}

		public static FileForm ShowDialogue(Window window, FileFormTypes fileFormType, OperationTypes operationType, CloseEvent closeFunction = null, string title = null, bool resizable = true, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			var openFileForm = new FileForm();
			openFileForm.Initialize(fileFormType, operationType, closeFunction, title, resizable, isDialog, category, file);
			openFileForm.Show(window);

			openFileForm.UpdateSize();

			openFileForm.X = (openFileForm.Parent.Width / 2) - (openFileForm.Width / 2);
			openFileForm.Y = (openFileForm.Parent.Height / 2) - (openFileForm.Height / 2);

			return openFileForm;
		}

		public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			base.Update(gameTime);
		}

		void okButton_Click(object sender)
		{
			Result = DialogResult.OK;
			Close();
		}
		void cancelButton_Click(object sender)
		{
			Result = DialogResult.Cancel;
			Close();
		}
	}
}
