using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Tobo.Net
{
    public static class ByteBufferExtensions
    {
        public static ByteBuffer Add(this ByteBuffer buf, Quaternion value)
        {
            //Add(value.eulerAngles); // Should switch to smallest three in the future, but idc rn
            buf.Add(Compression.Rotation.Compress(value)); // ^^^ Done!

            return buf;
        }

        public static Quaternion GetQuaternion(this ByteBuffer buf)
        {
            if (buf.Unread < sizeof(int))//Util.QUATERNION_LENGTH)
            {
                Debug.LogError($"Failed to read quaternion ({buf.Unread} bytes unread)");
                return Quaternion.identity;
            }

            //return Quaternion.Euler(GetVector3()); // Should switch to smallest three in the future, but idc rn
            return Compression.Rotation.Decompress(buf.Read<uint>()); // ^^^ Done!
        }


        // https://github.com/tom-weiland VVV

        public static ByteBuffer AddBoolArray(this ByteBuffer buf, bool[] array, bool includeLength = true)
        {
            ushort byteLength = (ushort)(array.Length / 8 + (array.Length % 8 == 0 ? 0 : 1));
            if (buf.Unwritten < byteLength)
                throw new Exception($"Failed to write bool[] ({buf.Unwritten} bytes unwritten)");

            if (includeLength)
                buf.Add((ushort)array.Length);

            BitArray bits = new BitArray(array);
            bits.CopyTo(buf.Data, buf.WritePosition);
            buf.WritePosition += byteLength;
            return buf;
        }

        public static bool[] GetBoolArray(this ByteBuffer buf)
        {
            return GetBoolArray(buf, buf.Read<ushort>());
        }

        public static bool[] GetBoolArray(this ByteBuffer buf, ushort length)
        {
            ushort byteLength = (ushort)(length / 8 + (length % 8 == 0 ? 0 : 1));
            if (buf.Unread < byteLength)
            {
                Debug.LogError($"Failed to read bool[] ({buf.Unread} bytes unread)");
                length = (ushort)(buf.Unread / sizeof(ushort));
            }

            BitArray bits = new BitArray(buf.GetByteArray(byteLength));
            bool[] array = new bool[length];
            for (int i = 0; i < array.Length; i++)
                array[i] = bits.Get(i);

            return array;
        }

        public static void ReadData(this ByteBuffer buf, IntPtr ptr, int size)
        {
            buf.WritePosition = size;
            buf.ReadPosition = 0;

            int bufSize = buf.Data.Length;
            if (size > bufSize)
            {
                Debug.LogError($"Can't fully handle {size} bytes because it exceeds the maximum of {bufSize}, message will contain incomplete data!");
                Marshal.Copy(ptr, buf.Data, 0, bufSize);
                buf.SetReadable(bufSize);
            }
            else
            {
                Marshal.Copy(ptr, buf.Data, 0, size);
                buf.SetReadable(size);
            }
        }

        public static void ReadData(this ByteBuffer buf, byte[] bytes)
        {
            int size = bytes.Length;
            buf.WritePosition = size;
            buf.ReadPosition = 0;

            int bufSize = buf.Data.Length;
            if (size > bufSize)
            {
                Debug.LogError($"Can't fully handle {size} bytes because it exceeds the maximum of {bufSize}, message will contain incomplete data!");
                //Marshal.Copy(bytes, buf.Data, 0, bufSize);
                Buffer.BlockCopy(bytes, 0, buf.Data, 0, bufSize);
                buf.SetReadable(bufSize);
            }
            else
            {
                Buffer.BlockCopy(bytes, 0, buf.Data, 0, size);
                buf.SetReadable(size);
            }
        }
    }
}