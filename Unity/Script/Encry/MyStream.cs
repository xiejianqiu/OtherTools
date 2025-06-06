using System.IO;
public class MyStream : FileStream
{
#if UNITY_ANDROID || U1GAME_QK
    static public byte KEY = 66; // 密钥mask: 0010 1000
#else
    static public byte KEY = 67; // 密钥mask: 0010 1000
#endif
    public MyStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
    {

    }
    public MyStream(string path, FileMode mode) : base(path, mode)
    {

    }
    // 重载读接口,一边读，一遍解密;
    public override int Read(byte[] array, int offset, int count)
    {
        var index = base.Read(array, offset, count);
        for (int i = 0; i < array.Length; i++)
        {
            array[i] ^= KEY;
        }
        return index;

    }
    /// <summary>
    /// 重载写接口，先加密再写入;
    /// </summary>
    /// <param name="array"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    public override void Write(byte[] array, int offset, int count)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] ^= KEY;
        }
        base.Write(array, offset, count);
    }
}
