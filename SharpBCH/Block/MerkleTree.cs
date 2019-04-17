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
using System.Linq;
using System.Security.Cryptography;
using SharpBCH.Util;

namespace SharpBCH.Block
{
    public static class MerkleTree
    {
        /// <summary>
        ///     Builds merkle root from transaction hashes
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns></returns>
        public static string BuildMerkleRoot(string[] transactions)
        {
            // loop until all merkleLeaves are consumed and we are left with the root
            while (transactions.Length != 1)
            {
                var newHashList = new List<string>();
                // balanced length of remaining leaves
                var len = (transactions.Length % 2 != 0) ? transactions.Length - 1 : transactions.Length;

                // combine leaf pairs
                for (var i = 0; i < len; i += 2)
                    newHashList.Add(CalculateParentHash(transactions[i], transactions[i + 1]));

                // calculate last element for unbalanced tree
                if (len < transactions.Length)
                    newHashList.Add(CalculateParentHash(transactions[transactions.Length - 1], transactions[transactions.Length - 1]));

                // continue work from the new leaf list
                transactions = newHashList.ToArray();
            }

            return transactions[0];
        }

        /// <summary>
        ///     Calculates double sha256 hash of two merkle nodes
        /// </summary>
        /// <param name="a">merkle node 1 hash as hex</param>
        /// <param name="b">merkle node 2 hash as hex</param>
        /// <returns></returns>
        private static string CalculateParentHash(string a, string b)
        {
            // convert string to byte arrays, reverse each, and combine
            var mergeHash = ByteHexConverter.StringToByteArray(a).Reverse().Concat(ByteHexConverter.StringToByteArray(b).Reverse()).ToArray();

            // compute and return lowercase double sha256 hash of merged hashes, reversed, in hex 
            var sha256 = SHA256.Create();
            return ByteHexConverter.ByteArrayToHex(sha256.ComputeHash(sha256.ComputeHash(mergeHash)).Reverse().ToArray()).ToLower();
        }
    }
}