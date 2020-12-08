using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private static async Task GrpsShow()
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
            {
                //var client = new Greeter
            }
        }
    }
}
