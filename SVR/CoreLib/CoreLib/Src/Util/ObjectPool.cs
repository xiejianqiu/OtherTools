using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    public interface IPoolAllocatedObject<T> where T:IPoolAllocatedObject<T>,new()
    {
        void InitPool(ObjectPool<T> pool);
        T Downcast();
    }
    public sealed class ObjectPool<T> where T:IPoolAllocatedObject<T>,new()
    {
        public void Init(int initPoolSize)
        {
            for (int i = 0; i < initPoolSize; ++i)
            {
                T t = new T();
                t.InitPool(this);
                m_UnusedObjects.Enqueue(t);
            }
        }
        public T Alloc()
        {
			T t;			
			if (m_UnusedObjects.Count == 0)
			{
				t = new T ();
				if (null != t)
				{
					t.InitPool (this);
				}
			}
            else
            {
                t = m_UnusedObjects.Dequeue();
            }

			return t;
		}
		public void Recycle(IPoolAllocatedObject<T> t)
		{
			if (null != t) 
			{
				m_UnusedObjects.Enqueue(t.Downcast());
			}
		}
		public int Count
		{
			get
			{
				return m_UnusedObjects.Count;
			}
		}
		private Queue<T> m_UnusedObjects = new Queue<T>();
	}
}
