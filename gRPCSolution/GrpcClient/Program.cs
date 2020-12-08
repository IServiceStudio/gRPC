using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using GrpcServer.Protos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            GrpcMethod().GetAwaiter().GetResult();
            CustomeGrpcMethod().GetAwaiter().GetResult();
            ServerStreamingRPCMethod().GetAwaiter().GetResult();
            SelfIncreaseClientRPCMethod().GetAwaiter().GetResult();
            SelfIncreaseDoubleRPCMethod().GetAwaiter().GetResult();
        }

        //simple gRPC
        private static async Task GrpcMethod()
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                var client = new Greeter.GreeterClient(channel);
                var reply = await client.SayHelloAsync(new HelloRequest { Name = "gRPC" });
                Console.WriteLine($"gRPC:{reply.Message}");
            }
        }

        //Unary(URPC) gRPC--简单
        //传入一个请求对象，返回一个对象
        private static async Task CustomeGrpcMethod()
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                var client = new Test.TestClient(channel);
                var reply = await client.PlusAsync(new RequestPara { ILeft = 1, IRight = 2 });
                Console.WriteLine($"CustomergRPC:Message={reply.Message},Result={reply.Result}");
            }
        }

        //ServerStreamingRPC gRPC--服务端流式
        //传入一个请求对象，返回多个结果对象，服务端处理完成一个返回一个
        private static async Task ServerStreamingRPCMethod()
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                var client = new Test.TestClient(channel);
                IntArrayModel intArrayModel = new IntArrayModel();
                for (int i = 1; i < 10; i++)
                {
                    intArrayModel.Number.Add(i);
                }

                //var cancellationToken = new CancellationTokenSource();
                //cancellationToken.CancelAfter(TimeSpan.FromSeconds(2.5));
                //var reply = client.SelfIncreaseServer(intArrayModel, cancellationToken: cancellationToken.Token);

                var reply = client.SelfIncreaseServer(intArrayModel);
                await Task.Run(async () =>
                {
                    await foreach (var resp in reply?.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine(resp.Message);
                    }
                });
            }
        }

        //ClientStreamingRPC gRPC--客户端流式
        //传入多个请求对象，返回一个结果对象，分批传入分批处理，统一返回
        private static async Task SelfIncreaseClientRPCMethod()
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                var client = new Test.TestClient(channel);
                var reply = client.SelfIncreaseClient();
                for (int i = 0; i < 10; i++)
                {
                    int id = new Random(Guid.NewGuid().GetHashCode()).Next(0, 20);
                    Console.WriteLine($"发送数据{id}");
                    await reply.RequestStream.WriteAsync(new BatchTheCatReq() { Id = id });
                    await Task.Delay(100);
                }

                await reply.RequestStream.CompleteAsync();

                Console.WriteLine("接收处理结果");
                foreach (var item in reply.ResponseAsync.Result.Number)
                {
                    Console.WriteLine($"This is {item} Result");
                }
            }
        }

        //Bi-DirectionalStreamingRPC gRPC--双向流式
        //传入多个对象，返回多个结果对象
        private static async Task SelfIncreaseDoubleRPCMethod()
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                var client = new Test.TestClient(channel);
                var reply = client.SelfIncreaseDouble();
                var bathCatRespTask = Task.Run(async () =>
                {
                    await foreach (var resp in reply.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine($"返回结果：{resp.Message}");
                    }
                });
                for (int i = 0; i < 10; i++)
                {
                    int id = new Random().Next(2, 20);
                    Console.WriteLine($"传入参数:{id}");
                    await reply.RequestStream.WriteAsync(new BatchTheCatReq() { Id = id });
                    await Task.Delay(100);
                }

                await reply.RequestStream.CompleteAsync();
                Console.WriteLine("处理完毕");
                await bathCatRespTask;
            }
        }
    }
}
