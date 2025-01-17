// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRise.Particles
{

  /// <summary>
  /// The exception that is thrown when an error in the particle library occurs.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Fix in next version.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Fix in next version. Replace references with name (string) or index (int).")]
  //[Serializable]
  public class ParticleSystemException : Exception
  {
    /// <summary>
    /// Gets or sets the particle system that caused the exception.
    /// </summary>
    /// <value>The particle system that caused the exception.</value>
    public ParticleSystem ParticleSystem { get; set; }


    /// <summary>
    /// Gets or sets the particle effector that caused the exception.
    /// </summary>
    /// <value>The particle effector that caused the exception.</value>
    public ParticleEffector ParticleEffector { get; set; }


    /// <summary>
    /// Gets or sets the particle parameter that caused the exception.
    /// </summary>
    /// <value>The particle parameter that caused the exception.</value>
    public string ParticleParameter { get; set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemException"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemException"/> class.
    /// </summary>
    public ParticleSystemException()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemException"/> class with a 
    /// specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ParticleSystemException(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemException"/> class with a
    /// specified error message and a reference to the inner exception that is the cause of this
    /// exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <see langword="null"/> if no
    /// inner exception is specified.
    /// </param>
    public ParticleSystemException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
