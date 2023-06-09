﻿using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace Math0424.Networking
{
    public class MyEasyNetworkManager
    {

        private readonly ushort CommsId;
        public List<IMyPlayer> TempPlayers { get; private set; }
        
        //got a packet that made it through inspection
        public Action<PacketIn> OnRecievedPacket;
        //check for sus packets
        public Action<PacketIn> ProcessPacket;

        public MyEasyNetworkManager(ushort CommsId)
        {
            this.CommsId = CommsId;
            TempPlayers = null;
        }

        public void Register()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(CommsId, RecivedPacket);
        }

        public void UnRegister()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(CommsId, RecivedPacket);
        }

        public void TransmitToServer(IPacket data, bool SendToAllPlayers = true, bool SendToSender = false)
        {
            PacketBase packet = new PacketBase(data.GetId(), SendToAllPlayers, SendToSender);
            packet.Wrap(data);
            MyAPIGateway.Multiplayer.SendMessageToServer(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet));
        }
        //
        //public void ServerTimerSync(ITPacket data, ulong playerId)
        public void ServerTimerSync(ITPacket data, bool SendToAllPlayers = true, bool SendToSender = false)
        {
            PacketBase packet = new PacketBase(data.GetTime(), SendToAllPlayers, SendToSender);
            packet.Wrap(data);
            MyAPIGateway.Multiplayer.SendMessageToServer(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet));
            //MyAPIGateway.Multiplayer.SendMessageTo(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet), playerId);
        }

        public void TransmitToPlayer(IPacket data, ulong playerId)
        {
            PacketBase packet = new PacketBase(data.GetId(), false, false);
            packet.Wrap(data);
            MyAPIGateway.Multiplayer.SendMessageTo(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet), playerId);
        }

        private void RecivedPacket(ushort handler, byte[] raw, ulong id, bool isFromServer)
        {
            try
            {
                PacketBase packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase>(raw);
                PacketIn packetIn = new PacketIn(packet.Id, packet.Data, id, isFromServer);

                ProcessPacket?.Invoke(packetIn);
                if (packetIn.IsCancelled)
                {
                    return;
                }

                if (packet.SendToAllPlayers && MyAPIGateway.Session.IsServer)
                {
                    TransmitPacketToAllPlayers(id, packet);
                }

                if ((!isFromServer && MyAPIGateway.Session.IsServer) ||
                    (isFromServer && (!MyAPIGateway.Session.IsServer || packet.SendToSender)) ||
                    (isFromServer && MyAPIGateway.Session.IsServer))
                {
                    OnRecievedPacket?.Invoke(packetIn);
                }

            }
            catch (Exception e)
            {
			}
        }

        private void TransmitPacketToAllPlayers(ulong sender, PacketBase packet)
        {
            if (TempPlayers == null)
                TempPlayers = new List<IMyPlayer>(MyAPIGateway.Session.SessionSettings.MaxPlayers);
            else
                TempPlayers.Clear();

            MyAPIGateway.Players.GetPlayers(TempPlayers);

            foreach (var p in TempPlayers)
            {
                if (p.IsBot || p.SteamUserId == MyAPIGateway.Multiplayer.ServerId || (!packet.SendToSender && p.SteamUserId == sender))
                    continue;

                MyAPIGateway.Multiplayer.SendMessageTo(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet), p.SteamUserId);
            }
        }

        [ProtoContract]
        private class PacketBase
        {
            [ProtoMember(1)]
            public readonly int Id;
            [ProtoMember(2)]
            public readonly bool SendToAllPlayers;
            [ProtoMember(3)]
            public readonly bool SendToSender;

            [ProtoMember(4)]
            public byte[] Data;



            public PacketBase() { }

            public PacketBase(int Id, bool SendToAllPlayers, bool SendToSender)
            {
                this.Id = Id;
                this.SendToAllPlayers = SendToAllPlayers;
                this.SendToSender = SendToSender;
            }

            public void Wrap(object data)
            {
                Data = MyAPIGateway.Utilities.SerializeToBinary(data);
            }
        }

        public interface IPacket
        {
            int GetId();
        }

        public interface ITPacket
        {
            int GetTime();
        }

        public class PacketIn {
            public bool IsCancelled { protected set; get; }
            public int PacketId { protected set; get; }
            public ulong SenderId { protected set; get; }
            public bool IsFromServer { protected set; get; }
            
            private readonly byte[] Data;

            public PacketIn(int packetId, byte[] data, ulong senderId, bool isFromServer)
            {
                this.PacketId = packetId;
                this.SenderId = senderId;
                this.IsFromServer = isFromServer;
                this.Data = data;
            }

            public T UnWrap<T>()
            {
                return MyAPIGateway.Utilities.SerializeFromBinary<T>(Data);
            }

            public void SetCancelled(bool value)
            {
                this.IsCancelled = value;
            }
        }

    }
}
