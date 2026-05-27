using System;
using UnityEngine;

namespace MVVM
{
    public class BindableProperty<T>
    {
        private T m_value;
        public event Action<T> onValueChanged;

        public T Value
        {
            get => m_value;
            set
            {
                if (!Equals(m_value, value))
                {
                    m_value = value;
                    onValueChanged?.Invoke(m_value);
                }
            }
        }

        public BindableProperty(T initValue = default)
        {
            m_value = initValue;
        }

        public void Bind(Action<T> action)
        {
            onValueChanged += action;
            onValueChanged?.Invoke(Value);
        }

        public void UnBind(Action<T> action)
        {
            onValueChanged -= action;
        }
    }


    public abstract class BaseViewModel
    {
        public virtual void Init() { }
    }
}