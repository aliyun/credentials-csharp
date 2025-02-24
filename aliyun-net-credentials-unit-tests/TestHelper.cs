using System;
using System.Linq;
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

        internal static object RunStaticMethodWithReturn(Type t, string strMethod, object[] aobjParams)
        {
            BindingFlags eFlags =
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic;
            return RunMethod(t, strMethod, null, aobjParams, eFlags);
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

        public static object RunInstanceMethodNew(Type t, string strMethod, object objInstance, object[] aobjParams)
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public |
                                  BindingFlags.NonPublic;
            return RunMethodNew(t, strMethod,
                objInstance, aobjParams, eFlags);
        }

        private static object RunMethodNew(Type t, string strMethod, object objInstance, object[] aobjParams,
            BindingFlags eFlags)
        {
            var methods = t.GetMethods(eFlags)
                .Where(m => m.Name == strMethod)
                .ToList();

            // not found
            if (!methods.Any())
            {
                throw new ArgumentException(string.Format("There is no method \"{0}\" for type \"{1}\".", strMethod,
                    t));
            }

            MethodInfo methodToInvoke = null;
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();

                // 检查参数个数和类型是否匹配
                if (parameters.Length != aobjParams.Length) continue;
                var matched = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    // 处理参数类型兼容性，如果参数可以为null，可以放宽这里的检查
                    if (aobjParams[i] == null || parameters[i].ParameterType.IsInstanceOfType(aobjParams[i])) continue;
                    matched = false;
                    break;
                }

                if (matched)
                {
                    methodToInvoke = method;
                    break;
                }
            }

            // 找到匹配的方法
            if (methodToInvoke != null)
            {
                try
                {
                    var objRet = methodToInvoke.Invoke(objInstance, aobjParams);
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

            throw new ArgumentException(string.Format(
                "No suitable overload found for method \"{0}\" with the specified parameter types.", strMethod));
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

        public static string GetIniFilePath()
        {
            return HomePath + Slash + "configTest.ini";
        }

        public static string GetOIDCTokenFilePath()
        {
            return HomePath + Slash + "OIDCToken.txt";
        }

        public static string GetCLIConfigFilePath(string type)
        {
            switch (type)
            {
                case "invalid":
                    return HomePath + Slash + "invalid_cli_config.json";
                case "empty":
                    return HomePath + Slash + "empty_file.json";
                case "mock_empty":
                    return HomePath + Slash + "mock_empty_cli_config.json";
                case "full":
                    return HomePath + Slash + "full_cli_config.json";
                case "aliyun":
                    return HomePath + Slash + Aliyun.Credentials.Configure.Constants.ConfigStorePath + Slash + "config.json";
                default:
                    return "";
            }
        }

        public static void SetPrivateField(Type t, string fieldName, object obj, object value)
        {
            t.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(obj, value);
        }
    }
}