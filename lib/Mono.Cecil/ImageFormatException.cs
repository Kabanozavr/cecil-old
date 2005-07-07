/*
 * Copyright (c) 2004, 2005 DotNetGuru and the individuals listed
 * on the ChangeLog entries.
 *
 * Authors :
 *   Jb Evain   (jbevain@gmail.com)
 *
 * This is a free software distributed under a MIT/X11 license
 * See LICENSE.MIT file for more details
 *
 *****************************************************************************/

namespace Mono.Cecil {

	using System;

	public class ImageFormatException : Exception {

		internal ImageFormatException () : base()
		{
		}

		internal ImageFormatException (string message) : base(message)
		{
		}

		internal ImageFormatException (string message, params string[] parameters) : base(string.Format(message, parameters))
		{
		}

		internal ImageFormatException (string message, Exception inner) : base(message, inner)
		{
		}
	}
}
