using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace TakaGUI.Services
{
	/// <summary>
	/// Summary:
	///		It makes sure that point fontSprite resource isn't loaded twice.
	///		
	/// Todo:
	///		Add point fontSprite grouping feature, so that you can unload the whole group at the same time.
	/// </summary>
	public class ResourceManager : IResourceManager
	{
		protected ContentManager content;
		protected Dictionary<int, List<object>> loadedAssets = new Dictionary<int, List<object>>();
		protected Dictionary<object, string> resourceNames = new Dictionary<object, string>();

		public ResourceManager(ContentManager _content)
		{
			content = _content;
		}

		public T Load<T>(string assetName, int resourceGroup)
		{
			T resource = content.Load<T>(assetName);

			RegisterResource(resource, assetName, resourceGroup);

			return resource;
		}

		public void RegisterResource(object resource, string assetName, int resourceGroup)
		{
			if (!loadedAssets.Keys.Contains(resourceGroup))
				loadedAssets.Add(resourceGroup, new List<object>());

			loadedAssets[resourceGroup].Add(resource);
			if (!resourceNames.ContainsKey(resource))
				resourceNames.Add(resource, assetName);
		}

		public void UnloadGroup(int id)
		{
			foreach (object resource in loadedAssets[id])
			{
				int numberOfIdentical = 0;
				foreach (List<object> list in loadedAssets.Values)
					foreach (object elem in list)
						if (elem == resource)
							numberOfIdentical += 1;

				var iDisposableType = typeof(IDisposable);
				if (numberOfIdentical == 1)
				{
					if (iDisposableType.IsAssignableFrom(resource.GetType()))
						((IDisposable)resource).Dispose();
					resourceNames.Remove(resource);
				}
			}

			loadedAssets[id].Clear();
			loadedAssets.Remove(id);
		}
		public void UnloadAll()
		{
			foreach (int id in new List<int>(loadedAssets.Keys))
				UnloadGroup(id);
		}

		public string GetResourceName(object resource)
		{
			return resourceNames[resource];
		}
	}

	public interface IResourceManager
	{
		string GetResourceName(object resource);
		T Load<T>(string assetName, int resourceGroup);
		void RegisterResource(object resource, string assetName, int resourceGroup);
		void UnloadAll();
		void UnloadGroup(int id);
	}
}
