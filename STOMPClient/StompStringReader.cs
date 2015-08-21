using System.Linq;

namespace StompClient
{
    internal class StompStringReader
    {
        private string _String;
        private int _Cursor;

        internal StompStringReader(string SourceString)
        {
            _String = SourceString;
            _Cursor = 0;
        }

        public void Seek(int Pos)
        {
            _Cursor = Pos;
            if (_Cursor < 0)
                _Cursor = 0;
        }

        public bool EOF { get { return _Cursor >= _String.Length; } }

        internal string ReadUntil(params char[] Characters)
        {
            if (EOF)
                return "";

            int Ptr = _Cursor;

            // Read up to anything in Characters, but don't include that character
            for (; Ptr < _String.Length && !Characters.Contains(_String[Ptr]); Ptr++)
                ;

            string Output = _String.Substring(_Cursor, Ptr - _Cursor);
            _Cursor = Ptr;

            return Output;
        }

        internal int SkipUntil(params char[] Characters)
        {
            int _Last = _Cursor;
            for (; !EOF && !Characters.Contains(_String[_Cursor]); _Cursor++)
                ;

            return _Cursor - _Last;
        }

        internal int SkipThrough(params char[] Characters)
        {
            int _Last = _Cursor;
            for (; !EOF && Characters.Contains(_String[_Cursor]); _Cursor++)
                ;
            return _Cursor - _Last;
        }

        internal void Shuttle(int Amt)
        {
            Seek(_Cursor + Amt);
        }

        internal string ReadThrough(params char[] Characters)
        {
            int Ptr = _Cursor;

            for (; Ptr < _String.Length && Characters.Contains(_String[Ptr]); Ptr++)
                ;

            string Output = _String.Substring(_Cursor, Ptr - _Cursor);
            _Cursor = Ptr;

            return Output;
        }
    }
}
