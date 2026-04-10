using System;
using System.Collections.Generic;
using UnityEngine;
namespace TheVayuputra.Core
{
    [Serializable]
    public class ObservableValue<T>
    {
        [SerializeField]
        private T _value;

        /// <summary>
        /// Invoked when value changes.
        /// </summary>
        private event Action<T> OnValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                // Prevent unnecessary invokes
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;

                _value = value;
                NotifyValueChanged();
            }
        }

        public ObservableValue()
        {
            _value = default;
        }

        public ObservableValue(T initialValue)
        {
            _value = initialValue;
        }

        public ObservableValue(T initialValue, Action<T> onChange)
        {
            _value = initialValue;
            AddListener(onChange);
        }

        private void NotifyValueChanged()
        {
            if (OnValueChanged == null)
                return;

            foreach (Action<T> listener in OnValueChanged.GetInvocationList())
            {
                // Static method → always safe
                if (listener.Target == null)
                {
                    listener.Invoke(_value);
                    continue;
                }

                // Instance method on Unity object
                if (listener.Target is UnityEngine.Object unityObj && unityObj == null)
                {
                    // Unity object destroyed → auto-remove
                    OnValueChanged -= listener;
                    continue;
                }

                listener.Invoke(_value);
            }
        }



        public void AddListener(Action<T> listener, bool invokeImmediately = true)
        {
            if (listener == null)
                return;

            OnValueChanged += listener;

            if (invokeImmediately)
                listener(_value);
        }

        public void RemoveListener(Action<T> listener)
        {
            if (listener == null)
                return;

            OnValueChanged -= listener;
        }

        public void ClearListeners()
        {
            OnValueChanged = null;
        }
    }
}