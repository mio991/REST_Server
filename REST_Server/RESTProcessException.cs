using System;

namespace REST_Server
{
    /// <summary>
    /// Exception if a Request runs in an Error
    /// </summary>
	public class RESTProcessException : Exception
	{
        /// <summary>
        /// HTTP-Errorcode 404
        /// </summary>
        public static readonly RESTProcessException ResourceNotFound = new RESTProcessException("Resouce Not Found", 404);
        /// <summary>
        /// HTTP-Errorcode 401
        /// </summary>
        public static readonly RESTProcessException NotEnoughRights = new RESTProcessException("Not enough rights to get the Resource", 401);
		private int m_ErrorCode;

        /// <summary>
        /// The HTTP-Errorcode of the Exception
        /// </summary>
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

