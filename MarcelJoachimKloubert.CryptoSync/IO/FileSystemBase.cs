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
using System.IO;

namespace MarcelJoachimKloubert.CryptoSync.IO
{
    public abstract class FileSystemBase : DisposableBase, IFileSystem
    {
        #region Constructors

        protected FileSystemBase(string rootDir)
            : this(rootDir, null)
        {
        }

        protected FileSystemBase(string rootDir, object sync)
            : base(sync)
        {
            if (rootDir == null)
            {
                throw new ArgumentNullException(nameof(rootDir));
            }

            if (!Path.IsPathRooted(rootDir))
            {
                rootDir = Path.Combine(Environment.CurrentDirectory, rootDir);
            }

            RootDirectory = new DirectoryInfo(rootDir).FullName;
        }

        #endregion Constructors

        #region Properties

        public string RootDirectory { get; }

        #endregion Properties

        #region Methods

        public abstract IEnumerable<IDirectory> EnumerateDirectories();

        public abstract IEnumerable<IFile> EnumerateFiles();

        #endregion Methods
    }
}