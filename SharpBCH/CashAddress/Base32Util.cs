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

namespace SharpBCH.CashAddress
{
    /// <summary>
    ///     Base32 Encoder/Decoder Utility
    /// </summary>
    internal static class Base32Util
    {
        // character set/alphabet for base32
        private static readonly char[] Digits = "qpzry9x8gf2tvdw0s3jn54khce6mua7l".ToCharArray();
        // character set reverse mapping (character to index)
        private static readonly Dictionary<char, int> CharMap = new Dictionary<char, int>();

        /// <summary>
        ///     Static constructor to initialize the reverse character map
        /// </summary>
        static Base32Util()
        {
            // create character map / index lookup table
            for (var i = 0; i < Digits.Length; i++)
                CharMap[Digits[i]] = i;
        }

        /// <summary>
        ///     Exception to wrap all base32 encode/decode exceptions
        /// </summary>
        public class Base32EncoderException : Exception
        {
            public Base32EncoderException(string message, Exception internalException) : base(message, internalException)
            {
            }
        }

        /// <summary>
        ///     Encodes byte data as a Base32 string
        ///     - expected input is raw hash160 byte from output script
        /// </summary>
        /// <param name="data">byte data</param>
        /// <returns>base32 string</returns>
        public static string Encode(byte[] data)
        {
            try
            {
                if (data.Length == 0)
                    throw new ArgumentException("Data to encode cannot be empty.");

                return EncodeInternal(data);
            }
            catch (Exception e)
            {
                throw new Base32EncoderException("Error encoding byte data.", e);
            }
        }

        /// <summary>
        ///     Decodes Base32 string in byte data
        ///     - given proper input, results in raw hash160 byte data for output script usage
        /// </summary>
        /// <param name="base32">base32 input string to convert</param>
        /// <returns>byte data</returns>
        public static byte[] Decode(string base32)
        {
            try
            {
                if (base32.Length == 0)
                    throw new ArgumentException("Invalid encoded string");

                return DecodeInternal(base32);
            }
            catch (Exception e)
            {
                throw new Base32EncoderException("Error decoding byte data.", e);
            }
        }

        /// <summary>
        ///     Encodes byte data as a Base32 string
        /// </summary>
        /// <param name="data">byte data</param>
        /// <returns>base32 string</returns>
        private static string EncodeInternal(IEnumerable<byte> data)
        {
            var base32 = string.Empty;
            foreach (var value in data)
            {
                if (value < 32)
                    base32 += Digits[value];
                else
                    throw new ArgumentException("Invalid value encountered in input string: " + value);
            }
            return base32;
        }

        /// <summary>
        ///     Decodes Base32 string in byte data
        /// </summary>
        /// <param name="encoded">base32 string</param>
        /// <returns>byte data</returns>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static byte[] DecodeInternal(string encoded)
        {
            var result = new byte[encoded.Length];
            var next = 0;
            foreach (var c in encoded.ToCharArray())
            {
                if (!CharMap.ContainsKey(c)) throw new ArgumentException("Invalid character: " + c);
                result[next++] = (byte)CharMap[c];
            }

            return result;
        }
    }
}