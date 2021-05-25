using System;

namespace bm472.Data
{
    public class PacketModel
    {
        public int Length { get; set; } = 0;
        public string Source { get; set; } = "unknown";
        public string Protocol { get; set; } = "unknown";
        public string SrcPort { get; set; } = "unknown";
        public string Destination { get; set; } = "unknown";
        public string DestPort { get; set; } = "unknown";
        public DateTime Timestamp { get; set; } = DateTime.Now;

    }
}
