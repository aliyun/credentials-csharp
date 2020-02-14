using System;
using System.Reflection;
using System.Threading.Tasks;

namespace aliyun_net_credentials_unit_tests
{
    public class TestHelper
    {
        public static readonly string Slash = Environment.OSVersion.Platform == PlatformID.Unix ? "/" : "\\";
        public static readonly string HomePath = Environment.CurrentDirectory;

        /// <summary>
        /// 调用静态方法
        /// </summary>
        /// <param name="t">类全名</param>
        /// <param name="strMethod">方法名</param>
        /// <param name="aobjParams">参数表</param>
        /// <returns>函数返回值</returns>
        public static void RunStaticMethod(Type t, string strMethod, object[] aobjParams)
        {
            BindingFlags eFlags =
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic;
            RunMethod(t, strMethod, null, aobjParams, eFlags);
        }

        /// <summary>
        /// 调用实例方法
        /// </summary>
        /// <param name="t">类全名</param>
        /// <param name="strMethod">方法名</param>
        /// <param name="objInstance">类的实例</param>
        /// <param name="aobjParams">参数表</param>
        ///<returns>函数返回值</returns>
        public static object RunInstanceMethod(Type t, string strMethod, object objInstance, object[] aobjParams)
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic;
            return RunMethod(t, strMethod,
                objInstance, aobjParams, eFlags);
        }

        private static object RunMethod(Type t, string strMethod, object objInstance, object[] aobjParams,
            BindingFlags eFlags)
        {
            MethodInfo m;
            try
            {
                m = t.GetMethod(strMethod, eFlags);
                if (m == null)
                {
                    throw new ArgumentException("There is no method '" + strMethod + "' for type'" + t + "'.");
                }

                object objRet = m.Invoke(objInstance, aobjParams);
                return objRet; 
            }
            catch (TargetInvocationException ex)
            {
                if (null != ex.InnerException)
                {
                    throw ex.InnerException;
                }
 
                throw;
            }
        }

        /// <summary>
        /// 调用静态异步方法
        /// </summary>
        /// <param name="t">类全名</param>
        /// <param name="strMethod">方法名</param>
        /// <param name="aobjParams">参数表</param>
        /// <returns>函数返回值</returns>
        public static void RunStaticMethodAsync(Type t, string strMethod, object[] aobjParams)
        {
            BindingFlags eFlags =
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic;
            RunMethodAsync(t, strMethod, null, aobjParams, eFlags);
        }

        /// <summary>
        /// 调用实例异步方法
        /// </summary>
        /// <param name="t">类全名</param>
        /// <param name="strMethod">方法名</param>
        /// <param name="objInstance">类的实例</param>
        /// <param name="aobjParams">参数表</param>
        ///<returns>函数返回值</returns>
        public static object RunInstanceMethodAsync(Type t, string strMethod, object objInstance, object[] aobjParams)
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic;
            return RunMethodAsync(t, strMethod,
                objInstance, aobjParams, eFlags);
        }

        private static object RunMethodAsync(Type t, string strMethod, object objInstance, object[] aobjParams,
            BindingFlags eFlags)
        {
            MethodInfo m;
            try
            {
                m = t.GetMethod(strMethod, eFlags);
                if (m == null)
                {
                    throw new ArgumentException("There is no method '" + strMethod + "' for type'" + t + "'.");
                }

                var task = m.Invoke(objInstance, aobjParams) as Task;
                if (task.Status == TaskStatus.WaitingForActivation || task.Status == TaskStatus.WaitingToRun)
                {
                    task.Wait();
                }
                if (task.Status == TaskStatus.Faulted)
                {
                    throw task.Exception;
                }
                return task.GetType().GetProperty("Result").GetValue(task, null);
            }
            catch (TargetInvocationException ex)
            {
                if (null != ex.InnerException)
                {
                    throw ex.InnerException;
                }

                throw;
            }
            catch (AggregateException ex)
            {
                if (null != ex.InnerException)
                {
                    throw ex.InnerException;
                }
                throw;
            }
        }

        public static void RemoveEnvironmentValue()
        {
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_ID", null);
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_SECRET", null);
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_REGION_ID", null);
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_PROFILE", null);
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_ECS_METADATA", null);
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_CREDENTIALS_FILE", null);
        }

        public static void InitializeEnvironmentValue()
        {
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_ID", "ACCESS_KEY_ID");
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_ACCESS_KEY_SECRET", "ACCESS_KEY_SECRET");
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_REGION_ID", "cn-hangzhou");
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_PROFILE", "profile");
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_ECS_METADATA", "metadata");
            Environment.SetEnvironmentVariable("ALIBABA_CLOUD_CREDENTIALS_FILE", "file");
        }

        public static string GetIniFilePath()
        {
            return HomePath + Slash + "configTest.ini";
        }
    }
}
