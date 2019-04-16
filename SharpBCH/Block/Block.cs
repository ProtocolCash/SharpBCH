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
using SharpBCH.Bitcoin;
using SharpBCH.Transaction;

namespace SharpBCH.Block
{        
    
    /// <summary>
    ///     Exception specific to Block decoding and input issues
    ///     - thrown by the public functions of this class
    ///     - should wrap an innerException
    /// </summary>
    public class BlockException : Exception
    {
        public BlockException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     Represents and decodes a full bitcoin block
    /// </summary>
    public class Block : Serializer
    {
        public BlockHeader Header { get; private set; }
        public Transaction.Transaction[] Transactions { get; private set; }

        public uint BlockSize { get; private set; }
        public string BlockHash => Header.BlockHashHex;

        public bool LengthMatch { get; private set; }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        ///     - decodes the block data
        /// </summary>
        /// <param name="blockBytes">raw block of block as byte array</param>
        public Block(IEnumerable<byte> blockBytes) : base(blockBytes)
        {
            try
            {
                Decode();
            }
            catch (Exception e)
            {
                throw new BlockException("Error decoding block.", e);
            }
        }

        /// <summary>
        ///     Decode - Decodes a raw bitcoin block
        /// </summary>
        private void Decode()
        {
            BlockSize = (uint)ByteData.Length;

            // header is 80bytes
            Header = new BlockHeader(ReadSlice(80));

            // get the number of transactions
            Transactions = new Transaction.Transaction[ReadVarLenInt()];

            // read all the transactions
            for (var i = 0; i < Transactions.Length; ++i)
                // decode the transaction
                Transactions[i] = DecodeTX();

            // validate length - should be at the end of the block
            LengthMatch = Offset == ByteData.Length;
        }

        /// <summary>
        ///     Decode - Decodes a raw bitcoin transaction
        /// </summary>
        private Transaction.Transaction DecodeTX()
        {
            var origOffset = Offset;
            // tx version - uint32
            var txVersion = ReadUInt();

            // get the number of inputs - vin length
            var inputs = new Input[ReadVarLenInt()];

            // read all the inputs
            for (var i = 0; i < inputs.Length; ++i)
                inputs[i] = new Input
                {
                    Hash = ReadSlice(32),
                    Index = ReadUInt(),
                    Script = ReadSlice(
                        (int)ReadVarLenInt()), // script length maximum is 520 bytes, so casting to int should be fine
                    Sequence = ReadUInt()
                };


            // get the number of outputs - v out length
            var outputs = new Output[ReadVarLenInt()];

            // read all the outputs
            for (var i = 0; i < outputs.Length; ++i)
                outputs[i] = new Output
                {
                    // script length maximum is 1650 bytes, so casting to int should be fine
                    // https://github.com/Bitcoin-ABC/bitcoin-abc/blob/master/src/policy/policy.h#L46
                    Value = ReadUInt64(),
                    Script = ReadSlice(
                        (int)ReadVarLenInt()) 
                };

            // transaction lock_time
            var lockTime = ReadUInt();

            // hash the entire transaction for the TXID
            var txLength = Offset - origOffset;
            Offset = origOffset;

            var byteData = ReadSlice(txLength);


            return new Transaction.Transaction(byteData, BlockHash, txVersion, inputs, outputs, lockTime);
        }
    }
}
