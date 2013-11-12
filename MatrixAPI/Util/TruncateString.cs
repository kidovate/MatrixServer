using System;

namespace MatrixAPI
{
	public static class TruncateString
	{
		public static string Truncate( this string value, int maxLength )
		{
			return value.Length <= maxLength ? value : value.Substring(0, maxLength); 
		}
	}
}

