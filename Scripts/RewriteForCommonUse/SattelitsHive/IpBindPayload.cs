using System.Collections.Generic;
using SpaceEngineers.UWBlockPrograms.Helpers;
using SpaceEngineers.UWBlockPrograms.Helpers.Serialization;

namespace SpaceEngineers.UWBlockPrograms.Models
{
    class IpBindPayload : IMessage
    {
        public string NewIp { get; set; }
        public string MasterIp { get; set; }

        public IpBindPayload() { }
        public IpBindPayload(string rawMessage)
        {
            FillFromRaw(rawMessage);
        }

        public void FillFromRaw(string rawMessage)
        {
            var messageData = ListSerialiser.Deserialize(rawMessage);

            NewIp = ListSerialiser.GetValue(messageData, nameof(NewIp));
            MasterIp = ListSerialiser.GetValue(messageData, nameof(MasterIp));
        }

        public string GetRaw()
        {
            var values = new List<KeyValuePair<string, string>>(){
                    new KeyValuePair<string, string>(nameof(NewIp), NewIp),
                    new KeyValuePair<string, string>(nameof(MasterIp), MasterIp)
                };

            return ListSerialiser.Serialize(values);
        }
    }

}