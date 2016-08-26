using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Data;

namespace TakaGUI.DrawBoxes.Forms
{
	public class Dialogue : Form
	{
		public override void Initialize(string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}
		public virtual void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			if (category == null)
				category = DefaultCategory;
			if (file == null)
				file = DefaultSkinFile;

			base.Initialize(category, file);

			CanResizeFormHorizontally = resizable;
			CanResizeFormVertically = resizable;

			Title = title;
			if (title == null)
				Header = false;

			if (closeFunction != null)
				IsClosing += closeFunction;
		}

		public void Show(Window window)
		{
			window.AddDrawBox(this);
			Parent.PutDialogOnStack(this);
		}
	}
}
