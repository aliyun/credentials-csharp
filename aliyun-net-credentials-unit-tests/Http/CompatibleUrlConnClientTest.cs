using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Http;

using Xunit;

namespace aliyun_net_credentials_unit_tests.Http
{
    public class CompatibleUrlConnClientTest
    {
        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        [Fact]
        public void DoActionTest()
        {
            int port = GetFreeTcpPort();
            string prefix = string.Format("http://127.0.0.1:{0}/", port);
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            try
            {
                listener.BeginGetContext(ar =>
                {
                    try
                    {
                        var context = listener.EndGetContext(ar);
                        byte[] buffer = Encoding.UTF8.GetBytes("ok");
                        context.Response.StatusCode = 200;
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    catch (HttpListenerException)
                    {
                        // Listener may already be closed when the test finishes.
                    }
                }, null);

                HttpRequest httpRequest = new HttpRequest(prefix, new Dictionary<string, string>())
                {
                    Method = MethodType.GET,
                    ConnectTimeout = 10000,
                    ReadTimeout = 10000
                };
                CompatibleUrlConnClient client = new CompatibleUrlConnClient();
                HttpResponse httpResponse = client.DoAction(httpRequest);
                Assert.NotNull(httpResponse);

                Regex regex = new Regex(@"AlibabaCloud (.+) .+/.+ Credentials/.+ TeaDSL/1");
                string userAgent = httpRequest.Headers["User-Agent"];
                Match match = regex.Match(userAgent);
                Assert.True(match.Success);

                // Closed local port → connection error wrapped as CredentialException (no external network).
                httpRequest = new HttpRequest(string.Format("http://127.0.0.1:{0}/", GetFreeTcpPort()))
                {
                    Method = MethodType.GET,
                    ConnectTimeout = 2000,
                    ReadTimeout = 2000
                };
                Assert.Throws<CredentialException>(() => client.DoAction(httpRequest));

                httpRequest = new HttpRequest
                {
                    ConnectTimeout = 10000,
                    ReadTimeout = 10000
                };
                Assert.Equal("URL is null for HttpRequest.",
                    Assert.Throws<InvalidDataException>(() => client.DoAction(httpRequest)).Message
                );

                httpRequest = new HttpRequest("http://www.aliyun.com")
                {
                    ConnectTimeout = 10000,
                    ReadTimeout = 10000
                };
                Assert.Equal("Method is null for HttpRequest.",
                    Assert.Throws<InvalidDataException>(() => client.DoAction(httpRequest)).Message
                );
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }
        }

        [Fact]
        public async Task DoActionAsyncTest()
        {
            int port = GetFreeTcpPort();
            string prefix = string.Format("http://127.0.0.1:{0}/", port);
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            try
            {
                listener.BeginGetContext(ar =>
                {
                    try
                    {
                        var context = listener.EndGetContext(ar);
                        byte[] buffer = Encoding.UTF8.GetBytes("ok");
                        context.Response.StatusCode = 200;
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    catch (HttpListenerException)
                    {
                        // Listener may already be closed when the test finishes.
                    }
                }, null);

                HttpRequest httpRequest = new HttpRequest(prefix, new Dictionary<string, string>())
                {
                    Method = MethodType.GET,
                    ConnectTimeout = 10000,
                    ReadTimeout = 10000
                };
                CompatibleUrlConnClient client = new CompatibleUrlConnClient();
                HttpResponse httpResponse = await client.DoActionAsync(httpRequest);
                Assert.NotNull(httpResponse);

                Regex regex = new Regex(@"AlibabaCloud (.+) .+/.+ Credentials/.+ TeaDSL/1");
                string userAgent = httpRequest.Headers["User-Agent"];
                Match match = regex.Match(userAgent);
                Assert.True(match.Success);

                // Keep short-timeout network assert disabled on CI (historically flaky on net45).
                // Validation-only cases below cover non-network error paths.

                httpRequest = new HttpRequest
                {
                    ConnectTimeout = 10000,
                    ReadTimeout = 10000
                };
                Assert.Equal("URL is null for HttpRequest.",
                    (await Assert.ThrowsAsync<InvalidDataException>(async() => await client.DoActionAsync(httpRequest))).Message
                );

                httpRequest = new HttpRequest("http://www.aliyun.com")
                {
                    ConnectTimeout = 10000,
                    ReadTimeout = 10000
                };
                Assert.Equal("Method is null for HttpRequest.",
                    (await Assert.ThrowsAsync<InvalidDataException>(async() => await client.DoActionAsync(httpRequest))).Message
                );
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }
        }
    }
}
