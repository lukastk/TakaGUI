using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TakaGUI.Cores
{
	public static class StateManager
	{
		static readonly List<State> States = new List<State>();

		static State currentState;

		public static void Push(State state, GameTime gameTime)
		{
			currentState = state;

			States.Add(state);
			state.Update(gameTime, StatePurpose.Initiate);
		}

		public static void Pop(GameTime gameTime)
		{
			currentState.Update(gameTime, StatePurpose.Stop);
			States.RemoveAt(States.Count - 1);

			if (States.Count != 0)
				currentState = States.Last();
			else
				currentState = null;
		}

		public static void PopAll(GameTime gameTime)
		{
			for (int i = 0; i < States.Count; i++)
				Pop(gameTime);
		}

		public static void Update(GameTime gameTime)
		{
			currentState.Update(gameTime, StatePurpose.Work);
		}
	}

	public class State
	{
		bool stopThisState = false;
		protected GameTime gameTime;

		public virtual void Update(GameTime _gameTime, StatePurpose purpose)
		{
			gameTime = _gameTime;

			if (stopThisState)
			{
				StateManager.Pop(gameTime);
				return;
			}

			switch (purpose)
			{
				case StatePurpose.Initiate:
					Initialize();
					break;
				case StatePurpose.Stop:
					Stop();
					break;
				case StatePurpose.Work:
					Work();
					break;
			}
		}

		public virtual void Initialize()
		{
		}

		public virtual void Stop()
		{
		}

		public virtual void Work()
		{
		}

		public void StopThisState()
		{
			stopThisState = true;
		}
	}

	public enum StatePurpose
	{
		Initiate, Stop, Work
	}
}
