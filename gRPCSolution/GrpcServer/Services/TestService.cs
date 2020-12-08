using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcServer.Services
{
    public class TestService : Test.TestBase
    {
        public override Task<HelloReply_Test> SayHello(HelloRequest_Test request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply_Test() { Message = $"gRPC.test:Id={request.Id},Name={request.Name}" });
        }

        public override Task<CountResult> Count(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new CountResult() { Count = DateTime.Now.Year });
        }

        public override Task<ResponseResult> Plus(RequestPara request, ServerCallContext context)
        {
            return Task.FromResult(new ResponseResult { Result = request.ILeft + request.IRight, Message = "Sucess" });
        }

        public override async Task SelfIncreaseServer(IntArrayModel request, IServerStreamWriter<BatchTheCatResp> responseStream, ServerCallContext context)
        {
            foreach (var item in request.Number)
            {
                int number = item;
                await responseStream.WriteAsync(new BatchTheCatResp { Message = $"number={++number}" });
                await Task.Delay(500);
            }
        }

        public override async Task<IntArrayModel> SelfIncreaseClient(IAsyncStreamReader<BatchTheCatReq> requestStream, ServerCallContext context)
        {
            var intArrayModel = new IntArrayModel();
            while (await requestStream.MoveNext())
            {
                intArrayModel.Number.Add(requestStream.Current.Id + 1);
                Thread.Sleep(100);
            }
            return intArrayModel;
        }

        public override async Task SelfIncreaseDouble(IAsyncStreamReader<BatchTheCatReq> requestStream, IServerStreamWriter<BatchTheCatResp> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                await responseStream.WriteAsync(new BatchTheCatResp() { Message = $"number={++requestStream.Current.Id}" });
                await Task.Delay(500);
            }
        }
    }
}
