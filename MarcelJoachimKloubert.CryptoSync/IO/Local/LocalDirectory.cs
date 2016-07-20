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
using System.Linq;
using PathClass = System.IO.Path;

namespace MarcelJoachimKloubert.CryptoSync.IO.Local
{
    public class LocalDirectory : FileSystemItemBase, IDirectory
    {
        #region Constructors

        public LocalDirectory(LocalFileSystem fileSystem, DirectoryInfo dir, LocalDirectory parent)
            : this(fileSystem, dir, parent, null)
        {
        }

        public LocalDirectory(LocalFileSystem fileSystem, DirectoryInfo dir, LocalDirectory parent, object sync)
            : base(fileSystem, sync)
        {
            if (dir == null)
            {
                throw new ArgumentNullException(nameof(dir));
            }

            Parent = parent;
            LocalPath = dir.FullName;
        }

        #endregion Constructors

        #region Properties

        public new LocalFileSystem FileSystem => (LocalFileSystem)base.FileSystem;
        public string LocalPath { get; }

        public string Name => PathClass.GetFileName(LocalPath);

        public LocalDirectory Parent { get; }

        IDirectory IDirectory.Parent => Parent;

        public override string Path
        {
            get
            {
                var parts = new List<string>();

                IDirectory dir = this;
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

        public IEnumerable<IDirectory> EnumerateDirectories()
        {
            var dir = new DirectoryInfo(LocalPath);
            if (!dir.Exists)
            {
                yield break;
            }

            foreach (var subDir in dir.EnumerateDirectories()
                                      .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                yield return new LocalDirectory(FileSystem, subDir, this);
            }
        }

        public IEnumerable<IFile> EnumerateFiles()
        {
            var dir = new DirectoryInfo(LocalPath);
            if (!dir.Exists)
            {
                yield break;
            }

            foreach (var file in dir.EnumerateFiles()
                                    .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                yield return new LocalFile(FileSystem, file, this);
            }
        }

        #endregion Methods
    }
}