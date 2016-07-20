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
using System;
using System.IO;
using System.Security.Cryptography;

namespace MarcelJoachimKloubert.CryptoSync.Cryptography
{
    /// <summary>
    /// A Rijndael crypter.
    /// </summary>
    public sealed class RijndaelCrypter : DisposableBase, ICrypter
    {
        #region Fields

        private byte[] _password;
        private byte[] _salt;

        #endregion Fields

        #region Constructors

        public RijndaelCrypter(byte[] pwd, byte[] salt, int iterations)
        {
            if (pwd == null)
            {
                throw new ArgumentNullException(nameof(pwd));
            }

            if (salt == null)
            {
                throw new ArgumentNullException(nameof(salt));
            }

            _password = pwd;
            _salt = salt;
            Iterations = iterations;
        }

        #endregion Constructors

        #region Properties

        public int Iterations { get; }
        public byte[] Password => _password;
        public byte[] Salt => _salt;

        #endregion Properties

        #region Methods

        public void Decrypt(Stream src, Stream dest)
        {
            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            var cs = OpenRead(src);
            cs.CopyTo(dest);
        }

        public void Encrypt(Stream src, Stream dest)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            var cs = OpenWrite(dest);
            src.CopyTo(cs);

            if (!cs.HasFlushedFinalBlock)
            {
                cs.FlushFinalBlock();
            }
        }

        public CryptoStream OpenRead(Stream stream)
        {
            return CreateCryptoStream(stream, CryptoStreamMode.Read);
        }

        Stream ICrypter.OpenRead(Stream stream) => OpenRead(stream);

        public CryptoStream OpenWrite(Stream stream)
        {
            return CreateCryptoStream(stream, CryptoStreamMode.Write);
        }

        Stream ICrypter.OpenWrite(Stream stream) => OpenRead(stream);

        protected override void Dispose(bool disposing, ref bool isDisposed)
        {
            base.Dispose(disposing, ref isDisposed);

            CollectionHelper.Destroy(ref _password);
            CollectionHelper.Destroy(ref _salt);
        }

        private CryptoStream CreateCryptoStream(Stream baseStream, CryptoStreamMode mode)
        {
            using (var alg = Rijndael.Create())
            {
                using (var db = new Rfc2898DeriveBytes(_password, _salt, Iterations))
                {
                    alg.Key = db.GetBytes(32);
                    alg.IV = db.GetBytes(16);

                    ICryptoTransform transform;
                    switch (mode)
                    {
                        case CryptoStreamMode.Read:
                            transform = alg.CreateDecryptor();
                            break;

                        case CryptoStreamMode.Write:
                            transform = alg.CreateEncryptor();
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return new CryptoStream(baseStream, transform, mode);
                }
            }
        }

        #endregion Methods
    }
}