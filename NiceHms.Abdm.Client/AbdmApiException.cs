using System;

namespace NiceHms.Abdm
{
    public class AbdmApiException : Exception
    {
        public int StatusCode { get; private set; }
        public string ResponseBody { get; private set; }

        public AbdmApiException(int statusCode, string responseBody)
            : base("ABDM API returned " + statusCode + ": " + responseBody)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
