using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server {
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }

    // Using a Dictionary is fine if pre-filled, but we must be careful about thread access.
    public static Dictionary<byte, Client> clients = new Dictionary<byte, Client>();

    public delegate void PacketHandler(byte _fromClient, Packet _packet);

    public static Dictionary<byte, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;
    private static readonly object udpLock = new object();
    private static bool isStopping = false;

    public static void Start(int _maxPlayers, int _port) {
        MaxPlayers = _maxPlayers;
        Port = _port;
        isStopping = false;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on {Port}.");
    }

    private static void TCPConnectCallback(IAsyncResult _result) {
        if (isStopping || tcpListener == null) return;

        try {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (byte i = 1; i <= MaxPlayers; i++) {
                if (clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full.");
            _client.Close(); // Server full
        }
        catch (Exception ex) {
            Debug.LogError($"TCP Accept Error: {ex.Message}");
        }
    }

    private static void UDPReceiveCallback(IAsyncResult _result) {
        if (isStopping || udpListener == null) return;

        try {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4) return;

            using (Packet _packet = new Packet(_data)) {
                byte _clientId = _packet.ReadByte();

                if (_clientId == 0 || !clients.ContainsKey(_clientId)) return;

                // SECURITY: Only allow UDP if TCP is already connected. 
                // Prevents session hijacking by random UDP packets.
                if (clients[_clientId].tcp.socket == null) return;

                if (clients[_clientId].udp.endPoint == null) {
                    // If this is a new connection, authorize it
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                // SECURITY: Ensure the UDP packet comes from the same source as the initial connection
                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString()) {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception ex) {
            Debug.LogError($"UDP Receive Error: {ex.Message}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
        if (isStopping || udpListener == null) return;

        try {
            if (_clientEndPoint != null) {
                byte[] data = _packet.ToArray();

                // Prevent fragmentation issues by keeping packets under MTU (safe approx 1200-1400)
                if (data.Length > 1400) {
                    Debug.LogWarning($"UDP Packet too large ({data.Length} bytes). Discarding.");
                    return;
                }

                lock (udpLock) {
                    udpListener?.BeginSend(data, data.Length, _clientEndPoint, SendCallback, null);
                }
            }
        }
        catch (Exception ex) {
            Debug.LogError($"UDP Send Error: {ex.Message}");
        }
    }

    private static void SendCallback(IAsyncResult _result) {
        try {
            udpListener?.EndSend(_result);
        }
        catch (Exception ex) {
            Debug.LogError($"UDP SendCallback Error: {ex.Message}");
        }
    }

    private static void InitializeServerData() {
        clients.Clear();
        for (byte i = 1; i <= MaxPlayers; i++) {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<byte, PacketHandler>() {
            { (byte)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
        };
    }

    public static void Stop() {
        isStopping = true;

        foreach (Client client in clients.Values) {
            client.Disconnect();
        }

        tcpListener?.Stop();
        tcpListener = null;

        lock (udpLock) {
            udpListener?.Close();
            udpListener = null;
        }

        Debug.Log("Server stopped.");
    }
}