using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Squadmania.Squad.Rcon
{
    /// <summary>
    /// Represents a single Source Rcon Packet
    /// </summary>
    public readonly struct Packet
    {
        public static readonly Packet Empty = new(0, 0, Array.Empty<byte>());
        
        public const int SizeFieldLength = 4;
        public const int IdFieldLength = 4;
        public const int TypeFieldLength = 4;
        public const int EmptyStringLength = 1;
        public const byte EmptyStringTerminator = 0x00;
        
        public bool IsBroken { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <param name="encoding"></param>
        /// <param name="isBroken">Whether or not this is a broken Rcon Packet</param>
        public Packet(
            int id,
            int type,
            string body,
            bool isBroken = false,
            Encoding? encoding = null
        ) : this(id, type, (encoding ?? Encoding.UTF8).GetBytes(body), isBroken)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <param name="isBroken">Whether or not this is a broken Rcon Packet</param>
        public Packet(
            int id,
            int type,
            byte[] body,
            bool isBroken = false
        )
        {
            Size = IdFieldLength + TypeFieldLength + body.Length + EmptyStringLength + EmptyStringLength;

            Type = type;
            Id = id;
            Body = body;
            
            IsBroken = isBroken;
        }

        public int Size { get; }
        public int Id { get; }
        public int Type { get; }
        public byte[] Body { get; }
        
        public static Packet Read(
            Stream stream
        )
        {
            var buffer = new byte[4];
            var bytesRead = stream.Read(buffer, 0, 4);
            if (bytesRead != 4)
            {
                throw new Exception("invalid amount of bytes for size received");
            }


            var size = BinaryPrimitives.ReadInt32LittleEndian(buffer);

            buffer = new byte[size];
            bytesRead = stream.Read(buffer, 0, size);
            if (bytesRead != size)
            {
                throw new Exception("invalid amount of bytes for rest of packet received");
            }
            
            var id = BinaryPrimitives.ReadInt32LittleEndian(buffer[new Range(0, 4)]);
            var type = BinaryPrimitives.ReadInt32LittleEndian(buffer[new Range(4, 8)]);
            var body = buffer[new Range(8, buffer.Length - 2)];
            
            return new Packet(
                id,
                type,
                body
            );
        }

        public static int ParseSize(byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                throw new Exception("invalid packet size bytes received");
            }

            return BinaryPrimitives.ReadInt32LittleEndian(bytes);
        }

        /// <summary>
        /// Parses a packet from a given byte array.
        /// </summary>
        /// <param name="bytes">Packet bytes - 4 byte size header excluded!</param>
        /// <returns></returns>
        public static Packet Parse(byte[] bytes)
        {
            var id = BinaryPrimitives.ReadInt32LittleEndian(bytes[new Range(0, 4)]);
            var type = BinaryPrimitives.ReadInt32LittleEndian(bytes[new Range(4, 8)]);
            var body = bytes[new Range(8, bytes.Length - 2)];
            
            return new Packet(
                id,
                type,
                body,
                body.SequenceEqual(new byte[]{0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00})
            );
        }

        public byte[] ToArray()
        {
            using var packetMemoryStream = new MemoryStream();
            
            var buffer = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, Size);
            packetMemoryStream.Write(buffer);
            
            BinaryPrimitives.WriteInt32LittleEndian(buffer, Id);
            packetMemoryStream.Write(buffer);

            BinaryPrimitives.WriteInt32LittleEndian(buffer, Type);
            packetMemoryStream.Write(buffer);
            
            packetMemoryStream.Write(Body); // Body: Body.length Bytes
            packetMemoryStream.WriteByte(EmptyStringTerminator); // Body: Null Terminator Byte
            packetMemoryStream.WriteByte(EmptyStringTerminator); // Empty String: 1 Byte, null-terminated
            
            return packetMemoryStream.ToArray();
        }

        public override string ToString()
        {
            return $"Size: {Size}, Id: {Id}, Type: {Type}, Body Size: {Body.Length}; Hex: {string.Join("", ToArray().Select(x => x.ToString("X2")))}";
        }
    }
}