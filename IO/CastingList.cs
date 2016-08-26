using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TakaGUI.IO
{
	public class CastingList<T> : IList<T>
	{
		public IList InternalList { get; private set; }

		public CastingList(IList internalList)
		{
			InternalList = internalList;
		}

		public int IndexOf(T item)
		{
			return InternalList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public T this[int index]
		{
			get
			{
				return (T)InternalList[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public void Add(T item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T item)
		{
			return InternalList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			new List<T>(InternalList.Cast<T>()).CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return InternalList.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new List<T>(this).GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return InternalList.GetEnumerator();
		}
	}
}
