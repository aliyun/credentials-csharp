using System;
using System.Threading;
using System.Threading.Tasks;
using Aliyun.Credentials.Exceptions;
using Aliyun.Credentials.Models;
using Aliyun.Credentials.Policy;
using Aliyun.Credentials.Provider;
using Aliyun.Credentials.Utils;
using Xunit;
using Xunit.Abstractions;

namespace aliyun_net_credentials_unit_tests.Provider
{
    public class RefreshCachedSupplierTest : IDisposable
    {
        private readonly ITestOutputHelper output;

        public RefreshCachedSupplierTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private RefreshResult<CredentialModel> RefreshFunc()
        {
            Console.WriteLine("Refreshing credentials synchronously");
            return new RefreshResult<CredentialModel>.Builder(new CredentialModel { AccessKeyId = "newAccessKey" })
                .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                .PrefetchTime(DateTime.UtcNow.AddMinutes(-10).GetTimeMillis())
                .Build();
        }

        private async Task<RefreshResult<CredentialModel>> RefreshFuncAsync()
        {
            await Task.Delay(100);
            Console.WriteLine("Refreshing credentials asynchronously");
            return new RefreshResult<CredentialModel>.Builder(new CredentialModel
                    { AccessKeyId = "newAccessKeyForAsync" })
                .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                .PrefetchTime(DateTime.UtcNow.AddMinutes(-10).GetTimeMillis())
                .Build();
        }

        [Fact]
        public async Task GetAsync_Returns_Cached_Value_When_Not_Stale()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>(RefreshFunc, RefreshFuncAsync);
            var initialValue = await supplier.GetAsync();
            var result = await supplier.GetAsync();
            Assert.Equal(initialValue.AccessKeyId, result.AccessKeyId);
        }

        [Fact]
        public void TestprefetchStrategy()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync)
                .AsyncUpdateEnabled(false)
                .Build();

            var result = supplier.Get();
            Assert.Equal("newAccessKey", result.AccessKeyId);

            Assert.IsType<OneCallerBlocks>(typeof(RefreshCachedSupplier<CredentialModel>)
                .GetField("prefetchStrategy",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(supplier));

            supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync)
                .AsyncUpdateEnabled(true)
                .Build();
            Assert.IsType<NonBlocking>(typeof(RefreshCachedSupplier<CredentialModel>)
                .GetField("prefetchStrategy",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(supplier));
        }

        [Fact]
        public void Test_HandleFetchedSuccess()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync).Build();

            var freshResult = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(10).GetTimeMillis());
            var updatedValue = TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                "HandleFetchedSuccess", supplier, new object[] { freshResult });
            Assert.Equal(freshResult.Value, ((RefreshResult<CredentialModel>)updatedValue).Value);
            Assert.Equal(freshResult.StaleTime, ((RefreshResult<CredentialModel>)updatedValue).StaleTime);

            var now = DateTime.UtcNow;
            var needUpdateNextTime = now.AddMinutes(-10).GetTimeMillis();
            freshResult = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                needUpdateNextTime);
            updatedValue = TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                "HandleFetchedSuccess", supplier, new object[] { freshResult });
            Assert.True(now.GetTimeMillis() + 10 > ((RefreshResult<CredentialModel>)updatedValue).StaleTime);
            Assert.True(now.GetTimeMillis() <= ((RefreshResult<CredentialModel>)updatedValue).StaleTime);

            freshResult = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(-20).GetTimeMillis());
            var ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                    "HandleFetchedSuccess", supplier, new object[] { freshResult });
            });
            Assert.Equal("No cached value was found.", ex.Message);

            var cachedValue = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(10).GetTimeMillis());

            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);

            freshResult = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(-20).GetTimeMillis());
            updatedValue = TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                "HandleFetchedSuccess", supplier, new object[] { freshResult });
            Assert.Equal(cachedValue.StaleTime, ((RefreshResult<CredentialModel>)updatedValue).StaleTime);

            cachedValue = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(-10).GetTimeMillis());

            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);

            freshResult = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(-20).GetTimeMillis());
            now = DateTime.UtcNow;
            updatedValue = TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                "HandleFetchedSuccess", supplier, new object[] { freshResult });
            Assert.True(now.GetTimeMillis() + 1010 > ((RefreshResult<CredentialModel>)updatedValue).StaleTime);
            Assert.True(now.GetTimeMillis() + 1000 <= ((RefreshResult<CredentialModel>)updatedValue).StaleTime);

            supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync)
                .StaleValueBehavior(StaleValueBehavior.Allow).Build();
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);
            now = DateTime.UtcNow;
            updatedValue = TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                "HandleFetchedSuccess", supplier, new object[] { freshResult });
            Assert.True(now.GetTimeMillis() + 50000 < ((RefreshResult<CredentialModel>)updatedValue).StaleTime);
            Assert.True(now.GetTimeMillis() + 70100 > ((RefreshResult<CredentialModel>)updatedValue).StaleTime);
        }

        [Fact]
        public void TestHandleFetchedFailure()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync).Build();
            var exception = new CredentialException("exception for test");

            var ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                    "HandleFetchedFailure", supplier, new object[] { exception });
            });
            Assert.Equal("exception for test", ex.Message);

            var cachedValue = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(10).GetTimeMillis());
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);
            var updatedValue = TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                "HandleFetchedFailure", supplier, new object[] { exception });
            Assert.Equal(cachedValue.StaleTime, ((RefreshResult<CredentialModel>)updatedValue).StaleTime);

            cachedValue = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(-10).GetTimeMillis());
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);
            ex = Assert.Throws<CredentialException>(() =>
            {
                TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                    "HandleFetchedFailure", supplier, new object[] { exception });
            });
            Assert.Equal("exception for test", ex.Message);

            supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync)
                .StaleValueBehavior(StaleValueBehavior.Allow).Build();
            cachedValue = new RefreshResult<CredentialModel>(new CredentialModel { AccessKeyId = "newAccessKey" },
                DateTime.UtcNow.AddMinutes(-10).GetTimeMillis());
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);
            var newStaleTime = DateTime.UtcNow.GetTimeMillis();
            updatedValue = TestHelper.RunInstanceMethod(typeof(RefreshCachedSupplier<CredentialModel>),
                "HandleFetchedFailure", supplier, new object[] { exception });
            Assert.True(newStaleTime + 1000 < ((RefreshResult<CredentialModel>)updatedValue).StaleTime);
            Assert.True(newStaleTime + 10000 > ((RefreshResult<CredentialModel>)updatedValue).StaleTime);
        }

        [Fact]
        public void TestJitterTime()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync).Build();
            var res = TestHelper.RunStaticMethodWithReturn(typeof(RefreshCachedSupplier<CredentialModel>),
                "MaxStaleFailureJitter",
                new object[] { 3 });
            Assert.Equal(10000L, res);

            res = TestHelper.RunStaticMethodWithReturn(typeof(RefreshCachedSupplier<CredentialModel>),
                "MaxStaleFailureJitter",
                new object[] { 10 });
            Assert.Equal(51200L, res);

            res = TestHelper.RunInstanceMethodNew(typeof(RefreshCachedSupplier<CredentialModel>), "JitterTime",
                supplier,
                new object[] { 1735627102627L, 1000L, 10000L });
            Assert.True(1735627112627L > (long)res);
            Assert.True(1735627103627L < (long)res);

            // Test TestHelper.RunInstanceMethodNew
            // 方法不存在
            var ex1 = Assert.Throws<ArgumentException>(() =>
            {
                TestHelper.RunInstanceMethodNew(typeof(RefreshCachedSupplier<CredentialModel>), "NotExist", supplier,
                    new object[] { 1735627102627L, 1000, 10000 });
            });
            Assert.Equal(
                "There is no method \"NotExist\" for type \"Aliyun.Credentials.Provider.RefreshCachedSupplier`1[Aliyun.Credentials.Models.CredentialModel]\".",
                ex1.Message);
            // 类型不匹配
            ex1 = Assert.Throws<ArgumentException>(() =>
            {
                TestHelper.RunInstanceMethodNew(typeof(RefreshCachedSupplier<CredentialModel>), "JitterTime", supplier,
                    new object[] { 1735627102627L, 1000, 10000 });
            });
            Assert.Equal("No suitable overload found for method \"JitterTime\" with the specified parameter types.",
                ex1.Message);
        }

        [Fact]
        public async void TestShouldInitiateCachePrefetch()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync).Build();
            // 缓存未过期，无需prefetch
            var cachedValue =
                new RefreshResult<CredentialModel>.Builder(new CredentialModel { AccessKeyId = "accessKey" })
                    .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .PrefetchTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .Build();
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);

            var res = supplier.Get();
            Assert.Equal("accessKey", res.AccessKeyId);

            // 缓存未过期，需要prefetch
            cachedValue =
                new RefreshResult<CredentialModel>.Builder(new CredentialModel { AccessKeyId = "accessKey" })
                    .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .PrefetchTime(DateTime.UtcNow.AddMinutes(-10).GetTimeMillis())
                    .Build();
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);

            res = supplier.Get();
            Assert.Equal("newAccessKey", res.AccessKeyId);
        }

        [Fact]
        public async void TestShouldInitiateCachePrefetchAsync()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync).Build();
            // 缓存未过期，无需prefetch
            var cachedValue =
                new RefreshResult<CredentialModel>.Builder(new CredentialModel { AccessKeyId = "accessKey" })
                    .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .PrefetchTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .Build();
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);

            var res = await supplier.GetAsync();
            Assert.Equal("accessKey", res.AccessKeyId);

            // 缓存未过期，需要prefetch
            cachedValue =
                new RefreshResult<CredentialModel>.Builder(new CredentialModel { AccessKeyId = "accessKey" })
                    .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .PrefetchTime(DateTime.UtcNow.AddMinutes(-10).GetTimeMillis())
                    .Build();
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);

            res = await supplier.GetAsync();
            Assert.Equal("newAccessKeyForAsync", res.AccessKeyId);
        }

        [Fact]
        public async void TestNonBlocking()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync)
                .AsyncUpdateEnabled(true)
                .Build();

            var cachedValue =
                new RefreshResult<CredentialModel>.Builder(new CredentialModel { AccessKeyId = "accessKey" })
                    .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .PrefetchTime(DateTime.UtcNow.AddMinutes(-10).GetTimeMillis())
                    .Build();
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);
            var res = supplier.Get();
            Assert.Equal("accessKey", res.AccessKeyId);
            await Task.Delay(1000);
            res = supplier.Get();
            Assert.Equal("newAccessKey", res.AccessKeyId);
        }

        [Fact]
        public async void TestNonBlockingAsync()
        {
            var supplier = new RefreshCachedSupplier<CredentialModel>.Builder(RefreshFunc, RefreshFuncAsync)
                .AsyncUpdateEnabled(true)
                .Build();

            var cachedValue =
                new RefreshResult<CredentialModel>.Builder(new CredentialModel { AccessKeyId = "accessKey" })
                    .StaleTime(DateTime.UtcNow.AddMinutes(10).GetTimeMillis())
                    .PrefetchTime(DateTime.UtcNow.AddMinutes(-10).GetTimeMillis())
                    .Build();
            TestHelper.SetPrivateField(typeof(RefreshCachedSupplier<CredentialModel>), "cachedValue", supplier,
                cachedValue);

            var res = await supplier.GetAsync();
            Assert.Equal("accessKey", res.AccessKeyId);
            await Task.Delay(1000);
            res = await supplier.GetAsync();
            Assert.Equal("newAccessKeyForAsync", res.AccessKeyId);
        }

        [Fact]
        public async Task NonBlocking_Prefetch_When_Not_Refreshing()
        {
            var nonBlocking = new NonBlocking();
            // Create a TaskCompletionSource to await the action
            var tcs = new TaskCompletionSource<bool>();

            void MockAction()
            {
                tcs.SetResult(true);
            }

            nonBlocking.Prefetch(MockAction);
            var result = await tcs.Task;
            Assert.True(result);

            var wasCalled = false;
            nonBlocking.Prefetch(() =>
            {
                Thread.Sleep(100);
                wasCalled = true;
            });
            await Task.Delay(1);
            Assert.False(wasCalled);
            nonBlocking.Dispose();
        }

        [Fact]
        public async void NonBlocking_Prefetch_When_Is_Refreshing_Manual()
        {
            var nonBlocking = new NonBlocking();
            var wasCalled = false;

            TestHelper.SetPrivateField(typeof(NonBlocking), "currentlyRefreshing", nonBlocking, 1);
            nonBlocking.Prefetch(() => wasCalled = true);
            await Task.Delay(10);
            Assert.False(wasCalled);
            nonBlocking.Dispose();
        }

        [Fact]
        public async void NonBlocking_Prefetch_When_Is_Refreshing_Manual_Async()
        {
            var nonBlocking = new NonBlocking();
            var wasCalled = false;

            TestHelper.SetPrivateField(typeof(NonBlocking), "currentlyRefreshing", nonBlocking, 1);

            await nonBlocking.PrefetchAsync(async () =>
            {
                await Task.Delay(100);
                wasCalled = true;
            });
            Assert.False(wasCalled);
            nonBlocking.Dispose();
        }

        [Fact]
        public void NonBlocking_Prefetch_ConcurrentTasks_SignalFull0()
        {
            var nonBlocking = new NonBlocking();
            var wasCalled1 = false;
            var wasCalled2 = false;

            TestHelper.SetPrivateField(typeof(NonBlocking), "concurrentRefreshLeases", nonBlocking,
                new SemaphoreSlim(0, 1));

            var tasks = new[]
            {
                Task.Run(() => nonBlocking.Prefetch(() => { wasCalled1 = true; })),
                Task.Run(() => nonBlocking.Prefetch(() => { wasCalled2 = true; })),
            };

            Task.WaitAll(tasks);
            Assert.False(wasCalled1);
            Assert.False(wasCalled2);
            nonBlocking.Dispose();
        }


        [Fact]
        public void NonBlocking_Prefetch_ConcurrentTasks_SignalFull1()
        {
            var nonBlocking = new NonBlocking();
            var wasCalled1 = false;
            var wasCalled2 = false;

            TestHelper.SetPrivateField(typeof(NonBlocking), "concurrentRefreshLeases", nonBlocking,
                new SemaphoreSlim(1, 1));

            // 一个任务还没结束时（信号量/锁未释放），第二个任务开始了，就算拿到了锁，但是信号量不够
            var tasks = new[]
            {
                Task.Run(() => nonBlocking.Prefetch(() =>
                {
                    Thread.Sleep(1000);
                    wasCalled1 = true;
                })),
                Task.Run(() => nonBlocking.Prefetch(() =>
                {
                    Thread.Sleep(1000);
                    wasCalled2 = true;
                })),
            };

            Task.WaitAll(tasks);
            Thread.Sleep(1100);
            Assert.True(wasCalled1 || wasCalled2, "At least two variable must be true");
            Assert.False(wasCalled1 && wasCalled2, "Both variables cannot be true at the same time");
            nonBlocking.Dispose();
        }

        [Fact]
        public void NonBlocking_Prefetch_ConcurrentTasks_SignalFull2()
        {
            var nonBlocking = new NonBlocking();
            var wasCalled1 = false;
            var wasCalled2 = false;
            var wasCalled3 = false;

            TestHelper.SetPrivateField(typeof(NonBlocking), "concurrentRefreshLeases", nonBlocking,
                new SemaphoreSlim(2, 2));

            var tasks = new[]
            {
                Task.Run(() => nonBlocking.Prefetch(() =>
                {
                    Thread.Sleep(100);
                    wasCalled1 = true;
                })),
                Task.Run(() => nonBlocking.Prefetch(() =>
                {
                    Thread.Sleep(800);
                    wasCalled2 = true;
                })),
                Task.Run(() => nonBlocking.Prefetch(() =>
                {
                    Thread.Sleep(3000);
                    wasCalled3 = true;
                }))
            };

            Task.WaitAll(tasks);
            Thread.Sleep(3100);

            Assert.True(wasCalled1 || wasCalled2 || wasCalled3, "At least one variable must be true");
            Assert.False(wasCalled1 && wasCalled2 && wasCalled3, "Both variables cannot be true at the same time");
            nonBlocking.Dispose();
        }

        [Fact]
        public async void NonBlocking_Prefetch_ConcurrentTasks_SignalFullAsync1()
        {
            var nonBlocking = new NonBlocking();
            var wasCalled1 = false;
            var wasCalled2 = false;

            TestHelper.SetPrivateField(typeof(NonBlocking), "concurrentRefreshLeases", nonBlocking,
                new SemaphoreSlim(1, 1));

            var tasks = new[]
            {
                Task.Run(() => nonBlocking.PrefetchAsync(async () =>
                {
                    wasCalled1 = true;
                    await Task.Delay(1000);
                })),
                Task.Run(() => nonBlocking.PrefetchAsync(async () =>
                {
                    wasCalled2 = true;
                    await Task.Delay(2000);
                })),
            };
            await Task.WhenAll(tasks);
            await Task.Delay(2100);

            Assert.True(wasCalled1 || wasCalled2, "At least one variable must be true");
            Assert.False(wasCalled1 && wasCalled2, "Both variables cannot be true at the same time");
            nonBlocking.Dispose();
        }

        [Fact]
        public async void NonBlocking_Prefetch_ConcurrentTasks_SignalFullAsync2()
        {
            var nonBlocking = new NonBlocking();
            var wasCalled1 = false;
            var wasCalled2 = false;
            var wasCalled3 = false;

            TestHelper.SetPrivateField(typeof(NonBlocking), "concurrentRefreshLeases", nonBlocking,
                new SemaphoreSlim(2, 2));

            var tasks = new[]
            {
                Task.Run(() => nonBlocking.PrefetchAsync(async () =>
                {
                    wasCalled1 = true;
                    await Task.Delay(800);
                })),
                Task.Run(() => nonBlocking.PrefetchAsync(async () =>
                {
                    wasCalled2 = true;
                    await Task.Delay(1200);
                })),
                Task.Run(() => nonBlocking.PrefetchAsync(async () =>
                {
                    wasCalled3 = true;
                    await Task.Delay(2000);
                })),
            };
            await Task.WhenAll(tasks);
            await Task.Delay(2100);

            Assert.True(wasCalled1 || wasCalled2 || wasCalled3, "At least one variable must be true");
            Assert.False(wasCalled1 && wasCalled2 && wasCalled3, "Both variables cannot be true at the same time");
            nonBlocking.Dispose();
        }

        [Fact]
        public async void OneCaller_Prefetch_When_Is_Refreshing_Manual()
        {
            var oneCaller = new OneCallerBlocks();
            var wasCalled = false;

            TestHelper.SetPrivateField(typeof(OneCallerBlocks), "currentlyRefreshing", oneCaller, 1);

            oneCaller.Prefetch(() => wasCalled = true);
            await Task.Delay(10);
            Assert.False(wasCalled);
            oneCaller.Dispose();
        }

        [Fact]
        public async void OneCaller_Prefetch_When_Is_Refreshing_Manual_Async()
        {
            var oneCaller = new OneCallerBlocks();
            var wasCalled = false;

            TestHelper.SetPrivateField(typeof(OneCallerBlocks), "currentlyRefreshing", oneCaller, 1);

            await oneCaller.PrefetchAsync(async () =>
            {
                await Task.Delay(100);
                wasCalled = true;
            });
            Assert.False(wasCalled);
            oneCaller.Dispose();
        }


        public void Dispose()
        {
        }
    }
}