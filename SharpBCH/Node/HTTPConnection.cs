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
using System.IO;
using System.Net;

namespace SharpBCH.Node
{
    /// <summary>
    ///     Represents a Web/HTTP Connection
    ///     - Provides SimpleWebRequest (uri only)
    ///         and AdvancedWebRequest (authentication, contentType, method, and raw data supported)
    /// </summary>
    public abstract class HTTPConnection : NodeConnection
    {
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(45);

        /// <summary>
        ///     Constructor
        /// </summary>
        protected HTTPConnection()
        {
        }

        /// <summary>
        ///     Makes a simple unauthenticated http request and returns the response
        /// </summary>
        /// <param name="connectTo">uri to get response from</param>
        /// <returns>response from http request</returns>
        protected string SimpleWebRequest(Uri connectTo)
        {
            // create HttpWebRequest
            var webRequest = (HttpWebRequest)WebRequest.Create(connectTo.AbsoluteUri);
            webRequest.Timeout = (int)RequestTimeout.TotalMilliseconds;

            return GetWebRequestResponse(webRequest);
        }

        /// <summary>
        ///     Makes an http request and returns the response
        /// </summary>
        /// <param name="connectTo">Uri to connect to</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="password">Password for authentication</param>
        /// <param name="contentType">ContentType header e.g. application/json-rpc </param>
        /// <param name="method">http method type e.g. POST, GET</param>
        /// <param name="byteData">raw byte data to write to the request stream e.g. encoded json</param>
        /// <returns>response from http request</returns>
        protected string AdvancedWebRequest(Uri connectTo, string username, string password, string contentType, string method, byte[] byteData)
        {
            // create HttpWebRequest
            var webRequest = (HttpWebRequest)WebRequest.Create(connectTo.AbsoluteUri);
            webRequest.Credentials = new NetworkCredential(username, password);
            webRequest.ContentType = contentType;
            webRequest.Method = method;
            webRequest.KeepAlive = false;
            webRequest.Timeout = (int)RequestTimeout.TotalMilliseconds;

            // may throw WebException
            using (var dataStream = webRequest.GetRequestStream())
                dataStream.Write(byteData, 0, byteData.Length);

            return GetWebRequestResponse(webRequest);
        }

        /// <summary>
        ///     Returns the result of a webRequest, given a fully formed WebRequest
        /// </summary>
        /// <param name="webRequest">request to get response for</param>
        /// <returns>web server response</returns>
        private string GetWebRequestResponse(HttpWebRequest webRequest)
        {
            try
            {
                HttpWebResponse webResponse;
                using (webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var str = webResponse.GetResponseStream())
                    {
                        if (str == null) return "";
                        using (var sr = new StreamReader(str))
                            return sr.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Response == null) return null;
                using (var str = e.Response.GetResponseStream())
                {
                    using (var sr = new StreamReader(str ?? throw new InvalidOperationException()))
                    {
                        var tempRet = sr.ReadToEnd();
                        return tempRet;
                    }
                }
            }
        }
    }
}