using System;

namespace PangolinRPCNet.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // SERVERS
            var rpcServer1 = new RPCServer(5001);
            rpcServer1.SetOnMessageAction(OnMessageAction);

            var rpcServer2 = new RPCServer(5002);
            rpcServer2.SetOnMessageAction(OnMessageAction);

            var rpcServer3 = new RPCServer(5003);
            rpcServer3.SetOnMessageAction(OnMessageAction);

            // CLIENTS
            var rcpServer1Client1 = new RPCClient("localhost", 5002);
            rcpServer1Client1.SetOnMessageAction(OnMessageAction);

            var rcpServer1Client2 = new RPCClient("localhost", 5003);
            rcpServer1Client2.SetOnMessageAction(OnMessageAction);

            var rcpServer2Client1 = new RPCClient("localhost", 5001);
            rcpServer2Client1.SetOnMessageAction(OnMessageAction);

            var rcpServer2Client2 = new RPCClient("localhost", 5003);
            rcpServer2Client2.SetOnMessageAction(OnMessageAction);

            var rcpServer3Client1 = new RPCClient("localhost", 5001);
            rcpServer3Client1.SetOnMessageAction(OnMessageAction);

            var rcpServer3Client2 = new RPCClient("localhost", 5001);
            rcpServer3Client2.SetOnMessageAction(OnMessageAction);

            // SEND MESSAGES
            rcpServer3Client2.Send((Message message) =>
            {
                System.Console.WriteLine($"Callback => {message.ToString()}");
            }, new Message("show", null, new { Text = "hello world" }));

            rpcServer1.Dispose();
        }

        static dynamic OnMessageAction(Message message)
        {
            if (message.Command == "show")
            {
                System.Console.WriteLine($"Command => {message.ToString()} => '{message.Payload.Text}'");
            }

            return "OK";
        }
    }
}
