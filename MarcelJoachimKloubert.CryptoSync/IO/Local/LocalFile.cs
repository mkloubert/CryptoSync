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
using PathClass = System.IO.Path;

namespace MarcelJoachimKloubert.CryptoSync.IO.Local
{
    public class LocalFile : FileSystemItemBase, IFile
    {
        #region Constructors

        public LocalFile(LocalFileSystem fileSystem, FileInfo file, LocalDirectory dir)
            : this(fileSystem, file, dir, null)
        {
        }

        public LocalFile(LocalFileSystem fileSystem, FileInfo file, LocalDirectory dir, object sync)
            : base(fileSystem, sync)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            Directory = dir;
            LocalPath = file.FullName;
        }

        #endregion Constructors

        #region Properties

        public LocalDirectory Directory { get; }

        public new LocalFileSystem FileSystem => (LocalFileSystem)base.FileSystem;
        IDirectory IFile.Directory => Directory;
        public string LocalPath { get; }

        public string Name => PathClass.GetFileName(LocalPath);

        public override string Path
        {
            get
            {
                var parts = new List<string> { Name };

                IDirectory dir = Directory;
                while (dir != null)
                {
                    parts.Add(dir.Name);

                    dir = dir.Parent;
                }

                return "/" + string.Join("/", parts);
            }
        }

        #endregion Properties

        #region Methods

        Stream IFile.OpenWrite() => OpenWrite();

        Stream IFile.OpenRead() => OpenRead();

        public FileStream OpenRead()
        {
            return new FileStream(LocalPath, FileMode.Open, FileAccess.Read);
        }

        public FileStream OpenWrite()
        {
            return new FileStream(LocalPath, FileMode.Open, FileAccess.Write);
        }

        #endregion Methods
    }
}