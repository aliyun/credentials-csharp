using System;

namespace Aliyun.Credentials.Exceptions
{
    public class CredentialException : Exception
    {
        public CredentialException(string errCode, string errMsg, string requestId) : base(
            string.Format("{0} : {1} + [ RequestId : {2} ]", errCode, errMsg, requestId))
        {
            ErrorMessage = errMsg;
            RequestId = requestId;
            ErrorCode = errCode;
        }

        public CredentialException(string errCode, string errMsg) : base(errCode + " : " + errMsg)
        {
            ErrorCode = errCode;
            ErrorMessage = errMsg;
        }

        public CredentialException(string message) : base(message)
        {
            ErrorMessage = message;
        }

        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string RequestId { get; set; }
    }
}
