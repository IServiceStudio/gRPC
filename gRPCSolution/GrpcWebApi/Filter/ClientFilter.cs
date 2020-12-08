using Grpc.Core;
using Grpc.Core.Interceptors;
using System;

namespace GrpcWebApi.Filter
{
    public class ClientFilter : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine("客户端异步请求");
            return base.AsyncUnaryCall(request, context, continuation);
        }
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            Console.WriteLine("客户端同步请求");
            return base.BlockingUnaryCall(request, context, continuation);
        }
    }
}
