namespace PangolinRPCNet
{
    public class Message
    {
        public string Command { get; set; }

        public string CorrelationId { get; set; }

        public dynamic Payload { get; set; }

        public Message(string command, string correlationId, dynamic payload)
        {
            Command = command;

            CorrelationId = correlationId;

            Payload = payload;
        }

        public override string ToString()
        {
            return $"{Command} ({CorrelationId})";
        }
    
    }

}
