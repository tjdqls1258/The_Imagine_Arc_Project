using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Util_Patten
{
    public class ObserverStation<T> : MonoBehaviour
    {
        private List<IObserver<T>> m_observers = new();

        private T m_value;
        public T Value
        {
            get { return m_value; }
            private set 
            {
                m_value = value;
                NotifyObserver();
            }
        }


        public void Subscribe(IObserver<T> observer)
        {
            if (m_observers.Contains(observer) == false)
                m_observers.Add(observer);
        }

        public void UnSubscribe(IObserver<T> observer)
        {
            if(m_observers.Contains(observer))
                m_observers.Remove(observer);
        }

        public void SetValue(T value)
        {
            Value = value;
        }

        protected void NotifyObserver()
        {
            foreach (var observer in m_observers)
            {
                if(observer != null)
                    observer.Update(m_value);
            }
        }
    }

    public interface IObserver<T>
    {
        public void Update(T value);
    }

    public class ObserverEventStation<T>
    {
        public UnityAction<T> m_updateEvent;
        private T m_value;
        public T Value
        {
            get { return m_value; }
            private set
            {
                m_value = value;
                NotifyObserver();
            }
        }

        public void AddListener(UnityAction<T> unityAction)
        {
            m_updateEvent -= unityAction;
            m_updateEvent += unityAction;
        }

        public void RemoveListener(UnityAction<T> unityAction)
        {
            m_updateEvent -= unityAction;
        }


        protected void NotifyObserver()
        {
            m_updateEvent?.Invoke(m_value);
        }
    }
}