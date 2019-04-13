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

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SharpBCH.Bitcoin;
using SharpBCH.Util;

namespace SharpBCH.Transaction
{

    /// <summary>
    ///     Represents a bitcoin transaction
    /// </summary>
    public class Transaction : Serializer
    {
        public Input[] Inputs { get; private set; }
        public Output[] Outputs { get; private set; }

        public string TXIDHex { get; set; }
        public uint TXVersion { get; private set; }
        public uint LockTime { get; private set; }
        public bool LengthMatch { get; private set; }
        public string IncludedInBlockHex { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        ///     - decodes the transaction data
        /// </summary>
        /// <param name="txBytes">raw transaction as byte array</param>
        public Transaction(IEnumerable<byte> txBytes) : base(txBytes)
        {
            var sha256 = new SHA256Managed();
            // double sha256 hash, reverse bytes, then convert to hex
            TXIDHex = ByteHexConverter.ByteArrayToHex(sha256.ComputeHash(sha256.ComputeHash(ByteData)).Reverse().ToArray());
            Decode();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        ///     - creates a transaction object given transaction properties
        ///     - used for transactions found in blocks
        /// </summary>
        public Transaction(IEnumerable<byte> txBytes, string inclusionBlockHex, uint txVersion, Input[] inputs, Output[] outputs, uint lockTime) : base(txBytes)
        {
            var sha256 = new SHA256Managed();
            TXIDHex = ByteHexConverter.ByteArrayToHex(sha256.ComputeHash(sha256.ComputeHash(ByteData)).Reverse().ToArray());
            IncludedInBlockHex = inclusionBlockHex;
            TXVersion = txVersion;
            Inputs = inputs;
            Outputs = outputs;
            LockTime = lockTime;
            LengthMatch = true;
        }

        /// <summary>
        ///     Decode - Decodes a raw bitcoin transaction
        /// </summary>
        private void Decode()
        {
            // tx version - uint32
            TXVersion = ReadUInt();

            // get the number of inputs - vin length
            Inputs = new Input[ReadVarLenInt()];

            // read all the inputs
            for (var i = 0; i < Inputs.Length; ++i)
                Inputs[i] = new Input
                {
                    Hash = ReadSlice(32),
                    Index = ReadUInt(),
                    Script = ReadSlice(
                        (int)ReadVarLenInt()), // script length maximum is 520 bytes, so casting to int should be fine
                    Sequence = ReadUInt()
                };


            // get the number of inputs - vout length
            Outputs = new Output[ReadVarLenInt()];

            // read all the outputs
            for (var i = 0; i < Outputs.Length; ++i)
                Outputs[i] = new Output
                {
                    Value = ReadUInt64(),
                    Script = ReadSlice(
                        (int)ReadVarLenInt()) // script length maximum is 520 bytes, so casting to int should be fine
                };

            LockTime = ReadUInt();

            // strict validation - we should be at the end of the transaction
            LengthMatch = Offset == ByteData.Length;
        }
    }
}
