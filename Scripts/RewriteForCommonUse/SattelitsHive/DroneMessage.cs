using System;
using System.Collections.Generic;
using SpaceEngineers.UWBlockPrograms.Helpers;
using SpaceEngineers.UWBlockPrograms.Helpers.Serialization;

namespace SpaceEngineers.UWBlockPrograms.Models
{
    enum DroneMessageType {
            Acc,
            ConfigureNet,
            ConfigureNetRespone,
            Promote,
            PromoteForbid,
            IpBind,
            Ping,
            Move
        }
        class DroneMessage : IMessage {
            public long SenderName { get; set; }
            public DroneMessageType Type { get; set; }
            public int MessageId { get; set; }
            public string Payload { get; set; } = "";
            public int TimeStamp { get; set; }

            public DroneMessage()
            {
            }
            public DroneMessage(string rawMessage)
            {
                TimeStamp = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
                FillFromRaw(rawMessage);
            }

            public void FillFromRaw(string rawMessage)
            {
                var messageData = ListSerialiser.Deserialize(rawMessage);

                SenderName = long.Parse(ListSerialiser.GetValue(messageData, nameof(SenderName)));
                Type = (DroneMessageType)Convert.ToInt32(ListSerialiser.GetValue(messageData, nameof(Type)));
                Payload = ListSerialiser.GetValue(messageData, nameof(Payload));
                MessageId = Convert.ToInt32(ListSerialiser.GetValue(messageData, nameof(MessageId)));
            }

            public string GetRaw()
            {
                var values = new List<KeyValuePair<string, string>>(){
                    new KeyValuePair<string, string>(nameof(SenderName), SenderName.ToString()),
                    new KeyValuePair<string, string>(nameof(Type), ((int)Type).ToString()),
                    new KeyValuePair<string, string>(nameof(Payload), Payload),
                    new KeyValuePair<string, string>(nameof(MessageId), MessageId.ToString()),
                };

                return ListSerialiser.Serialize(values);
            }
        }  
}