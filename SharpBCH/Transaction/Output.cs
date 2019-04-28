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
        /// <returns>human readable script and output value</returns>
        public override string ToString()
        {
            return "Script: " + Script + "; Value: " + Value;
        }

        /// <summary>
        ///     value spent to the output
        /// </summary>
        public ulong Value { get; set; }

        /// <summary>
        ///     type of output, if known type
        /// </summary>
        public ScriptType Type { get; private set; }

        /// <summary>
        ///     cash address to which the output spends
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        ///     the output script
        /// </summary>
        public Script.Script Script;

        /// <summary>
        ///     full output script as hex
        /// </summary>
        public string ScriptHex => Script.ScriptBytes.Length > 0 ? ByteHexConverter.ByteArrayToHex(Script.ScriptBytes) : null;

        /// <summary>
        ///     public getter/setter for the output script
        /// </summary>
        public byte[] ScriptBytes
        {
            get => Script.ScriptBytes;
            set
            {
                // save and decode the output script
                Script = new Script.Script(value);
                // identify the type of output script
                Type = GetOutputType();

                // decode the address data to Cash Account format
                // for anything else, set address to an empty string
                switch (Type)
                {
                    case ScriptType.P2PKH:
                        Address = CashAddress.CashAddress.EncodeCashAddress(AddressPrefix.bitcoincash, ScriptType.P2PKH,
                            Script.DataChunks[0]);
                        break;
                    case ScriptType.P2SH:
                        Address = CashAddress.CashAddress.EncodeCashAddress(AddressPrefix.bitcoincash, ScriptType.P2SH,
                            Script.DataChunks[0]);
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
            if (Script.OpCodes[0] == OpCodeType.OP_RETURN)
                return ScriptType.DATA;

            switch (Script.OpCodes.Count)
            {
                // P2PKH - Pay-to-PublicKey-Hash
                // OP_DUP OP_HASH160 <address> OP_EQUALVERIFY OP_CHECKSIG
                case 5 when
                    Script.OpCodes[0] == OpCodeType.OP_DUP &&
                    Script.OpCodes[1] == OpCodeType.OP_HASH160 &&
                    Script.OpCodes[2] == OpCodeType.OP_DATA &&
                    Script.OpCodes[3] == OpCodeType.OP_EQUALVERIFY &&
                    Script.OpCodes[4] == OpCodeType.OP_CHECKSIG:
                    return ScriptType.P2PKH;

                // P2SH - Pay-to-Script-Hash
                // OP_HASH160 <address=Hash160(RedeemScript)> OP_EQUAL
                case 3 when
                    Script.OpCodes[0] == OpCodeType.OP_HASH160 &&
                    Script.OpCodes[1] == OpCodeType.OP_DATA &&
                    Script.OpCodes[2] == OpCodeType.OP_EQUAL:
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
                return Script.DataChunks[0];
            return new byte[0];
        }

        /// <summary>
        ///     Returns the all data chunks from a script, IF the script is an op_return
        /// </summary>
        /// <returns></returns>
        public List<byte[]> GetOpReturnData()
        {
            return Type != ScriptType.DATA ? new List<byte[]>() : Script.DataChunks;
        }
    }
}