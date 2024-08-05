using System;
using System.Collections.Generic;
using System.Linq;
using SpaceEngineers.UWBlockPrograms.Models;
using SpaceEngineers.UWBlockPrograms.Transiever.Wrappers;
using SpaceEngineers.UWBlockPrograms.Transmission.Common;
using SpaceEngineers.Wrappers;
using VRageMath;

namespace SpaceEngineers.UWBlockPrograms.Transiever.Common
{
    enum TranscieverType
    {
        Unknown,
        Master,
        Submaster,
        Soldier
    }
    class TrancieverDrone
    {
        // Contains 3 groups of int(from left): 1 - global net id, 2 - local net id, 3 - personal net id
        public int[] Ip { get; set; }
        public int[] MasterIp { get; set; }
        public int[] SubmasterIp { get; set; }
        public long MyName { get; set; }
        public Dictionary<string, TrancieverDrone> IpTable { get; set; }
        public TranscieverType Type { get; set; }
        private MessageService _messageService;
        private Dictionary<int, DroneMessage> _pendingMessages;
        private Autopilot _autopilot;
        private Logger _logger;
        private Queue<CoordinatesPayload> _gridBuildQueue = new Queue<CoordinatesPayload>();
        private List<CoordinatesPayload> _occupiedGridPlaces = new List<CoordinatesPayload>();

        public TrancieverDrone()
        {
        }
        public TrancieverDrone(MessageService messageService, Autopilot autopilot, Logger logger)
        {
            IpTable = new Dictionary<string, TrancieverDrone>();
            _pendingMessages = new Dictionary<int, DroneMessage>();
            _autopilot = autopilot;
            _logger = logger;

            Type = TranscieverType.Unknown;
            _messageService = messageService;

            foreach (var i in CommonFunctions.GetShiftedGridMatrix(500, _autopilot.PointPhysicalInfo.Position))
                _gridBuildQueue.Enqueue(new CoordinatesPayload(i));

            TryConfigureNet();
        }

        public void OnUpdate(int tick)
        {
            _autopilot.OnUpdate();
            if (tick % 60 == 0 && _pendingMessages.Any(it => CommonFunctions.GetTimeStampNow() - it.Value.TimeStamp > 5))
            {
                OperatePendingMessages();
                if (Type == TranscieverType.Master)
                {

                }
                else
                {
                    var hasMaster = IpTable.Any(it => it.Value.Type == TranscieverType.Master);

                    if (hasMaster)
                    {
                        var masterName = IpTable.First(it => it.Value.Type == TranscieverType.Master);
                        //_messageService.SendPingMessage();
                    }
                    //_messageService.SendPingMessage();
                }
            }
        }

        public void OnMessageRecieved(DroneMessage message)
        {
            switch (message.Type)
            {
                case DroneMessageType.Acc:
                    OperateAccMessage(message);
                    break;
                case DroneMessageType.ConfigureNet:
                    OperateConfigureMessage(message);
                    break;
                case DroneMessageType.IpBind:
                    OperateIpBindMessage(message);
                    break;
                case DroneMessageType.Promote:
                    OperatePromoteMessage(message);
                    break;
                case DroneMessageType.PromoteForbid:
                    OperatePromoteForbidMessage(message);
                    break;
                case DroneMessageType.ConfigureNetRespone:
                    OperateConfigureNetResponeMessage(message);
                    break;
                case DroneMessageType.Move:
                    OperateMoveMessage(message);
                    break;
            }
        }

        private void OperateMoveMessage(DroneMessage message)
        {
            _logger.LogTrace(message.Payload);
            var payload = new MovePayload(message.Payload);
            var moveToPos = payload.MoveToCoords;

            _autopilot.Go(new Vector3D(moveToPos.X, moveToPos.Y, moveToPos.Z));
        }
        private void OperateConfigureNetResponeMessage(DroneMessage message)
        {
            if (!IpTable.ContainsKey(message.Payload))
            {
                IpTable.Add(message.Payload, GetTrancieverModelForTable(MasterIp, message.SenderName, TranscieverType.Soldier));
                return;
            }
        }

        private void OperatePromoteMessage(DroneMessage message)
        {
            if (Type == TranscieverType.Master)
            {
                _messageService.SendPromoteForbidMessage(GetFreeIpString(), CommonFunctions.IpToString(Ip));
                return;
            }

            var newMasterIp = message.Payload;
            MasterIp = CommonFunctions.IpStringToArray(newMasterIp);

            if (IpTable.Any(it => it.Key == newMasterIp))
            {
                IpTable[newMasterIp] = GetTrancieverModelForTable(MasterIp, message.SenderName, TranscieverType.Master);
            }
            else
            {
                IpTable.Add(newMasterIp, GetTrancieverModelForTable(MasterIp, message.SenderName, TranscieverType.Master));
            }
        }

        private void OperatePromoteForbidMessage(DroneMessage message)
        {
            var payload = new IpBindPayload(message.Payload);

            var masterIp = CommonFunctions.IpStringToArray(payload.MasterIp);
            if (Type == TranscieverType.Soldier && MasterIp != masterIp)
            {
                MasterIp = masterIp;
                return;
            }

            Type = TranscieverType.Soldier;
            Ip = CommonFunctions.IpStringToArray(payload.NewIp);
            MasterIp = masterIp;
        }

        private void OperateIpBindMessage(DroneMessage message)
        {
            var payload = new IpBindPayload(message.Payload);
            Ip = CommonFunctions.IpStringToArray(payload.NewIp);
            MasterIp = CommonFunctions.IpStringToArray(payload.MasterIp);
            Type = TranscieverType.Soldier;
        }

        private void OperateConfigureMessage(DroneMessage message)
        {
            if (Type == TranscieverType.Master)
            {
                var newIpArr = GetFreeIp();
                var newIp = CommonFunctions.IpToString(newIpArr);

                var payload = new IpBindPayload()
                {
                    NewIp = newIp,
                    MasterIp = CommonFunctions.IpToString(Ip)
                };
                _messageService.SendIpBindMessage(message.SenderName, payload.GetRaw());

                IpTable.Add(newIp, new TrancieverDrone() { MyName = message.SenderName, Ip = newIpArr, Type = TranscieverType.Soldier });
                var masterPos = _autopilot.PointPhysicalInfo.Position;

                if (_gridBuildQueue.Count > 0)
                {
                    var moveMessage = new MovePayload()
                    {
                        MasterCoords = new CoordinatesPayload(_autopilot.PointPhysicalInfo.Position, "MasterCoords"),
                        MoveToCoords = _gridBuildQueue.Dequeue()
                    };

                    _messageService.SendMoveMessage(message.SenderName, moveMessage);
                }
            }

            if (Type == TranscieverType.Soldier)
            {
                _messageService.SendConfigureNetResponseMessage(message.SenderName, Type);
                return;
            }

            if (MasterIp == null)
            {

            }
        }

        private void OperateAccMessage(DroneMessage message)
        {
            _pendingMessages.Remove(message.MessageId);
        }

        private void TryConfigureNet()
        {
            var message = _messageService.SendConfugureNetMessage();
            _pendingMessages.Add(message.MessageId, message);
        }

        private string GetFreeIpString() => CommonFunctions.IpToString(GetFreeIp());

        private int[] GetFreeIp()
        {
            if (Type == TranscieverType.Master)
            {
                int[] newIp = new int[3] { 1, 1, 1 };
                if (Ip != null)
                    Array.Copy(Ip, newIp, Ip.Length);

                newIp[2] += 1;
                while (IpTable.ContainsKey(CommonFunctions.IpToString(newIp)))
                {
                    newIp[2]++;
                }

                return newIp;
            }

            return null;
        }

        private TrancieverDrone GetTrancieverModelForTable(int[] ip, long name, TranscieverType type)
        {
            return new TrancieverDrone()
            {
                Ip = ip,
                MyName = name,
                Type = type
            };
        }

        public void OperatePendingMessages()
        {
            var staleMessageIds = new List<int>();

            foreach (var i in _pendingMessages)
            {
                if (CommonFunctions.GetTimeStampNow() - i.Value.TimeStamp < 2)
                    continue;

                if (i.Value.Type == DroneMessageType.ConfigureNet)
                {
                    Type = TranscieverType.Master;
                    Ip = GetFreeIp();
                    _messageService.SendPromoteMessage(CommonFunctions.IpToString(Ip));
                }
                if (i.Value.Type == DroneMessageType.IpBind)
                {
                    IpTable.Remove(i.Value.Payload);
                }

                staleMessageIds.Add(i.Key);
            }

            staleMessageIds.ForEach(it => _pendingMessages.Remove(it));
        }
    }
}