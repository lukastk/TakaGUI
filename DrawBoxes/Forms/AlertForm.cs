﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;

namespace TakaGUI.DrawBoxes.Forms
{
	public class AlertForm : Dialogue
	{
		private AlertForm()
		{
		}

		string text;
		public static AlertForm ShowDialogue(Window window, string title, string _text, CloseEvent closeFunction = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			var alertForm = new AlertForm();
			alertForm.Initialize(closeFunction, title, resizable, isDialog, category, file);

			alertForm.text = _text;
			alertForm.Show(window);

			return alertForm;
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

			ResizableButton okButton = new ResizableButton();
			okButton.Initialize();
			okButton.Title = "OK";
			okButton.FitToText();
			AddDrawBox(okButton);
			Push.ToTheBottomSideOf(okButton, label, 3, Push.VerticalAlign.Center);

			okButton.Click += okButton_Click;

			Wrap();

			UpdateSize();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void okButton_Click(object sender)
		{
			Close();
		}
	}
}
