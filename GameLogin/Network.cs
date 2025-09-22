﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GamePackets;

namespace GameLogin
{
    public sealed class Network
    {
        public static Network Instance { get; } = new Network();

        private Socket _client;
        public int ConnectAttempt = 0;
        public bool Connected;
        public DateTime TimeOutTime, TimeConnected, RetryTime = DateTime.UtcNow.AddSeconds(5);

        private ConcurrentQueue<GamePacket> ReceivedPackets = new ConcurrentQueue<GamePacket>();
        private ConcurrentQueue<GamePacket> SendPackets = new ConcurrentQueue<GamePacket>();

        private byte[] _rawData = new byte[0];
        private readonly byte[] _rawBytes = new byte[8 * 1024];

        static Network()
        {
            GamePacket.AddProcessMethodTable<Network>(PacketSource.Server);
        }

        public void Connect()
        {
            if (_client != null)
                Disconnect();

            ConnectAttempt++;

            var endPoint = new IPEndPoint(IPAddress.Parse(AppConfig.Current.AccountServerAddressIP), AppConfig.Current.AccountServerAddressPort);
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            _client.BeginConnect(endPoint, Connection, null);
        }

        private void Connection(IAsyncResult result)
        {
            try
            {
                if (_client == null) return;

                _client.EndConnect(result);

                if (!_client.Connected)
                {
                    Connect();
                    return;
                }

                _rawData = new byte[0];

                TimeConnected = DateTime.UtcNow;
                Connected = true;

                BeginReceive();
            }
            catch (SocketException ex)
            {
                //Settings.SaveError(ex.ToString());
            }
            catch (Exception ex)
            {
                //Settings.SaveError(ex.ToString());
                Disconnect();
            }
        }

        private void BeginReceive()
        {
            if (!Connected) return;
            if (_client == null || !_client.Connected) return;

            try
            {
                _client.BeginReceive(_rawBytes, 0, _rawBytes.Length, SocketFlags.None, ReceiveData, _rawBytes);
            }
            catch
            {
                Disconnect();
            }
        }

        private void ReceiveData(IAsyncResult result)
        {
            if (!Connected) return;
            if (_client == null || !_client.Connected) return;

            try
            {
                var dataRead = _client.EndReceive(result);
                if (dataRead == 0)
                {
                    Disconnect();
                    return;
                }

                byte[] src = result.AsyncState as byte[];
                byte[] dst = new byte[_rawData.Length + dataRead];
                Buffer.BlockCopy(_rawData, 0, dst, 0, _rawData.Length);
                Buffer.BlockCopy(src, 0, dst, _rawData.Length, dataRead);
                _rawData = dst;

                while (true)
                {
                    GamePacket packet = GamePacket.DeserializeServerPacket(_rawData, out _rawData);
                    if (packet == null)
                        break;

                    ReceivedPackets.Enqueue(packet);
                }

                BeginReceive();
            }
            catch (Exception ex)
            {
                Disconnect();
            }
        }

        private void BeginSend(List<byte> data)
        {
            if (!Connected) return;
            if (_client == null || !_client.Connected || data.Count == 0) return;

            try
            {
                _client.BeginSend(data.ToArray(), 0, data.Count, SocketFlags.None, SendComplete, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private void SendComplete(IAsyncResult result)
        {
            try
            {
                _client.EndSend(result);
            }
            catch
            { }
        }

        public void Disconnect()
        {
            if (!Connected) return;

            Connected = false;
            TimeConnected = DateTime.MinValue;

            SendPackets.Clear();
            ReceivedPackets.Clear();

            _client?.Close();
            _client = null;
        }

        public void Process()
        {
            if (_client == null || !_client.Connected)
            {
                if (Connected)
                {
                    Disconnect();
                    return;
                }
                else if (DateTime.UtcNow >= RetryTime)
                {
                    RetryTime = DateTime.UtcNow.AddSeconds(5);
                    Connect();
                }
                return;
            }

            ProcessReceivedPackets();
            SendAllPackets();
        }

        public void SendPacket(GamePacket p)
        {
            if (p != null)
                SendPackets.Enqueue(p);
        }

        private void ProcessReceivedPackets()
        {
            while (!ReceivedPackets.IsEmpty)
            {
                if (ReceivedPackets.TryDequeue(out var p))
                {
                    if (!GamePacket.PacketProcessMethodTable.TryGetValue(p.PacketType, out var method))
                    {
                        Disconnect();
                        break;
                    }
                    method.Invoke(this, [p]);
                }
            }
        }

        private void SendAllPackets()
        {
            List<byte> data = new List<byte>();
            while (!SendPackets.IsEmpty)
            {
                if (SendPackets.TryDequeue(out var packet))
                    data.AddRange(packet.SerializeToBytes());
            }
            if (data.Count > 0)
                BeginSend(data);
        }

        public void Process(AccountRegisterSuccessPacket P)
        {
            MainForm.Instance.AccountRegisterSuccessUpdate();
        }

        public void Process(AccountRegisterFailPacket P)
        {
            MainForm.Instance.AccountRegisterFailUpdate(P.ErrorCode);
        }

        public void Process(AccountChangePasswordSuccessPacket P)
        {
            MainForm.Instance.AccountChangePasswordSuccessUpdate();
        }

        public void Process(AccountChangePasswordFailPacket P)
        {
            MainForm.Instance.AccountChangePasswordFailUpdate(P.ErrorCode);
        }

        public void Process(AccountLogInSuccessPacket P)
        {
            MainForm.Instance.AccountLogInSuccessUpdate(P.ServerListInformation);
        }

        public void Process(AccountLogInFailPacket P)
        {
            MainForm.Instance.AccountLogInFailUpdate(P.ErrorCode);
        }

        public void Process(AccountLogOutSuccessPacket P)
        {
            MainForm.Instance.AccountLogOutSuccessUpdate();
        }

        public void Process(AccountStartGameSuccessPacket P)
        {
            MainForm.Instance.AccountStartGameSuccessUpdate(P.Ticket);
        }

        public void Process(AccountStartGameFailPacket P)
        {
            MainForm.Instance.AccountStartGameFailUpdate(P.ErrorCode);
        }
    }
}