using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sweaty_T_Shirt.Models
{
    /// <summary>
    /// Exceptions thrown by repositories that should handled by controllers and added to the Model errors.
    /// </summary>
    public class RepositoryException : Exception
    {
        public RepositoryException(string message) :base(message)
        {
        }
    }
}