using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.Cores;
using Microsoft.Xna.Framework;

namespace TakaGUI.Services
{
	public class GUIManager : DrawableGameComponent, IGUIManager
	{
		//Services
		IMouseInput mouseInput;
		ICursorManager cursorManager;
		IGraphicsManager graphicsManager;

		public List<Window> GameScreens
		{
			get;
			private set;
		}
		/// <summary>
		/// Default value is true.
		/// </summary>
		public bool DrawCursor
		{
			get;
			set;
		}

		public GUIManager(Game game)
			: base(game)
		{
			game.Components.Add(this);

			GameScreens = new List<Window>();

			DrawCursor = true;
		}

		public override void Initialize()
		{
			base.Initialize();

			mouseInput = (IMouseInput)Game.Services.GetService(typeof(IMouseInput));
			cursorManager = (ICursorManager)Game.Services.GetService(typeof(ICursorManager));
			graphicsManager = (IGraphicsManager)Game.Services.GetService(typeof(IGraphicsManager));
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			cursorManager.UpdateCursor(); //This needs to be before the screens updates, otherwise they can't change the cursor-state.

			foreach (Window screen in GameScreens)
				screen.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			foreach (Window screen in GameScreens)
				screen.Draw(gameTime);

			if (DrawCursor)
				cursorManager.DrawCursor(graphicsManager.GetRender(), mouseInput.X, mouseInput.Y);
		}
	}

	public interface IGUIManager
	{
		List<Window> GameScreens { get; }
		bool DrawCursor { get; set; }

		bool Enabled { get; set; }
		bool Visible { get; set; }
	}
}
