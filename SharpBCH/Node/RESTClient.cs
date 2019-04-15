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
using Newtonsoft.Json.Linq;
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
            _connectTo = new UriBuilder("http", hostname, port, "rest").Uri;
        }
        
        /// <summary>
        ///     Gets raw transaction data for a given txid
        /// </summary>
        /// <param name="txid">transaction id in hex</param>
        /// <returns>raw transaction byte data</returns>
        public byte[] GetRawTransaction(string txid)
        {
            var ret = SendCommand("tx/" + txid + ".hex");
            var transactionHex = ret.GetValue("result").ToObject<string>();
            return ByteHexConverter.StringToByteArray(transactionHex);
        }

        /// <summary>
        ///     Gets raw block data at the given height
        /// </summary>
        /// <returns>raw block data as hex</returns>
        public string GetRawBlock(string blockHash)
        {
            var ret = SendCommand("block/" + blockHash + ".hex");
            var result = ret.GetValue("result");
            return result.Value<string>();
        }

        /// <summary>
        ///     Gets raw block headers starting from a given block hash
        /// </summary>
        /// <returns>raw block data as hex</returns>
        public string[] GetBlockHeadersStartingAt(string blockHash, int count)
        {
            var ret = SendCommand("headers/" + count + "/" + blockHash + ".hex");
            var result = ret.GetValue("result");
            return result.Value<string[]>();
        }

        /// <summary>
        ///     Gets the block hash at a given height
        /// </summary>
        /// <returns>block hash in hex</returns>
        public string GetBlockHash(int height)
        {
            var ret = SendCommand("blockhashbyheight/" + height + ".hex");
            var result = ret.GetValue("result");
            return result.Value<string>();
        }

        /// <summary>
        ///     Gets the height of a given block
        /// </summary>
        /// <returns>block height</returns>
        public int GetBlockHeight(string blockHash)
        {
            var ret = SendCommand("notxdetails/" + blockHash + ".json");
            // TODO: get just the height out of the json return
            var result = ret.GetValue("result");
            return result.Value<int>("height");
        }

        /// <summary>
        ///     Gets the current block height
        /// </summary>
        /// <returns>block height</returns>
        public JObject GetChainInfo()
        {
            var ret = SendCommand("chaininfo.json");
            var result = ret.GetValue("result");
            return result.Value<JObject>();
        }

        /// <summary>
        ///     Gets all transaction ids in the mem pool
        /// </summary>
        /// <returns>array of txid in hex</returns>
        public JObject GetMemPool()
        {
            var ret = SendCommand("mempool/contents.json");
            return ret.GetValue("result").ToObject<JObject>();
        }

        /// <summary>
        ///     Gets various information about the TX mempool
        ///     - size : (numeric) the number of transactions in the TX mempool
        ///     - bytes : (numeric) size of the TX mempool in bytes
        ///     - usage : (numeric) total TX mempool memory usage
        ///     - maxmempool : (numeric) maximum memory usage for the mempool in bytes
        ///     - mempoolminfee : (numeric) minimum feerate(BTC per KB) for tx to be accepted
        /// </summary>
        /// <returns>array of txid in hex</returns>
        public JObject GetMemPoolInfo()
        {
            var ret = SendCommand("mempool/info.json");
            return ret.GetValue("result").ToObject<JObject>();
        }

        /// <summary>
        ///     Sends a command to the REST interface
        /// </summary>
        /// <param name="method">method to call</param>
        /// <returns></returns>
        private JObject SendCommand(string method)
        {
            // generate the request uri from base uri and method
            var uriBuilder = new UriBuilder(_connectTo);
            uriBuilder.Path += method;

            // TODO: error handling
            var ret = SimpleWebRequest(uriBuilder.Uri);
            return new JObject(ret);
        }

    }
}