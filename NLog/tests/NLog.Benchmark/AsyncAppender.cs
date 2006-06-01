#region Copyright & License
//
// Copyright 2001-2005 The Apache Software Foundation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

#if LOG4NET || LOG4NET_WITH_FASTLOGGER

using System;
using System.Threading;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace SampleAppendersApp.Appender
{
	/// <summary>
	/// Appender that forwards LoggingEvents asynchronously
	/// </summary>
	/// <remarks>
	/// This appender forwards LoggingEvents to a list of attached appenders.
	/// The events are forwarded asynchronously using the ThreadPool.
	/// This allows the calling thread to be released quickly, however it does
	/// not guarantee the ordering of events delivered to the attached appenders.
	/// </remarks>
	public sealed class AsyncAppender : IAppender, IBulkAppender, IOptionHandler, IAppenderAttachable
	{
		private string m_name;

		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		public void ActivateOptions() 
		{
		}

		public FixFlags Fix
		{
			get { return m_fixFlags; }
			set { m_fixFlags = value; }
		}

		public void Close()
		{
			// Remove all the attached appenders
			lock(this)
			{
				if (m_appenderAttachedImpl != null)
				{
					m_appenderAttachedImpl.RemoveAllAppenders();
				}
			}
		}

		public void DoAppend(LoggingEvent loggingEvent)
		{
			loggingEvent.Fix = m_fixFlags;
			System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncAppend), loggingEvent);
		}

		public void DoAppend(LoggingEvent[] loggingEvents)
		{
			foreach(LoggingEvent loggingEvent in loggingEvents)
			{
				loggingEvent.Fix = m_fixFlags;
			}
			System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncAppend), loggingEvents);
		}

		private void AsyncAppend(object state)
		{
			if (m_appenderAttachedImpl != null)
			{
				LoggingEvent loggingEvent = state as LoggingEvent;
				if (loggingEvent != null)
				{
					m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvent);
				}
				else
				{
					LoggingEvent[] loggingEvents = state as LoggingEvent[];
					if (loggingEvents != null)
					{
						m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvents);
					}
				}
			}
		}

		#region IAppenderAttachable Members

		public void AddAppender(IAppender newAppender) 
		{
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}
			lock(this)
			{
				if (m_appenderAttachedImpl == null) 
				{
					m_appenderAttachedImpl = new log4net.Util.AppenderAttachedImpl();
				}
				m_appenderAttachedImpl.AddAppender(newAppender);
			}
		}

		public AppenderCollection Appenders
		{
			get
			{
				lock(this)
				{
					if (m_appenderAttachedImpl == null)
					{
						return AppenderCollection.EmptyCollection;
					}
					else 
					{
						return m_appenderAttachedImpl.Appenders;
					}
				}
			}
		}

		public IAppender GetAppender(string name) 
		{
			lock(this)
			{
				if (m_appenderAttachedImpl == null || name == null)
				{
					return null;
				}

				return m_appenderAttachedImpl.GetAppender(name);
			}
		}

		public void RemoveAllAppenders() 
		{
			lock(this)
			{
				if (m_appenderAttachedImpl != null) 
				{
					m_appenderAttachedImpl.RemoveAllAppenders();
					m_appenderAttachedImpl = null;
				}
			}
		}

		public IAppender RemoveAppender(IAppender appender) 
		{
			lock(this)
			{
				if (appender != null && m_appenderAttachedImpl != null) 
				{
					return m_appenderAttachedImpl.RemoveAppender(appender);
				}
			}
			return null;
		}

		public IAppender RemoveAppender(string name) 
		{
			lock(this)
			{
				if (name != null && m_appenderAttachedImpl != null)
				{
					return m_appenderAttachedImpl.RemoveAppender(name);
				}
			}
			return null;
		}

		#endregion

		private AppenderAttachedImpl m_appenderAttachedImpl;
		private FixFlags m_fixFlags = FixFlags.All;
	}
}

#endif
