namespace SpaceEngineers.UWBlockPrograms.Models
{
    interface IMessage
    {
        void FillFromRaw(string rawMessage);
        string GetRaw();
    }
}