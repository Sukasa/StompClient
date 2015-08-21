using System;

namespace StompClient
{
    public class StompRingBuffer<T>
    {
        private T[] _Buffer;
        private int _WritePtr;
        private int _ReadPtr;
        private int _Avail;
        private int _Written;

        private int _SeekOffset;

        public StompRingBuffer(int BufferSize)
        {
            _Buffer = new T[BufferSize];
            _Avail = BufferSize;
            _Written = 0;
        }

        public int Available { get { return _Avail; } }

        public void Write(T[] Data, int Length)
        {
            _Avail -= Length;
            if (_Avail < 0)
            {
                _Avail += Length;
                throw new InvalidOperationException("Unable to add data to ring buffer - ring buffer full");
            }

            if (_WritePtr + Length > _Buffer.Length)
            {
                // Split write into two
                int Split = _Buffer.Length - _WritePtr;
                Array.Copy(Data, 0, _Buffer, _WritePtr, Split);
                Array.Copy(Data, Length - Split, _Buffer, 0, Length - Split);
            }
            else
            {
                // Single write
                Array.Copy(Data, 0, _Buffer, _WritePtr, Length);
            }

            _Written += Length;
            _WritePtr = (_WritePtr + Length) % _Buffer.Length;
        }

        public void Write(T[] Data)
        {
            Write(Data, Data.Length);
        }

        public int Seek(int Amount)
        {
            _SeekOffset += Amount;
            if (_SeekOffset > 0)
                _SeekOffset = 0;

            if (_SeekOffset < _Avail - _Buffer.Length)
                _SeekOffset = _Avail - _Buffer.Length;

            if (_SeekOffset < -_Written)
                _SeekOffset = -_Written;

            return _SeekOffset;
        }

        public T[] Read(int Amount)
        {
            if (Amount > _Buffer.Length - _Avail)
                throw new InvalidOperationException("Cannot read past end of ring");

            int ReadFrom = (_ReadPtr + _SeekOffset + _Buffer.Length) % _Buffer.Length;
            T[] Data = new T[Amount];

            _SeekOffset += Amount;

            if (_SeekOffset > 0)
            {
                _Avail += _SeekOffset;
                _ReadPtr += _SeekOffset;
                _SeekOffset = 0;
            }

            if (ReadFrom + Amount >= _Buffer.Length)
            {
                int Split = _Buffer.Length - ReadFrom;
                Array.Copy(_Buffer, Amount - Split, Data, 0, Split);
                Array.Copy(_Buffer, 0, Data, ReadFrom, Amount - Split);
            }
            else
            {
                Array.Copy(_Buffer, ReadFrom, Data, 0, Amount);
            }

            return Data;
        }

        public T Peek()
        {
            return _Buffer[(_ReadPtr + _SeekOffset + _Buffer.Length) % _Buffer.Length];
        }

        public int DistanceTo(T Value)
        {
            int Distance = 0;

            for (int i = _ReadPtr + _SeekOffset; i != _WritePtr; i = (i + 1) % _Buffer.Length)
            {
                if (Object.Equals(_Buffer[i], Value))
                    return Distance;

                Distance++;
            }

            return -1;
        }
    }
}
