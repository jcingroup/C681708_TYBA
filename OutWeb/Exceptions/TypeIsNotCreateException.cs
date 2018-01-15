using System;

namespace OutWeb.Exceptions
{
    public class TypeIsNotCreateException : Exception
    {

        public TypeIsNotCreateException() : base("請先建立分類!")
        {
        }
    }
}