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

namespace MarcelJoachimKloubert.CryptoSync
{
    /// <summary>
    /// A basic disposable object.
    /// </summary>
    public abstract class DisposableBase : CryptoObject, IDisposable
    {
        #region Constructors

        protected DisposableBase()
        {
        }

        protected DisposableBase(object sync)
            : base(sync)
        {
        }

        #endregion Constructors

        #region Destructors

        ~DisposableBase()
        {
            DisposeInner(false);
        }

        #endregion Destructors

        #region Events

        public event EventHandler DisposeCancelled;

        public event EventHandler Disposed;

        public event EventHandler Disposing;

        #endregion Events

        #region Properties

        public bool IsDisposed
        {
            get { return Get<bool>(); }

            private set { Set(value); }
        }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            DisposeInner(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing, ref bool isDisposed)
        {
            // dummy
        }

        private void DisposeInner(bool disposing)
        {
            if (disposing && IsDisposed)
            {
                return;
            }

            var newIsDisposedValue = IsDisposed;
            if (disposing)
            {
                newIsDisposedValue = true;
                RaiseEventHandler(Disposing);
            }

            Dispose(disposing, ref newIsDisposedValue);

            IsDisposed = newIsDisposedValue;
            if (disposing)
            {
                if (newIsDisposedValue)
                {
                    RaiseEventHandler(Disposed);
                }
                else
                {
                    RaiseEventHandler(DisposeCancelled);
                }
            }
        }

        #endregion Methods
    }
}