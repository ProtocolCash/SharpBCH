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
using System.Runtime.InteropServices;

namespace SharpBCH.CashAddress
{
    /// <summary>
    ///     (Probably over-) optimized ByteToHex conversion script
    ///     - doesn't get faster then this unsafe code!
    /// </summary>
    public unsafe class ByteHexConverter
    {
        private static readonly uint[] Lookup32Unsafe = CreateLookup32Unsafe();

        private static readonly uint* Lookup32UnsafeP =
            (uint*)GCHandle.Alloc(Lookup32Unsafe, GCHandleType.Pinned).AddrOfPinnedObject();

        /// <summary>
        ///     Create a table of hex pairs from 0 to 255
        /// </summary>
        /// <returns>lookup table array</returns>
        private static uint[] CreateLookup32Unsafe()
        {
            var result = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var s = i.ToString("X2");
                if (BitConverter.IsLittleEndian)
                    result[i] = s[0] + ((uint)s[1] << 16);
                else
                    result[i] = s[1] + ((uint)s[0] << 16);
            }

            return result;
        }

        /// <summary>
        ///     Use lookup table to convert Byte Array to hex string
        /// </summary>
        /// <param name="bytes">input array to convert</param>
        /// <returns>hex conversion of the byte array</returns>
        public static string ByteArrayToHex(byte[] bytes)
        {
            var lookupP = Lookup32UnsafeP;
            var result = new char[bytes.Length * 2];
            fixed (byte* bytesP = bytes)
            fixed (char* resultP = result)
            {
                var resultP2 = (uint*)resultP;
                for (var i = 0; i < bytes.Length; i++) resultP2[i] = lookupP[bytesP[i]];
            }

            return new string(result);
        }

        /// <summary>
        ///     Convert hex to byte array using ToByte
        /// </summary>
        /// <param name="hex">hex string input to convert</param>
        /// <returns>byte array representation of the hex string</returns>
        public static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}