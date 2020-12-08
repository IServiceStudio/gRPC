using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Threading.Tasks;

namespace GrpcServer.Filter
{
    public class ServerFilter : Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine("接受客户端的请求");
            return base.UnaryServerHandler(request, context, continuation);
        }
    }
}
