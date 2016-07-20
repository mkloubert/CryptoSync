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
using MarcelJoachimKloubert.CryptoSync.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarcelJoachimKloubert.CryptoSync.IO.Crypted
{
    public sealed class CryptedFileSystem : FileSystemBase
    {
        #region Fields

        /// <summary>
        /// The name of a meta file.
        /// </summary>
        public const string META_FILE = "0.bin";

        #endregion Fields

        #region Constructors

        public CryptedFileSystem(ICrypter crypter, string rootDir)
            : this(crypter, rootDir, null)
        {
        }

        public CryptedFileSystem(ICrypter crypter, string rootDir, object sync)
            : base(rootDir, sync)
        {
            if (crypter == null)
            {
                throw new ArgumentNullException(nameof(crypter));
            }

            Crypter = crypter;
        }

        #endregion Constructors

        #region Properties

        public ICrypter Crypter { get; }

        #endregion Properties

        #region Methods

        public static MetaFile TryGetMetaFile(string path, ICrypter crypter)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (crypter == null)
            {
                throw new ArgumentNullException(nameof(crypter));
            }

            try
            {
                var metaFile = new FileInfo(Path.Combine(path, META_FILE));
                if (metaFile.Exists)
                {
                    using (var fs = metaFile.OpenRead())
                    {
                        return crypter.DecryptObject<MetaFile>(fs);
                    }
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        public override IEnumerable<IDirectory> EnumerateDirectories()
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

                        var realDir = new DirectoryInfo(Path.Combine(RootDirectory, dir.realName));
                        if (!realDir.Exists)
                        {
                            continue;
                        }

                        yield return new CryptedDirectory(this, realDir, dir, null);
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

        public override IEnumerable<IFile> EnumerateFiles()
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
                                       .OrderBy(x => x.name, StringComparer.InvariantCultureIgnoreCase).GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        var file = e.Current;

                        var realFile = new FileInfo(Path.Combine(RootDirectory, file.realName));
                        if (!realFile.Exists)
                        {
                            continue;
                        }

                        yield return new CryptedFile(this, realFile, file, null);
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
            return TryGetMetaFile(RootDirectory, Crypter);
        }

        #endregion Methods
    }
}