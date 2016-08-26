using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TakaGUI.Cores;

namespace TakaGUI.Services
{
	public class KeyboardInput : GameComponent, IKeyboardInput
	{
		public List<IPressedKey> PressedKeys
		{
			get;
			private set;
		}
		public List<Keys> ReleasedKeys
		{
			get;
			private set;
		}
		public List<Keys> ClickedKeys
		{
			get;
			private set;
		}

		public KeyboardInput(Game game)
			: base(game)
		{
			game.Components.Add(this);

			PressedKeys = new List<IPressedKey>();
			ReleasedKeys = new List<Keys>();
			ClickedKeys = new List<Keys>();

			EnabledChanged += new EventHandler<EventArgs>(KeyboardInput_EnabledChanged);
		}

		void KeyboardInput_EnabledChanged(object sender, EventArgs e)
		{
			if (!Enabled)
				Stop();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			ReleasedKeys.Clear();
			ClickedKeys.Clear();

			KeyboardState state = Keyboard.GetState();

			foreach (IPressedKey pressedKey in new List<IPressedKey>(PressedKeys))
			{
				bool isPressed = false;
				foreach (Keys key in state.GetPressedKeys())
					if (key == pressedKey.Key)
						isPressed = true;

				if (!isPressed)
				{
					PressedKeys.Remove(pressedKey);
					ReleasedKeys.Add(pressedKey.Key);
				}
			}

			foreach (Keys key in state.GetPressedKeys())
			{
				bool isAlreadyPressed = false;

				foreach (IPressedKey pressedKey in PressedKeys)
					if (key == pressedKey.Key)
						isAlreadyPressed = true;

				if (!isAlreadyPressed)
				{
					PressedKeys.Add(new PressedKey(key, gameTime.TotalGameTime.TotalSeconds));
					ClickedKeys.Add(key);
				}
			}
		}

		public void Stop()
		{
			PressedKeys.Clear();
			ReleasedKeys.Clear();
			ClickedKeys.Clear();
		}

		public bool IsClicked(Keys key)
		{
			return ClickedKeys.Contains(key);
		}

		public bool HasReleased(Keys key)
		{
			return ReleasedKeys.Contains(key);
		}

		public bool IsPressed(Keys key)
		{
			foreach (PressedKey pressedKey in PressedKeys)
			{
				if (key == pressedKey.Key)
					return true;
			}

			return false;
		}
	}

	public interface IKeyboardInput
	{
		List<IPressedKey> PressedKeys { get; }
		List<Keys> ReleasedKeys { get; }
		List<Keys> ClickedKeys { get; }

		bool IsClicked(Keys key);
		bool HasReleased(Keys key);
		bool IsPressed(Keys key);

		bool Enabled { get; set; }
	}

	public class PressedKey : IPressedKey
	{
		public Keys Key { get; private set; }
		public double TimeClicked { get; private set; }

		public PressedKey(Keys key, double timeClicked)
		{
			Key = key;
			TimeClicked = timeClicked;
		}
	}

	public interface IPressedKey
	{
		Keys Key { get; }
		double TimeClicked { get; }
	}
}
