using System;
using System.Linq;
using Microsoft.Xna.Framework;
using TakaGUI.Data;
using TakaGUI.Machines;
using TakaGUI.Services;

namespace TakaGUI.DrawBoxes
{
	public class DoubleField : DrawBox
	{
		public event DoubleChangedEvent ValueChanged;

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
		}
		double _Value;
		public double Value
		{
			get { return _Value; }
			set
			{
				double oldValue = _Value;
				_Value = value;

				if (_Value < _MinValue)
					_Value = _MinValue;
				else if (_Value > _MaxValue)
					_Value = _MaxValue;
				
				if (oldValue != _Value && ValueChanged != null)
					ValueChanged(this, oldValue, _Value);

				inputMachine.Text = Value.ToString().Replace(',', '.');
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

		public const string AllowedChars = ".-0123456789";

		double _MinValue = double.MinValue;
		public double MinValue
		{
			get { return _MinValue; }
			set
			{
				_MinValue = value;

				if (_MinValue > MaxValue)
					_MinValue = MaxValue;

				Value = Value;
			}
		}
		double _MaxValue = double.MaxValue;
		public double MaxValue
		{
			get { return _MinValue; }
			set
			{
				_MaxValue = value;

				if (_MaxValue < MinValue)
					_MaxValue = MinValue;

				Value = Value;
			}
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

		public DoubleField()
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

			base.BaseInitialize();
		}

		bool inputMachine_TextAdded(object sender, int position, string input)
		{
			foreach (char c in input)
				if (!AllowedChars.Contains(c))
					return false;

			return true;
		}

		public override void Idle(GameTime gameTime)
		{
			base.Idle(gameTime);

			if (!HasFocus && Text == "")
				inputMachine.Text = Value.ToString().Replace(',', '.');

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
			if (HasFocus)
				textToDraw = Text.Insert(CursorPosition, "|");

			Font.DrawString(textToDraw, new Point(x + LeftBorder.Width + textMargin, y + base.Height / 2 - Font.CharHeight / 2), FontColor, render);

			render.End();
		}
		
		void TextField_FocusChanged(object sender, bool newValue)
		{
			if (!newValue)
			{
				inputMachine.EndCurrentInput();

				if (inputMachine.Text.Contains('.'))
				{
					int firstIndex = inputMachine.Text.IndexOf('.');
					inputMachine.Text = inputMachine.Text.Replace(".", "").Insert(firstIndex, ".");
				}

				if (inputMachine.Text.Contains('-'))
					inputMachine.Text = "-" + inputMachine.Text.Replace("-", "");

				if (inputMachine.Text.Length != 0)
				{
					try
					{
						Value = Convert.ToDouble(inputMachine.Text);
					}
					catch (OverflowException)
					{
						Value = 0;
					}
					catch (FormatException)
					{
						try
						{
							Value = Convert.ToDouble(inputMachine.Text.Replace('.', ','));
						}
						catch (OverflowException)
						{
							Value = 0;
						}
					}
				}
				else
					Value = 0;
			}

			inputMachine.Text = Value.ToString().Replace(',', '.');
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
