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

namespace SharpBCH.CashAddress
{
    /// <summary>
    ///     Decoder for Cash Addresses
    ///     https://www.bitcoincash.org/spec/cashaddr.html
    /// </summary>
    public class CashAddress
    {
        /// <summary>
        ///     Exception specific to Cash Account formatting and input issues
        ///     - thrown by the public functions of this class
        ///     - should wrap an innerException
        /// </summary>
        public class CashAddressException : Exception
        {
            public CashAddressException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        /// <summary>
        ///     Encodes a Cash Address from prefix, type, and hash160
        /// </summary>
        /// <param name="prefix">Cash Address prefix</param>
        /// <param name="scriptType">Bitcoin script type</param>
        /// <param name="hash160">Cash Address formatted address</param>
        /// <returns>cash address formatted bitcoin address</returns>
        public static string EncodeCashAddress(AddressPrefix prefix, ScriptType scriptType, byte[] hash160)
        {
            try
            {
                // validate hash length
                if (!ValidLength.Contains(hash160.Length * 8))
                    throw new ArgumentException("Invalid Hash Length. Received: " + hash160.Length +
                                                ". Expected: " + string.Join(", ", ValidLength) + ".");

                // validate script type
                if (!Enum.IsDefined(typeof(ScriptType), scriptType))
                    throw new ArgumentException("Invalid Script Type. Received: " + hash160.Length +
                                                ". Expected: " + string.Join(", ",
                                                    Enum.GetValues(typeof(ScriptType))).Cast<ScriptType>() + ".");
                // validate prefix
                if (!Enum.IsDefined(typeof(AddressPrefix), prefix))
                    throw new ArgumentException("Invalid Prefix. Received: " + prefix +
                                                ". Expected: " + string.Join(", ",
                                                    Enum.GetValues(typeof(AddressPrefix))).Cast<AddressPrefix>() + ".");

                // encode and return result
                return EncodeWithHeaderAndChecksum(prefix.ToString(), (byte)scriptType, hash160);
            }
            catch (Exception e)
            {
                throw new CashAddressException("Error decoding cash address.", e);
            }
        }

        /// <summary>
        ///     Decodes a CashAddress into prefix, type, and hash160
        /// </summary>
        /// <param name="address">Cash Address formatted address</param>
        /// <returns></returns>
        public static DecodedBitcoinAddress DecodeCashAddress(string address)
        {
            try
            {
                // validate prefix
                var validPrefix = false;
                foreach (var prefix in Enum.GetValues(typeof(AddressPrefix)))
                    if (address.StartsWith(prefix + ":"))
                        validPrefix = true;
                if (!validPrefix)
                    throw new ArgumentException("Invalid Prefix. Expected: " + string.Join(", ",
                                                    Enum.GetValues(typeof(AddressPrefix))).Cast<AddressPrefix>() + ".");

                // TODO: validate checksum

                return Decode(address);
            }
            catch (Exception e)
            {
                throw new CashAddressException("Error decoding cash address.", e);
            }
        }

        // Used by polymod checksum generation
        private static readonly ulong[] Generator = { 0x98f2bc8e61, 0x79b76d99e2, 0xf33e5fb3c4, 0xae2eabe2a8, 0x1e4f43e470 };
        // valid hash lengths in bits
        private static readonly int[] ValidLength = { 160, 192, 224, 256, 320, 384, 448, 512 };

        /// <summary>
        ///     Given Cash Address version byte, returns address type (Key Hash or Script Hash)
        /// </summary>
        /// <param name="versionByte"></param>
        /// <returns>P2PKH (key hash) or P2SH (script hash)</returns>
        private static ScriptType GetType(byte versionByte)
        {
            switch (versionByte)
            {
                case 0:
                    return ScriptType.P2PKH;
                case 8:
                    return ScriptType.P2SH;
                default:
                    throw new ArgumentException("Invalid address type in version byte: " + versionByte);
            }
        }

        /// <summary>
        ///     Decodes a CashAddress into prefix, type, and hash160
        /// </summary>
        /// <param name="address">Cash Address formatted address</param>
        /// <returns></returns>
        private static DecodedBitcoinAddress Decode(string address)
        {
            // split at the separator colon; format address in lower case
            var pieces = address.ToLower().Split(':');
            // trim the prefix (should be "bitcoincash" or "bchtest") - the first chunk separated by colon
            var prefix = pieces[0];
            // base32 decode the payload (second chunk)
            var payload = Base32Util.Decode(pieces[1]);
            // trim the checksum
            var data = payload.Take(payload.Length - 8).ToArray();
            // convert byte data from 5bit to 8bit
            var payloadData = ConvertBits(data, 5, 8, true);
            // version byte (and type) is determined by the first byte in the result
            var versionByte = payloadData[0];
            var type = GetType(versionByte);
            // hash is the rest of the data after the version byte
            var hash = payloadData.Skip(1).ToArray();

            return new DecodedBitcoinAddress(prefix, type, hash);
        }

        /// <summary>
        ///     Lookup size of hash in version byte table
        /// </summary>
        /// <param name="hash">Hash to encode in 8bit</param>
        /// <returns>size bits</returns>
        private static byte GetHashSizeBits(IReadOnlyCollection<byte> hash)
        {
            return (hash.Count < 40) ?
                (byte)((hash.Count - 20) / 4) :
                (byte)(((hash.Count) - 40) / 4 + 4);
        }

        /// <summary>
        ///     Encodes a cash address
        /// </summary>
        /// <param name="scriptType">the bitcoin script type in byte format (P2PKH = 0x0, or P2SH = 0x8)</param>
        /// <param name="hash">byte array representing address</param>
        /// <param name="header">network  - bitcoincash, bchtest, bchreg</param>
        /// <returns>base32 encoded string with checksum and header</returns>
        private static string EncodeWithHeaderAndChecksum(string header, byte scriptType, byte[] hash)
        {
            // calculate the version byte - in bits, 0 reserved, 1-4 address type, 4-7 size of hash
            var versionByte = scriptType + GetHashSizeBits(hash);
            // convert byte array from 8bits to 5, append prefix 0 bit and version byte
            var base5 = ConvertBits(new byte[1] { (byte)versionByte }.Concat(hash).ToArray(), 8, 5);
            // generate checksum from base5 array
            var checksumBytes = CreateChecksum(base5, header);
            // append the checksum to the base5 array
            var combined = base5.Concat(checksumBytes).ToArray();
            // return address as base32 encoded with header
            return header + ":" + Base32Util.Encode(combined);
        }

        /// <summary>
        /// Computes a checksum from the given input data as specified for the cash address
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startValue"></param>
        /// <returns></returns>
        private static ulong PolyMod(byte[] data, ulong startValue = 1)
        {
            foreach (var value in data)
            {
                var topBits = startValue >> 35;
                startValue = ((startValue & 0x07ffffffff) << 5) ^ value;
                startValue = Generator.Where((t, j) => ((topBits >> j) & 1).Equals(1)).Aggregate(startValue, (current, t) => current ^ t);
            }

            return startValue ^ 1;
        }

        /// <summary>
        ///     Creates the 40 bits BCH checksum for a cash address
        /// </summary>
        /// <param name="data">
        ///     base5 byte array of:
        ///     - The lower 5 bits of each character of the prefix. - e.g. “bit…” becomes 2,9,20,…
        ///     - A zero for the separator (5 zero bits).
        ///     - The payload by chunks of 5 bits. If necessary, the payload is padded to the right
        ///       with zero bits to complete any unfinished chunk at the end.
        ///     - Eight zeros as a “template” for the checksum.
        /// </param>
        /// <param name="header">is a mainnet transaciton</param>
        /// <returns>cash address checksum bytes</returns>
        private static IEnumerable<byte> CreateChecksum(IEnumerable<byte> data, string header)
        {
            // start values calculated with https://play.golang.org/p/o4-mMftR44D
            ulong startValue;
            switch (header)
            {
                case "bitcoincash":
                    startValue = 1058337025301;
                    break;
                case "bchtest":
                    startValue = 584719417569;
                    break;
                case "bchreg":
                    startValue = 36616869088;
                    break;
                default:
                    throw new ArgumentException("Invalid header/prefix.");
            }

            // calculate polymod from known header startValue
            var checksum = PolyMod(data.Concat(new byte[8]).ToArray(), startValue);

            // convert to byte 5 array
            var result = new byte[8];
            for (var i = 0; i < 8; ++i)
                result[i] = (byte)((checksum >> (5 * (7 - i))) & 0x1f);

            return result;
        }

        /// <summary>
        ///     Converts an array of bytes from FROM bits to TO bits
        /// </summary>
        /// <param name="data">byte array in FROM bits</param>
        /// <param name="from">word size in source array</param>
        /// <param name="to">word size for destination array</param>
        /// <param name="strictMode">leaves prefix 0 bits</param>
        /// <returns>byte array in TO bits</returns>
        private static byte[] ConvertBits(byte[] data, int from, int to, bool strictMode = false)
        {
            var d = data.Length * from / (double)to;
            var length = strictMode ? (int)Math.Floor(d) : (int)Math.Ceiling(d);
            var mask = (1 << to) - 1;
            var result = new byte[length];
            var index = 0;
            var accumulator = 0;
            var bits = 0;
            foreach (var value in data)
            {
                accumulator = (accumulator << from) | value;
                bits += from;
                while (bits >= to)
                {
                    bits -= to;
                    result[index] = (byte)((accumulator >> bits) & mask);
                    ++index;
                }
            }

            if (strictMode) return result;
            if (bits <= 0) return result;

            result[index] = (byte)((accumulator << (to - bits)) & mask);
            ++index;

            return result;
        }
    }
}
