using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            GrpsShow().GetAwaiter().GetResult();
        }

        private static async Task GrpsShow()
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                var client = new Greeter.GreeterClient(channel);
                var reply = await client.SayHelloAsync(new HelloRequest { Name = "gRPC" });
                Console.WriteLine($"gRPC:{reply.Message}");
            }
        }
    }
}
