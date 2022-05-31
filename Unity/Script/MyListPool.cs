using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using pools;
namespace pools {
    internal class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive { get { return m_Stack.Count; } }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        public void Recycle(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
            {
                //Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }
    }
}
internal class MyListPool<T>
{
    private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => {
        l.Clear();
    });

    public static List<T> Get()
    {
        return s_ListPool.Get();
    }

    public static void Recycle(List<T> toRelease)
    {
        if (null != toRelease)
        {
            s_ListPool.Recycle(toRelease);
            toRelease = null;
        }
    }
}
internal class MyStrBuilder
{
    private static readonly ObjectPool<StringBuilder> s_ListPool = new ObjectPool<StringBuilder>(g => g.Clear(), r => r.Clear());

    public static StringBuilder Get()
    {
        return s_ListPool.Get();
    }

    public static void Recycle(StringBuilder toRelease)
    {
        if (null != toRelease)
        {
            s_ListPool.Recycle(toRelease);
            toRelease = null;
        }
    }
}
