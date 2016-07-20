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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace MarcelJoachimKloubert.CryptoSync
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class CryptoSyncExtensionMethods
    {
        #region Methods

        public static T DecryptObject<T>(this ICrypter crypter, Stream src)
        {
            if (crypter == null)
            {
                throw new ArgumentNullException(nameof(crypter));
            }

            byte[] bin;
            using (var temp = new MemoryStream())
            {
                crypter.Decrypt(src, temp);

                bin = temp.ToArray();
            }

            string json;
            try
            {
                json = Encoding.UTF8.GetString(bin);
                return JsonConvert.DeserializeObject<T>(json);
            }
            finally
            {
                json = null;
                CollectionHelper.Destroy(ref bin);

                try
                {
                    GC.Collect();
                }
                catch
                {
                    // ignore
                }
            }
        }

        public static void EncryptObject<T>(this ICrypter crypter, T obj, Stream dest)
        {
            if (crypter == null)
            {
                throw new ArgumentNullException(nameof(crypter));
            }

            var json = JsonConvert.SerializeObject(obj);
            var bin = Encoding.UTF8.GetBytes(json);
            try
            {
                using (var temp = new MemoryStream(bin, false))
                {
                    crypter.Encrypt(temp, dest);
                }
            }
            finally
            {
                json = null;
                CollectionHelper.Destroy(ref bin);

                try
                {
                    GC.Collect();
                }
                catch
                {
                    // ignore
                }
            }
        }

        #endregion Methods
    }
}