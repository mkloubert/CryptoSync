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

namespace MarcelJoachimKloubert.CryptoSync.IO.Local
{
    public sealed class LocalFileSystem : FileSystemBase
    {
        #region Constructors

        public LocalFileSystem(string rootDir)
            : base(rootDir)
        {
        }

        public LocalFileSystem(string rootDir, object sync)
            : base(rootDir, sync)
        {
        }

        #endregion Constructors

        #region Methods

        public override IEnumerable<IDirectory> EnumerateDirectories()
        {
            var dir = new DirectoryInfo(RootDirectory);
            if (!dir.Exists)
            {
                yield break;
            }

            foreach (var subDir in dir.EnumerateDirectories()
                                      .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                yield return new LocalDirectory(this, subDir, null);
            }
        }

        public override IEnumerable<IFile> EnumerateFiles()
        {
            var dir = new DirectoryInfo(RootDirectory);
            if (!dir.Exists)
            {
                yield break;
            }

            foreach (var file in dir.EnumerateFiles()
                                    .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                yield return new LocalFile(this, file, null);
            }
        }

        #endregion Methods
    }
}