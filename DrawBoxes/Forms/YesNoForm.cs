﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;

namespace TakaGUI.DrawBoxes.Forms
{
	public class YesNoForm : Dialogue
	{
		public DialogResult DialogResult = DialogResult.NotFinished;

		private YesNoForm()
		{
		}

		string text;
		public static YesNoForm ShowDialogue(Window window, string title, string _text, CloseEvent closeFunction = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			var yesNoForm = new YesNoForm();
			yesNoForm.Initialize(closeFunction, title, resizable, isDialog, category, file);

			yesNoForm.text = _text;
			yesNoForm.Show(window);

			return yesNoForm;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			if (text == null)
				text = "";

			Label label = new Label(text);
			label.Initialize();
			AddDrawBox(label);
			label.X = 3;
			label.Y = 1;

			ResizableButton yesButton = new ResizableButton();
			yesButton.Initialize();
			yesButton.Title = "Yes";
			yesButton.FitToText();
			AddDrawBox(yesButton);
			Push.ToTheBottomSideOf(yesButton, label, 3, Push.VerticalAlign.Left);
			yesButton.Click += yesButton_Click;

			ResizableButton noButton = new ResizableButton();
			noButton.Initialize();
			noButton.Title = "No";
			noButton.FitToText();
			AddDrawBox(noButton);
			Push.ToTheRightSideOf(noButton, yesButton, 3, Push.HorizontalAlign.Top);
			noButton.Click += noButton_Click;

			Wrap();

			UpdateSize();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void yesButton_Click(object sender)
		{
			DialogResult = Forms.DialogResult.Yes;
			Close();
		}

		void noButton_Click(object sender)
		{
			DialogResult = Forms.DialogResult.No;
			Close();
		}
	}
}
