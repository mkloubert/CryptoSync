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

namespace MarcelJoachimKloubert.CryptoSync.Tasks
{
    /// <summary>
    /// A basic task.
    /// </summary>
    public abstract class TaskBase : DisposableBase, ITask
    {
        #region Constructors

        protected TaskBase(TaskManager manager)
            : this(manager, null)
        {
        }

        protected TaskBase(TaskManager manager, object sync)
            : base(sync)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            Manager = manager;
        }

        #endregion Constructors

        #region Properties

        public TaskManager Manager { get; }

        #endregion Properties

        #region Methods

        public abstract void Run();

        #endregion Methods
    }
}