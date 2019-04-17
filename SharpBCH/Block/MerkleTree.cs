using System;
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