using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PangolinRPCNet
{
    public class RPC : IDisposable
    {
        protected IDictionary<string, Action<Message>> _correlationActions { get; set; }

        protected Func<Message, dynamic> _onMessageAction { get; set; }

        protected IList<RPC> _rpcs { get; set; }

        protected Socket _socket { get; set; }

        protected Thread _thread { get; set; }

        public RPC(IList<RPC> rpcs, Socket socket)
        {
            _correlationActions = new Dictionary<string, Action<Message>>();

            _rpcs = rpcs;

            _socket = socket;

            _thread = new Thread(new ThreadStart(() =>
            {
                HandleSocket(_socket);
            }));

            _thread.Start();
        }

        public void Dispose()
        {
            _socket.Close();
            _socket.Dispose();

            if (_rpcs != null)
            {
                _rpcs.Remove(this);
            }
        }

        public void Send(Action<Message> action, Message message)
        {
            if (string.IsNullOrWhiteSpace(message.CorrelationId))
            {
                message.CorrelationId = Guid.NewGuid().ToString();
            }

            if (action != null)
            {
                _correlationActions.Add(message.CorrelationId, action);
            }

            SendMessage(message, _socket);
        }

        public void SetOnMessageAction(Func<Message, dynamic> action)
        {
            _onMessageAction = action;
        }

        protected void HandleBytes(byte[] bytes, Socket socket)
        {
            var content = Encoding.ASCII.GetString(bytes);

            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(content);

            Console.WriteLine($"Received message from {socket.RemoteEndPoint}");

            if (_correlationActions.ContainsKey(message.CorrelationId))
            {
                var correlationAction = _correlationActions[message.CorrelationId];

                correlationAction(message);

            } else
            {
                var response =_onMessageAction(message);

                SendMessage(new Message(message.Command, message.CorrelationId, response), socket);
            }
        }

        protected void HandleSocket(Socket socket)
        {
            var bytes = new byte[256];

            var buffer = new byte[1024];

            var totalNumberOfBytesReceived = 0;

            while (socket.Connected)
            {
                int numberOfBytesReceived = socket.Receive(bytes);

                totalNumberOfBytesReceived += numberOfBytesReceived;

                for (var index = 0; index < numberOfBytesReceived; index++)
                {
                    var bytesIndex = totalNumberOfBytesReceived - numberOfBytesReceived + index;

                    buffer[bytesIndex] = bytes[index];

                    if (bytesIndex == buffer.Length - 1)
                    {
                        HandleBytes(buffer, socket);

                        // TODO: Clear buffer instead of new
                        buffer = new byte[buffer.Length];

                        totalNumberOfBytesReceived -= buffer.Length;
                    }
                }
            }

            this.Dispose();
        }

        protected void SendMessage(Message message, Socket socket)
        {
            var buffer = Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));

            var bytes = new byte[256];

            for (var index = 0; index < buffer.Length; index = index + bytes.Length)
            {
                bytes = buffer.Skip(index).Take(bytes.Length).ToArray();

                socket.Send(bytes);
            }

            for (var index = 0; index < 1024 - buffer.Length; index = index + bytes.Length)
            {
                bytes = new byte[Math.Min(bytes.Length, 1024 - index)];

                socket.Send(bytes);
            }

            Console.WriteLine($"Sent message to {socket.RemoteEndPoint}");
        }
    }
}
