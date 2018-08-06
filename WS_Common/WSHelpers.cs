using System;
using System.Text;

namespace WS_Common
{
    /// <summary>
    /// Classe auxiliar para o tratamento de dados do websocket
    /// </summary>
    public static class WSHelpers
    {

        /// <summary>
        /// Codifica os dados 
        /// </summary>
        /// <param name="texto"></param>
        /// <returns></returns>
        public static byte[] EncodedData(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return new byte[0];
            byte[] btexto = Encoding.UTF8.GetBytes(texto);
            byte[] ret = new byte[0];

            if (btexto.Length < 126)
            {
                ret = new byte[btexto.Length + 2];

                ret[0] = 129;   // String com menos de 126 caracteres, sem mapa
                ret[1] = (byte)btexto.Length;
                for (int i = 0; i < btexto.Length; i++)
                    ret[i + 2] = btexto[i];


            }
            else if (btexto.Length < 65536)
            {
                ret = new byte[btexto.Length + 4];
                ret[0] = 129;   // String com mais de 125 caracteres, sem mapa
                ret[1] = 0x7e;
                ret[2] = (byte)(btexto.Length >> 8);
                ret[3] = (byte)(btexto.Length & 0xFF);
                for (int i = 0; i < btexto.Length; i++)
                    ret[i + 4] = btexto[i];
            } else
            {
                ret = new byte[btexto.Length + 6];
                ret[0] = 129;
                ret[1] = 0x7f;
                ret[2] = (byte)(btexto.Length >> 24);
                ret[3] = (byte)((btexto.Length >> 16) & 0xFF);
                ret[4] = (byte)((btexto.Length >> 8) & 0xFF);
                ret[5] = (byte)(btexto.Length & 0xFF);
                for (int i = 0; i < btexto.Length; i++)
                    ret[i + 6] = btexto[i];
            }


            return ret;

        }

        public static string DecodedData(byte[] buffer, int length)
        {
            byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
                throw new Exception("The buffer length is small than the data length");

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.UTF8.GetString(buffer, dataIndex, dataLength);
        }
    }
}
