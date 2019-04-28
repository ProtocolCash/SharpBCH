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

using SharpBCH.Util;

namespace SharpBCH.Transaction
{
    /// <summary>
    ///     Represents a Bitcoin Transaction Input
    /// </summary>
    public class Input
    {
        /// <summary>
        /// hash of the transaction that created the output being redeemed
        /// </summary>
        public byte[] Hash;

        /// <summary>
        /// index of the output for redeemed UTXO in the previous transaction
        /// </summary>
        public uint Index;


        public string ScriptHex => _script.ScriptBytes.Length > 0 ? ByteHexConverter.ByteArrayToHex(_script.ScriptBytes) : null;

        /// <summary>
        /// public getter/setter for the input script
        /// </summary>
        public byte[] Script
        {
            get => _script.ScriptBytes;
            set => _script = new Script.Script(value);
        }

        /// <summary>
        /// sequence number
        /// - at least one input must have a non-max sequence number, otherwise transaction lock_time is ignored 
        /// </summary>
        public uint Sequence;
        
        // storage of the input script 
        private Script.Script _script;
    }
}