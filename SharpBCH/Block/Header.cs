/*
 * Copyright (c) 2019 ProtocolCash
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SharpBCH.Bitcoin;
using SharpBCH.Util;

namespace SharpBCH.Block
{
    /// <summary>
    ///     Represents a bitcoin block header
    ///     https://en.bitcoin.it/wiki/Block_hashing_algorithm
    /// </summary>
    public class BlockHeader : Serializer
    {
        public uint BlockVersion { get; private set; }
        public string PrevBlockHash { get; private set; }
        public byte[] MerkleRootHash { get; private set; }
        public uint TimeStamp { get; private set; }
        public uint DiffTarget { get; private set; }
        public uint Nonce { get; private set; }
        public string BlockHashHex { get; private set; }

        public bool LengthMatch { get; private set; }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        ///     - creates an empty block header object
        ///     - used for LiteDB queries
        /// </summary>
        public BlockHeader() : base(new List<byte>())
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        ///     - decodes the block header data
        /// </summary>
        /// <param name="headerBytes">raw block header as byte array</param>
        public BlockHeader(IEnumerable<byte> headerBytes) : base(headerBytes)
        {
            try
            {
                Decode();
            }
            catch (Exception e)
            {
                throw new BlockException("Error decoding block header.", e);
            }
        }

        /// <summary>
        ///     Decode - Decodes a raw bitcoin block header
        /// </summary>
        private void Decode()
        {
            // block version number
            BlockVersion = ReadUInt();

            // previous block hash
            PrevBlockHash = ByteHexConverter.ByteArrayToHex(ReadSlice(32).Reverse().ToArray());

            // merkle root hash
            MerkleRootHash = ReadSlice(32);

            // block timestamp (seconds since 1970-01-01T00:00 UTC)
            TimeStamp = ReadUInt();

            // difficulty target in compact format
            DiffTarget = ReadUInt();

            // nonce
            Nonce = ReadUInt();

            // strict validation - we should be at the end of the header
            LengthMatch = Offset == ByteData.Length;

            // block hash = sha256(sha256(header_data)) -> reverse byte data -> convert to hex
            SHA256 sha256 = new SHA256Managed();
            BlockHashHex = ByteHexConverter.ByteArrayToHex(sha256.ComputeHash(sha256.ComputeHash(ByteData)).Reverse().ToArray());
        }
    }
}