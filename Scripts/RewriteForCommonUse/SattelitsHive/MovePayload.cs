using System.Collections.Generic;
using SpaceEngineers.Helpers.Serialization;

namespace SpaceEngineers.UWBlockPrograms.Models
{
    class MovePayload : IMessage
    {
        public CoordinatesPayload MasterCoords { get; set; }
        public CoordinatesPayload MoveToCoords { get; set; }

        public MovePayload() { }
        public MovePayload(string rawMessage)
        {
            FillFromRaw(rawMessage);
        }

        public void FillFromRaw(string rawMessage)
        {
            var messageData = ListSerialiser.Deserialize(rawMessage);

            MasterCoords = new CoordinatesPayload(ListSerialiser.GetValue(messageData, nameof(MasterCoords)));
            MoveToCoords = new CoordinatesPayload(ListSerialiser.GetValue(messageData, nameof(MoveToCoords)));
        }

        public string GetRaw()
        {
            var values = new List<KeyValuePair<string, string>>(){
                    new KeyValuePair<string, string>(nameof(MasterCoords), MasterCoords.GetRaw()),
                    new KeyValuePair<string, string>(nameof(MoveToCoords), MoveToCoords.GetRaw()),
                };

            return ListSerialiser.Serialize(values);
        }
    }
}