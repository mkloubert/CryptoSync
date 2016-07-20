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

using MarcelJoachimKloubert.CryptoSync.Helpers;
using MarcelJoachimKloubert.CryptoSync.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarcelJoachimKloubert.CryptoSync.Cryptography;
using MarcelJoachimKloubert.CryptoSync.IO;
using MarcelJoachimKloubert.CryptoSync.IO.Crypted;
using MarcelJoachimKloubert.CryptoSync.IO.Local;

namespace MarcelJoachimKloubert.CryptoSync
{
    public sealed class SyncContext : DisposableBase
    {
        #region Fields

        /// <summary>
        /// The default salt.
        /// </summary>
        public static readonly byte[] DEFAULT_SALT = { 0x70, 0x66, 0x58, 0xE6, 0x09, 0x2C, 0x79, 0x83, 0x5F, 0xD3, 0xC9, 0xFF, 0x6E, 0x0C, 0x08, 0xD9, 0x81, 0x10, 0xC3, 0xF5, 0xAC, 0x94, 0x6B, 0x5B, 0xC8, 0xA7, 0xC4, 0x6E, 0x54, 0xD3, 0x9E, 0x29 };

        public const int DEFAULT_ITERATIONS = 1000;

        private ICrypter _crypter;
        private FileSystemWatcher _watcher;

        #endregion Fields

        #region Constructors

        private SyncContext()
        {
            Tasks = new TaskManager(this);
        }

        #endregion Constructors

        #region Events

        public event EventHandler Started;

        public event EventHandler Starting;

        public event EventHandler Stopped;

        public event EventHandler Stopping;

        #endregion Events

        #region Properties

        public ICrypter Crypter { get { return _crypter; } private set { _crypter = value; } }

        /// <summary>
        /// Gets the destination directory.
        /// </summary>
        public IFileSystem Destination { get; private set; }

        /// <summary>
        /// Gets if the context is currently running or not.
        /// </summary>
        public bool IsRunning
        {
            get { return Get<bool>(); }

            private set { Set(value); }
        }

        
        /// <summary>
        /// Gets the source directory.
        /// </summary>
        public IFileSystem Source { get; private set; }

        public TaskManager Tasks { get; }

        #endregion Properties

        #region Methods

        public static SyncContext Create(string src, string dest,
                                         byte[] pwd, byte[] salt = null, int? iterations = null)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            if (pwd == null)
            {
                throw new ArgumentNullException(nameof(pwd));
            }

            if (salt == null || salt.Length < 1)
            {
                salt = DEFAULT_SALT.ToArray();
            }

            if (!iterations.HasValue)
            {
                iterations = DEFAULT_ITERATIONS;
            }

            if (!Path.IsPathRooted(src))
            {
                src = Path.Combine(Environment.CurrentDirectory, src);
            }

            if (!Path.IsPathRooted(dest))
            {
                dest = Path.Combine(Environment.CurrentDirectory, dest);
            }

            var srcDir = new DirectoryInfo(src);
            var destDir = new DirectoryInfo(dest);

            var newContext = new SyncContext
            {
                Crypter = new RijndaelCrypter(pwd, salt, iterations.Value),
            };
            newContext.Source = new LocalFileSystem(srcDir.FullName);
            newContext.Destination = new CryptedFileSystem(newContext.Crypter, destDir.FullName);

            return newContext;
        }

        /// <summary>
        /// Starts sync process.
        /// </summary>
        public void Start()
        {
            lock (_SYNC)
            {
                OnStart();
            }
        }

        /// <summary>
        /// Stops sync process.
        /// </summary>
        public void Stop()
        {
            lock (_SYNC)
            {
                OnStop();
            }
        }

        protected override IDictionary<string, object> CreatePropertyStorage()
        {
            return new ConcurrentDictionary<string, object>();
        }

        protected override void Dispose(bool disposing, ref bool isDisposed)
        {
            lock (_SYNC)
            {
                base.Dispose(disposing, ref isDisposed);

                OnStop(disposing);

                ObjectHelper.Dispose(ref _crypter);
            }
        }

        private void DisposeWatcher()
        {
            var w = _watcher;
            if (w == null)
            {
                return;
            }

            try
            {
                w.EnableRaisingEvents = false;
                w.Disposed -= Watcher_Disposed;
                w.Changed -= Watcher_Changed;
                w.Created -= Watcher_Created;
                w.Deleted -= Watcher_Deleted;
                w.Error -= Watcher_Error;
                w.Renamed -= Watcher_Renamed;
            }
            finally
            {
                ObjectHelper.Dispose(ref _watcher);
            }
        }

        private void OnStart()
        {
            if (IsRunning)
            {
                return;
            }

            RaiseEventHandler(Starting);

            DisposeWatcher();

            var newWatcher = new FileSystemWatcher
            {
                IncludeSubdirectories = true,
                Path = Source.RootDirectory,
            };
            newWatcher.Changed += Watcher_Changed;
            newWatcher.Created += Watcher_Created;
            newWatcher.Deleted += Watcher_Deleted;
            newWatcher.Error += Watcher_Error;
            newWatcher.Renamed += Watcher_Renamed;
            newWatcher.Disposed += Watcher_Disposed;

            _watcher = newWatcher;
            newWatcher.EnableRaisingEvents = true;

            IsRunning = true;
            RaiseEventHandler(Started);
        }

        private void OnStop(bool? disposing = null)
        {
            if (!IsRunning)
            {
                return;
            }

            if (!disposing.HasValue)
            {
                RaiseEventHandler(Stopping);
            }

            DisposeWatcher();
            IsRunning = false;

            if (!disposing.HasValue)
            {
                RaiseEventHandler(Stopped);
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            InvokeThreadSafe((state) =>
            {
                state.Context.Tasks.Add(new FileChangedTask(state.Context.Tasks, state.EventArgs.FullPath));
            }, new
            {
                Context = this,
                EventArgs = e,
            });
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            InvokeThreadSafe((state) =>
            {
                state.Context.Tasks.Add(new FileCreatedTask(state.Context.Tasks, state.EventArgs.FullPath));
            }, new
            {
                Context = this,
                EventArgs = e,
            });
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            InvokeThreadSafe((state) =>
            {
                state.Context.Tasks.Add(new FileDeletedTask(state.Context.Tasks, state.EventArgs.FullPath));
            }, new
            {
                Context = this,
                EventArgs = e,
            });
        }

        private void Watcher_Disposed(object sender, EventArgs e)
        {
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            RaiseError(e.GetException());
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            InvokeThreadSafe((state) =>
            {
                state.Context.Tasks.Add(new FileRenamedTask(state.Context.Tasks,
                                                            state.EventArgs.OldFullPath, state.EventArgs.FullPath));
            }, new
            {
                Context = this,
                EventArgs = e,
            });
        }

        #endregion Methods
    }
}