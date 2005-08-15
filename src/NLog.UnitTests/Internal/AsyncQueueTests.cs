// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Xml;
using System.Collections;
using System.Threading;

using NLog;
using NLog.Internal;

using NUnit.Framework;

namespace NLog.UnitTests
{
    [TestFixture]
	public class AsyncQueueTests
	{
        [Test]
        public void AsyncQueueTest1()
        {
            AsyncRequestQueue q = new AsyncRequestQueue(3, AsyncRequestQueue.OverflowAction.None);
            q.Enqueue(1);
            q.Enqueue(2);
            q.Enqueue(3);
            q.Enqueue(4);
            ArrayList tmp = q.DequeueBatch(5);

            Assert.AreEqual(4, tmp.Count);
            Assert.AreEqual(1, tmp[0]);
            Assert.AreEqual(2, tmp[1]);
            Assert.AreEqual(3, tmp[2]);
            Assert.AreEqual(4, tmp[3]);
        }

        [Test]
        public void AsyncQueueTest2()
        {
            AsyncRequestQueue q = new AsyncRequestQueue(3, AsyncRequestQueue.OverflowAction.Discard);
            q.Enqueue(1);
            q.Enqueue(2);
            q.Enqueue(3);
            q.Enqueue(4);
            ArrayList tmp = q.DequeueBatch(5);

            Assert.AreEqual(3, tmp.Count);
            Assert.AreEqual(1, tmp[0]);
            Assert.AreEqual(2, tmp[1]);
            Assert.AreEqual(3, tmp[2]);
        }

        AsyncRequestQueue _queue;
        int _sum;

        [Test]
        public void AsyncQueueTest3()
        {
            _queue = new AsyncRequestQueue(1, AsyncRequestQueue.OverflowAction.Block);
            _sum = 0;
            Thread receiverThread = new Thread(new ThreadStart(AsyncQueueTest3Thread));
            receiverThread.Start();
            _queue.Enqueue(1);
            _queue.Enqueue(2);
            _queue.Enqueue(3);
            _queue.Enqueue(4);
            _queue.Enqueue(5);
            _queue.Enqueue(6);
            _queue.Enqueue(7);
            _queue.Enqueue(8);
            receiverThread.Join();
            Assert.AreEqual(_sum, 36);
        }

        [Test]
        public void AsyncQueueTest4()
        {
            for (int i = 1; i < 40; ++i)
            {
                _queue = new AsyncRequestQueue(i, AsyncRequestQueue.OverflowAction.Block);
                _sum = 0;
                Thread receiverThread1 = new Thread(new ThreadStart(AsyncQueueTest3Thread));
                receiverThread1.Start();
                Thread receiverThread2 = new Thread(new ThreadStart(AsyncQueueTest3Thread));
                receiverThread2.Start();
                Thread receiverThread3 = new Thread(new ThreadStart(AsyncQueueTest3Thread));
                receiverThread3.Start();
                Thread receiverThread4 = new Thread(new ThreadStart(AsyncQueueTest3Thread));
                receiverThread4.Start();
                DateTime dt0 = DateTime.Now;
                _queue.Enqueue(1);
                _queue.Enqueue(2);
                _queue.Enqueue(3);
                _queue.Enqueue(4);
                _queue.Enqueue(5);
                _queue.Enqueue(6);
                _queue.Enqueue(7);
                _queue.Enqueue(8);
                receiverThread1.Join();
                receiverThread2.Join();
                receiverThread3.Join();
                receiverThread4.Join();
            }
        }

        void AsyncQueueTest3Thread()
        {
            while (_sum < 36)
            {
                ArrayList tmp = _queue.DequeueBatch(5);
                // Console.WriteLine("Dequeued {0}", tmp.Count);
                if (tmp.Count == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                foreach (int i in tmp)
                {
                    _sum += i;
                }
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
