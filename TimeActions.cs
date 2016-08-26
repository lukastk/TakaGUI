using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TakaGUI
{
	public class TimeActions : GameComponent
	{
		static TimeActions instance;

		static List<TimeAction> actions = new List<TimeAction>();
		static GameTime currentGameTime;

		TimeActions(Game game)
			: base(game)
		{
		}

		public static void Initialize(Game game)
		{
			if (instance == null)
				instance = new TimeActions(game);
			else
				throw new Exception("Can only initialize TimeActions once.");
		}

		public static void Add(Action<GameTime> action, TimeSpan timeSpan)
		{
			var timeAction = new TimeAction();
			timeAction.Action = action;
			timeAction.StartTime = currentGameTime;
			timeAction.TimeUntil = timeSpan;

			actions.Add(timeAction);
		}
		public static void Add(Action<GameTime> action, float time, TimeUnits timeUnit = TimeUnits.Seconds)
		{
			int miliseconds = 0;

			switch (timeUnit)
			{
				case TimeUnits.Miliseconds:
					miliseconds = (int)Math.Round(time, 0);
					break;
				case TimeUnits.Seconds:
					miliseconds = (int)Math.Round(time / 1000, 0);
					break;
				case TimeUnits.Minutes:
					miliseconds = (int)Math.Round(time / (60 * 1000), 0);
					break;
				case TimeUnits.Hour:
					miliseconds = (int)Math.Round(time / (60 * 60 * 1000), 0);
					break;
				case TimeUnits.Day:
					miliseconds = (int)Math.Round(time / (60 * 60 * 24 * 1000), 0);
					break;
			}

			var timeAction = new TimeAction();
			timeAction.Action = action;
			timeAction.StartTime = currentGameTime;
			timeAction.TimeUntil = new TimeSpan(0, 0, 0, 0, miliseconds);

			actions.Add(timeAction);
		}

		public static void Add(Func<object[], Action<GameTime>> template, TimeSpan timeSpan, params object[] parameters)
		{
			var timeAction = new TimeAction();
			timeAction.Action = template(parameters);
			timeAction.StartTime = currentGameTime;
			timeAction.TimeUntil = timeSpan;

			actions.Add(timeAction);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			currentGameTime = gameTime;

			foreach (var timeAction in actions)
			{
				if (timeAction.StartTime.TotalGameTime.Add(timeAction.TimeUntil).CompareTo(gameTime.TotalGameTime) >= 0)
					timeAction.Action(gameTime);
			}
		}

		class TimeAction
		{
			public GameTime StartTime;
			public TimeSpan TimeUntil;
			public Action<GameTime> Action;
		}
	}

	public enum TimeUnits
	{
		Miliseconds,
		Seconds,
		Minutes,
		Hour,
		Day
	}
}
