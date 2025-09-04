using Castle.DynamicProxy;
using CoreRemoting.Serialization.Bson;
using DotNetty.Buffers;
using DotNettyCoreRemoting.RemoteDelegates;
using DotNettyCoreRemoting.RpcMessaging;
using DotNettyCoreRemoting.Serialization;
using DotNettyCoreRemoting.Util;
using Serialize.Linq.Extensions;
using stakx.DynamicProxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotNettyCoreRemoting
{
    public class ServiceProxy<TServiceInterface> : AsyncInterceptor
    {
        private DotNettyRPCClient _client;

        private readonly string _serviceName;

        private readonly int _timeout;

        public ServiceProxy(DotNettyRPCClient client, string serviceName = "", int timeout = 60)
        {
            _client = client;

            var serviceInterfaceType = typeof(TServiceInterface);

            _serviceName =
                string.IsNullOrWhiteSpace(serviceName)
                    ? serviceInterfaceType.FullName
                    : serviceName;

            _timeout = timeout;
        }


        ///// <summary>
        ///// Maps a delegate argument into a serializable RemoteDelegateInfo object.
        ///// </summary>
        ///// <param name="argumentType">Type of argument to be mapped</param>
        ///// <param name="argument">Argument to be wrapped</param>
        ///// <param name="mappedArgument">Out: Mapped argument</param>
        ///// <returns>True if mapping applied, otherwise false</returns>
        private bool MapDelegateArgument(Type argumentType, object argument, out object mappedArgument)
        {
            if (argumentType == null || !typeof(Delegate).IsAssignableFrom(argumentType))
            {
                mappedArgument = argument;
                return false;
            }

            var delegateReturnType = argumentType.GetMethod("Invoke")?.ReturnType;

            if (delegateReturnType != null && delegateReturnType != typeof(void))
                throw new NotSupportedException("Only void delegates are supported.");

            var remoteDelegateInfo =
                new RemoteDelegateInfo(
                    handlerKey: _client.ClientDelegateRegistry.RegisterClientDelegate((Delegate)argument, this),
                    delegateTypeName: argumentType.FullName + ", " + argumentType.Assembly.GetName().Name);

            mappedArgument = remoteDelegateInfo;
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
            var isLinqExpression = false;

            if (argumentType != null && argumentType.IsGenericType)
            {
                var baseType = argumentType.BaseType;
                if (baseType != null && baseType.IsGenericType)
                {
                    if (baseType.GetGenericTypeDefinition() == typeof(Expression<>))
                    {
                        isLinqExpression = true;
                    }
                }
            }

            if (!isLinqExpression)
            {
                mappedArgument = argument;
                return false;
            }

            var expression = (Expression)argument;
            mappedArgument = expression.ToExpressionNode();

            return true;
        }

        /// <summary>
        /// Maps non serializable arguments into a serializable form.
        /// </summary>
        /// <param name="arguments">Arguments</param>
        /// <returns>Array of arguments (includes mapped ones)</returns>
        private object[] MapArguments(IEnumerable<object> arguments)
        {
            var mappedArguments =
                arguments.Select(argument =>
                {
                    var type = argument?.GetType();

                    if (MapDelegateArgument(type, argument, out var mappedArgument))
                        return mappedArgument;

                    if (MapLinqExpressionArgument(type, argument, out mappedArgument))
                        return mappedArgument;

                    return argument;
                }).ToArray();

            return mappedArguments;
        }


        protected override void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            var arguments = MapArguments(invocation.Arguments);

            var remoteMethodCallMessage =
                _client.MethodCallMessageBuilder.BuildMethodCallMessage(
                  _client.Serializer,
                  remoteServiceName: _serviceName,
                  targetMethod: method,
                  args: arguments);

            var client = AsyncHelper.RunSync(() => DotNettyClientManager._bootstrap.ConnectAsync(_client.ipEndPoint()));

            if (client == null)
                throw new Exception("Failed to connect to the server!");

            try
            {

                var waitKey = client.Id.AsShortText();
                DotNettyClientManager._clientWait.Start(waitKey);

                var sendBuffer = Unpooled.WrappedBuffer(_client.Serializer.Serialize(remoteMethodCallMessage));
                AsyncHelper.RunSync(() => client.WriteAndFlushAsync(sendBuffer));

                var ReturnResponse = DotNettyClientManager._clientWait.Wait(waitKey, TimeSpan.FromSeconds(_timeout)).ReturnResponse;

                var clientRpcContext = _client.Serializer.Deserialize<ClientRpcContext>(ReturnResponse);

                if (clientRpcContext.Error)
                {
                    throw new RemoteInvocationException(clientRpcContext.ErrorMessage);
                }

                var resultMessage = clientRpcContext.ResultMessage;

                if (resultMessage == null)
                {
                    invocation.ReturnValue = null;
                    return;
                }

                var parameterInfos = method.GetParameters();

                var serializer = _client.Serializer;

                foreach (var outParameterValue in resultMessage.OutParameters)
                {
                    var parameterInfo =
                        parameterInfos.First(p => p.Name == outParameterValue.ParameterName);

                    if (outParameterValue.IsOutValueNull)
                        invocation.Arguments[parameterInfo.Position] = null;
                    else
                    {
                        if (serializer.EnvelopeNeededForParameterSerialization)
                        {
                            var outParamEnvelope =
                                serializer.Deserialize<Envelope>((byte[])outParameterValue.OutValue);

                            invocation.Arguments[parameterInfo.Position] = outParamEnvelope.Value;
                        }
                        else
                        {
                            var outParamValue =
                                serializer.Deserialize(parameterInfo.ParameterType, (byte[])outParameterValue.OutValue);

                            invocation.Arguments[parameterInfo.Position] = outParamValue;
                        }
                    }
                }

                var returnValue =
                    resultMessage.IsReturnValueNull
                        ? null
                        : resultMessage.ReturnValue is Envelope returnValueEnvelope
                            ? returnValueEnvelope.Value
                            : resultMessage.ReturnValue;

                invocation.ReturnValue = returnValue;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }
        }

        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            var method = invocation.Method;

            var arguments = MapArguments(invocation.Arguments);

            var remoteMethodCallMessage =
                _client.MethodCallMessageBuilder.BuildMethodCallMessage(
                  _client.Serializer,
                  remoteServiceName: _serviceName,
                  targetMethod: method,
                  args: arguments);

            var client = await DotNettyClientManager._bootstrap.ConnectAsync(_client.ipEndPoint());


            if (client == null)
                throw new Exception("Failed to connect to the server!");

            try
            {

                var waitKey = client.Id.AsShortText();
                DotNettyClientManager._clientWait.Start(waitKey);

                var sendBuffer = Unpooled.WrappedBuffer(_client.Serializer.Serialize(remoteMethodCallMessage));
                await client.WriteAndFlushAsync(sendBuffer);

                var ReturnResponse = DotNettyClientManager._clientWait.Wait(waitKey, TimeSpan.FromSeconds(_timeout)).ReturnResponse;

                var clientRpcContext = _client.Serializer.Deserialize<ClientRpcContext>(ReturnResponse);

                if (clientRpcContext.Error)
                {
                    throw new RemoteInvocationException(clientRpcContext.ErrorMessage);
                }

                var resultMessage = clientRpcContext.ResultMessage;

                if (resultMessage == null)
                {
                    invocation.Result = null;
                    return;
                }


                var returnValue =
                    resultMessage.IsReturnValueNull
                        ? null
                        : resultMessage.ReturnValue is Envelope returnValueEnvelope
                            ? returnValueEnvelope.Value
                            : resultMessage.ReturnValue;

                invocation.Result = returnValue;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
