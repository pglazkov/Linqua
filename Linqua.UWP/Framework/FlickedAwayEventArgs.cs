using System;

namespace Linqua.Framework
{
    public class FlickedAwayEventArgs : EventArgs
    {
        public FlickedAwayEventArgs(FlickDirection direction)
        {
            Direction = direction;
        }

        public FlickDirection Direction { get; private set; }
    }
}