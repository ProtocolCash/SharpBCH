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
using SharpBCH.Util;

namespace SharpBCH.Script
{

    /// <summary>
    ///     Exception specific to Bitcoin Script validity issues
    ///     - thrown by the public functions of this class
    ///     - should wrap an innerException
    /// </summary>
    public class BitcoinScriptException : Exception
    {
        public BitcoinScriptException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    ///     Represents a Bitcoin Script
    /// </summary>
    public class Script
    {
        /// <summary>
        ///     Creates a human readable script with data as hex from the bitcoin script
        /// </summary>
        /// <returns>human readable script</returns>
        public override string ToString()
        {
            var ret = string.Join(" ", OpCodes);
            foreach (var data in DataChunks)
                ret = ret.ReplaceFirst("OP_DATA", ByteHexConverter.ByteArrayToHex(data));
            return ret;
        }

        // byte array of the script
        public byte[] ScriptBytes { get; }
        // ordered op codes of the script
        public List<OpCodeType> OpCodes { get; }
        // data in the script
        public List<byte[]> DataChunks { get; }

        /// <summary>
        ///     Constructor
        ///     - saves the specified raw script
        ///     - generates op_code list and retrieves data chunks
        /// </summary>
        /// <param name="script">script as byte array</param>
        public Script(IEnumerable<byte> script)
        {
            // save the script bytes
            ScriptBytes = script.ToArray();
            // create the op_code and data storage
            OpCodes = new List<OpCodeType>();
            DataChunks = new List<byte[]>();

            try
            {
                // populate op_codes and retrieve data sections from the script
                Decode();
            }
            catch (Exception e)
            {
                throw new BitcoinScriptException("Error decoding script.", e);
            }
        }

        /// <summary>
        ///     Decodes the script into op_codes and data sections
        /// </summary>
        private void Decode()
        {
            // Iterate over each byte in the script.
            // When a data chunk (non-op_code section) is encountered,
            //   i will be incremented so as to skip reading op_codes from the data
            for (var i = 0; i < ScriptBytes.Length; i++)
            {
                // OP_PUSH - indicates a data section of ScriptBytes[i] length
                if (ScriptBytes[i] > 0 && ScriptBytes[i] < 76)
                {
                    // save the data chunk and add an OP_DATA code to the script
                    var dataLength = ScriptBytes[i];
                    DataChunks.Add(ScriptBytes.Skip(i + 1).Take(dataLength).ToArray());
                    OpCodes.Add(OpCodeType.OP_DATA);
                    // increment i to skip over the data section during further op_code processing
                    i += dataLength;
                }
                else switch (ScriptBytes[i])
                    {
                        // OP_PUSHDATA1 - indicates a data section of ScriptBytes[i+1] length
                        case (byte)OpCodeType.OP_PUSHDATA1:
                            {
                                // save the data chunk and an OP_DATA code to the script
                                var dataLength = ScriptBytes[i + 1];
                                DataChunks.Add(ScriptBytes.Skip(i + 2).Take(dataLength).ToArray());
                                OpCodes.Add(OpCodeType.OP_DATA);
                                // increment i to skip over the data section during further op_code processing
                                i += 1 + dataLength;
                                break;
                            }
                        // OP_PUSHDATA2 - indicates a data section with 2 bytes indicating length
                        case (byte)OpCodeType.OP_PUSHDATA2:
                            {
                                // get 2 byte count and data
                                var dataLength = BitConverter.ToInt16(ScriptBytes, i + 1);
                                DataChunks.Add(ScriptBytes.Skip(i + 3).Take(dataLength).ToArray());
                                OpCodes.Add(OpCodeType.OP_DATA);
                                i += 2 + dataLength;
                                break;
                            }
                        // OP_PUSHDATA4 - indicates a data section with 4 bytes indicating length
                        case (byte)OpCodeType.OP_PUSHDATA4:
                            {
                                // get 4 byte count and data
                                var dataLength = BitConverter.ToInt32(ScriptBytes, i + 1);
                                DataChunks.Add(ScriptBytes.Skip(i + 5).Take(dataLength).ToArray());
                                OpCodes.Add(OpCodeType.OP_DATA);
                                i += 4 + dataLength;
                                break;
                            }
                        // any other OP_CODE (non-data identifier)
                        default:
                            {
                                // check if this is a valid/known op code, and add it to the list
                                if (Enum.IsDefined(typeof(OpCodeType), ScriptBytes[i]) && ScriptBytes[i] != 218) // 218 = DA
                                    OpCodes.Add((OpCodeType)ScriptBytes[i]);
                                else
                                {
                                    throw new ArgumentException("Invalid op_code encountered at byte " + i + ": " + (int) ScriptBytes[i]);
                                }
                                break;
                            }
                    }
            }
        }
    }

    
}