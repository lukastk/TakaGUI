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
	public class TextField : DrawBox
	{
		public event StringChangedEvent TextChanged;

		#region Textures
		public static string DefaultCategory = "TextField";

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

		TextInputMachine inputMachine;
		public string Text
		{
			get { return inputMachine.Text; }
			set
			{
				inputMachine.Text = value;
			}
		}
		public int CursorPosition
		{
			get { return inputMachine.Cursor; }
			set { inputMachine.Cursor = value; }
		}
		public MonoFont Font
		{
			get
			{
				if (inputMachine == null)
					return null;

				return inputMachine.Font;
			}
			set
			{
				if (inputMachine == null)
					return;

				inputMachine.Font = value;
			}
		}
		public Color FontColor = Color.White;
		public bool MaxCharsIsWidth = true;

		public bool CanChangeValue
		{
			get { return inputMachine.Enabled; }
			set { inputMachine.Enabled = value; }
		}

		const int edgeSize = 1;

		const int textMargin = 2;
		public override int MinWidth { get { return edgeSize * 2; } }
		public override int MinHeight
		{
			get
			{
				int charHeight = Font != null ? Font.CharHeight : 0;

				return charHeight + textMargin * 2;
			}
		}
		public override int MaxHeight
		{
			get { return MinHeight; }
		}

		public bool PasswordMode = false;
		public string PasswordChar = "*";

		public TextField()
		{
			FocusChanged += new BooleanChangedEvent(TextField_FocusChanged);
			DrawBoxHasFocus += new DefaultEvent(TextField_DrawBoxHasFocus);
		}

		public void SetWidthInCharacters(int characterAmount)
		{
			Width = characterAmount * (Font.CharWidth + Font.HorizontalSpace) - Font.HorizontalSpace;
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

			Inside = GetTexture(file, category, "Inside");

			inputMachine = new TextInputMachine(KeyboardInput, GetMonoFont(file, category, "Font"));

			inputMachine.TextAdded += new TextInputMachine.TextAddedEvent(inputMachine_TextAdded);
			inputMachine.TextRemoved += new TextInputMachine.TextRemovedEvent(inputMachine_TextRemoved);

			base.BaseInitialize();
		}

		bool inputMachine_TextAdded(object sender, int position, string input)
		{
			if (TextChanged != null)
				TextChanged(this, Text, Text.Insert(position, input));

			return true;
		}

		bool inputMachine_TextRemoved(object sender, int position)
		{
			if (TextChanged != null)
				TextChanged(this, Text, Text.Remove(position));

			return true;
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (MaxCharsIsWidth)
			{
				inputMachine.MaxChars = Width / (Font.CharWidth + Font.HorizontalSpace);
			}

			if (HasFocus)
			{
				inputMachine.Update(gameTime);
			}
		}

		public override void Project(GameTime gameTime, int x, int y, IRender render)
		{
			render.Begin();

			render.DrawBody(new Rectangle(x, y, Width, Height), Color.White,
				TopBorder,
				TopRightCorner,
				RightBorder,
				BottomRightCorner,
				BottomBorder,
				BottomLeftCorner,
				LeftBorder,
				TopLeftCorner,
				Inside);

			string textToDraw = Text;

			if (PasswordMode)
			{
				int l = textToDraw.Length;
				textToDraw = "";
				for (int i = 0; i < l; i++)
					textToDraw += PasswordChar;
			}
			
			if (HasFocus)
				textToDraw = textToDraw.Insert(CursorPosition, "|");

			Font.DrawString(textToDraw, new Point(x + LeftBorder.Width + textMargin, y + base.Height / 2 - Font.CharHeight / 2), FontColor, render);

			render.End();
		}
		
		void TextField_FocusChanged(object sender, bool newValue)
		{
			if (!newValue)
			{
				inputMachine.EndCurrentInput();
			}
		}
		void TextField_DrawBoxHasFocus(object sender)
		{
			if (MouseInput.IsClicked(MouseButtons.Left))
			{
				CursorPosition = (MouseInput.X - RealX) / (Font.CharWidth + Font.HorizontalSpace);
			}
		}
	}
}
