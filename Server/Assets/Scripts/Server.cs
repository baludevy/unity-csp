using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server {
    public delegate void PacketHandler(byte _fromClient, Packet _packet);

    public static Dictionary<byte, Client> clients = new();

    public static Dictionary<byte, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;
    private static readonly object udpLock = new();
    private static bool isStopping;
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }

    public static void Start(int _maxPlayers, int _port) {
        MaxPlayers = _maxPlayers;
        Port = _port;
        isStopping = false;

        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        lock (udpLock) {
            udpListener = new UdpClient(Port);
        }

        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"server now running on port {Port}");
    }

    private static void TCPConnectCallback(IAsyncResult _result) {
        if (isStopping || tcpListener == null) return;

        try {
            var _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            for (byte i = 1; i <= MaxPlayers; i++)
                if (clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(_client);
                    return;
                }

            Debug.Log($"{_client.Client.RemoteEndPoint} cant fit in");
            _client.Close();
        }
        catch (Exception ex) {
            Debug.LogError($"tcp accept error: {ex.Message}");
        }
    }

    private static void UDPReceiveCallback(IAsyncResult _result) {
        if (isStopping || udpListener == null) return;

        try {
            var _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4) return;

            using (var _packet = new Packet(_data)) {
                var _clientId = _packet.ReadByte();

                if (_clientId == 0 || !clients.ContainsKey(_clientId)) return;

                if (clients[_clientId].tcp.socket == null) return;

                if (clients[_clientId].udp.endPoint == null) {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    clients[_clientId].udp.HandleData(_packet);
            }
        }
        catch (Exception ex) {
            Debug.LogError($"udp receive Error: {ex.Message}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
        if (isStopping || udpListener == null) return;

        try {
            if (_clientEndPoint != null) {
                var data = _packet.ToArray();

                if (data.Length > 1400) return;

                lock (udpLock) {
                    udpListener?.BeginSend(data, data.Length, _clientEndPoint, SendCallback, null);
                }
            }
        }
        catch (Exception ex) {
            Debug.LogError($"udp send error: {ex.Message}");
        }
    }

    private static void SendCallback(IAsyncResult _result) {
        try {
            udpListener?.EndSend(_result);
        }
        catch (Exception ex) {
            Debug.LogError($"udp SendCallback error: {ex.Message}");
        }
    }

    private static void InitializeServerData() {
        clients.Clear();
        for (byte i = 1; i <= MaxPlayers; i++) clients.Add(i, new Client(i));

        packetHandlers = new Dictionary<byte, PacketHandler> {
            { (byte)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived }
        };
    }

    public static void Stop() {
        isStopping = true;

        foreach (var client in clients.Values) client.Disconnect();

        tcpListener?.Stop();
        tcpListener = null;

        lock (udpLock) {
            udpListener?.Close();
            udpListener = null;
        }

        Debug.Log("server stopped owo");
    }
}