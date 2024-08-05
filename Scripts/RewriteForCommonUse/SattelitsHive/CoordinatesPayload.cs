using System.Collections.Generic;
using SpaceEngineers.Helpers.Serialization;
using VRageMath;

namespace SpaceEngineers.UWBlockPrograms.Models
{
    class CoordinatesPayload : IMessage {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
            public string PointName { get; set; } = "";

            public CoordinatesPayload() { }
            public CoordinatesPayload(Vector3 vector, string name = null)
            {
                if (!string.IsNullOrEmpty(name))
                    PointName = name;

                X = vector.X;
                Y = vector.Y;
                Z = vector.Z;
            }
            public CoordinatesPayload(string rawMessage)
            {
                FillFromRaw(rawMessage);
            }

            public void FillFromRaw(string rawMessage)
            {
                var messageData = ListSerialiser.Deserialize(rawMessage);

                X = double.Parse(ListSerialiser.GetValue(messageData, nameof(X)));
                Y = double.Parse(ListSerialiser.GetValue(messageData, nameof(Y)));
                Z = double.Parse(ListSerialiser.GetValue(messageData, nameof(Z)));
                PointName = ListSerialiser.GetValue(messageData, nameof(PointName));
            }

            public string GetRaw()
            {
                var values = new List<KeyValuePair<string, string>>(){
                    new KeyValuePair<string, string>(nameof(X), X.ToString()),
                    new KeyValuePair<string, string>(nameof(Y), Y.ToString()),
                    new KeyValuePair<string, string>(nameof(Z), Z.ToString()),
                    new KeyValuePair<string, string>(nameof(PointName), PointName),
                };

                return ListSerialiser.Serialize(values);
            }
        }
}