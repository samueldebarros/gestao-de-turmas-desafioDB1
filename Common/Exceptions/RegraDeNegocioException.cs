using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Exceptions;

public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string message) : base(message) { }
}
