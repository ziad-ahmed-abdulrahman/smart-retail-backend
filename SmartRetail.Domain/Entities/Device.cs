namespace SmartRetail.Domain.Entities
{
    public enum DeviceType
    {
        SmartGate,
        SmartCamera,
        WeightSensor,
        RfidReader,
        PosTerminal
    }

    public class Device
    {
        public int Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public bool IsOnline { get; set; } = false;
        public DateTime? LastPing { get; set; }

        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;
    }
}
