﻿//  This file is part of CryptoSync (https://github.com/mkloubert/CryptoSync).
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
using System.Runtime.Serialization;

namespace MarcelJoachimKloubert.CryptoSync.Serialization
{
    [DataContract]
    public sealed class CryptedDirectory
    {
        #region Fields

        [DataMember]
        public DateTimeOffset? changeTime;

        [DataMember]
        public DateTimeOffset? creationTime;

        [DataMember]
        public string name;

        [DataMember]
        public string realName;

        #endregion Fields
    }
}