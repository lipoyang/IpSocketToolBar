# PacketData
パケットデータを表します。  

## コンストラクタ
|  名前  |  説明  |
| ---- | ---- |
| PacketPayload(data, endian) | data: パケットデータのバイト配列データ<br>endian: エンディアン指定(省略可) |
| PacketPayload(size, endian) | size: パケットデータの全バイト数<br>endian: エンディアン指定(省略可)|
| PacketPayload(stringData) | stringData: 文字列データ|

## フィールド
|  名前  |  説明  |
| ---- | ---- |
| Data |  パケットデータのバイト配列データ |

## メソッド

### バイト/文字/文字列データの格納と取得
|  名前  |  説明  |
| ---- | ---- |
| SetByte(offset, value)  |  1バイトのデータ(制御コードなど)を格納します。 |
| SetChar(offset, value)  |  1文字のアスキー文字を格納します。 |
| SetString(offset, stringData)  |  文字列を格納します。 |
| GetByte(offset)  |  1バイトのデータ(制御コードなど)を取得します。 |
| GetChar(offset)  |  1文字のアスキー文字を取得します。 |
| GetString(offset, length)  |  文字列を取得します。 |
| ToString()  |  パケットデータを文字列に変換します。 |

### 整数/実数データの格納と取得 (バイナリー形式)
|  名前  |  説明  |
| ---- | ---- |
| SetInt(offset, width, value)  |  整数値を格納します。 |
| SetFloat(offset, value)  |  float型実数値を格納します。 |
| GetInt(offset, width)  |  非負整数値を取得します。 |
| GetIntU(offset, width)  |  符号なし整数値を取得します。 |
| GetIntS(offset, width)  |  符号つき整数値を取得します。 |
| GetFloat(offset)  |   float型実数値を取得します。 |

### 整数/実数データの格納と取得 (アスキー形式)
|  名前  |  説明  |
| ---- | ---- |
| SetHex(offset, width, value)  |  整数値を16進文字列に変換して格納します。 |
| SetDec(offset, width, value)  |  非負整数値を10進文字列に変換して格納します。 |
| GetHex(offset, width, out int value)  |  16進文字列を非負整数値に変換して取得します。 |
| GetDec(offset, width, out int value)  |  10進文字列を非負整数値に変換して取得します。 |
| GetHexU(offset, width, out uint value)  |  16進文字列を符号なし整数値に変換して取得します。 |
| GetHexS(offset, width, out int value)  |  16進文字列を符号つき整数値に変換して取得します。 |

### チェックサム系
|  名前  |  説明  |
| ---- | ---- |
| Sum(start, length)  |  算術加算によるチェックサム値を計算します。 |
| SetSum(offset, start, length)  |  算術加算によるチェックサム値を計算して指定位置に格納します。 |
| CheckSum(offset, start, length)  |  算術加算によるチェックサム値を計算して指定位置の値と比較します。 |
| Xor(start, length)  |  排他的論理和によるチェックサム値を計算します。 |
| SetXor(offset, start, length)  |  排他的論理和によるチェックサム値を計算して指定位置に格納します。 |
| CheckXor(offset, start, length)  |  排他的論理和によるチェックサム値を計算して指定位置の値と比較します。 |
