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

using MarcelJoachimKloubert.CryptoSync.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CryptedDirectoryInfo = MarcelJoachimKloubert.CryptoSync.Serialization.CryptedDirectory;
using PathClass = System.IO.Path;

namespace MarcelJoachimKloubert.CryptoSync.IO.Crypted
{
    public sealed class CryptedDirectory : FileSystemItemBase, IDirectory
    {
        #region Constructors

        public CryptedDirectory(CryptedFileSystem fileSystem, DirectoryInfo dir, CryptedDirectoryInfo dirInfo, CryptedDirectory parent)
            : this(fileSystem, dir, dirInfo, parent, null)
        {
        }

        public CryptedDirectory(CryptedFileSystem fileSystem, DirectoryInfo dir, CryptedDirectoryInfo dirInfo, CryptedDirectory parent, object sync)
            : base(fileSystem, sync)
        {
            if (dir == null)
            {
                throw new ArgumentNullException(nameof(dir));
            }

            if (dirInfo == null)
            {
                throw new ArgumentNullException(nameof(dirInfo));
            }

            Info = dirInfo;
            Parent = parent;
            LocalPath = dir.FullName;
        }

        #endregion Constructors

        #region Properties

        public new CryptedFileSystem FileSystem => (CryptedFileSystem)base.FileSystem;

        IDirectory IDirectory.Parent => Parent;
        public CryptedDirectoryInfo Info { get; }
        public string LocalPath { get; }

        public string Name => Info.name;

        public CryptedDirectory Parent { get; }

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
            var metaFile = TryGetMetaFile();
            try
            {
                if (metaFile?.directories == null)
                {
                    yield break;
                }

                using (var e = metaFile.directories
                                       .Where(x => !string.IsNullOrWhiteSpace(x?.realName))
                                       .OrderBy(x => x.name, StringComparer.InvariantCultureIgnoreCase)
                                       .GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        var dir = e.Current;

                        var realDir = new DirectoryInfo(PathClass.Combine(LocalPath, dir.realName));
                        if (!realDir.Exists)
                        {
                            continue;
                        }

                        yield return new CryptedDirectory(FileSystem, realDir, dir, null);
                    }
                }
            }
            finally
            {
                metaFile = null;

                try
                {
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                }
            }
        }

        public IEnumerable<IFile> EnumerateFiles()
        {
            var metaFile = TryGetMetaFile();
            try
            {
                if (metaFile?.files == null)
                {
                    yield break;
                }

                using (var e = metaFile.files
                                       .Where(x => !string.IsNullOrWhiteSpace(x?.realName))
                                       .OrderBy(x => x.name, StringComparer.InvariantCultureIgnoreCase)
                                       .GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        var file = e.Current;

                        var realFile = new FileInfo(PathClass.Combine(LocalPath, file.realName));
                        if (!realFile.Exists)
                        {
                            continue;
                        }

                        yield return new CryptedFile(FileSystem, realFile, file, null);
                    }
                }
            }
            finally
            {
                metaFile = null;

                try
                {
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                }
            }
        }

        private MetaFile TryGetMetaFile()
        {
            return CryptedFileSystem.TryGetMetaFile(LocalPath, FileSystem.Crypter);
        }

        #endregion Methods
    }
}