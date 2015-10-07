using System;

namespace LangDetector.Core.Events
{
    public class BytesLeidosEventArgs : EventArgs
    {
        public long BytesLeidos { get; set; }
        public long BytesTotales { get; set; }
    }
}
