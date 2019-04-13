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

namespace SharpBCH.Bitcoin
{
    /// <summary>
    ///     Exception specific to Serializer formatting and input issues
    ///     - thrown by the protected functions of this class
    /// </summary>
    public class SerializerException : Exception
    {
        public SerializerException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Bitcoin Data Serializer
    ///     - reads in raw byte data from a bitcoin transaction or block
    /// </summary>
    public abstract class Serializer
    {
        protected byte[] ByteData;
        protected int Offset;

        /// <summary>
        ///     Constructor
        ///     - saves the raw bytes
        /// </summary>
        /// <param name="byteData">raw byte array</param>
        protected Serializer(IEnumerable<byte> byteData)
        {
            ByteData = byteData.ToArray();
        }

        /// <summary>
        ///     ReadUInt - returns a 16bit integer from the transaction bytes at the current offset
        /// </summary>
        /// <returns>int</returns>
        private uint ReadUInt16()
        {
            var ret = BitConverter.ToUInt16(ByteData, Offset);
            Offset += 2;
            return ret;
        }

        /// <summary>
        ///     ReadUInt - returns a 32bit integer from the transaction bytes at the current offset
        /// </summary>
        /// <returns>int</returns>
        protected uint ReadUInt()
        {
            var ret = BitConverter.ToUInt32(ByteData, Offset);
            Offset += 4;
            return ret;
        }

        /// <summary>
        ///     ReadUInt - returns a 64bit integer from the transaction bytes at the current offset
        /// </summary>
        /// <returns>int</returns>
        protected ulong ReadUInt64()
        {
            var ret = BitConverter.ToUInt64(ByteData, Offset);
            Offset += 8;
            return ret;
        }

        /// <summary>
        ///     ReadVector - returns a vector slice from the transaction bytes at the current offset
        /// </summary>
        /// <returns>bytes</returns>
        protected byte[] ReadVector()
        {
            var count = ReadVarLenInt();
            var ret = new byte[0];
            if (count > int.MaxValue)
                throw new SerializerException("ReadVarLenInt failed - count is above maximum expected value.");

            for (var i = 0; i < (int)count; i++)
            {
                var slice = ReadSlice((int)ReadVarLenInt());
                var buff = new byte[ret.Length + slice.Length];
                Buffer.BlockCopy(ret, 0, buff, 0, ret.Length);
                Buffer.BlockCopy(slice, 0, buff, ret.Length, slice.Length);
                ret = buff;
            }

            return ret;
        }

        /// <summary>
        ///     ReadSlice - returns a slice of the transaction bytes at the current offset
        /// </summary>
        /// <param name="size">size of slice to return</param>
        /// <returns>bytes</returns>
        protected byte[] ReadSlice(int size)
        {
            Offset += size;
            return ByteData.Skip(Offset - size).Take(size).ToArray();
        }

        /// <summary>
        ///     ReadVarLenInt - returns a variable length bitcoin integer from the transaction bytes at the current offset
        ///     - if first byte is below 0xFD, length is single byte (uint8)
        ///     - if first byte is 0xFD, length is the following 2 bytes (uint16)
        ///     - if first byte is 0xFE, length is the following 4 bytes (uint32)
        ///     - if first byte is 0xFF, length is the following 8 bytes (uint64)
        /// </summary>
        /// <returns>int</returns>
        protected ulong ReadVarLenInt()
        {
            if (ByteData[Offset] < 253)
                return ByteData[Offset++];

            Offset++;
            if (ByteData[Offset - 1] == 253)
                return ReadUInt16();

            return ByteData[Offset - 1] == 254 ? ReadUInt() : ReadUInt64();
        }
    }
}