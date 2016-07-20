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
using System.IO;

namespace MarcelJoachimKloubert.CryptoSync.Cryptography
{
    /// <summary>
    /// En- / Decrypts data.
    /// </summary>
    public interface ICrypter : IDisposable
    {
        #region Methods

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="dest">The destination stream.</param>
        void Decrypt(Stream src, Stream dest);

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="dest">The destination stream.</param>
        void Encrypt(Stream src, Stream dest);

        /// <summary>
        /// Opens a crypted stream for reading.
        /// </summary>
        /// <param name="stream">The crypted base stream.</param>
        /// <returns>The opened stream.</returns>
        Stream OpenRead(Stream stream);

        /// <summary>
        /// Opens a stream for writing.
        /// </summary>
        /// <param name="stream">The target base stream.</param>
        /// <returns>The opened stream.</returns>
        Stream OpenWrite(Stream stream);

        #endregion Methods
    }
}