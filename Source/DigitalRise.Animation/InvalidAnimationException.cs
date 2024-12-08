﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
using System.Runtime.Serialization;
#endif


namespace DigitalRise.Animation
{
	/// <summary>
	/// Occurs when an animation encounters an invalid state.
	/// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
	[Serializable]
#endif
	public class InvalidAnimationException : AnimationException
	{
		/// <overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidAnimationException"/> class.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidAnimationException"/> class.
		/// </summary>
		public InvalidAnimationException()
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidAnimationException"/> class with a
		/// specified error message.
		/// </summary>
		/// <param name="message">The message.</param>
		public InvalidAnimationException(string message)
		  : base(message)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidAnimationException"/> class with a
		/// specified error message and a reference to the inner exception that is the cause of this
		/// exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception, or <see langword="null"/> if no
		/// inner exception is specified.
		/// </param>
		public InvalidAnimationException(string message, Exception innerException)
		  : base(message, innerException)
		{
		}
	}
}
