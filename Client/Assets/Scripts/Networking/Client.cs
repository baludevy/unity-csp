using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour {
    public static Client Instance;
    private static readonly int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 42069;
    public byte myId;
    public TCP tcp;
    public UDP udp;

    public static bool isConnected;

    private delegate void PacketHandler(Packet packet);

    private static Dictionary<byte, PacketHandler> packetHandlers;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else if (Instance != this) {
            Destroy(this);
        }
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    public void ConnectToServer(string _ipAddress) {
        ip = _ipAddress;

        // Reset state before connecting
        tcp = new TCP();
        udp = new UDP();

        InitializeClientData();
        isConnected = true;
        tcp.Connect();
    }

    public class TCP {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect() {
            socket = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(Instance.ip, Instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result) {
            try {
                socket.EndConnect(_result);

                if (!socket.Connected) return;

                stream = socket.GetStream();
                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex) {
                Debug.LogError($"connection failed: {ex.Message}");
                Instance.Disconnect();
            }
        }

        public void SendData(Packet _packet) {
            try {
                if (socket != null && stream != null) {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception ex) {
                Debug.LogError($"tcp send error: {ex.Message}");
                Instance.Disconnect();
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                if (stream == null) return;

                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0) {
                    Instance.Disconnect();
                    return;
                }

                ClientHandle.bytesReceived += _byteLength;
                ClientHandle.packetsReceived++;

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch {
                Instance.Disconnect();
            }
        }

        private bool HandleData(byte[] _data) {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4) {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0) return true;
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using Packet _packet = new Packet(_packetBytes);

                    int _packetId = _packet.ReadByte();
                    if (packetHandlers.ContainsKey((byte)_packetId)) {
                        packetHandlers[(byte)_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4) {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) return true;
                }
            }

            return _packetLength <= 1;
        }

        public void Disconnect() {
            try {
                socket?.Close();
            }
            catch {
                // ignored
            }

            socket = null;
            stream = null;
            receivedData = null;
            receiveBuffer = null;
        }
    }

    public class UDP {
        private UdpClient socket;
        private IPEndPoint endPoint = new(IPAddress.Parse(Instance.ip), Instance.port);

        public void Connect() {
            try {
                socket = new UdpClient(0);
                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using Packet _packet = new Packet();
                SendData(_packet);
            }
            catch (Exception ex) {
                Debug.LogError($"udp connect error: {ex.Message}");
            }
        }

        public void SendData(Packet _packet) {
            try {
                _packet.InsertByte(Instance.myId);
                socket?.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
            }
            catch (Exception ex) {
                Debug.LogError($"udp send error: {ex.Message}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4) return;
                HandleData(_data);
            }
            catch {
                Instance.Disconnect();
            }
        }

        private void HandleData(byte[] _data) {
            using Packet _packet = new Packet(_data);
            int _packetLength = _packet.ReadInt();
            byte[] _packetBytes = _packet.ReadBytes(_packetLength);

            ClientHandle.bytesReceived += _packetLength;
            ClientHandle.packetsReceived++;

            ThreadManager.ExecuteOnMainThread(() => {
                using Packet _innerPacket = new Packet(_packetBytes);
                int _packetId = _innerPacket.ReadByte();
                if (packetHandlers.ContainsKey((byte)_packetId)) {
                    packetHandlers[(byte)_packetId](_innerPacket);
                }
            });
        }

        public void Disconnect() {
            try {
                socket?.Close();
            }
            catch {
                // ignored
            }

            socket = null;
            endPoint = null;
        }
    }

    private void InitializeClientData() {
        packetHandlers = new Dictionary<byte, PacketHandler> {
            { (byte)ServerPackets.welcome, ClientHandle.Welcome },
            { (byte)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (byte)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            { (byte)ServerPackets.worldState, ClientHandle.WorldSnapshot },
        };
    }

    public void Disconnect() {
        if (!isConnected) return;

        isConnected = false;
        tcp?.Disconnect();
        udp?.Disconnect();

        Debug.Log("disconnected owo");

        ThreadManager.ExecuteOnMainThread(() => { ClientManager.Instance?.ClientDisconnected(); });
    }
}