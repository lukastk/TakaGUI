#region File Description
//-----------------------------------------------------------------------------
// SpriteSheet.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TakaGUI.Data;
using TakaGUI.Services;
#endregion

namespace TakaGUI
{
	/// <summary>
	/// A sprite sheet contains many individual sprite images, packed into different
	/// areas of a single larger fontSprite, along with information describing where in
	/// that fontSprite each sprite is located. Sprite sheets can make your game drawing
	/// more efficient, because they reduce the number of times the graphics hardware
	/// needs to switch from one fontSprite to another.
	/// </summary>
	public class SpriteSheet : ISpriteSheet
	{
		// Single fontSprite contains many separate sprite images.
		[ContentSerializer]
		Texture2D texture = null;

		// Remember where in the fontSprite each sprite has been placed.
		[ContentSerializer]
		Rectangle[] spriteRectangles = null;
		
		// Store the original sprite filenames, so we can look up sprites by name.
		[ContentSerializer]
		Dictionary<string, int> spriteNames = null;

		/// <summary>
		/// Gets the single large fontSprite used by this sprite sheet.
		/// </summary>
		public Texture2D Texture
		{
			get { return texture; }
		}

		/// <summary>
		/// Looks up the location of the specified sprite within the big fontSprite.
		/// </summary>
		public Rectangle SourceRectangle(string spriteName)
		{
			int spriteIndex = GetIndex(spriteName);

			return spriteRectangles[spriteIndex];
		}

		/// <summary>
		/// Looks up the location of the specified sprite within the big fontSprite.
		/// </summary>
		public Rectangle SourceRectangle(int spriteIndex)
		{
			if ((spriteIndex < 0) || (spriteIndex >= spriteRectangles.Length))
				throw new ArgumentOutOfRangeException("spriteIndex");

			return spriteRectangles[spriteIndex];
		}

		/// <summary>
		/// Looks up the numeric index of the specified sprite. This is useful when
		/// implementing animation by cycling through a series of related sprites.
		/// </summary>
		public int GetIndex(string spriteName)
		{
			int index;

			if (!spriteNames.TryGetValue(spriteName, out index))
			{
				string error = "SpriteSheet does not contain a sprite named '{0}'.";

				throw new KeyNotFoundException(string.Format(error, spriteName));
			}

			return index;
		}

		public string GetName(int index)
		{
			foreach (string name in spriteNames.Keys)
				if (spriteNames[name] == index)
					return name;

			return null;
		}

		/// <summary>
		/// Makes a Spritesheet with a single sprite out of a texture, because it isn't automatically registered
		/// with the ResourceManager, the function registers the SpriteBatch with ResourceManager.RegisterResource().
		/// </summary>
		/// <param name="textureAssetName"></param>
		/// <param name="resourceGroup"></param>
		/// <returns></returns>
		public static ISprite GetSingleSprite(IResourceManager resourceManager, string textureAssetName, int resourceGroup)
		{
			SpriteSheet spriteSheet = new SpriteSheet();
			spriteSheet.texture = resourceManager.Load<Texture2D>(textureAssetName, resourceGroup);
			spriteSheet.spriteRectangles = new Rectangle[] { new Rectangle(0, 0, spriteSheet.texture.Width, spriteSheet.texture.Height) };

			spriteSheet.spriteNames = new Dictionary<string, int>();
			spriteSheet.spriteNames.Add(textureAssetName, 0);

			resourceManager.RegisterResource(spriteSheet, textureAssetName, resourceGroup);

			return new Sprite(0, spriteSheet);
		}

		public void Dispose()
		{
			texture.Dispose();
		}
	}

	public class Sprite : ISprite
	{
		public ISpriteSheet SpriteSheet
		{
			get;
			private set;
		}
		public Texture2D Texture
		{
			get;
			private set;
		}
		public Rectangle SourceRectangle
		{
			get;
			private set;
		}
		public int Index
		{
			get;
			private set;
		}
		public string Name
		{
			get;
			private set;
		}
		public int Width
		{
			get;
			private set;
		}
		public int Height
		{
			get;
			private set;
		}

		public Sprite(int index, ISpriteSheet spriteSheet)
		{
			Index = index;
			SpriteSheet = spriteSheet;

			Texture = SpriteSheet.Texture;
			SourceRectangle = SpriteSheet.SourceRectangle(Index);
			Name = SpriteSheet.GetName(index);
			Width = SourceRectangle.Width;
			Height = SourceRectangle.Height;
		}

		public void Dispose()
		{
		}
	}

	public interface ISpriteSheet : IDisposable
	{
		/// <summary>
		/// Gets the single large fontSprite used by this sprite sheet.
		/// </summary>
		Texture2D Texture { get; }

		/// <summary>
		/// Looks up the location of the specified sprite within the big fontSprite.
		/// </summary>
		Rectangle SourceRectangle(string spriteName);

		/// <summary>
		/// Looks up the location of the specified sprite within the big fontSprite.
		/// </summary>
		Rectangle SourceRectangle(int spriteIndex);

		/// <summary>
		/// Looks up the numeric index of the specified sprite. This is useful when
		/// implementing animation by cycling through a series of related sprites.
		/// </summary>
		int GetIndex(string spriteName);

		string GetName(int index);
	}

	public interface ISprite : IDisposable
	{
		ISpriteSheet SpriteSheet { get; }
		Texture2D Texture { get; }
		Rectangle SourceRectangle { get; }
		int Index { get; }
		string Name { get; }
		int Width { get; }
		int Height { get; }
	}
}
