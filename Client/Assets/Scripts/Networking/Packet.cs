using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public enum ServerPackets : byte {
    welcome = 1,
    spawnPlayer,
    playerDisconnected,
    worldState,
}

public enum ClientPackets : byte {
    welcomeReceived = 1,
    playerInput,
}

public sealed class Packet : IDisposable {
    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos;

    public Packet() {
        buffer = new List<byte>();
        readPos = 0;
    }

    public Packet(byte id) {
        buffer = new List<byte>();
        readPos = 0;
        Write(id);
    }

    public Packet(byte[] data) {
        buffer = new List<byte>();
        readPos = 0;
        SetBytes(data);
    }

    #region Functions

    public void SetBytes(byte[] data) {
        buffer.Clear();
        buffer.AddRange(data);
        readableBuffer = buffer.ToArray();
    }

    public void WriteLength() {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
    }

    public void InsertInt(int value) {
        buffer.InsertRange(0, BitConverter.GetBytes(value));
    }

    public void InsertByte(byte value) // Added missing method
    {
        buffer.InsertRange(0, new[] { value });
    }

    public byte[] ToArray() {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }

    public int Length() {
        return buffer.Count;
    }

    public int UnreadLength() {
        return Length() - readPos;
    }

    public void Reset(bool shouldReset = true) {
        if (shouldReset) {
            buffer.Clear();
            readableBuffer = null;
            readPos = 0;
        }
        else {
            readPos -= 4; // Assuming last read was an int
        }
    }

    #endregion

    #region Write Data

    public void Write(byte value) {
        buffer.Add(value);
    }

    public void Write(byte[] value) {
        buffer.AddRange(value);
    }

    public void Write(short value) {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(int value) {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(long value) {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(float value) {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(bool value) {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void Write(string value) {
        Write(value.Length);
        buffer.AddRange(Encoding.ASCII.GetBytes(value));
    }

    public void Write(Vector3 value) {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    public void Write(Quaternion value) {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    #endregion

    #region Read Data

    public byte ReadByte(bool moveReadPos = true) {
        if (UnreadLength() >= 1) {
            byte value = readableBuffer[readPos];
            if (moveReadPos) {
                readPos += 1;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'byte'!");
    }

    public byte[] ReadBytes(int length, bool moveReadPos = true) {
        if (UnreadLength() >= length) {
            byte[] value = buffer.GetRange(readPos, length).ToArray();
            if (moveReadPos) {
                readPos += length;
            }

            return value;
        }

        throw new Exception($"Could not read {length} bytes!");
    }

    public short ReadShort(bool moveReadPos = true) {
        if (UnreadLength() >= 2) {
            short value = BitConverter.ToInt16(readableBuffer, readPos);
            if (moveReadPos) {
                readPos += 2;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'short'!");
    }

    public int ReadInt(bool moveReadPos = true) {
        if (UnreadLength() >= 4) {
            int value = BitConverter.ToInt32(readableBuffer, readPos);
            if (moveReadPos) {
                readPos += 4;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'int'!");
    }

    public uint ReadUInt(bool moveReadPos = true) {
        if (UnreadLength() >= 4) {
            uint value = BitConverter.ToUInt32(readableBuffer, readPos);
            if (moveReadPos) {
                readPos += 4;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'uint'!");
    }

    public long ReadLong(bool moveReadPos = true) {
        if (UnreadLength() >= 8) {
            long value = BitConverter.ToInt64(readableBuffer, readPos);
            if (moveReadPos) {
                readPos += 8;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'long'!");
    }

    public float ReadFloat(bool moveReadPos = true) {
        if (UnreadLength() >= 4) {
            float value = BitConverter.ToSingle(readableBuffer, readPos);
            if (moveReadPos) {
                readPos += 4;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'float'!");
    }

    public bool ReadBool(bool moveReadPos = true) {
        if (UnreadLength() >= 1) {
            bool value = BitConverter.ToBoolean(readableBuffer, readPos);
            if (moveReadPos) {
                readPos += 1;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'bool'!");
    }

    public string ReadString(bool moveReadPos = true) {
        int length = ReadInt();
        if (UnreadLength() >= length) {
            string value = Encoding.ASCII.GetString(readableBuffer, readPos, length);
            if (moveReadPos && length > 0) {
                readPos += length;
            }

            return value;
        }

        throw new Exception("Could not read value of type 'string'!");
    }

    public Vector3 ReadVector3(bool moveReadPos = true) {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    public Quaternion ReadQuaternion(bool moveReadPos = true) {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos),
            ReadFloat(moveReadPos));
    }

    #endregion

    #region Dispose

    private bool disposed;

    private void Dispose(bool disposing) {
        if (!disposed) {
            if (disposing) {
                buffer.Clear();
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Packet() {
        Dispose(false);
    }

    #endregion
}