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

using System.Linq;
using SharpBCH.Script;
using SharpBCH.Util;

namespace SharpBCH.Transaction
{
    /// <summary>
    ///     Represents a Bitcoin Transaction Output
    /// </summary>
    public class Output
    {
        /// <summary>
        ///     Provides a human readable script with data as hex from the bitcoin script
        /// </summary>
        /// <returns>human readable script</returns>
        public override string ToString()
        {
            return _script.ToString();
        }

        // value spent to the output
        public ulong Value { get; set; }
        // type of output, if known type
        public ScriptType Type { get; private set; }
        // cash address to which the output spends
        public string Address { get; private set; }

        // storage of the output script
        private Script.Script _script;

        public string ScriptDataHex => _script.DataChunks.Count > 0 ? ByteHexConverter.ByteArrayToHex(_script.DataChunks[0]) : null;

        // public getter/setter for the output script
        public byte[] Script
        {
            get => _script.ScriptBytes;
            set
            {
                // save and decode the output script
                _script = new Script.Script(value);
                // identify the type of output script
                Type = GetOutputType();

                // decode the address data to Cash Account format
                // for anything else, set address to an empty string
                switch (Type)
                {
                    case ScriptType.P2PKH:
                        Address = CashAddress.CashAddress.EncodeCashAddress(AddressPrefix.bitcoincash, ScriptType.P2PKH,
                            _script.DataChunks[0]);
                        break;
                    case ScriptType.P2SH:
                        Address = CashAddress.CashAddress.EncodeCashAddress(AddressPrefix.bitcoincash, ScriptType.P2SH,
                            _script.DataChunks[0]);
                        break;
                    default:
                        Address = "";
                        break;
                }
            }
        }

        /// <summary>
        ///     Identifies the type of output for a bitcoin script
        /// </summary>
        /// <returns>type of output</returns>
        private ScriptType GetOutputType()
        {
            // OP_RETURN Data output (non-transactional)
            if (_script.OpCodes[0] == OpCodeType.OP_RETURN)
                return ScriptType.DATA;

            switch (_script.OpCodes.Count)
            {
                // P2PKH - Pay-to-PublicKey-Hash
                // OP_DUP OP_HASH160 <address> OP_EQUALVERIFY OP_CHECKSIG
                case 5 when
                    _script.OpCodes[0] == OpCodeType.OP_DUP &&
                    _script.OpCodes[1] == OpCodeType.OP_HASH160 &&
                    _script.OpCodes[2] == OpCodeType.OP_DATA &&
                    _script.OpCodes[3] == OpCodeType.OP_EQUALVERIFY &&
                    _script.OpCodes[4] == OpCodeType.OP_CHECKSIG:
                    return ScriptType.P2PKH;

                // P2SH - Pay-to-Script-Hash
                // OP_HASH160 <address=Hash160(RedeemScript)> OP_EQUAL
                case 3 when
                    _script.OpCodes[0] == OpCodeType.OP_HASH160 &&
                    _script.OpCodes[1] == OpCodeType.OP_DATA &&
                    _script.OpCodes[2] == OpCodeType.OP_EQUAL:
                    return ScriptType.P2SH;

                // anything else
                default:
                    return ScriptType.OTHER;
            }
        }

        /// <summary>
        ///     Returns the hash160 encoded data from a P2PKH or P2SH output
        /// </summary>
        /// <returns></returns>
        public byte[] GetHash160()
        {
            if (Type == ScriptType.P2PKH || Type == ScriptType.P2SH)
                return _script.DataChunks[0];
            return new byte[0];
        }

        /// <summary>
        ///     Returns the given number of bytes of op_return data from a P2PKH or P2SH output
        /// </summary>
        /// <returns></returns>
        public byte[] GetOpReturnData(int maxBytes)
        {
            if (Type != ScriptType.DATA)
                return new byte[0];

            return _script.DataChunks[0].Length <= maxBytes ? _script.DataChunks[0] :
                _script.DataChunks[0].SkipLast(maxBytes - _script.DataChunks[0].Length).ToArray();
        }
    }
}