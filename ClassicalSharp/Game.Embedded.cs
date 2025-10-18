using System;
using System.Runtime.InteropServices;
#if !USE_DX
using OpenTK.Graphics.OpenGL;
#endif


namespace ClassicalSharp
{
    public delegate void EmbeddedFrameReady(byte[] bgra, int width, int height);

    public partial class Game
    {
        public EmbeddedFrameReady EmbeddedFrameSink;
        public static bool HostedMode = false;
        public bool EmbeddedFlipY = true;

        byte[] _capBuffer;
        byte[] _capScratch;

        public IPlatformWindow PlatformWindow
        {
            get { return window; }
        }

        internal void EmitEmbeddedFrame()
        {
            if (EmbeddedFrameSink == null) 
                return;

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            if (w <= 0 || h <= 0) 
                return;

            int len = w * h * 4;
            if (_capBuffer == null || _capBuffer.Length != len) 
                _capBuffer = new byte[len];
            if (EmbeddedFlipY && (_capScratch == null || _capScratch.Length != len)) 
                _capScratch = new byte[len];

            GCHandle handle = GCHandle.Alloc(_capBuffer, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                GL.ReadPixels(0, 0, w, h, PixelFormat.Bgra, PixelType.UnsignedByte, ptr);
            }
            finally
            {
                handle.Free();
            }

            if (EmbeddedFlipY)
            {
                int row = w * 4;
                int y, srcOff, dstOff;
                for (y = 0; y < h; y++)
                {
                    srcOff = y * row;
                    dstOff = (h - 1 - y) * row;
                    Buffer.BlockCopy(_capBuffer, srcOff, _capScratch, dstOff, row);
                }
                EmbeddedFrameSink(_capScratch, w, h);
            }
            else
            {
                EmbeddedFrameSink(_capBuffer, w, h);
            }
        }
    }
}
