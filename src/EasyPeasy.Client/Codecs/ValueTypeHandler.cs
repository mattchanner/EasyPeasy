// ---------------------------------------------------------------------------------
// <copyright file="ValueTypeHandler.cs">
//
//   The MIT License (MIT)
//     Copyright © 2013 Matt Channer (mchanner at gmail dot com)
//    
//     Permission is hereby granted, free of charge, to any person obtaining a 
//     copy of this software and associated documentation files (the “Software”),
//     to deal in the Software without restriction, including without limitation 
//     the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//     and/or sell copies of the Software, and to permit persons to whom the 
//     Software is furnished to do so, subject to the following conditions:
//   
//     The above copyright notice and this permission notice shall be included 
//     in all copies or substantial portions of the Software.
//   
//     THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS 
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//     THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
//     THE SOFTWARE.
// </copyright>
// <summary>
//   A type handler for integer values
// </summary>
// ---------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Net;

namespace EasyPeasy.Client.Codecs
{
    /// <summary>
    /// A handler to read and write various value types
    /// </summary>
    internal class ValueTypeHandler : IMediaTypeHandler
    {
        /// <summary> The type code to read and write. </summary>
        private readonly TypeCode typeCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeHandler"/> class.
        /// </summary>
        /// <param name="typeCode"> The type code to read and write. </param>
        public ValueTypeHandler(TypeCode typeCode)
        {
            this.typeCode = typeCode;
        }

        /// <summary>
        /// When called, this method is responsible for writing the value to the stream
        /// </summary>
        /// <param name="request">The web request being written to </param>
        /// <param name="value">The value to write</param>
        /// <param name="body">The stream to write to</param>
        public void WriteObject(WebRequest request, object value, Stream body)
        {
            BinaryWriter writer = new BinaryWriter(body);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    writer.Write((bool)value);
                    break;
                case TypeCode.Char:
                    writer.Write((char)value);
                    break;
                case TypeCode.SByte:
                    writer.Write((sbyte)value);
                    break;
                case TypeCode.Byte:
                    writer.Write((byte)value);
                    break;
                case TypeCode.Int16:
                    writer.Write((short)value);
                    break;
                case TypeCode.UInt16:
                    writer.Write((ushort)value);
                    break;
                case TypeCode.Int32:
                    writer.Write((int)value);
                    break;
                case TypeCode.UInt32:
                    writer.Write((uint)value);
                    break;
                case TypeCode.Int64:
                    writer.Write((long)value);
                    break;
                case TypeCode.UInt64:
                    writer.Write((ulong)value);
                    break;
                case TypeCode.Single:
                    writer.Write((float)value);
                    break;
                case TypeCode.Double:
                    writer.Write((double)value);
                    break;
                case TypeCode.Decimal:
                    writer.Write((decimal)value);
                    break;
                case TypeCode.DateTime:
                    DateTime dateTime = (DateTime)value;
                    writer.Write(dateTime.ToUniversalTime().ToLongDateString());
                    break;
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.String:
                    throw new EasyPeasyException("TypeCode not supported by serializer: " + typeCode);
            }

            writer.Flush();
        }

        /// <summary>
        /// When called, this method is responsible for reading the contents of the body stream in order
        /// to generate a response of the type appropriate for the defined media type.
        /// </summary>
        /// <param name="response"> The response being read from. </param>
        /// <param name="body"> The stream to write to </param>
        /// <param name="objectType"> The type to de-serialize.  </param>
        /// <returns> The <see cref="object"/> read from the stream.   </returns>
        public object ReadObject(WebResponse response, Stream body, Type objectType)
        {
            BinaryReader reader = new BinaryReader(body);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return reader.ReadBoolean();
                case TypeCode.Char:
                    return reader.ReadChar();
                case TypeCode.SByte:
                    return reader.ReadSByte();
                case TypeCode.Byte:
                    return reader.ReadByte();
                case TypeCode.Int16:
                    return reader.ReadInt16();
                case TypeCode.UInt16:
                    return reader.ReadUInt16();
                case TypeCode.Int32:
                    return reader.ReadInt32();
                case TypeCode.UInt32:
                    return reader.ReadUInt32();
                case TypeCode.Int64:
                    return reader.ReadInt64();
                case TypeCode.UInt64:
                    return reader.ReadUInt64();
                case TypeCode.Single:
                    return reader.ReadSingle();
                case TypeCode.Double:
                    return reader.ReadDouble();
                case TypeCode.Decimal:
                    return reader.ReadDecimal();
                case TypeCode.DateTime:
                    string dateTimeString = reader.ReadString();
                    return DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture);
                /*
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.String:
                 * */
                default:
                    throw new EasyPeasyException("TypeCode not supported by serializer: " + typeCode);
            }
        }
    }
}
