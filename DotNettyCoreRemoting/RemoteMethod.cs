using CoreRemoting.RemoteDelegates;
using DotNettyCoreRemoting.DependencyInjection;
using DotNettyCoreRemoting.Logging;
using DotNettyCoreRemoting.RemoteDelegates;
using DotNettyCoreRemoting.RpcMessaging;
using DotNettyCoreRemoting.Serialization;
using DotNettyCoreRemoting.Util;
using Serialize.Linq.Interfaces;
using Serialize.Linq.Nodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNettyCoreRemoting
{
    public class RemoteMethod
    {
        private readonly IDependencyInjectionContainer _ServiceRegistry;
        private IDelegateProxyFactory _delegateProxyFactory;
        private readonly RemoteDelegateInvocationEventAggregator _remoteDelegateInvocationEventAggregator;
        private MethodCallMessageBuilder MethodCallMessageBuilder { get; set; }

        private ISerializerAdapter _serializer { get; set; }


        /// <summary>
        /// Gets the remote delegate invocation event aggregator.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        internal RemoteDelegateInvocationEventAggregator RemoteDelegateInvocation =>
            _remoteDelegateInvocationEventAggregator;


        private ConcurrentDictionary<Guid, IDelegateProxy> _delegateProxyCache;

        public RemoteMethod(IDependencyInjectionContainer ServiceRegistry, ISerializerAdapter serializer)
        {

            _ServiceRegistry = ServiceRegistry;

            _serializer = serializer;

            _delegateProxyFactory = ServiceRegistry.GetService<IDelegateProxyFactory>();
            _delegateProxyCache = new ConcurrentDictionary<Guid, IDelegateProxy>();
            _remoteDelegateInvocationEventAggregator = new RemoteDelegateInvocationEventAggregator();

            MethodCallMessageBuilder = new MethodCallMessageBuilder();

        }

        public ClientRpcContext InvokeRemoteMethod(MethodCallMessage callMessage)
        {

            ClientRpcContext clientRpcContext = new ClientRpcContext();
            try
            {

                var registration = _ServiceRegistry.GetServiceRegistration(callMessage.ServiceName);


                var service = _ServiceRegistry.GetService(callMessage.ServiceName);
                if (service == null)
                {
                    throw new InvalidOperationException($"Service '{callMessage.ServiceName}' does not exist or is not properly registered.");
                }

                var serviceInterfaceType = registration.InterfaceType;

                callMessage.UnwrapParametersFromDeserializedMethodCallMessage(
                        out var parameterValues,
                        out var parameterTypes);

                var method = GetMethodInfo(callMessage, serviceInterfaceType, parameterTypes);


                parameterValues = MapArguments(parameterValues, parameterTypes);


                object result = method.Invoke(service, parameterValues);


                var returnType = method.ReturnType;
                if (result != null && typeof(Task).IsAssignableFrom(returnType))
                {

                    var resultTask = (Task)result;

                    // 安全同步等待 Task

                    resultTask.ConfigureAwait(false).GetAwaiter().GetResult();


                    if (returnType.IsGenericType)
                    {

                        result = returnType.GetProperty("Result")?.GetValue(resultTask);
                    }
                    else // ordinary non-generic task
                    {

                        result = null;
                    }
                }


                clientRpcContext.ResultMessage = MethodCallMessageBuilder.BuildMethodCallResultMessage(_serializer,
                      method: method,
                      args: parameterValues,
                      returnValue: result);


            }
            catch (Exception ex)
            {
                var actualEx = ex is TargetInvocationException tie ? tie.InnerException ?? ex : ex;
                Logger.Error(typeof(RemoteMethod), $"远程方法调用异常 - 服务名: {callMessage?.ServiceName ?? "未知"}, 方法名: {callMessage?.MethodName ?? "未知"}", actualEx);
                clientRpcContext.Error = true;
                clientRpcContext.ErrorMessage = ExceptionHelper.GetExceptionAllMsg(actualEx);
            }

            return clientRpcContext;
        }


        private MethodInfo GetMethodInfo(MethodCallMessage callMessage, Type serviceInterfaceType, Type[] parameterTypes)
        {
            MethodInfo method;

            if (callMessage.GenericArgumentTypeNames != null && callMessage.GenericArgumentTypeNames.Length > 0)
            {
                var methods =
                    serviceInterfaceType.GetMethods().ToList();

                foreach (var inheritedInterface in serviceInterfaceType.GetInterfaces())
                {
                    methods.AddRange(inheritedInterface.GetMethods());
                }

                method =
                    methods.SingleOrDefault(m =>
                    m.IsGenericMethod &&
                        m.Name.Equals(callMessage.MethodName, StringComparison.Ordinal));

                if (method != null)
                {
                    Type[] genericArguments =
                        callMessage.GenericArgumentTypeNames
                            .Select(typeName => Type.GetType(typeName))
                            .ToArray();

                    method = method.MakeGenericMethod(genericArguments);
                }
            }
            else
            {
                method =
                    serviceInterfaceType.GetMethod(
                        name: callMessage.MethodName,
                        types: parameterTypes);

                if (method == null)
                {
                    foreach (var inheritedInterface in serviceInterfaceType.GetInterfaces())
                    {
                        method =
                            inheritedInterface.GetMethod(
                                name: callMessage.MethodName,
                                types: parameterTypes);

                        if (method != null)
                            break;
                    }
                }
            }

            return method;
        }

        /// <summary>
        /// Maps non serializable arguments into a serializable form.
        /// </summary>
        /// <param name="arguments">Array of parameter values</param>
        /// <param name="argumentTypes">Array of parameter types</param>
        /// <returns>Array of arguments (includes mapped ones)</returns>
        private object[] MapArguments(object[] arguments, Type[] argumentTypes)
        {
            object[] mappedArguments = new object[arguments.Length];

            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var type = argumentTypes[i];

                if (MapDelegateArgument(argument, out var mappedArgument))
                    mappedArguments[i] = mappedArgument;
                else if (MapLinqExpressionArgument(type, argument, out mappedArgument))
                    mappedArguments[i] = mappedArgument;
                else
                    mappedArguments[i] = argument;
            }

            return mappedArguments;
        }

        /// <summary>
        /// Maps a delegate argument into a delegate proxy.
        /// </summary>
        /// <param name="argument">argument value</param>
        /// <param name="mappedArgument">Out: argument value where delegate value is mapped into delegate proxy</param>
        /// <returns>True if mapping applied, otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if no session is provided</exception>
        private bool MapDelegateArgument(object argument, out object mappedArgument)
        {
            if (!(argument is RemoteDelegateInfo remoteDelegateInfo))
            {
                mappedArgument = argument;
                return false;
            }

            if (_delegateProxyCache.TryGetValue(remoteDelegateInfo.HandlerKey, out var value))
            {
                mappedArgument = value.ProxiedDelegate;
                return true;
            }

            var delegateType = Type.GetType(remoteDelegateInfo.DelegateTypeName);

            // Forge a delegate proxy and initiate remote delegate invocation, when it is invoked
            var delegateProxy =
                _delegateProxyFactory.Create(delegateType, delegateArgs =>
                    RemoteDelegateInvocation
                        .InvokeRemoteDelegate(
                            delegateType: delegateType,
                            handlerKey: remoteDelegateInfo.HandlerKey,
                            remoteDelegateArguments: delegateArgs));

            _delegateProxyCache.TryAdd(remoteDelegateInfo.HandlerKey, delegateProxy);

            mappedArgument = delegateProxy.ProxiedDelegate;
            return true;
        }

        /// <summary>
        /// Maps a Linq expression argument into a serializable ExpressionNode object.
        /// </summary>
        /// <param name="argumentType">Type of argument to be mapped</param>
        /// <param name="argument">Argument to be wrapped</param>
        /// <param name="mappedArgument">Out: Mapped argument</param>
        /// <returns>True if mapping applied, otherwise false</returns>
        private bool MapLinqExpressionArgument(Type argumentType, object argument, out object mappedArgument)
        {
            var isLinqExpression =
                argumentType.IsGenericType &&
                argumentType.BaseType == typeof(LambdaExpression);

            if (!isLinqExpression)
            {
                mappedArgument = argument;
                return false;
            }

            var expression = ((ExpressionNode)argument).ToExpression();
            mappedArgument = expression;

            return true;
        }


    }
}
