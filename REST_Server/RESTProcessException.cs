using System;

namespace REST_Server
{
	public class RESTProcessException : Exception
	{
		private int m_ErrorCode;

		public int ErrorCode
		{
			get {
				return m_ErrorCode;
			}
		}

		public RESTProcessException(string message, int code, Exception innerException) : base(message, innerException)
		{
			m_ErrorCode = code;
		}

		public RESTProcessException(string message, int code) : base(message)
		{
			m_ErrorCode = code;
		}
	}
}

