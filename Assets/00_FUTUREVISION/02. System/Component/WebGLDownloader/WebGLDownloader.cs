using System.Runtime.InteropServices;

public static class WebGLDownloader
{
    [DllImport("__Internal")]
    public static extern void DownloadFile(byte[] array, int byteLength, string fileName);
}