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

namespace MarcelJoachimKloubert.CryptoSync.Tasks
{
    /// <summary>
    /// A task manager.
    /// </summary>
    public sealed class TaskManager : DisposableBase
    {
        #region Fields

        private readonly IList<ITask> _TASKS = new List<ITask>();

        #endregion Fields

        #region Constructors

        public TaskManager(SyncContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Context = context;
        }

        #endregion Constructors

        #region Properties

        public SyncContext Context { get; }

        #endregion Properties

        #region Methods

        public void Add(ITask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            lock (_SYNC)
            {
                _TASKS.Add(task);
            }
        }

        public void Flush()
        {
            lock (_SYNC)
            {
                while (_TASKS.Count > 0)
                {
                    InvokeNextTask();
                }
            }
        }

        private bool? InvokeNextTask()
        {
            ITask task = null;
            if (_TASKS.Count > 0)
            {
                task = _TASKS[0];
                _TASKS.RemoveAt(0);
            }

            if (task != null)
            {
                try
                {
                    task.Run();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return null;
        }

        #endregion Methods
    }
}