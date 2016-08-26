using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TakaGUI
{
	public delegate void TimerEvent(int ticks);

	/// <summary>
	/// Summary:
	///		Calls its event Tick every x miliseconds.
	/// </summary>
	public class Timer
	{
		public event TimerEvent Tick;

		/// <summary>
		/// The miliseconds between every tick.
		/// </summary>
		public readonly double TickMiliseconds;
		/// <summary>
		/// The amount of ticks since Start() has been called.
		/// </summary>
		public int Ticks
		{
			get;
			private set;
		}

		/// <summary>
		/// The time until the next tick.
		/// </summary>
		double nextTick;

		/// <summary>
		/// If true, the timer has started to tick.
		/// </summary>
		public bool Started
		{
			get;
			private set;
		}
		/// <summary>
		/// If true, the Tick event won't be called immedietly after starting the timer.
		/// </summary>
		public bool SkipFirstTick;

		public enum TimeUnits { Milisecond, Second, Minute, Hour, Day }
		public Timer(double time, TimeUnits timeUnit)
		{
			switch (timeUnit)
			{
				case TimeUnits.Milisecond:
					TickMiliseconds = time;
					break;
				case TimeUnits.Second:
					TickMiliseconds = time * 1000;
					break;
				case TimeUnits.Minute:
					TickMiliseconds = time * 1000 * 60;
					break;
				case TimeUnits.Hour:
					TickMiliseconds = time * 1000 * 60 * 60;
					break;
				case TimeUnits.Day:
					TickMiliseconds = time * 1000 * 60 * 60 * 24;
					break;
			}
		}

		/// <summary>
		/// Starts the timer.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Start(GameTime gameTime)
		{
			Started = true;

			if (SkipFirstTick)
				nextTick = gameTime.TotalGameTime.TotalMilliseconds + TickMiliseconds;
		}

		/// <summary>
		/// Stops the timer without reseting the tick count.
		/// </summary>
		public void Stop()
		{
			nextTick = 0;
			Started = false;
		}

		/// <summary>
		/// Stops the timer and resets the tick count.
		/// </summary>
		public void End()
		{
			Ticks = 0;
			Stop();
		}

		/// <summary>
		/// Has to be called every update loop for it to function properly.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{
			if (Started && nextTick <= Math.Round(gameTime.TotalGameTime.TotalMilliseconds, 0))
			{
				Ticks += 1;

				if (Tick != null)
					Tick(Ticks);

				nextTick = (int)Math.Round(gameTime.TotalGameTime.TotalMilliseconds + TickMiliseconds, 0);
			}
		}
	}
}
