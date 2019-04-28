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

namespace SharpBCH.Script
{
    /// <summary>
    ///     Exception specific to Cash Account formatting and input issues
    ///     - thrown by the public functions of this class
    ///     - should wrap an innerException
    /// </summary>
    public class BitcoinScriptBuilderException : Exception
    {
        public BitcoinScriptBuilderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     Bitcoin Script Builder
    ///     - Creates bitcoin output scripts from a cashAddress or scriptType and public key hash/script hash
    /// </summary>
    public static class ScriptBuilder
    {
        /// <summary>
        ///     Create a Bitcoin Script for a given Cash Address
        /// </summary>
        /// <param name="cashAddress">Cash Address to create an output script to pay</param>
        /// <returns>Bitcoin Output Script to spend to the given Cash Address</returns>
        public static Script CreateOutputScript(string cashAddress)
        {
            try
            {
                // decode the cash address
                var decoded = CashAddress.CashAddress.DecodeCashAddress(cashAddress);
                // create output script with decoded type and hash160
                return CreateOutputScript(decoded.Type, decoded.Hash);
            }
            catch (BitcoinScriptBuilderException)
            {
                // just rethrow internally handled exception 
                throw;
            }
            catch (Exception e)
            {
                // wrap 
                throw new BitcoinScriptBuilderException("Error creating output script.", e);
            }
        }

        /// <summary>
        ///     Create a Bitcoin Script for a given scriptType and public key hash
        /// </summary>
        /// <param name="scriptType">type of hash</param>
        /// <param name="hash160">public key hash or script hash</param>
        /// <returns>Bitcoin Output Script to spend to the given scriptType and hash</returns>
        public static Script CreateOutputScript(ScriptType scriptType, byte[] hash160)
        {
            try
            {
                // create the raw bytes for the script
                var script = new List<byte>();
                switch (scriptType)
                {
                    // P2PKH is OP_DUP OP_HASH160 <hash160> OP_EQUALVERIFY OP_CHECKSIG
                    case ScriptType.P2PKH:
                        script.Add((byte) OpCodeType.OP_DUP);
                        script.Add((byte) OpCodeType.OP_HASH160);
                        script = script.Concat(GetOpPushForLength((uint) hash160.Length)).ToList();
                        script = script.Concat(hash160).ToList();
                        script.Add((byte) OpCodeType.OP_EQUALVERIFY);
                        script.Add((byte) OpCodeType.OP_CHECKSIG);
                        break;

                    // P2SH is OP_HASH160 <hash160> OP_EQUAL
                    case ScriptType.P2SH:
                        script.Add((byte)OpCodeType.OP_HASH160);
                        script = script.Concat(GetOpPushForLength((uint) hash160.Length)).ToList();
                        script = script.Concat(hash160).ToList();
                        script.Add((byte)OpCodeType.OP_EQUAL);
                        break;

                    // error for any other script type
                    default:
                        throw new ArgumentException("Invalid Script Type. Received: " + hash160.Length +
                                                    ". Expected: " + string.Join(", ",
                                                        Enum.GetValues(typeof(ScriptType))).Cast<ScriptType>() + ".");
                }

                // build the script and return
                return new Script(script);
            }
            catch (Exception e)
            {
                throw new BitcoinScriptBuilderException("Error creating output script.", e);
            }
        }

        /// <summary>
        ///     Create a Bitcoin OP_RETURN Script containing the given data
        /// </summary>
        /// <param name="data">data to include in the op_return</param>
        /// <returns>Bitcoin OP_RETURN Output Script containing the given data</returns>
        public static Script CreateOutputOpReturn(byte[] data)
        {
            try
            {
                // An OP_RETURN is simply OP_RETURN <data>
                var script = new List<byte>{ (byte) OpCodeType.OP_RETURN }.Concat(data);

                // build the script and return
                return new Script(script);

            }
            catch (Exception e)
            {
                throw new BitcoinScriptBuilderException("Error creating output script.", e);
            }
        }

        /// <summary>
        ///     Gets the OP_CODE(s) needed to push data of a given length
        /// </summary>
        /// <param name="dataLength">length of data to push to script stack</param>
        /// <param name="allowOpZero">Use OP_0 for pushing 0 length if true, otherwise uses PUSHDATA1 for length 0</param>
        /// <returns>Opcode bytes</returns>
        public static byte[] GetOpPushForLength(ulong dataLength, bool allowOpZero = true)
        {
            if (dataLength == 0 && !allowOpZero)
                return new[] { (byte)OpCodeType.OP_PUSHDATA1, (byte)dataLength };
            if (dataLength == 0 && allowOpZero)
                return new byte[0];
            // Opcode 1-75 indicate a data push of that length
            if (dataLength > 1 && dataLength < 76)
                return new[] { (byte)dataLength };
            // 1 byte pushdata
            if (dataLength < 255)
                return new[] { (byte) OpCodeType.OP_PUSHDATA1, (byte)dataLength };
            // 2 byte pushdata
            if (dataLength < 255 * 255)
                return new[] { (byte)OpCodeType.OP_PUSHDATA2, (byte)((dataLength & 0xFF00) >> 8), (byte)(dataLength & 0x00FF) };
            // 4 byte pushdata
            return new[] { (byte)OpCodeType.OP_PUSHDATA4, (byte)((dataLength & 0xFF000000) >> 24),
            (byte)((dataLength & 0x00FF0000) >> 16), (byte)((dataLength & 0xFF00) >> 8), (byte)(dataLength & 0x00FF) };
            
        }
    }
}