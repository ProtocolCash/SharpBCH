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
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpBCH.Util;

namespace SharpBCH.Node
{
    public class RPCClient : HTTPConnection
    {
        private readonly string _username;
        private readonly string _password;
        private readonly Uri _connectTo;

        /// <summary>
        ///     Contructor
        ///     - saves connection and auth data
        /// </summary>
        /// <param name="connectTo">Bitcoin RPC URI</param>
        /// <param name="username">RPC Username</param>
        /// <param name="password">RPC Password</param>
        public RPCClient(Uri connectTo, string username, string password)
        {
            _connectTo = connectTo;
            _username = username;
            _password = password;
        }

        /// <summary>
        ///     Gets the current block height
        /// </summary>
        /// <returns>block height</returns>
        public int GetBlockCount()
        {
            var ret = SendCommand("getblockcount");
            var result = ret.GetValue("result");
            return result.Value<int>();
        }

        /// <summary>
        ///     Gets raw block data at the given height
        /// </summary>
        /// <returns>raw block data as hex</returns>
        public string GetRawBlock(string blockHash)
        {
            var ret = SendCommand("getblock", blockHash, 0);
            var result = ret.GetValue("result");
            return result.Value<string>();
        }

        /// <summary>
        ///     Gets the block hash at a given height
        /// </summary>
        /// <returns>block hash in hex</returns>
        public string GetBlockHash(int height)
        {
            var ret = SendCommand("getblockhash", height);
            var result = ret.GetValue("result");
            return result.Value<string>();
        }

        /// <summary>
        ///     Gets the height of a given block
        /// </summary>
        /// <returns>block height</returns>
        public int GetBlockHeight(string blockHash)
        {
            var ret = SendCommand("getblock", blockHash);
            var result = ret.GetValue("result");
            return result.Value<int>("height");
        }

        /// <summary>
        ///     Gets raw transaction data for a given txid
        /// </summary>
        /// <param name="txid">transaction id in hex</param>
        /// <returns>raw transaction byte data</returns>
        public byte[] GetRawTransaction(string txid)
        {
            var ret = SendCommand("getrawtransaction", txid);
            var transactionHex = ret.GetValue("result").ToObject<string>();
            return ByteHexConverter.StringToByteArray(transactionHex);
        }

        /// <summary>
        ///     Gets all transaction ids in the mem pool
        /// </summary>
        /// <returns>array of txid in hex</returns>
        public string[] GetMemPool()
        {
            var ret = SendCommand("getrawmempool");
            return ret.GetValue("result").ToObject<string[]>();
        }

        /// <summary>
        ///     Sends a command to the JSON/RPC interface
        /// </summary>
        /// <param name="method">method to call</param>
        /// <param name="parameters">parameters to pass</param>
        /// <returns></returns>
        private JObject SendCommand(string method, params object[] parameters)
        {
            var requestJson = new JObject
            {
                ["jsonrpc"] = "1.0",
                ["id"] = "1",
                ["method"] = method
            };
            if (parameters != null && parameters.Length > 0)
            {
                var props = new JArray();
                foreach (var p in parameters)
                    props.Add(p);
                requestJson.Add(new JProperty("params", props));
            }
            var request = JsonConvert.SerializeObject(requestJson);
            // serialize json for the request
            var byteData = Encoding.UTF8.GetBytes(request);

            // TODO: some error handling?
            return new JObject(AdvancedWebRequest(_connectTo, _username, _password, "application/json-rpc", "POST", byteData));
        }
    }
}