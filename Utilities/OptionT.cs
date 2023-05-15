///	© Copyright 2022, Lucas Leonardo Conti - DeadlySmileTM

using System;

namespace ParadoxFramework.Utilities
{
    public struct OptionT<T>
    {
        private T _result;
        private bool _hasValue;

        public OptionT(T value) : this()
        {
            if (value != null)
            {
                _result = value;
                _hasValue = true;
            }
            else
                _hasValue = false;
        }
        public OptionT(T value, T defaultValue) : this()
        {
            _hasValue = true;

            if (value != null)
                _result = value;
            else
                _result = defaultValue;
        }

        /// <summary>
        /// Create a new OptionT with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static OptionT<T> NewOption(T value) => new(value);

        /// <summary>
        /// Return true if the given OptionT has a value.
        /// </summary>
        /// <returns></returns>
        public bool HasValue() => _hasValue;
        /// <summary>
        /// Return true if the given OptionT value is null;
        /// </summary>
        /// <returns></returns>
        public bool IsNull() => !_hasValue;
        /// <summary>
        /// Unwraps the OptionT, if is null return a default value.
        /// </summary>
        /// <returns></returns>
        public T Get() => _result;
        /// <summary>
        /// Unwraps the OptionT, if is null return the given default value.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T Get(T defaultValue)
        {
            if (_hasValue)
                return _result;

            return defaultValue;
        }
        /// <summary>
        /// Unwraps the OptionT using a function. If is null is returned untoched.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public T GetMapped(Func<T, T> map)
        {
            if (_hasValue)
                return map(_result);

            return _result;
        }
        /// <summary>
        /// Unwraps the OptionT using a function. If is null return the given default value.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetMapped(Func<T, T> map, T defaultValue)
        {
            if (_hasValue)
                return map(_result);

            return map(defaultValue);
        }
    }
}
