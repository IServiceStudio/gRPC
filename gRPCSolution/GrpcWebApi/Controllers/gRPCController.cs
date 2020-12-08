using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using GrpcServer.Protos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GrpcWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class gRPCController : ControllerBase
    {
        private readonly Greeter.GreeterClient greeterClient;
        private readonly Test.TestClient testClient;

        public gRPCController(Greeter.GreeterClient greeterClient, Test.TestClient testClient)
        {
            this.greeterClient = greeterClient;
            this.testClient = testClient;
        }
        public async Task Get()
        {
            await GrpcMethod();
            await CustomeGrpcMethod();
            await ServerStreamingRPCMethod();
            await SelfIncreaseClientRPCMethod();
            await SelfIncreaseDoubleRPCMethod();
        }

        //simple gRPC
        private async Task GrpcMethod()
        {
            var reply = await greeterClient.SayHelloAsync(new HelloRequest { Name = "gRPC" });
            Console.WriteLine($"gRPC:{reply.Message}");
        }

        //Unary(URPC) gRPC--简单
        //传入一个请求对象，返回一个对象
        private async Task CustomeGrpcMethod()
        {
            var reply = await testClient.PlusAsync(new RequestPara { ILeft = 1, IRight = 2 });
            Console.WriteLine($"CustomergRPC:Message={reply.Message},Result={reply.Result}");
        }

        //ServerStreamingRPC gRPC--服务端流式
        //传入一个请求对象，返回多个结果对象，服务端处理完成一个返回一个
        private async Task ServerStreamingRPCMethod()
        {
            IntArrayModel intArrayModel = new IntArrayModel();
            for (int i = 1; i < 10; i++)
            {
                intArrayModel.Number.Add(i);
            }

            //var cancellationToken = new CancellationTokenSource();
            //cancellationToken.CancelAfter(TimeSpan.FromSeconds(2.5));
            //var reply = client.SelfIncreaseServer(intArrayModel, cancellationToken: cancellationToken.Token);

            var reply = testClient.SelfIncreaseServer(intArrayModel);
            await Task.Run(async () =>
            {
                await foreach (var resp in reply?.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(resp.Message);
                }
            });
        }

        //ClientStreamingRPC gRPC--客户端流式
        //传入多个请求对象，返回一个结果对象，分批传入分批处理，统一返回
        private async Task SelfIncreaseClientRPCMethod()
        {
            var reply = testClient.SelfIncreaseClient();
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

        //Bi-DirectionalStreamingRPC gRPC--双向流式
        //传入多个对象，返回多个结果对象
        private async Task SelfIncreaseDoubleRPCMethod()
        {
            var reply = testClient.SelfIncreaseDouble();
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