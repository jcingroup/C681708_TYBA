using System;

namespace OutWeb.Exceptions
{
    public class ProductKindRelationExcption : Exception
    {
        public ProductKindRelationExcption(string message) : base(message)
        {
        }
    }
}