using System;

namespace RPA_PHARMA.Core.Domain.Exceptions
{
    /// <summary>
    /// Exceção lançada exclusivamente quando uma regra de negócio ou compliance farmacêutico é violada.
    /// No REFramework, isso deve ser mapeado (Catch) como uma BusinessRuleException.
    /// </summary>
    public class PharmaBusinessException : Exception
    {
        public PharmaBusinessException(string message) : base(message)
        {
        }
    }
}