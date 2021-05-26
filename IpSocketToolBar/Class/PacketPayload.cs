using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IpSocketToolBar
{
    /// <summary>
    /// パケットのペイロード
    /// </summary>
    public class PacketPayload
    {
        #region 公開フィールド

        /// <summary>
        /// ペイロードのバイト列
        /// </summary>
        public byte[] Data;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">バイト列</param>
        /// <param name="endian">エンディアン</param>
        public PacketPayload(byte[] data, Endian endian = Endian.Big)
        {
            this.Data = data;
            this.Endian = endian;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="size">バイト数</param>
        /// <param name="endian">エンディアン</param>
        public PacketPayload(int size, Endian endian = Endian.Big)
        {
            this.Data = new byte[size];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="stringData">文字列</param>
        public PacketPayload(string stringData)
        {
            this.Data = Encoding.ASCII.GetBytes(stringData);
        }

        #endregion

        #region バイト/文字/文字列データの格納と取得

        /// <summary>
        /// パケットデータを文字列に変換する
        /// </summary>
        /// <returns>文字列</returns>
        public override string ToString()
        {
            return Encoding.ASCII.GetString(Data);
        }

        /// <summary>
        /// 1バイトのデータ(制御コードなど)を格納する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="value">1バイトのデータ</param>
        public void SetByte(int offset, byte value)
        {
            Data[offset] = value;
        }

        /// <summary>
        /// 1文字のアスキー文字を格納する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="value">1文字のデータ</param>
        public void SetChar(int offset, char value)
        {
            Data[offset] = (byte)value;
        }

        /// <summary>
        /// 文字列を格納する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="stringData">文字列データ</param>
        public void SetString(int offset, string stringData)
        {
            byte[] bData = Encoding.ASCII.GetBytes(stringData);
            Array.Copy(bData, 0, this.Data, offset, bData.Length);
        }

        /// <summary>
        /// 1バイトのデータ(制御コードなど)を取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <returns>1バイトのデータ</returns>
        public byte GetByte(int offset)
        {
            return Data[offset];
        }

        /// <summary>
        /// 1文字のアスキー文字を取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <returns>1文字のデータ</returns>
        public char GetChar(int offset)
        {
            return (char)Data[offset];
        }

        /// <summary>
        /// 文字列を取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="value">文字列データ</param>
        public string GetString(int offset, int length)
        {
            byte[] bData = new byte[length];
            Array.Copy(this.Data, offset, bData, 0, length);
            string strData = Encoding.ASCII.GetString(bData);
            return strData;
        }

        #endregion

        #region 整数/実数データの格納と取得 (バイナリー形式)
        
        /// <summary>
        /// 整数値を格納する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">バイト数</param>
        /// <param name="value">数値</param>
        public void SetInt(int offset, int width, int value)
        {
            // ビッグエンディアンの場合
            if(Endian == Endian.Big)
            {
                for (int i = width - 1; i >= 0; i--)
                {
                    Data[offset + i] = (byte)(value & 0xFF);
                    value >>= 8;
                }
            }
            // リトルエンディアンの場合
            else
            {
                for(int i = 0; i < width; i++)
                {
                    Data[offset + i] = (byte)(value & 0xFF);
                    value >>= 8;
                }
            }
        }

        /// <summary>
        /// float型実数値を格納する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="value">数値</param>
        public void SetFloat(int offset, float value)
        {
            byte[] bData = BitConverter.GetBytes(value);

            if (Endian == Endian.Big)
            {
                // ビッグエンディアンの場合はバイト順を反転
                Array.Reverse(bData);
            }
            Array.Copy(bData, 0, Data, offset, 4);
        }


        /// <summary>
        /// 非負整数値を取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">バイト数</param>
        /// <returns>数値を返す</returns>
        public int GetInt(int offset, int width)
        {
            int value = 0;

            // ビッグエンディアンの場合
            if (Endian == Endian.Big)
            {
                for (int i = 0; i < width; i++)
                {
                    value <<= 8;
                    value |= Data[offset + i];
                }
            }
            // リトルエンディアンの場合
            else
            {
                for (int i = width - 1; i >= 0; i--)
                {
                    value <<= 8;
                    value |= Data[offset + i];
                }
            }
            return value;
        }

        /// <summary>
        /// 符号なし整数値を取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">バイト数</param>
        /// <returns>数値を返す</returns>
        public uint GetIntU(int offset, int width)
        {
            uint value = 0;

            // ビッグエンディアンの場合
            if (Endian == Endian.Big)
            {
                for (int i = 0; i < width; i++)
                {
                    value <<= 8;
                    value |= Data[offset + i];
                }
            }
            // リトルエンディアンの場合
            else
            {
                for (int i = width - 1; i >= 0; i--)
                {
                    value <<= 8;
                    value |= Data[offset + i];
                }
            }
            return value;
        }

        /// <summary>
        /// 符号つき整数値を取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">バイト数</param>
        /// <returns>数値を返す</returns>
        public int GetIntS(int offset, int width)
        {
            int value;
            uint uValue = GetIntU(offset, width);

            if ((uValue & B_SIGN[width - 1]) == 0) {
                value = (int)uValue;
            } else {
                if (width == 4) {
                    value = (int)uValue;
                } else {
                    value = (int)uValue - B_COMP[width - 1];
                }
            }
            return value;
        }

        /// <summary>
        /// float型実数値を取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <returns>数値を返す</returns>
        public float GetFloat(int offset)
        {
            byte[] bData = new byte[4];
            Array.Copy(this.Data, offset, bData, 0, 4);

            if (Endian == Endian.Big)
            {
                // ビッグエンディアンの場合はバイト順を反転
                Array.Reverse(bData);
            }
            float value = BitConverter.ToSingle(bData, 0);

            return value;
        }

        #endregion

        #region 整数/実数データの格納と取得 (アスキー形式)

        /// <summary>
        /// 整数値を16進文字列に変換して格納する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">桁数</param>
        /// <param name="value">数値</param>
        public void SetHex(int offset, int width, int value)
        {
            for(int i= width - 1; i >= 0; i--)
            {
                Data[offset + i] = HEXCHAR[value & 0x0000000F];
                value >>= 4;
            } 
        }

        /// <summary>
        /// 非負整数値を10進文字列に変換して格納する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">桁数</param>
        /// <param name="value">数値</param>
        public void SetDec(int offset, int width, int value)
        {
            for (int i = width - 1; i >= 0; i--)
            {
                Data[offset + i] = HEXCHAR[value % 10];
                value /= 10;
            }
        }

        /// <summary>
        /// 16進文字列を非負整数値に変換して取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">文字数</param>
        /// <param name="value">数値を返す</param>
        /// <returns>成否</returns>
        public bool GetHex(int offset, int width, out int value)
        {
            value = 0;
            for (int i = 0; i < width; i++)
            {
                value <<= 4;

                if (Hex2Int(Data[offset + i], out int digitVal))
                {
                    value += digitVal;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 16進文字列を符号なし整数値に変換して取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">文字数</param>
        /// <param name="value">数値を返す</param>
        /// <returns>成否</returns>
        public bool GetHexU(int offset, int width, out uint value)
        {
            value = 0;
            for (int i = 0; i < width; i++)
            {
                value <<= 4;

                if (Hex2Int(Data[offset + i], out int digitVal))
                {
                    value += (uint)digitVal;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 16進文字列を符号つき整数値に変換して取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">文字数</param>
        /// <param name="value">数値を返す</param>
        /// <returns>成否</returns>
        public bool GetHexS(int offset, int width, out int value)
        {
            if(GetHexU(offset, width, out uint uValue))
            {
                if((uValue & A_SIGN[width - 1]) == 0) {
                    value = (int)uValue;
                } else {
                    if(width == 8) {
                        value = (int)uValue;
                    } else {
                        value = (int)uValue - A_COMP[width - 1];
                    }
                }
                return true;
            } else {
                value = 0;
                return false;
            }
        }


        /// <summary>
        /// 10進文字列を非負整数値に変換して取得する
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="width">文字数</param>
        /// <param name="value">数値を返す</param>
        /// <returns>成否</returns>
        public bool GetDec(int offset, int width, out int value)
        {
            value = 0;
            for (int i = 0; i < width; i++)
            {
                value *= 10;

                if (Dec2Int(Data[offset + i], out int digitVal))
                {
                    value += digitVal;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region チェックサムの計算
        
        /// <summary>
        /// 算術加算によるチェックサム値を計算する
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="length">バイト数</param>
        /// <returns>チェックサム値</returns>
        public byte Sum(int start, int length)
        {
            byte sum = 0;

            for(int i = 0; i < length; i++)
            {
                sum += this.Data[start + i];
            }
            return sum;
        }

        /// <summary>
        /// 算術加算によるチェックサム値を計算して指定位置に格納する
        /// </summary>
        /// <param name="offset">格納位置</param>
        /// <param name="start">開始位置</param>
        /// <param name="length">バイト数</param>
        public void SetSum(int offset, int start, int length)
        {
            byte sum = this.Sum(start, length);
            this.SetByte(offset, sum);
        }

        /// <summary>
        /// 算術加算によるチェックサム値を計算して指定位置の値と比較する
        /// </summary>
        /// <param name="offset">格納位置</param>
        /// <param name="start">開始位置</param>
        /// <param name="length">バイト数</param>
        /// <returns>一致すればtrue</returns>
        public bool CheckSum(int offset, int start, int length)
        {
            byte sum = this.Sum(start, length);
            byte val = this.GetByte(offset);
            return (sum == val);
        }

        /// <summary>
        /// 排他的論理和によるチェックサム値を計算する
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="length">バイト数</param>
        /// <returns>チェックサム値</returns>
        public byte Xor(int start, int length)
        {
            byte xor = 0;

            for (int i = 0; i < length; i++)
            {
                xor ^= this.Data[start + i];
            }
            return xor;
        }

        /// <summary>
        /// 排他的論理和によるチェックサム値を計算して指定位置に格納する
        /// </summary>
        /// <param name="offset">格納位置</param>
        /// <param name="start">開始位置</param>
        /// <param name="length">バイト数</param>
        public void SetXor(int offset, int start, int length)
        {
            byte xor = this.Xor(start, length);
            this.SetByte(offset, xor);
        }

        /// <summary>
        /// 排他的論理和によるチェックサム値を計算して指定位置の値と比較する
        /// </summary>
        /// <param name="offset">格納位置</param>
        /// <param name="start">開始位置</param>
        /// <param name="length">バイト数</param>
        /// <returns>一致すればtrue</returns>
        public bool CheckXor(int offset, int start, int length)
        {
            byte xor = this.Xor(start, length);
            byte val = this.GetByte(offset);
            return (xor == val);
        }

        #endregion

        #region 内部処理

        // エンディアン
        Endian Endian;

        // 符号ビットのテーブル (バイナリー形式用)
        readonly uint[] B_SIGN = {
            0x80,
            0x8000,
            0x800000,
            0x80000000
        };

        // 補数換算用のテーブル (バイナリー形式用)
        readonly int[] B_COMP = {
            0x100,
            0x10000,
            0x1000000,
        };
        
        // 数値を表現するキャラクタのテーブル (0123456789ABCDEF)
        readonly byte[] HEXCHAR = {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
            0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46
        };

        // 0～9, A～F, a～f のキャラクタを数値に変換
        static bool Hex2Int(byte c, out int val)
        {
            if((0x30 <= c) && (c <= 0x39))
            {
                val = c - 0x30;
                return true;
            }
            else if ((0x41 <= c) && (c <= 0x46))
            {
                val = 10 + (c - 0x41);
                return true;
            }
            else if ((0x61 <= c) && (c <= 0x66))
            {
                val = 10 + (c - 0x61);
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        // 0～9のキャラクタを数値に変換
        static bool Dec2Int(byte c, out int val)
        {
            if ((0x30 <= c) && (c <= 0x39))
            {
                val = c - 0x30;
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        // 符号ビットのテーブル (アスキー形式用)
        readonly uint[] A_SIGN = {
            0x8,
            0x80,
            0x800,
            0x8000,
            0x80000,
            0x800000,
            0x8000000,
            0x80000000
        };

        // 補数換算用のテーブル (アスキー形式用)
        readonly int[] A_COMP = {
            0x10,
            0x100,
            0x1000,
            0x10000,
            0x100000,
            0x1000000,
            0x10000000,
        };

        #endregion
    }

    #region 定数定義

    /// <summary>
    /// エンディアン
    /// </summary>
    public enum Endian
    {
        Big,
        Little
    }

    /// <summary>
    /// アスキー制御キャラクタコード
    /// </summary>
    public static class AsciiCode
    {
        public const byte NULL = 0x00;
        public const byte SOH = 0x01;
        public const byte STX = 0x02;
        public const byte ETX = 0x03;
        public const byte EOT = 0x04;
        public const byte ENG = 0x05;
        public const byte ACK = 0x06;
        public const byte BEL = 0x07;
        public const byte BS = 0x08;
        public const byte HT = 0x09;
        public const byte LF = 0x0A;
        public const byte VT = 0x0B;
        public const byte FF = 0x0C;
        public const byte CR = 0x0D;
        public const byte SO = 0x0E;
        public const byte SI = 0x0F;
        public const byte DLE = 0x10;
        public const byte DC1 = 0x11;
        public const byte DC2 = 0x12;
        public const byte DC3 = 0x13;
        public const byte DC4 = 0x14;
        public const byte NAK = 0x15;
        public const byte SYN = 0x16;
        public const byte ETB = 0x17;
        public const byte CAN = 0x18;
        public const byte EM = 0x19;
        public const byte SUB = 0x1A;
        public const byte ESC = 0x1B;
        public const byte FS = 0x1C;
        public const byte GS = 0x1D;
        public const byte RS = 0x1E;
        public const byte US = 0x1F;
        public const byte DEL = 0x7F;
    }
    #endregion
}
