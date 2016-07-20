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

using System.IO;

namespace MarcelJoachimKloubert.CryptoSync.Tasks
{
    public abstract class FileTaskBase : TaskBase
    {
        #region Constructors

        protected FileTaskBase(TaskManager manager, string path)
            : this(manager, path, null)
        {
        }

        protected FileTaskBase(TaskManager manager, string path, object sync)
            : base(manager, sync)
        {
            Path = path;
        }

        #endregion Constructors

        #region Properties

        public virtual bool? IsDir
        {
            get
            {
                var exists = Directory.Exists(Path);
                if (exists || File.Exists(Path))
                {
                    return exists;
                }

                return null;
            }
        }

        public virtual bool? IsFile
        {
            get
            {
                var exists = File.Exists(Path);
                if (exists || Directory.Exists(Path))
                {
                    return exists;
                }

                return null;
            }
        }

        public string Path { get; }

        #endregion Properties

        #region Methods

        protected virtual FileSystemInfo GetFileSystemInfo()
        {
            if (true == IsFile)
            {
                return new FileInfo(Path);
            }

            if (true == IsDir)
            {
                return new DirectoryInfo(Path);
            }

            return null;
        }

        #endregion Methods
    }
}