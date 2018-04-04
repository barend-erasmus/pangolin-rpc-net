using System;
using System.Net.Sockets;

namespace PangolinRPCNet
{
    public class RPCClient
    {
        protected string _host { get; set; }

        protected int _port { get; set; }

        protected RPC _rpc { get; set; }

        protected TcpClient _tcpClient { get; set; }

        public RPCClient(string host, int port)
        {
            _host = host;

            _port = port;

            _tcpClient = new TcpClient(_host, _port);

            _rpc = new RPC(_tcpClient.Client);
        }

        public void Send(Action<Message> action, Message message)
        {
            if (string.IsNullOrWhiteSpace(message.CorrelationId))
            {
                message.CorrelationId = Guid.NewGuid().ToString();
            }

            _rpc.Send(action, message);
        }

        public void SetOnMessageAction(Func<Message, dynamic> action)
        {
            _rpc.SetOnMessageAction(action);
        }
    }
}
