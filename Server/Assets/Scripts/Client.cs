using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client {
    private const int dataBufferSize = 4096;

    private byte id;
    public Player player;
    public TCP tcp;
    public UDP udp;

    public Client(byte _clientId) {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP {
        public TcpClient socket;
        private readonly byte id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(byte _id) {
            id = _id;
        }

        public void Connect(TcpClient _socket) {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            ServerSend.Welcome(id);
        }

        public void SendData(Packet _packet) {
            try {
                if (socket != null && stream != null) {
                    byte[] data = _packet.ToArray();
                    stream.BeginWrite(data, 0, data.Length, SendCallback, null);
                }
            }
            catch (Exception ex) {
                Debug.Log($"Error sending data to player {id} via TCP: {ex.Message}");
                Server.clients[id].Disconnect();
            }
        }

        private void SendCallback(IAsyncResult _result) {
            try {
                stream?.EndWrite(_result);
            }
            catch (Exception) {
                // ignored
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                if (stream == null) return;

                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0) {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex) {
                Debug.Log($"Error receiving TCP data for client {id}: {ex.Message}");
                Server.clients[id].Disconnect();
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
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadByte();
                        if (Server.packetHandlers.ContainsKey((byte)_packetId)) {
                            Server.packetHandlers[(byte)_packetId](id, _packet);
                        }
                        else {
                            Debug.LogWarning($"Unknown packet ID: {_packetId}");
                        }
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
            }

            socket = null;
            stream = null;
            receivedData = null;
            receiveBuffer = null;
        }
    }

    public class UDP {
        public IPEndPoint endPoint;
        private readonly byte id;

        public UDP(byte _id) {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint) {
            endPoint = _endPoint;
        }

        public void SendData(Packet _packet) {
            Server.SendUDPData(endPoint, _packet);
        }

        public void HandleData(Packet _packetData) {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_packetBytes)) {
                    int _packetId = _packet.ReadByte();

                    if (_packetId == (byte)ClientPackets.playerInput) {
                        ServerHandle.PlayerInput(id, _packet);
                    }
                    else if (Server.packetHandlers.ContainsKey((byte)_packetId)) {
                        Server.packetHandlers[(byte)_packetId](id, _packet);
                    }
                }
            });
        }

        public void Disconnect() {
            endPoint = null;
        }
    }

    public void Disconnect() {
        Debug.Log($"{player?.username ?? "Unknown Player"} (ID: {id}) has disconnected.");

        ThreadManager.ExecuteOnMainThread(() => {
            if (player != null) {
                ServerSend.PlayerDisconnected(player);
                UnityEngine.Object.Destroy(player.gameObject);
                player = null;
            }
        });

        tcp.Disconnect();
        udp.Disconnect();
    }

    public void SendIntoGame(string _playerName) {
        ThreadManager.ExecuteOnMainThread(() => {
            if (player != null) return;

            player = NetworkManager.Instance.InstantiatePlayer();
            player.Initialize(id, _playerName);

            foreach (Client _client in Server.clients.Values) {
                if (_client.player != null) {
                    ServerSend.SpawnPlayer(id, _client.player);

                    if (_client.id != id) {
                        ServerSend.SpawnPlayer(_client.id, player);
                    }
                }
            }

            Debug.Log($"Client {id} ('{_playerName}') successfully entered the game.");
        });
    }
}