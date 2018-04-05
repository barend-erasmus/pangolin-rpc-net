using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PangolinRPCNet
{
    public class RPCServer : IDisposable
    {
        protected TcpListener _tcpListener { get; set; }

        protected Func<Message, dynamic> _onMessageAction { get; set; }

        protected int _port { get; set; }

        protected IList<RPC> _rpcs { get; set; }

        protected Thread _thread { get; set; }

        public RPCServer(int port)
        {
            _port = port;

            _rpcs = new List<RPC>();

            Listen();
        }

        public void Dispose()
        {
            foreach (var rpc in _rpcs)
            {
                rpc.Dispose();
            }

            _tcpListener.Stop();

            _thread.Abort();
        }

        public void Send(Action<Message> action, Message message)
        {
            if (string.IsNullOrWhiteSpace(message.CorrelationId))
            {
                message.CorrelationId = Guid.NewGuid().ToString();
            }

            for (var index = 0; index < _rpcs.Count; index++)
            {
                var rpc = _rpcs[index];

                rpc.Send(action, message);
            }
        }

        public void SetOnMessageAction(Func<Message, dynamic> action)
        {
            _onMessageAction = action;

            for (var index = 0; index < _rpcs.Count; index++)
            {
                var rpc = _rpcs[index];

                rpc.SetOnMessageAction(action);
            }
        }

        protected void Listen()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);

            _tcpListener.Start();

            _thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (_tcpListener.Pending())
                    {
                        var socket = _tcpListener.AcceptSocket();

                        var rpc = new RPC(_rpcs, socket);

                        rpc.SetOnMessageAction(_onMessageAction);

                        _rpcs.Add(rpc);
                    }

                    Thread.Sleep(100);
                }
            }));

            _thread.Start();
        }
    }
}
