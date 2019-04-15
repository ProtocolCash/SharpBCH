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
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace SharpBCH.Node
{
    public class ZMQClient : NodeConnection, IDisposable
    {
        private readonly Thread _worker;
        private readonly Uri _connectTo;
        private readonly string _subscribeTopic;
        private readonly Action<string, byte[], int> _callback;

        /// <summary>
        ///     Returns the Uri for this ZMQ client
        /// </summary>
        /// <returns>Uri that the ZMQ client is subscribed to</returns>
        public Uri GetUri()
        {
            return _connectTo;
        }

        /// <summary>
        ///     Returns the subscription topic for this ZMQ Client
        /// </summary>
        /// <returns>Subscription topic string</returns>
        public string GetSubscribeTopic()
        {
            return _subscribeTopic;
        }

        /// <summary>
        ///     Constructs a ZMQ Subscriber Client
        ///     - creates Uri from hostname/port
        ///     - saves callback and subscribe topic
        ///     - 
        /// </summary>
        /// <param name="hostname">ZMQ publisher hostname</param>
        /// <param name="port">ZMQ publisher port</param>
        /// <param name="subscribeTopic">ZMQ topic to subscribe</param>
        /// <param name="callback">Callback for received frames</param>
        public ZMQClient(string hostname, int port, string subscribeTopic, Action<string, byte[], int> callback)
        {
            _connectTo = new UriBuilder("tcp", hostname, port).Uri;
            _subscribeTopic = subscribeTopic;
            _callback = callback;
            // start the ZMQ SUB worker
            _worker = new Thread(SubscribeWorker);
            _worker.Start();
        }

        /// <summary>
        ///     Dispose by aborting the worker thread
        /// </summary>
        public void Dispose()
        {
            _worker.Abort();
        }

        /// <summary>
        ///     Subscription worker
        ///     - Creates ZMQ subscriber socket and listens for specified frames
        /// </summary>
        private void SubscribeWorker()
        {
            using (var subscriber = new SubscriberSocket())
            {
                subscriber.Connect(_connectTo.AbsoluteUri.TrimEnd('/'));
                subscriber.Subscribe(_subscribeTopic);
                Console.WriteLine("Subscribed to ZeroMQ '" + _connectTo + "' publisher for '" + _subscribeTopic + "' frames.");

                while (true)
                {
                    // the publisher sends 3 frames per data packet
                    // first, the header frame
                    var replyHeader = subscriber.ReceiveFrameString();
                    // if this is not a valid header frame, continue to next frame
                    if (replyHeader != _subscribeTopic)
                    {
                        Console.WriteLine("Unexpected Frame Header! '" + _subscribeTopic + "' expected, but received " + replyHeader +
                                          ". Skipping and waiting for proper frame header.");
                        continue;
                    }

                    // next, the data frame
                    var replyFrame = subscriber.ReceiveFrameBytes();

                    // last, the reply counter frame
                    var replyCounter = BitConverter.ToInt32(subscriber.ReceiveFrameBytes(), 0);

                    // pass the received data to callback function
                    _callback(replyHeader, replyFrame, replyCounter);
                }
            }
        }
    }
}