using System;

namespace REST_Server
{
    //TODO implement a way to give http-statuscode specified infomation back to the client (like allowed methodes for code 405)
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

        /// <summary>
        /// HTTP-Errorcode 405
        /// </summary>
        public static readonly RESTProcessException MethodeNotAllowed = new RESTProcessException("Methode not Allowed", 405);



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

