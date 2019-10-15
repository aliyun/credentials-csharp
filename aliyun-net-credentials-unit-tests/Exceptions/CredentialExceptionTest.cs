using Aliyun.Credentials.Exceptions;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Exceptions
{
    public class CredentialExceptionTest
    {
        [Fact]
        public void Instance1()
        {
            var exception = new CredentialException("200", "message", "requestId");
            Assert.Equal("200", exception.ErrorCode);
            Assert.Equal("message", exception.ErrorMessage);
            Assert.Equal("requestId", exception.RequestId);
        }

        [Fact]
        public void Instance2()
        {
            var exception = new CredentialException("200", "message");
            Assert.Equal("200", exception.ErrorCode);
            Assert.Equal("message", exception.ErrorMessage);
            Assert.Null(exception.RequestId);
        }

        [Fact]
        public void Instance3()
        {
            var exception = new CredentialException("");
            Assert.Empty(exception.ErrorMessage);
            Assert.Null(exception.ErrorCode);
            Assert.Null(exception.RequestId);
        }
    }
}
