namespace PCG
{
    using System.Collections.Generic;

    public class PCGNodeOutput
    {
        private readonly Dictionary<string, List<PCGPoint>> streams = new Dictionary<string, List<PCGPoint>>();

        public IEnumerable<KeyValuePair<string, List<PCGPoint>>> Streams => streams;

        public void SetStream(string portName, List<PCGPoint> points)
        {
            streams[portName] = points ?? new List<PCGPoint>();
        }

        public List<PCGPoint> GetStream(string portName)
        {
            if (string.IsNullOrEmpty(portName))
            {
                portName = PCGNodeData.DefaultOutputPortName;
            }

            return streams.TryGetValue(portName, out var points) ? points : new List<PCGPoint>();
        }

        public bool HasStream(string portName)
        {
            return streams.ContainsKey(portName);
        }
    }
}
