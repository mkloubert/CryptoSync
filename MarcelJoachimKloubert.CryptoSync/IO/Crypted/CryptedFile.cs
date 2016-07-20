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

using MarcelJoachimKloubert.CryptoSync.Cryptography;
using MarcelJoachimKloubert.CryptoSync.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using CryptedFileInfo = MarcelJoachimKloubert.CryptoSync.Serialization.CryptedFile;

namespace MarcelJoachimKloubert.CryptoSync.IO.Crypted
{
    public sealed class CryptedFile : FileSystemItemBase, IFile
    {
        #region Constructors

        public CryptedFile(CryptedFileSystem fileSystem, FileInfo file, CryptedFileInfo fileInfo, CryptedDirectory dir)
            : this(fileSystem, file, fileInfo, dir, null)
        {
        }

        public CryptedFile(CryptedFileSystem fileSystem, FileInfo file, CryptedFileInfo fileInfo, CryptedDirectory dir, object sync)
            : base(fileSystem, sync)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            Info = fileInfo;
            Directory = dir;
            LocalPath = file.FullName;
        }

        #endregion Constructors

        #region Properties

        public CryptedDirectory Directory { get; }
        public new CryptedFileSystem FileSystem => (CryptedFileSystem)base.FileSystem;

        IDirectory IFile.Directory => Directory;

        public CryptedFileInfo Info { get; }
        public string LocalPath { get; }

        public string Name => Info.name;

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

        public Stream OpenRead()
        {
            var fs = new FileStream(LocalPath, FileMode.Open, FileAccess.Read);
            try
            {
                using (var crypter = GetCrypter())
                {
                    return crypter.OpenRead(fs);
                }
            }
            catch (Exception)
            {
                ObjectHelper.Dispose(ref fs);
                throw;
            }
        }

        public Stream OpenWrite()
        {
            var fs = new FileStream(LocalPath, FileMode.Open, FileAccess.Write);
            try
            {
                using (var crypter = GetCrypter())
                {
                    return crypter.OpenWrite(fs);
                }
            }
            catch (Exception)
            {
                ObjectHelper.Dispose(ref fs);
                throw;
            }
        }

        private ICrypter GetCrypter()
        {
            if (Info.crypter?.pwd?.Length > 0)
            {
                return new RijndaelCrypter(Info.crypter.pwd,
                                           Info.crypter.salt ?? SyncContext.DEFAULT_SALT,
                                           Info.crypter.iterations ?? SyncContext.DEFAULT_ITERATIONS);
            }

            return new NullCrypter();
        }

        #endregion Methods
    }
}