﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using TakaGUI.Data;

namespace TakaGUI.Services
{
	/// <summary>
	/// Summary:
	///		It manages graphics, and it is suppoused to be the only part of the code that has access to the graphics methods
	///		of XNA. From this class you get point Render instance, which you use to draw textures with. You can only have one instance
	///		of an active Render instance at point time, when you are done with it, you call its destroy method.
	///		
	///		Points:
	///			- Manages all XNA graphics.
	///			- Provides Render instances for drawing.
	/// </summary>
	public class GraphicsManager : GameComponent, IGraphicsManager
	{
		private IRender render;
		private GraphicsDeviceManager graphics;

		public int ScreenWidth
		{
			get {
				return graphics.GraphicsDevice.PresentationParameters.BackBufferWidth; }
		}
		public int ScreenHeight
		{
			get { return graphics.GraphicsDevice.PresentationParameters.BackBufferHeight; }
		}

		public GraphicsManager(Game game, GraphicsDeviceManager gdManager)
			: base(game)
		{
			game.Components.Add(this);

			graphics = gdManager;
		}

		public override void Initialize()
		{
			base.Initialize();

			render = new Render(Game.Services, graphics.GraphicsDevice);

			Enabled = false;
		}

		/// <summary>
		/// Returns the shared Render instance and also resets the viewport to default rowValues (fullscreen).
		/// </summary>
		/// <returns></returns>
		public IRender GetRender()
		{
			Viewport v = render.GraphicsDevice.Viewport;

			if (Game.IsActive)
			{
				v.X = 0;
				v.Y = 0;
				v.Width = Math.Min(render.GraphicsDevice.PresentationParameters.BackBufferWidth, ScreenWidth);
				v.Height = Math.Min(render.GraphicsDevice.PresentationParameters.BackBufferHeight, ScreenHeight);
			}

			render.GraphicsDevice.Viewport = v;

			render.EnableColorMultiplication = false;
			render.Multiply = new Vector3(1, 1, 1);

			return render;
		}
	}

	public interface IGraphicsManager
	{
		int ScreenWidth { get; }
		int ScreenHeight { get; }

		IRender GetRender();
	}

	/// <summary>
	/// Summary:
	///		Used to draw textures.
	///		
	/// How to use:
	///		You do NOT create your own instance of the class, you request an instance from GraphicsManager by calling
	///		GraphicsManager.GetRender(List<ViewRect> viewRect). The viewRect list is the list of the boundaries that
	///		the instance can draw in.
	///		
	/// TODO: inherit from spritebatch
	/// </summary>
	public class Render : SpriteBatch, IRender
	{
		public SpriteBatch SpriteBatch
		{
			get;
			private set;
		}

		//Services
		IGraphicsManager graphicsManager;
		IDebug debug;

		private static RenderTarget2D pixel;
		Viewport baseViewport;

		public bool ViewportIsInvalid
		{
			get;
			private set;
		}
		public Viewport ViewportInUse
		{
			get { return GraphicsDevice.Viewport; }
		}

		public bool EnableColorMultiplication { get; set; }
		public Vector3 Multiply { get; set; }

		public Render(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
			: base(graphicsDevice)
		{
			SpriteBatch = (SpriteBatch)this;

			graphicsManager = (IGraphicsManager)serviceProvider.GetService(typeof(IGraphicsManager));
			debug = (IDebug)serviceProvider.GetService(typeof(IDebug));

			baseViewport = GraphicsDevice.Viewport;

			LoadPixel();
			pixel.ContentLost += new EventHandler<EventArgs>(renderTarget_ContentLost);
		}

		void renderTarget_ContentLost(object sender, EventArgs e)
		{
			LoadPixel();
		}
		void LoadPixel()
		{
			pixel = new RenderTarget2D(GraphicsDevice, 1, 1);
			GraphicsDevice.SetRenderTarget(pixel);
			GraphicsDevice.Clear(Color.White);
			GraphicsDevice.SetRenderTarget(null);
		}

		Color MultiplyColor(Color color)
		{
			return new Color(Multiply * color.ToVector3());
		}

		public void Reset()
		{
			GraphicsDevice.Viewport = baseViewport;

			ViewportIsInvalid = false;
		}

		public void SetViewRect(ViewRect viewRect)
		{
			Viewport vp = GraphicsDevice.Viewport;
			vp.X = viewRect.X;
			vp.Y = viewRect.Y;
			vp.Width = viewRect.Width;
			vp.Height = viewRect.Height;

			if (vp.X < 0)
				vp.X = 0;
			if (vp.Y < 0)
				vp.Y = 0;

			if (vp.X >= graphicsManager.ScreenWidth)
			{
				vp.X = graphicsManager.ScreenWidth - 2;
				vp.Width = 1;
			}
			if (vp.Y >= graphicsManager.ScreenHeight)
			{
				vp.Y = graphicsManager.ScreenHeight - 2;
				vp.Height = 1;
			}

			if (vp.X + vp.Width >= graphicsManager.ScreenWidth)
				vp.Width = graphicsManager.ScreenWidth - vp.X - 1;
			if (vp.Y + vp.Height >= graphicsManager.ScreenHeight)
				vp.Height = graphicsManager.ScreenHeight - vp.Y - 1;

			if (vp.Width < 1)
			{
				vp.Width = 1;
				ViewportIsInvalid = true;
			}
			if (vp.Height < 1)
			{
				vp.Height = 1;
				ViewportIsInvalid = true;
			}

			GraphicsDevice.Viewport = vp;
		}
		public ViewRect GetViewRect()
		{
			return new ViewRect(GraphicsDevice.Viewport.X, GraphicsDevice.Viewport.Y, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
		}
		public void AddViewRect(ViewRect viewRect)
		{
			ViewRect v = GetViewRect();
			v.Add(viewRect);
			SetViewRect(v);
		}

		public void DrawSprite(ISprite sprite, Vector2 position, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;

			Draw(sprite.Texture, position, sprite.SourceRectangle, color);
		}
		public void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, position, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, sprite.SourceRectangle, color);
		}
		public void DrawSprite(ISprite sprite, Vector2 position, SpriteEffects spriteEffect, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;

			Draw(sprite.Texture, position, sprite.SourceRectangle, color, 0F, new Vector2(0, 0), 1F, spriteEffect, 0);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, SpriteEffects spriteEffect, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, 0F, new Vector2(0, 0), spriteEffect, 0); 
		}

		public void DrawSprite(ISprite sprite, Vector2 position, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;

			Draw(sprite.Texture, position, sprite.SourceRectangle, color, rotation, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, position, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, rotation, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, rotation, new Vector2(0, 0), SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, sprite.SourceRectangle, color, rotation, new Vector2(0, 0), SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Vector2 position, SpriteEffects spriteEffect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;

			Draw(sprite.Texture, position, sprite.SourceRectangle, color, rotation, new Vector2(0, 0), 1F, spriteEffect, 0);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, SpriteEffects spriteEffect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, rotation, new Vector2(0, 0), spriteEffect, 0); 
		}

		public void DrawSprite(ISprite sprite, Vector2 position, Vector2 origin, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;

			Draw(sprite.Texture, position, sprite.SourceRectangle, color, rotation, origin, 1f, SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Vector2 origin, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, position, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, rotation, origin, 1f, SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Vector2 origin, SpriteEffects spriteEffects, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, position, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, rotation, origin, 1f, spriteEffects, 0);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Vector2 origin, Rectangle sourceRect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, rotation, origin, SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Vector2 origin, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, sprite.SourceRectangle, color, rotation, origin, SpriteEffects.None, 0);
		}
		public void DrawSprite(ISprite sprite, Vector2 position, Vector2 origin, SpriteEffects spriteEffect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			position.X -= GraphicsDevice.Viewport.X;
			position.Y -= GraphicsDevice.Viewport.Y;

			Draw(sprite.Texture, position, sprite.SourceRectangle, color, rotation, origin, 1F, spriteEffect, 0);
		}
		public void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, Vector2 origin, SpriteEffects spriteEffect, Color color, float rotation)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			destinationRect.X -= GraphicsDevice.Viewport.X;
			destinationRect.Y -= GraphicsDevice.Viewport.Y;
			Draw(sprite.Texture, destinationRect, Rectangle.Intersect(sprite.SourceRectangle, sourceRect), color, rotation, origin, spriteEffect, 0);
		}

		public void DrawRect(Rectangle rect, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			rect.X -= GraphicsDevice.Viewport.X;
			rect.Y -= GraphicsDevice.Viewport.Y;
			rect.Width += 1;
			rect.Height += 1;

			Draw(pixel, rect, color);
		}
		//TODO: Transparent color doesnt work
		public void DrawLine(Vector2 a, Vector2 b, Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			a.X -= GraphicsDevice.Viewport.X;
			a.Y -= GraphicsDevice.Viewport.Y;
			b.X -= GraphicsDevice.Viewport.X;
			b.Y -= GraphicsDevice.Viewport.Y;

			var origin = new Vector2(0.5f, 0.0f);
			Vector2 diff = b - a;
			float angle;
			var scale = new Vector2(1.0f, diff.Length() / pixel.Height);

			angle = (float)(Math.Atan2(diff.Y, diff.X)) - MathHelper.PiOver2;

			Draw(pixel, a, null, color, angle, origin, scale, SpriteEffects.None, 1.0f);
		}
		public void DrawHorizontalLine(Point a, int steps, Color color)
		{
			a.X -= GraphicsDevice.Viewport.X;
			a.Y -= GraphicsDevice.Viewport.Y;
			Draw(pixel, new Rectangle(a.X, a.Y, steps, 1), color);
		}
		public void DrawVerticalLine(Point a, int steps, Color color)
		{
			a.X -= GraphicsDevice.Viewport.X;
			a.Y -= GraphicsDevice.Viewport.Y;
			Draw(pixel, new Rectangle(a.X, a.Y, 1, steps), color);
		}
		public void Plot(Vector2 point, Color color)
		{
			point.X -= GraphicsDevice.Viewport.X;
			point.Y -= GraphicsDevice.Viewport.Y;

			Draw(pixel, point, color);
		}

		public void DrawBody(Rectangle rect, Color color,
			ISprite top,
			ISprite topRight,
			ISprite right,
			ISprite bottomRight,
			ISprite bottom,
			ISprite bottomLeft,
			ISprite left,
			ISprite topLeft,
			ISprite inside)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			if (inside == null)
			{
				debug.AddExceptionInClass(typeof(GraphicsManager), "DrawBody", "FontSprite \"inside\" can't be null");
				return;
			}

			//TODO: doesnt really work in some cases.
			Point topSize = new Point((int)Math.Max(top.Width, 0), (int)Math.Max(top.Height, 0));
			Point topRightSize = new Point((int)Math.Max(topRight.Width, 0), (int)Math.Max(topRight.Height, 0));
			Point rightSize = new Point((int)Math.Max(right.Width, 0), (int)Math.Max(right.Height, 0));
			Point bottomRightSize = new Point((int)Math.Max(bottomRight.Width, 0), (int)Math.Max(bottomRight.Height, 0));
			Point bottomSize = new Point((int)Math.Max(bottom.Width, 0), (int)Math.Max(bottom.Height, 0));
			Point bottomLeftSize = new Point((int)Math.Max(bottomLeft.Width, 0), (int)Math.Max(bottomLeft.Height, 0));
			Point leftSize = new Point((int)Math.Max(left.Width, 0), (int)Math.Max(left.Height, 0));
			Point topLeftSize = new Point((int)Math.Max(topLeft.Width, 0), (int)Math.Max(topLeft.Height, 0));

			DrawSprite(topLeft, new Vector2(rect.X, rect.Y), color);
			DrawSprite(topRight, new Vector2(rect.X + rect.Width - topRightSize.X, rect.Y), color);
			DrawSprite(bottomLeft, new Vector2(rect.X, rect.Y + rect.Height - bottomLeftSize.Y), color);
			DrawSprite(bottomRight, new Vector2(rect.X + rect.Width - bottomRightSize.X, rect.Y + rect.Height - bottomRightSize.Y), color);

			DrawSprite(top, new Rectangle(
				/*X=*/			rect.X + topLeftSize.X,
				/*Y=*/			rect.Y,
				/*Width=*/		rect.Width - topLeftSize.X - topRightSize.X,
				/*Height=*/		topSize.Y),
								color);
			DrawSprite(bottom, new Rectangle(
				/*X=*/			rect.X + topLeftSize.X,
				/*Y=*/			rect.Y + rect.Height - bottomSize.Y,
				/*Width=*/		rect.Width - bottomLeftSize.X - bottomRightSize.X,
				/*Height=*/		bottomSize.Y),
								color);
			DrawSprite(left, new Rectangle(
				/*X=*/			rect.X,
				/*Y=*/			rect.Y + topLeftSize.Y,
				/*Width=*/		leftSize.X,
				/*Height=*/		rect.Height - topLeftSize.Y - bottomLeftSize.Y),
								color);
			DrawSprite(right, new Rectangle(
				/*X=*/			rect.X + rect.Width - topRightSize.X,
				/*Y=*/			rect.Y + topRightSize.Y,
				/*Width=*/		rightSize.X,
				/*Height=*/		rect.Height - topRightSize.Y - bottomRightSize.Y),
								color);

			DrawSprite(inside, new Rectangle(
				/*X=*/			rect.X + topLeftSize.X,
				/*Y=*/			rect.Y + topLeftSize.Y,
				/*Width=*/		rect.Width - topLeftSize.X - topRightSize.X,
				/*Height=*/		rect.Height - topLeftSize.Y - bottomLeftSize.Y),
								color);
		}

		public void Clear(Color color)
		{
			if (ViewportIsInvalid)
				return;

			if (EnableColorMultiplication)
				color = MultiplyColor(color);

			Draw(pixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), color);
		}
	}

	public interface IRender
	{
		/// <summary>
		/// The SpriteBatch that the IRender instance is or takes advantage of.
		/// </summary>
		SpriteBatch SpriteBatch { get; }

		bool ViewportIsInvalid { get; }
		Viewport ViewportInUse { get; }

		bool EnableColorMultiplication { get; set; }
		Vector3 Multiply { get; set; }

		void Reset();
		void SetViewRect(ViewRect viewRect);
		ViewRect GetViewRect();
		void AddViewRect(ViewRect viewRect);

		void DrawSprite(ISprite sprite, Vector2 position, Color color);
		void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Color color);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, Color color);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Color color);
		void DrawSprite(ISprite sprite, Vector2 position, SpriteEffects spriteEffect, Color color);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, SpriteEffects spriteEffect, Color color);

		void DrawSprite(ISprite sprite, Vector2 position, Color color, float rotation);
		void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Color color, float rotation);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, Color color, float rotation);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Color color, float rotation);
		void DrawSprite(ISprite sprite, Vector2 position, SpriteEffects spriteEffect, Color color, float rotation);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, SpriteEffects spriteEffect, Color color, float rotation);

		void DrawSprite(ISprite sprite, Vector2 position, Vector2 origin, Color color, float rotation);
		void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Vector2 origin, Color color, float rotation);
		void DrawSprite(ISprite sprite, Vector2 position, Rectangle sourceRect, Vector2 origin, SpriteEffects spriteEffects, Color color, float rotation);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Vector2 origin, Rectangle sourceRect, Color color, float rotation);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Vector2 origin, Color color, float rotation);
		void DrawSprite(ISprite sprite, Vector2 position, Vector2 origin, SpriteEffects spriteEffect, Color color, float rotation);
		void DrawSprite(ISprite sprite, Rectangle destinationRect, Rectangle sourceRect, Vector2 origin, SpriteEffects spriteEffect, Color color, float rotation);
		
		void DrawRect(Rectangle rect, Color color);
		void DrawLine(Vector2 a, Vector2 b, Color color);
		void DrawHorizontalLine(Point a, int steps, Color color);
		void DrawVerticalLine(Point a, int steps, Color color);
		void Plot(Microsoft.Xna.Framework.Vector2 point, Microsoft.Xna.Framework.Color color);
		void DrawBody(Rectangle rect,
			Color color,
			ISprite top,
			ISprite topRight,
			ISprite right,
			ISprite bottomRight,
			ISprite bottom,
			ISprite bottomLeft,
			ISprite left,
			ISprite topLeft,
			ISprite inside);

		void Clear(Color color);

		#region SpriteBatch
		GraphicsDevice GraphicsDevice { get; }
		bool IsDisposed { get; }
		string Name { get; set; }
		object Tag { get; set; }

		// Summary:
		//     Begins a sprite batch operation using deferred sort and default state objects
		//     (BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None,
		//     RasterizerState.CullCounterClockwise).
		void Begin();
		//
		// Summary:
		//     Begins a sprite batch operation using the specified sort and blend state
		//     object and default state objects (DepthStencilState.None, SamplerState.LinearClamp,
		//     RasterizerState.CullCounterClockwise). If you pass a null blend state, the
		//     default is BlendState.AlphaBlend.
		//
		// Parameters:
		//   sortMode:
		//     Sprite drawing order.
		//
		//   blendState:
		//     Blending options.
		void Begin(SpriteSortMode sortMode, BlendState blendState);
		//
		// Summary:
		//     Begins a sprite batch operation using the specified sort, blend, sampler,
		//     depth stencil and rasterizer state objects. Passing null for any of the state
		//     objects selects the default default state objects (BlendState.AlphaBlend,
		//     SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise).
		//
		// Parameters:
		//   sortMode:
		//     Sprite drawing order.
		//
		//   blendState:
		//     Blending options.
		//
		//   samplerState:
		//     Texture sampling options.
		//
		//   depthStencilState:
		//     Depth and stencil options.
		//
		//   rasterizerState:
		//     Rasterization options.
		void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState);
		//
		// Summary:
		//     Begins a sprite batch operation using the specified sort, blend, sampler,
		//     depth stencil and rasterizer state objects, plus a custom effect. Passing
		//     null for any of the state objects selects the default default state objects
		//     (BlendState.AlphaBlend, DepthStencilState.None, RasterizerState.CullCounterClockwise,
		//     SamplerState.LinearClamp). Passing a null effect selects the default SpriteBatch
		//     Class shader.
		//
		// Parameters:
		//   sortMode:
		//     Sprite drawing order.
		//
		//   blendState:
		//     Blending options.
		//
		//   samplerState:
		//     Texture sampling options.
		//
		//   depthStencilState:
		//     Depth and stencil options.
		//
		//   rasterizerState:
		//     Rasterization options.
		//
		//   effect:
		//     Effect state options.
		void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect);
		//
		// Summary:
		//     Begins a sprite batch operation using the specified sort, blend, sampler,
		//     depth stencil, rasterizer state objects, plus a custom effect and a 2D transformation
		//     matrix. Passing null for any of the state objects selects the default default
		//     state objects (BlendState.AlphaBlend, DepthStencilState.None, RasterizerState.CullCounterClockwise,
		//     SamplerState.LinearClamp). Passing a null effect selects the default SpriteBatch
		//     Class shader.
		//
		// Parameters:
		//   sortMode:
		//     Sprite drawing order.
		//
		//   blendState:
		//     Blending options.
		//
		//   samplerState:
		//     Texture sampling options.
		//
		//   depthStencilState:
		//     Depth and stencil options.
		//
		//   rasterizerState:
		//     Rasterization options.
		//
		//   effect:
		//     Effect state options.
		//
		//   transformMatrix:
		//     Transformation matrix for scale, rotate, translate options.
		void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix);
		//
		// Summary:
		//     Adds a sprite to a batch of sprites for rendering using the specified texture,
		//     destination rectangle, and color. Reference page contains links to related
		//     code samples.
		//
		// Parameters:
		//   texture:
		//     A texture.
		//
		//   destinationRectangle:
		//     A rectangle that specifies (in screen coordinates) the destination for drawing
		//     the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		void Draw(Texture2D texture, Rectangle destinationRectangle, Color color);
		//
		// Summary:
		//     Adds a sprite to a batch of sprites for rendering using the specified texture,
		//     position and color. Reference page contains links to related code samples.
		//
		// Parameters:
		//   texture:
		//     A texture.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		void Draw(Texture2D texture, Vector2 position, Color color);
		//
		// Summary:
		//     Adds a sprite to a batch of sprites for rendering using the specified texture,
		//     destination rectangle, source rectangle, and color.
		//
		// Parameters:
		//   texture:
		//     A texture.
		//
		//   destinationRectangle:
		//     A rectangle that specifies (in screen coordinates) the destination for drawing
		//     the sprite. If this rectangle is not the same size as the source rectangle,
		//     the sprite will be scaled to fit.
		//
		//   sourceRectangle:
		//     A rectangle that specifies (in texels) the source texels from a texture.
		//     Use null to draw the entire texture.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color);
		//
		// Summary:
		//     Adds a sprite to a batch of sprites for rendering using the specified texture,
		//     position, source rectangle, and color.
		//
		// Parameters:
		//   texture:
		//     A texture.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   sourceRectangle:
		//     A rectangle that specifies (in texels) the source texels from a texture.
		//     Use null to draw the entire texture.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color);
		//
		// Summary:
		//     Adds a sprite to a batch of sprites for rendering using the specified texture,
		//     destination rectangle, source rectangle, color, rotation, origin, effects
		//     and layer.
		//
		// Parameters:
		//   texture:
		//     A texture.
		//
		//   destinationRectangle:
		//     A rectangle that specifies (in screen coordinates) the destination for drawing
		//     the sprite. If this rectangle is not the same size as the source rectangle,
		//     the sprite will be scaled to fit.
		//
		//   sourceRectangle:
		//     A rectangle that specifies (in texels) the source texels from a texture.
		//     Use null to draw the entire texture.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		//
		//   rotation:
		//     Specifies the angle (in radians) to rotate the sprite about its center.
		//
		//   origin:
		//     The sprite origin; the default is (0,0) which represents the upper-left corner.
		//
		//   effects:
		//     Effects to apply.
		//
		//   layerDepth:
		//     The depth of a layer. By default, 0 represents the front layer and 1 represents
		//     a back layer. Use SpriteSortMode if you want sprites to be sorted during
		//     drawing.
		void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth);
		//
		// Summary:
		//     Adds a sprite to a batch of sprites for rendering using the specified texture,
		//     position, source rectangle, color, rotation, origin, scale, effects, and
		//     layer. Reference page contains links to related code samples.
		//
		// Parameters:
		//   texture:
		//     A texture.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   sourceRectangle:
		//     A rectangle that specifies (in texels) the source texels from a texture.
		//     Use null to draw the entire texture.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		//
		//   rotation:
		//     Specifies the angle (in radians) to rotate the sprite about its center.
		//
		//   origin:
		//     The sprite origin; the default is (0,0) which represents the upper-left corner.
		//
		//   scale:
		//     Scale factor.
		//
		//   effects:
		//     Effects to apply.
		//
		//   layerDepth:
		//     The depth of a layer. By default, 0 represents the front layer and 1 represents
		//     a back layer. Use SpriteSortMode if you want sprites to be sorted during
		//     drawing.
		void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
		//
		// Summary:
		//     Adds a sprite to a batch of sprites for rendering using the specified texture,
		//     position, source rectangle, color, rotation, origin, scale, effects and layer.
		//     Reference page contains links to related code samples.
		//
		// Parameters:
		//   texture:
		//     A texture.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   sourceRectangle:
		//     A rectangle that specifies (in texels) the source texels from a texture.
		//     Use null to draw the entire texture.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		//
		//   rotation:
		//     Specifies the angle (in radians) to rotate the sprite about its center.
		//
		//   origin:
		//     The sprite origin; the default is (0,0) which represents the upper-left corner.
		//
		//   scale:
		//     Scale factor.
		//
		//   effects:
		//     Effects to apply.
		//
		//   layerDepth:
		//     The depth of a layer. By default, 0 represents the front layer and 1 represents
		//     a back layer. Use SpriteSortMode if you want sprites to be sorted during
		//     drawing.
		void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);
		//
		// Summary:
		//     Adds a string to a batch of sprites for rendering using the specified font,
		//     text, position, and color.
		//
		// Parameters:
		//   spriteFont:
		//     A font for diplaying text.
		//
		//   text:
		//     A text string.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color);
		//
		// Summary:
		//     Adds a string to a batch of sprites for rendering using the specified font,
		//     text, position, and color.
		//
		// Parameters:
		//   spriteFont:
		//     A font for diplaying text.
		//
		//   text:
		//     Text string.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color);
		//
		// Summary:
		//     Adds a string to a batch of sprites for rendering using the specified font,
		//     text, position, color, rotation, origin, scale, effects and layer.
		//
		// Parameters:
		//   spriteFont:
		//     A font for diplaying text.
		//
		//   text:
		//     A text string.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		//
		//   rotation:
		//     Specifies the angle (in radians) to rotate the sprite about its center.
		//
		//   origin:
		//     The sprite origin; the default is (0,0) which represents the upper-left corner.
		//
		//   scale:
		//     Scale factor.
		//
		//   effects:
		//     Effects to apply.
		//
		//   layerDepth:
		//     The depth of a layer. By default, 0 represents the front layer and 1 represents
		//     a back layer. Use SpriteSortMode if you want sprites to be sorted during
		//     drawing.
		void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
		//
		// Summary:
		//     Adds a string to a batch of sprites for rendering using the specified font,
		//     text, position, color, rotation, origin, scale, effects and layer.
		//
		// Parameters:
		//   spriteFont:
		//     A font for diplaying text.
		//
		//   text:
		//     A text string.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		//
		//   rotation:
		//     Specifies the angle (in radians) to rotate the sprite about its center.
		//
		//   origin:
		//     The sprite origin; the default is (0,0) which represents the upper-left corner.
		//
		//   scale:
		//     Scale factor.
		//
		//   effects:
		//     Effects to apply.
		//
		//   layerDepth:
		//     The depth of a layer. By default, 0 represents the front layer and 1 represents
		//     a back layer. Use SpriteSortMode if you want sprites to be sorted during
		//     drawing.
		void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);
		//
		// Summary:
		//     Adds a string to a batch of sprites for rendering using the specified font,
		//     text, position, color, rotation, origin, scale, effects and layer.
		//
		// Parameters:
		//   spriteFont:
		//     A font for diplaying text.
		//
		//   text:
		//     Text string.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		//
		//   rotation:
		//     Specifies the angle (in radians) to rotate the sprite about its center.
		//
		//   origin:
		//     The sprite origin; the default is (0,0) which represents the upper-left corner.
		//
		//   scale:
		//     Scale factor.
		//
		//   effects:
		//     Effects to apply.
		//
		//   layerDepth:
		//     The depth of a layer. By default, 0 represents the front layer and 1 represents
		//     a back layer. Use SpriteSortMode if you want sprites to be sorted during
		//     drawing.
		void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
		//
		// Summary:
		//     Adds a string to a batch of sprites for rendering using the specified font,
		//     text, position, color, rotation, origin, scale, effects and layer.
		//
		// Parameters:
		//   spriteFont:
		//     A font for diplaying text.
		//
		//   text:
		//     Text string.
		//
		//   position:
		//     The location (in screen coordinates) to draw the sprite.
		//
		//   color:
		//     The color to tint a sprite. Use Color.White for full color with no tinting.
		//
		//   rotation:
		//     Specifies the angle (in radians) to rotate the sprite about its center.
		//
		//   origin:
		//     The sprite origin; the default is (0,0) which represents the upper-left corner.
		//
		//   scale:
		//     Scale factor.
		//
		//   effects:
		//     Effects to apply.
		//
		//   layerDepth:
		//     The depth of a layer. By default, 0 represents the front layer and 1 represents
		//     a back layer. Use SpriteSortMode if you want sprites to be sorted during
		//     drawing.
		void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);
		//
		// Summary:
		//     Flushes the sprite batch and restores the device state to how it was before
		//     Begin was called.
		void End();

		#endregion
	}

	public struct ViewRect
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public static ViewRect Empty
		{
			get;
			private set;
		}
		public bool IsEmpty
		{
			get;
			private set;
		}

		static ViewRect()
		{
			ViewRect empty = new ViewRect();
			empty.IsEmpty = true;
			Empty = empty;
		}

		public ViewRect(int x, int y, int width, int height) : this()
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			IsEmpty = false;
		}

		public void Add(ViewRect v)
		{
			int smallestRight = v.Width + v.X < Width + X ? v.Width + v.X : Width + X;
			int smallestBottom = v.Height + v.Y < Height + Y ? v.Height + v.Y : Height + Y;

			X = v.X > X ? v.X : X;
			Y = v.Y > Y ? v.Y : Y;
			Width = smallestRight - X;
			Height = smallestBottom - Y;
		}
		public ViewRect AddGet(ViewRect v)
		{
			v.Add(this);

			return v;
		}
	}

	//TODO: DrawOptions
}
