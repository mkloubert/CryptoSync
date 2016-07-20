//  This file is part of CryptoSync (https://github.com/mkloubert/CryptoSync).
//  Copyright (c) Marcel Joachim Kloubert <marcel.kloubert@gmx.net>
//
//  CryptoSync is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  CryptoSync is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with CryptoSync. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace MarcelJoachimKloubert.CryptoSync
{
    /// <summary>
    /// A basic object.
    /// </summary>
    public class CryptoObject : MarshalByRefObject, INotifyPropertyChanged
    {
        #region Fields

        protected readonly IDictionary<string, object> _PROPERTIES;

        /// <summary>
        /// Stores the object for thread safe operations.
        /// </summary>
        protected readonly object _SYNC;

        #endregion Fields

        #region Constructors

        public CryptoObject()
            : this(sync: null)
        {
        }

        public CryptoObject(object sync)
        {
            _SYNC = sync ?? new object();

            _PROPERTIES = CreatePropertyStorage() ?? new Dictionary<string, object>();
        }

        #endregion Constructors

        #region Events

        public event ErrorEventHandler Error;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the object for thread safe operations.
        /// </summary>
        public object SyncRoot => _SYNC;

        #endregion Properties

        #region Methods

        public void InvokeThreadSafe(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            InvokeThreadSafe((state) => action(), (object)null);
        }

        public void InvokeThreadSafe<S>(Action<S> action, S state)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            InvokeThreadSafe((s) =>
            {
                s.Action(s.State);
                return (object)null;
            }, new
            {
                Action = action,
                State = state,
            });
        }

        public R InvokeThreadSafe<R>(Func<R> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return InvokeThreadSafe((state) => func(), (object)null);
        }

        public R InvokeThreadSafe<S, R>(Func<S, R> func, S state)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            lock (_SYNC)
            {
                return func(state);
            }
        }

        public virtual bool Set<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            var oldValue = Get<T>(propertyName);
            if (!ArePropertyValuesEqual(oldValue, newValue, propertyName))
            {
                if (_PROPERTIES.ContainsKey(propertyName ?? ""))
                {
                    _PROPERTIES[propertyName] = newValue;
                }
                else
                {
                    _PROPERTIES.Add(propertyName, newValue);
                }

                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected virtual bool ArePropertyValuesEqual<T>(T x, T y, string propertyName)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        protected virtual T ConvertPropertyTo<T>(object input, string propertyName)
        {
            if (input == null || DBNull.Value.Equals(input))
            {
                input = default(T);
            }

            return (T)input;
        }

        protected virtual IDictionary<string, object> CreatePropertyStorage()
        {
            return null;
        }

        protected virtual T Get<T>([CallerMemberName] string propertyName = null)
        {
            object temp;
            _PROPERTIES.TryGetValue(propertyName ?? "", out temp);

            return ConvertPropertyTo<T>(temp, propertyName);
        }

        protected bool? RaiseError(Exception ex)
        {
            if (ex == null)
            {
                return null;
            }

            var handler = Error;
            handler?.Invoke(this, new ErrorEventArgs(ex));
            return handler != null;
        }

        /// <summary>
        /// Raises an event handler for that object.
        /// </summary>
        /// <param name="handler">The handler to raise.</param>
        /// <returns>Handler was raised or not.</returns>
        protected bool RaiseEventHandler(EventHandler handler)
        {
            handler?.Invoke(this, EventArgs.Empty);
            return handler != null;
        }

        protected virtual bool RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;

            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return handler != null;
        }

        #endregion Methods
    }
}