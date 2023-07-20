using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

namespace Tobo.Net
{
    public class ByteBuffer
    {
        private static readonly ByteBuffer buf1 = new ByteBuffer(BufferSize);
        private static readonly ByteBuffer buf2 = new ByteBuffer(BufferSize);

        const ushort BufferSize = 4096;

        public readonly byte[] Data;

        public int WritePosition = 0;
        public int ReadPosition = 0;
        public int Readable { get; private set; }
        public int Unread => Readable - ReadPosition;
        public int Unwritten => Data.Length - WritePosition;
        public int Written => WritePosition;

        static bool flag;

        private ByteBuffer() { }

        private ByteBuffer(ushort maxSize)
        {
            Data = new byte[maxSize];
        }

        public static ByteBuffer Get()
        {
            flag = !flag;
            // Motivation: If you are reading a buffer and choose to send a packet halfway
            //   through, and then keep reading buf

            if (flag)
            {
                buf1.Reset();
                return buf1;
            }
            else
            {
                buf2.Reset();
                return buf2;
            }
        }

        public void Reset()
        {
            WritePosition = 0;
            ReadPosition = 0;
        }

        internal void SetReadable(int readable)
        {
            Readable = readable;
        }

        #region Byte
        public ByteBuffer AddByte(byte value)
        {
            if (Unwritten < 1)
                throw new Exception($"Failed to write byte ({Unwritten} bytes unwritten)");

            Data[WritePosition++] = value;
            Readable++;
            return this;
        }

        public byte GetByte()
        {
            if (Unread < 1)
            {
                Debug.LogError($"Failed to read byte ({Unread} bytes unread)");
                return 0;
            }

            return Data[ReadPosition++];
        }

        public byte PeekByte()
        {
            if (Unread < 1)
            {
                Debug.LogError($"Failed to peek byte ({Unread} bytes unread)");
                return 0;
            }

            return Data[ReadPosition];
        }

        public ByteBuffer AddByteArray(byte[] value)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentNullException("value");

            if (Unwritten < value.Length)
                throw new Exception($"Failed to write byte[{value.Length}] ({Unwritten} bytes unwritten)");

            Array.Copy(value, 0, Data, WritePosition, value.Length);
            WritePosition += (ushort)value.Length;
            Readable += value.Length;
            return this;
        }

        public byte[] GetByteArray(int length)
        {
            byte[] value = new byte[length];

            if (Unread < length)
            {
                Debug.LogError($"Failed to read byte[{length}] ({Unread} bytes unread)");
                length = Unread;
            }

            Array.Copy(Data, ReadPosition, value, 0, length);
            ReadPosition += (ushort)length;
            return value;
        }
        #endregion

        #region Span

        public ByteBuffer WriteSpan(ReadOnlySpan<byte> span)
        {
            if (Unwritten < span.Length)
                throw new Exception($"Failed to write Span ({Unwritten} bytes unwritten)");

            Add((ushort)span.Length);
            span.CopyTo(new Span<byte>(Data, WritePosition, span.Length));
            WritePosition += (ushort)span.Length;
            Readable += span.Length;
            return this;
        }

        public unsafe ByteBuffer WriteSpan<T>(ReadOnlySpan<T> span) where T : unmanaged
        {
            Add((ushort)span.Length);

            if (Unwritten < span.Length * sizeof(T))
                throw new Exception($"Failed to write Span<{typeof(T)}> ({Unwritten} bytes unwritten)");

            Span<byte> bytes = new Span<byte>(Data, WritePosition, sizeof(T) * span.Length);

            for (int i = 0; i < span.Length; i++)
            {
                T t = span[i];
                MemoryMarshal.Write(bytes, ref t);
            }

            WritePosition += (ushort)(span.Length * sizeof(T));
            Readable += span.Length * sizeof(T);
            return this;
        }

        #endregion

        #region String
        public ByteBuffer AddString(string value)
        {
            // Generates garbage, idc rn its 3:58 am
            if (value == null || value.Length == 0)
            {
                Add((ushort)0);
                return this;
            }

            Add(value.ToCharArray());

            return this;
        }

        public ByteBuffer AddStringArray(string[] values)
        {
            if (values == null || values.Length == 0)
            {
                Add(0);
                return this;
            }

            Add(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                AddString(values[i]);
            }

            return this;
        }

        public string Read()
        {
            if (Peek<ushort>() == 0)
                return "";
            return new string(ReadArray<char>());
        }

        public string ReadString()
        {
            if (Peek<ushort>() == 0)
                return "";
            return new string(ReadArray<char>());
        }

        public string[] ReadStringArray()
        {
            int len = Read<int>();
            string[] array = new string[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = Read();
            }

            return array;
        }
        #endregion

        #region Buffer Struct

        public ByteBuffer WriteStruct<T>(T bufferStruct) where T : IBufferStruct
        {
            bufferStruct.Serialize(this);
            return this;
        }

        public ByteBuffer AddStruct<T>(T bufferStruct) where T : IBufferStruct
        {
            bufferStruct.Serialize(this);
            return this;
        }

        public T GetStruct<T>() where T : IBufferStruct, new()
        {
            //T bufferStruct = default(T);
            T bufferStruct = FastActivator<T>.Create();
            bufferStruct.Deserialize(this);
            return bufferStruct;
        }

        public T ReadStruct<T>() where T : IBufferStruct, new()
        {
            //T bufferStruct = default(T);
            T bufferStruct = FastActivator<T>.Create();
            bufferStruct.Deserialize(this);
            return bufferStruct;
        }

        #endregion

        #region Unmanaged

        public unsafe ByteBuffer Add<T>(T value) where T : unmanaged
        {
            int size = sizeof(T);

            if (Unwritten < size)
                throw new Exception($"Failed to write {typeof(T)} ({Unwritten} bytes unwritten)");

            Span<byte> span = new Span<byte>(Data, WritePosition, size);
            MemoryMarshal.Write(span, ref value);

            WritePosition += (ushort)size;
            Readable += size;
            return this;
        }

        /// <summary>
        /// Writes an array of values to the buffer
        /// </summary>
        /// <typeparam name="T">The unmanaged type to write,</typeparam>
        /// <param name="value">The array of values,</param>
        /// <param name="length">Set to ArrayLength.None if you wish to explicitly specify the length in the call to Read<![CDATA[<]]><typeparamref name="T"/><![CDATA[>]]>.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public unsafe ByteBuffer Add<T>(T[] value, ArrayLength length = ArrayLength.Short) where T : unmanaged
        {
            int size = sizeof(T);
            int lengthLength = length == ArrayLength.None ? 0 : (int)length + 1;

            if (Unwritten < size * value.Length + lengthLength)
                throw new Exception($"Failed to write {typeof(T)}[] ({Unwritten} bytes unwritten)");

            if (value == null)
            {
                Debug.LogWarning($"Tried to write null {typeof(T)}[] to ByteBuffer, writing empty array!");
                WriteArrayLength(length, 0);
                return this;
            }

            /*
            if (length != ArrayLength.None)
            {
                Bytes[writePos++] = (byte)length;
                ReadableLength++;
            }

            switch (length)
            {
                case ArrayLength.Byte:
                    byte lenB = (byte)value.Length;
                    Span<byte> lenSpan = span.Slice(0, (int)length);
                    MemoryMarshal.Write(lenSpan, ref lenB);
                    break;
                case ArrayLength.Short:
                    short lenS = (short)value.Length;
                    lenSpan = span.Slice(0, (int)length);
                    MemoryMarshal.Write(lenSpan, ref lenS);
                    break;
                case ArrayLength.Int:
                    int lenI = value.Length;
                    lenSpan = span.Slice(0, (int)length);
                    MemoryMarshal.Write(lenSpan, ref lenI);
                    break;
            }
            */

            WriteArrayLength(length, value.Length);
            Span<byte> span = new Span<byte>(Data, WritePosition, size * value.Length);

            if (value != null && value.Length > 0)
            {
                Span<byte> arrayBytes = MemoryMarshal.AsBytes(value.AsSpan());

                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = arrayBytes[i];
                }

                WritePosition += (ushort)(size * value.Length);
                Readable += size * value.Length;
            }
            return this;
        }

        private unsafe void WriteArrayLength(ArrayLength length, int count)
        {
            if (length != ArrayLength.None)
            {
                Data[WritePosition++] = (byte)length;
                Readable++;
            }

            Span<byte> lenSpan = Data.AsSpan(WritePosition, (int)length);
            WritePosition += (ushort)length;
            Readable += (int)length;

            switch (length)
            {
                case ArrayLength.Byte:
                    byte lenB = (byte)count;
                    MemoryMarshal.Write(lenSpan, ref lenB);
                    break;
                case ArrayLength.Short:
                    short lenS = (short)count;
                    MemoryMarshal.Write(lenSpan, ref lenS);
                    break;
                case ArrayLength.Int:
                    int lenI = count;
                    MemoryMarshal.Write(lenSpan, ref lenI);
                    break;
            }
        }

        public unsafe T Read<T>() where T : unmanaged
        {
            int size = sizeof(T);

            if (Unread < sizeof(T))
            {
                Debug.LogError($"Failed to read {typeof(T)} ({Unread} bytes unread)");
                return default(T);
            }

            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(Data, ReadPosition, sizeof(T));
            T value = MemoryMarshal.Read<T>(bytes);

            ReadPosition += (ushort)size;
            return value;
        }

        public unsafe ByteBuffer Read<T>(out T val) where T : unmanaged
        {
            val = Read<T>();
            return this;
        }

        public unsafe T Peek<T>() where T : unmanaged
        {
            T value = Read<T>();
            int size = sizeof(T);
            ReadPosition -= (ushort)size;
            return value;
        }

        bool logEmptyArrays = true;

        /// <summary>
        /// Reads an array of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to read.</typeparam>
        /// <param name="len">Set as negative if the length was included in the Write<![CDATA[<]]><typeparamref name="T"/><![CDATA[>]]> call.</param>
        /// <returns></returns>
        public unsafe T[] Read<T>(int len = -1) where T : unmanaged
        {
            int size = sizeof(T);
            if (len < 0)
            {
                ArrayLength length = (ArrayLength)Data[ReadPosition++];
                if (Unread < (int)length)
                {
                    Debug.LogError($"Failed to read {typeof(T)}[] ({Unread} bytes unread)");
                    return null;
                }
                switch (length)
                {
                    case ArrayLength.None:
                        Debug.LogError($"Failed to read {typeof(T)}[] (No Length)");
                        return null;
                    case ArrayLength.Byte:
                        len = Data[ReadPosition++];
                        break;
                    case ArrayLength.Short:
                        len = MemoryMarshal.Read<short>(new ReadOnlySpan<byte>(Data, ReadPosition, sizeof(short)));
                        ReadPosition += 2;
                        break;
                    case ArrayLength.Int:
                        len = MemoryMarshal.Read<int>(new ReadOnlySpan<byte>(Data, ReadPosition, sizeof(int)));
                        ReadPosition += 4;
                        break;
                }
            }

            if (len == 0)
            {
                //Debug.LogError($"Failed to read {typeof(T)}[] (Negative or Zero Length [{len}])");
                if (logEmptyArrays)
                {
                    Debug.LogWarning($"Failed to read {typeof(T)}[] (Zero Length), suppressing future warnings");
                    //Debug.LogWarning(Dump());
                    logEmptyArrays = false;
                }
                return null;
            }
            else if (len < 0)
            {
                Debug.LogError($"Failed to read {typeof(T)}[] (Negative Length [{len}])");
                return null;
            }

            if (Unread < sizeof(T) * len)
            {
                Debug.LogError($"Failed to read {typeof(T)}[] ({Unread} bytes unread)");
                return null;
            }

            ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(Data, ReadPosition, sizeof(T) * len);
            T[] values = new T[len];

            for (int i = 0; i < len; i++)
            {
                ReadOnlySpan<byte> valBytes = bytes.Slice(i * size, size);
                values[i] = MemoryMarshal.Read<T>(valBytes);
            }

            ReadPosition += (ushort)(size * len);
            return values;
        }

        public unsafe T[] ReadArray<T>(int len = -1) where T : unmanaged => Read<T>(len);

        #endregion

        public string Dump()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Message: Readable({Readable}), Unread({Unread})," +
                $"Written/WritePos({WritePosition}), Unwritten({Unwritten}), ReadPos({ReadPosition})");
            sb.Append("Bytes: ");
            for (int i = 0; i < WritePosition; i++)
            {
                sb.Append(Data[i] + " ");
            }

            sb.Append("(Written Data ends...)");

            return sb.ToString();
        }
    }

    public enum ArrayLength : byte
    {
        None = 0,
        Byte = 1,
        Short = 2,
        Int = 4
    }

    /// <summary>
    /// Used for customizing the behaviour of content added to buffers. Use WriteStruct<![CDATA[<]]>T<![CDATA[>]]>
    /// </summary>
    public interface IBufferStruct
    {
        void Serialize(ByteBuffer buf);

        void Deserialize(ByteBuffer buf);
    }
}