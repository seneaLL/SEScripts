
using System.Collections.Generic;
using SpaceEngineers.Helpers.Serialization;
using VRageMath;

namespace GravityZond.Measurements
{
    public class GravityPointData
    {
        public Vector3D Position { get; set; }
        public Vector3D Gravity { get; set; }
        public double GravityModule { get { return Gravity.Length(); } }

        public void FillFromRaw(string rawMessage)
        {
            var messageData = ListSerialiser.Deserialize(rawMessage);
            
            Vector3D position, gravity;

            Vector3D.TryParse(ListSerialiser.GetValue(messageData, nameof(Position)), out position);
            Position = position;
            Vector3D.TryParse(ListSerialiser.GetValue(messageData, nameof(Position)), out gravity);
            Gravity = gravity;
        }

        public string GetRaw()
        {
            var values = new List<KeyValuePair<string, string>>(){
                    new KeyValuePair<string, string>(nameof(Position), Position.ToString()),
                    new KeyValuePair<string, string>(nameof(Gravity), Gravity.ToString())
                };

            return ListSerialiser.Serialize(values);
        }
    }
}