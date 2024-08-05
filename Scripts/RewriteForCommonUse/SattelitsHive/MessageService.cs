using System;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.UWBlockPrograms.Models;
using SpaceEngineers.UWBlockPrograms.Transiever.Common;
using SpaceEngineers.Wrappers;

namespace SpaceEngineers.UWBlockPrograms.Transiever.Wrappers
{
    class MessageService {
            string _broadCastTag = "BroadCast";
            long _myName;
            Random _random;
            IMyBroadcastListener _broadcastListener;
            IMyUnicastListener _unicastListener;
            IMyIntergridCommunicationSystem _igc;
            DateTime _zeroDateTime = new DateTime(1970, 1, 1);
            Logger _logger;

            public MessageService(IMyIntergridCommunicationSystem IGC, Logger logger)
            {
                _igc = IGC;
                _logger = logger;
                _random = new Random();

                _broadcastListener = IGC.RegisterBroadcastListener(_broadCastTag);
                _unicastListener = IGC.UnicastListener;

                _myName = _igc.Me;
            }

            public DroneMessage SendPingMessage(long name, bool isBroadcast)
            {
                var message = ComposeBaseMessage(DroneMessageType.Ping, "");

                if (isBroadcast) {
                    SendBroadcastMessage(message);
                    return message;
                }

                SendUnicastMessage(name, message);
                return message;
            }

            public DroneMessage SendMoveMessage(long name, MovePayload payload)
            {
                var message = ComposeBaseMessage(DroneMessageType.Move, payload.GetRaw());
                SendUnicastMessage(name, message);
                return message;
            }
            public DroneMessage SendPromoteMessage(string myIp)
            {
                var message = ComposeBaseMessage(DroneMessageType.Promote, myIp);
                SendBroadcastMessage(message);
                return message;
            }
            public DroneMessage SendPromoteForbidMessage(string newIp, string masterIp)
            {
                var payload = new IpBindPayload() { MasterIp = masterIp, NewIp = newIp };
                var message = ComposeBaseMessage(DroneMessageType.PromoteForbid, payload.GetRaw());
                SendBroadcastMessage(message);
                return message;
            }

            public DroneMessage SendConfugureNetMessage()
            {
                var message = ComposeBaseMessage(DroneMessageType.ConfigureNet, _myName.ToString());

                SendBroadcastMessage(message);
                return message;
            }

            public DroneMessage SendConfigureNetResponseMessage(long name, TranscieverType myType)
            {
                var message = ComposeBaseMessage(DroneMessageType.ConfigureNetRespone, myType.ToString());

                SendUnicastMessage(name, message);
                return message;
            }

            public DroneMessage SendIpBindMessage(long name, string payload)
            {
                var message = ComposeBaseMessage(DroneMessageType.IpBind, payload);
                message.Type = DroneMessageType.IpBind;

                SendUnicastMessage(name, message);
                return message;
            }

            private void SendUnicastMessage(long address, DroneMessage message)
            {
                _logger.LogTrace($"Send Unicast: {message.MessageId}/ {message.Type} to = {address}\n{message.Payload}");
                _igc.SendUnicastMessage<string>(address, message.Type.ToString(), message.GetRaw());
            }

            private void SendBroadcastMessage(DroneMessage message)
            {
                _logger.LogTrace($"Send Broadcast: {message.MessageId}/ {message.Type}\n{message.Payload}");
                _igc.SendBroadcastMessage<string>(_broadCastTag, message.GetRaw());

            }
            public bool TryRecieve(out DroneMessage recievedMessage)
            {
                MyIGCMessage message;

                if (_broadcastListener.HasPendingMessage) {
                    message = _broadcastListener.AcceptMessage();

                    var droneMessage = new DroneMessage((string)message.Data);
                    SendUnicastMessage(message.Source, ComposeAccMessage(droneMessage.MessageId));
                    _logger.LogTrace($"Recieved Broadcast: from = {droneMessage.SenderName}/ type = {droneMessage.Type}\n<{droneMessage.Payload}>");

                    recievedMessage = droneMessage;
                    return true;
                }

                if (_unicastListener.HasPendingMessage) {
                    message = _unicastListener.AcceptMessage();
                    var droneMessage = new DroneMessage((string)message.Data);
                    recievedMessage = droneMessage;

                    if (message.Tag == DroneMessageType.Acc.ToString()) {
                        _logger.LogTrace($"Acc on {droneMessage.MessageId} from {message.Source}");
                        return true;
                    }

                    SendUnicastMessage(message.Source, ComposeAccMessage(droneMessage.MessageId));
                    _logger.LogTrace($"Recived Unicast: from = {droneMessage.SenderName}/ type = {droneMessage.Type}\n{droneMessage.Payload}");
                    return true;
                }

                recievedMessage = null;
                return false;
            }
            private DroneMessage ComposeAccMessage(int messageId)
            {
                var message = ComposeBaseMessage(DroneMessageType.Acc, "", messageId);
                return message;
            }

            private DroneMessage ComposeBaseMessage(DroneMessageType type, string payload, int messageId)
            {
                var message = ComposeBaseMessage(type, payload);
                message.MessageId = messageId;
                return message;
            }

            private DroneMessage ComposeBaseMessage(DroneMessageType type, string payload)
            {
                return new DroneMessage()
                {
                    SenderName = _myName,
                    MessageId = GenerateMessageId(),
                    TimeStamp = (int)(DateTime.Now - _zeroDateTime).TotalSeconds,
                    Type = type,
                    Payload = payload
                };
            }

            private long GetMyName() => _myName;

            private int GenerateMessageId() => _random.Next(0, 1000000);
        }   
}