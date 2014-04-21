using System;

namespace mio991.REST.Server
{
	public class URI
	{
		private string[] m_Segments;
		private int m_SegmentID = 0;

		public URI (string uri)
		{
			m_Segments = uri.Split (new char[] {Server.SEPERATORCHAR}, StringSplitOptions.RemoveEmptyEntries);
		}

		public string GetSegment()
		{
			return m_Segments[m_SegmentID];
		}

		public void Next()
		{
			m_SegmentID++;
		}

		public void Reset()
		{
			m_SegmentID = 0;
		}

		public bool IsEnded
		{
			get {
				return m_SegmentID == m_Segments.Length;
			}
		}
	}
}

