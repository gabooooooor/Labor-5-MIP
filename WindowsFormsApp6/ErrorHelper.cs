using System;

namespace WindowsFormsApp6
{
    public static class ErrorHelper
    {
        // lemegy a legbelso hibaig, ott van a valodi uzenet
        public static string GetMessage(Exception ex)
        {
            Exception inner = ex;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            return inner.Message;
        }
    }
}