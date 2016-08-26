using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TakaGUI.DrawBoxes.Forms
{
	public class FieldBuilder
	{
		List<Field> fieldHistory = new List<Field>();

		List<DrawBox> lastField
		{
			get { return fieldHistory.Last().DrawBoxes; }
		}
		Dictionary<DrawBox, DrawBoxAlignment> alignments = new Dictionary<DrawBox, DrawBoxAlignment>();
		SingleSlotBox container;

		public int FieldWidth;
		public int VerticalMargin;
		public int HorizontalMargin;

		public bool AlignTop = false;
		public bool AlignBottom = false;

		int extraVerticalMargin
		{
			get { return fieldHistory.Last().ExtraVerticalMargin; }
			set { fieldHistory.Last().ExtraVerticalMargin = value; }
		}

		void AddNewFieldList()
		{
			fieldHistory.Add(new Field());
		}
		public void MoveUpOneField()
		{
			if (fieldHistory.Count != 0)
				fieldHistory.Remove(fieldHistory.Last());
		}

		public void BuildSessionStart(SingleSlotBox _container)
		{
			container = _container;

			FieldWidth = 300;
			VerticalMargin = 5;
			HorizontalMargin = 3;

			container.Width = FieldWidth;

			AddNewFieldList();
		}
		public void BuildSessionEnd()
		{
			container.Wrap();

			container = null;
			foreach (var pair in alignments)
			{
				pair.Key.Alignment = pair.Value;
			}

			fieldHistory.Clear();
			alignments.Clear();
		}

		public void AddDrawBoxAsField(DrawBox drawBox, DrawBoxAlignment drawBoxAlignment)
		{
			container.AddDrawBox(drawBox);

			if (lastField.Count != 0)
				drawBox.Y = Push.GetBottomSide(lastField.ToArray()) + VerticalMargin + extraVerticalMargin;

			alignments.Add(drawBox, drawBoxAlignment);

			AddNewFieldList();
			lastField.Add(drawBox);
		}

		public void AddVerticalMargin(int extraMargin)
		{
			extraVerticalMargin += extraMargin;
		}
		public void RemoveAllExtraVerticalMargin()
		{
			extraVerticalMargin = 0;
		}

		Label AddLabel(string labelText)
		{
			var label = new Label();
			label.Initialize();
			container.AddDrawBox(label);
			label.Text = labelText;

			if (lastField.Count != 0)
				label.Y = Push.GetBottomSide(lastField.ToArray()) + VerticalMargin + extraVerticalMargin;
			extraVerticalMargin = 0;

			bool up = true;
			bool down = false;
			if (!AlignTop && AlignBottom)
			{
				up = false;
				down = true;
			}

			alignments.Add(label, new DrawBoxAlignment(up, down, true, false));

			return label;
		}

		public Label AddLabelField(string labelText)
		{
			var label = AddLabel(labelText);

			AddNewFieldList();
			lastField.Add(label);

			return label;
		}

		public TextField AddTextField(string labelText, int textFieldHeight = -1)
		{
			var label = AddLabel(labelText);

			var textField = new TextField();
			textField.Initialize();
			container.AddDrawBox(textField);
			if (textFieldHeight == -1)
				textField.Height = textField.Font.CharHeight + 4;
			else
				textField.Height = textFieldHeight;
			Push.ToTheRightSideOf(textField, label, HorizontalMargin, Push.HorizontalAlign.Top);

			textField.Width = FieldWidth - textField.X;

			AddNewFieldList();
			lastField.Add(label);
			lastField.Add(textField);

			alignments.Add(textField, new DrawBoxAlignment(AlignTop, AlignBottom, true, true));

			return textField;
		}
		public DrawBoxPair<TextField, ResizableButton> AddTextFieldWithButton(string labelText, string buttonTitle, DefaultEvent clickedEvent, int textFieldHeight = -1, int buttonHeight = -1)
		{
			var label = AddLabel(labelText);

			var button = new ResizableButton();
			button.Initialize();
			container.AddDrawBox(button);
			button.Title = buttonTitle;
			button.FitToText();
			if (buttonHeight != -1)
				button.Height = buttonHeight;

			var textField = new TextField();
			textField.Initialize();
			container.AddDrawBox(textField);
			if (textFieldHeight == -1)
				textField.Height = textField.Font.CharHeight + 4;
			else
				textField.Height = textFieldHeight;
			Push.ToTheRightSideOf(textField, label, HorizontalMargin, Push.HorizontalAlign.Top);
			textField.Width = FieldWidth - textField.X - button.Width - HorizontalMargin;

			Push.ToTheRightSideOf(button, textField, HorizontalMargin, Push.HorizontalAlign.Top);

			AddNewFieldList();
			lastField.Add(label);
			lastField.Add(textField);
			lastField.Add(button);

			alignments.Add(textField, new DrawBoxAlignment(AlignTop, AlignBottom, true, true));
			alignments.Add(button, new DrawBoxAlignment(AlignTop, AlignBottom, false, true));

			return new DrawBoxPair<TextField, ResizableButton>(textField, button);
		}
		public DrawBoxPair<TextField, ResizableButton> AddBrowseField(FileForm.FileFormTypes fileFormType, FileForm.OperationTypes operationType, string labelText, string buttonTitle = "Browse")
		{
			var pair = AddTextFieldWithButton(labelText, buttonTitle, null);

			var currentForm = container; //So that if container == null, the delegate can still refer to it.

			pair.DrawBox2.Click += delegate(object sender)
			{
				FileForm.ShowDialogue(currentForm.Parent, fileFormType, operationType, delegate(object _sender)
				{
					var fileForm = (FileForm)_sender;

					if (fileForm.Result == DialogResult.OK)
					{
						if (operationType == FileForm.OperationTypes.Open ||
							operationType == FileForm.OperationTypes.Select)
						{
							switch (fileFormType)
							{
								case FileForm.FileFormTypes.File:
									pair.DrawBox1.Text = fileForm.SelectedFile.FullName;
									break;
								case FileForm.FileFormTypes.Folder:
									pair.DrawBox1.Text = fileForm.SelectedFolder.FullName;
									break;
							}
						}
						else if (operationType == FileForm.OperationTypes.Save)
						{
							pair.DrawBox1.Text = fileForm.SaveDirectory;
						}
					}
				});
			};

			return pair;
		}

		public IntegerField AddIntegerField(string labelText, int integerFieldHeight = -1)
		{
			var label = AddLabel(labelText);

			var integerField = new IntegerField();
			integerField.Initialize();
			container.AddDrawBox(integerField);
			if (integerFieldHeight == -1)
				integerField.Height = integerField.Font.CharHeight + 4;
			else
				integerField.Height = integerFieldHeight;
			Push.ToTheRightSideOf(integerField, label, HorizontalMargin, Push.HorizontalAlign.Top);

			integerField.Width = FieldWidth - integerField.X;

			AddNewFieldList();
			lastField.Add(label);
			lastField.Add(integerField);

			alignments.Add(integerField, new DrawBoxAlignment(AlignTop, AlignBottom, true, true));

			return integerField;
		}
		public DoubleField AddDoubleField(string labelText, int doubleFieldHeight = -1)
		{
			var label = AddLabel(labelText);

			var doubleField = new DoubleField();
			doubleField.Initialize();
			container.AddDrawBox(doubleField);
			if (doubleFieldHeight == -1)
				doubleField.Height = doubleField.Font.CharHeight + 4;
			else
				doubleField.Height = doubleFieldHeight;
			Push.ToTheRightSideOf(doubleField, label, HorizontalMargin, Push.HorizontalAlign.Top);

			doubleField.Width = FieldWidth - doubleField.X;

			AddNewFieldList();
			lastField.Add(label);
			lastField.Add(doubleField);

			alignments.Add(doubleField, new DrawBoxAlignment(AlignTop, AlignBottom, true, true));

			return doubleField;
		}

		public ComboBox AddComboBoxField(string labelText, List<string> items = null)
		{
			var label = AddLabel(labelText);

			var comboBox = new ComboBox();
			comboBox.Initialize();
			container.AddDrawBox(comboBox);
			Push.ToTheRightSideOf(comboBox, label, 3, Push.HorizontalAlign.Top);
			comboBox.Width = FieldWidth - comboBox.X;

			if (items != null)
				comboBox.Items.AddRange(items);

			AddNewFieldList();
			lastField.Add(label);
			lastField.Add(comboBox);

			alignments.Add(comboBox, new DrawBoxAlignment(AlignTop, AlignBottom, true, true));

			return comboBox;
		}

		public enum ResizableButtonOrientation { Left, Right, FillWidth }
		public ResizableButton AddResizableButtonField(string buttonTitle, DefaultEvent clickedEvent = null, ResizableButtonOrientation orientation = ResizableButtonOrientation.Right)
		{
			var button = new ResizableButton();
			button.Initialize();
			if (orientation == ResizableButtonOrientation.Left)
				AddDrawBoxAsField(button, new DrawBoxAlignment(AlignTop, AlignBottom, true, false));
			else if (orientation == ResizableButtonOrientation.Right)
				AddDrawBoxAsField(button, new DrawBoxAlignment(AlignTop, AlignBottom, false, true));
			else if (orientation == ResizableButtonOrientation.FillWidth)
				AddDrawBoxAsField(button, new DrawBoxAlignment(AlignTop, AlignBottom, true, true));

			button.Title = buttonTitle;
			button.FitToText();
			if (orientation == ResizableButtonOrientation.FillWidth)
				button.Width = FieldWidth;

			button.Click += clickedEvent;

			if (orientation != ResizableButtonOrientation.Left)
				button.X = FieldWidth - button.Width;
			else
				button.X = 0;

			return button;
		}

		public CheckBox AddCheckBoxField(string labelText)
		{
			var label = AddLabel(labelText);

			var checkBox = new CheckBox();
			checkBox.Initialize();
			container.AddDrawBox(checkBox);
			Push.ToTheRightSideOf(checkBox, label, 3, Push.HorizontalAlign.Top);

			AddNewFieldList();
			lastField.Add(label);
			lastField.Add(checkBox);

			alignments.Add(checkBox, new DrawBoxAlignment(AlignTop, AlignBottom, true, false));

			return checkBox;
		}

		public ColumnListBox AddColumnListBox(string labelText, int height, int columns)
		{
			return AddColumnListBox(labelText, FieldWidth, height, columns);
		}
		public ColumnListBox AddColumnListBox(string labelText, int width, int height, int columns)
		{
			if (labelText != null)
				AddLabelField(labelText);

			var listBox = new ColumnListBox();
			listBox.Initialize(columns);
			AddDrawBoxAsField(listBox, new DrawBoxAlignment(AlignTop, AlignBottom, true, true));
			listBox.Width = width;
			listBox.Height = height;

			return listBox;
		}

		public struct DrawBoxPair<T1, T2>
			where T1 : DrawBox
			where T2 : DrawBox
		{
			public T1 DrawBox1;
			public T2 DrawBox2;

			public DrawBoxPair(T1 drawBox1, T2 drawBox2)
			{
				DrawBox1 = drawBox1;
				DrawBox2 = drawBox2;
			}
		}

		class Field
		{
			public List<DrawBox> DrawBoxes = new List<DrawBox>();
			public int ExtraVerticalMargin = 0; 
		}
	}
}
