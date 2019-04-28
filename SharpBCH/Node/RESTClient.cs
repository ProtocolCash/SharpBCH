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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpBCH.Block;
using SharpBCH.Util;

namespace SharpBCH.Node
{
    public class RESTClient : HTTPConnection
    {
        private readonly Uri _connectTo;

        /// <summary>
        ///     Contructor
        ///     - build connect
        /// </summary>
        /// <param name="hostname">Hostname to the REST interface e.g. 127.0.0.1</param>
        /// <param name="port">Port to the REST interface e.g. 8332</param>
        public RESTClient(string hostname, int port)
        {
            _connectTo = new UriBuilder("http", hostname, port, "rest/").Uri;
        }

        /// <summary>
        ///     Gets and decodes block data
        /// </summary>
        /// <returns>decoded block</returns>
        public Block.Block GetBlockByHash(string blockHash)
        {
            var ret = SendCommand("block/" + blockHash + ".hex").Trim();
            return WrapDecodeException(() => new Block.Block(ByteHexConverter.StringToByteArray(ret)));
        }

        /// <summary>
        ///     Gets raw block headers starting from a given block hash
        /// </summary>
        /// <returns>raw block data as hex</returns>
        public IEnumerable<BlockHeader> GetBlockHeadersStartingAt(string blockHash, int count)
        {
            var ret = SendCommand("headers/" + count + "/" + blockHash + ".hex");

            var headerBytes = WrapDecodeException(() => ByteHexConverter.StringToByteArray(ret.Trim()));

            // decode headers (80 bytes each)
            for (var headerStart = 0; headerStart < headerBytes.Length; headerStart += 80)
                yield return new BlockHeader(headerBytes.Skip(headerStart).Take(80));
        }

        /// <summary>
        ///     Gets the height of a given block
        /// </summary>
        /// <returns>block height</returns>
        public int GetBlockHeightByHash(string blockHash)
        {
            var ret = SendCommand("block/notxdetails/" + blockHash + ".json");
            // TODO: get just the height out of the json return
            var result = WrapDecodeException(() => JObject.Parse(ret).GetValue("height"));
            return WrapDecodeException(() => result.Value<int>());
        }

        /// <summary>
        ///     Gets the current block height
        /// </summary>
        /// <returns>block height</returns>
        public ChainInfo GetChainInfo()
        {
            var ret = SendCommand("chaininfo.json");
            var ret2 = WrapDecodeException(() => JsonConvert.DeserializeObject<ChainInfo>(ret));
            return ret2;
        }

        /// <summary>
        ///     Gets all transaction ids in the mem pool
        /// </summary>
        /// <returns>array of txid in hex</returns>
        public Dictionary<string, MemPoolTxInfo> GetMemPool()
        {
            var ret = SendCommand("mempool/contents.json");
            var jObject = WrapDecodeException(() => JObject.Parse(ret));

            return WrapDecodeException(() => jObject.Properties().ToDictionary(property => property.Name, 
                property => property.Value.ToObject<MemPoolTxInfo>()));
        }

        /// <summary>
        ///     Gets various information about the TX mempool
        ///     - size : (numeric) the number of transactions in the TX mempool
        ///     - bytes : (numeric) size of the TX mempool in bytes
        ///     - usage : (numeric) total TX mempool memory usage
        ///     - maxmempool : (numeric) maximum memory usage for the mempool in bytes
        ///     - mempoolminfee : (numeric) minimum feerate (BTC per KB) for tx to be accepted
        /// </summary>
        /// <returns>array of txid in hex</returns>
        public MemPoolInfo GetMemPoolInfo()
        {
            var ret = SendCommand("mempool/info.json");
            return WrapDecodeException(() => JsonConvert.DeserializeObject<MemPoolInfo>(ret));
        }

        /// <summary>
        ///     Gets raw block data at the given height
        /// </summary>
        /// <returns>raw block data as hex</returns>
        public string GetRawBlockByHash(string blockHash)
        {
            return SendCommand("block/" + blockHash + ".hex").Trim();
        }

        /// <summary>
        ///     Gets raw transaction data for a given txid
        /// </summary>
        /// <param name="txid">transaction id in hex</param>
        /// <returns>raw transaction data as hex</returns>
        public string GetRawTransactionById(string txid)
        {
            return SendCommand("tx/" + txid + ".hex").Trim();
        }

        /// <summary>
        ///     Gets and decodes the transaction data for a given txid
        /// </summary>
        /// <param name="txid">transaction id in hex</param>
        /// <returns>decoded transaction</returns>
        public Transaction.Transaction GetTransactionById(string txid)
        {
            var ret = SendCommand("tx/" + txid + ".hex").Trim();

            return WrapDecodeException(() => new Transaction.Transaction(ByteHexConverter.StringToByteArray(ret)));
        }

        /// <summary>
        ///     Sends a command to the REST interface
        /// </summary>
        /// <param name="method">method to call</param>
        /// <returns></returns>
        private string SendCommand(string method)
        {
            return WrapSendException(() =>
            {
                // generate the request uri from base uri and method
                var uriBuilder = new UriBuilder(_connectTo);
                uriBuilder.Path += method;

                // TODO: error handling?
                return SimpleWebRequest(uriBuilder.Uri);
            });
        }

        /// <summary>
        ///     Helper function to catch exceptions and wrap them with a RESTException for Decoding Errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private static T WrapDecodeException<T>(Func<T> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception e)
            {
                throw new RESTException("Failed to Decode Response", e);
            }
        }

        /// <summary>
        ///     Helper function to catch exceptions and wrap them with a RESTException for Send/Receive Errors
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static string WrapSendException(Func<string> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception e)
            {
                throw new RESTException("REST Request Failed", e);
            }
        }

        public class RESTException : Exception
        {
            public RESTException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        [JsonObject]
        public class ChainInfo
        {
            [JsonProperty("chain")] public string Chain { get; set; }
            [JsonProperty("blocks")] public ulong Blocks { get; set; }
            [JsonProperty("headers")] public string Headers { get; set; }
            [JsonProperty("bestblockhash")] public string BestBlockHash { get; set; }
            [JsonProperty("difficulty")] public double Difficulty { get; set; }
            [JsonProperty("mediantime")] public ulong MedianTime { get; set; }
            [JsonProperty("verificationprogress")] public double VerificationProgress { get; set; }
            [JsonProperty("chainwork")] public string ChainWork { get; set; }
            [JsonProperty("size_on_disk")] public ulong SizeOnDisk { get; set; }
            [JsonProperty("pruned")] public bool Pruned { get; set; }
            [JsonProperty("softforks")] public SoftForkInfo[] SoftForks { get; set; }
            [JsonProperty("warnings")] public string Warnings { get; set; }

            public class SoftForkInfo
            {
                [JsonProperty("id")] public string Id { get; set; }
                [JsonProperty("version")] public string Version { get; set; }
                [JsonProperty("reject")] private RejectInfo Reject { get; set; }

                public bool RejectStatus => Reject.Status;

                public class RejectInfo
                {
                    [JsonProperty("status")] public bool Status { get; set; }
                }
            }
        }

        [JsonObject]
        public class MemPoolInfo
        {
            [JsonProperty("bytes")] public ulong Bytes;
            [JsonProperty("maxmempool")] public ulong MaxMemPool;
            [JsonProperty("mempoolminfee")] public double MemPoolMinFee;
            [JsonProperty("size")] public uint Size;
            [JsonProperty("usage")] public ulong Usage;
        }

        [JsonObject]
        public class MemPoolTxInfo
        {
            [JsonProperty("size")] public uint Size;
            [JsonProperty("fee")] public double Fee;
            [JsonProperty("modifiedfee")] public double ModifiedFee;
            [JsonProperty("time")] public ulong Time;
            [JsonProperty("height")] public ulong Height;
            [JsonProperty("startingpriority")] public double StartingPriority;
            [JsonProperty("currentpriority")] public double CurrentPriority;
            [JsonProperty("descendantcount")] public uint DescendantCount;
            [JsonProperty("descendantsize")] public uint DescendantSize;
            [JsonProperty("descendantfees")] public ulong DescendantFees;
            [JsonProperty("ancestorcount")] public uint AncestorCount;
            [JsonProperty("ancestorsize")] public uint AncestorSize;
            [JsonProperty("ancestorfees")] public ulong AncestorFees;
            [JsonProperty("depends")] public string[] Depends;
        }
    }
}