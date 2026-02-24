using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Exceptions;

public class EntidadeNaoEncontradaException : Exception
{
    public EntidadeNaoEncontradaException(string message) : base(message) { }
}
