﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jil.Deserialize
{
    struct CustomStringBuilder
    {
        const int StringArrayStartSize = 2;
        const int CharArrayStartSize = 2;
        const int SingleCharArrayStartSize = 4;
        static readonly int[] EmptyIxs = new[] { -1 };

        int OverallIx;

        int StringPtr;
        int[] StringIxs;
        string[] Strings;

        int CharPtr;
        int[] CharIxs;
        char[][] Chars;

        int SingleCharPtr;
        int[] SingleCharIxs;
        char[] SingleChars;

        int Length;

        void IncreaseStringPtr()
        {
            if (Strings == null)
            {
                Strings = new string[StringArrayStartSize];
                StringIxs = new int[StringArrayStartSize];
                StringPtr = 0;
                return;
            }

            StringPtr++;
            if (Strings.Length == StringPtr)
            {
                var oldLen = Strings.Length;
                var newLen = oldLen * 2;
                var newStrings = new string[newLen];
                var newStringIxs = new int[newLen];
                Array.Copy(Strings, newStrings, oldLen);
                Array.Copy(StringIxs, newStringIxs, oldLen);
                Strings = newStrings;
                StringIxs = newStringIxs;
            }
        }

        void IncreaseCharPtr()
        {
            if (Chars == null)
            {
                Chars = new char[CharArrayStartSize][];
                CharIxs = new int[CharArrayStartSize];
                CharPtr = 0;
                return;
            }

            CharPtr++;
            if (Chars.Length == CharPtr)
            {
                var oldLen = Chars.Length;
                var newLen = oldLen * 2;
                var newChars = new char[newLen][];
                var newCharIxs = new int[newLen];
                Array.Copy(Chars, newChars, oldLen);
                Array.Copy(CharIxs, newCharIxs, oldLen);
                Chars = newChars;
                CharIxs = newCharIxs;
            }
        }

        void IncreaseSingleCharPtr()
        {
            if (SingleChars == null)
            {
                SingleChars = new char[SingleCharArrayStartSize];
                SingleCharIxs = new int[SingleCharArrayStartSize];
                SingleCharPtr = 0;
                return;
            }

            SingleCharPtr++;
            if (SingleChars.Length == SingleCharPtr)
            {
                var oldLen = SingleChars.Length;
                var newLen = oldLen * 2;
                var newChars = new char[newLen];
                var newCharIxs = new int[newLen];
                Array.Copy(SingleChars, newChars, oldLen);
                Array.Copy(SingleCharIxs, newCharIxs, oldLen);
                SingleChars = newChars;
                SingleCharIxs = newCharIxs;
            }
        }

        public void Append(string str)
        {
            IncreaseStringPtr();

            Strings[StringPtr] = str;
            StringIxs[StringPtr] = OverallIx;
            
            OverallIx++;
            Length += str.Length;
        }

        public void Append(char c)
        {
            IncreaseSingleCharPtr();

            SingleChars[SingleCharPtr] = c;
            SingleCharIxs[SingleCharPtr] = OverallIx;

            OverallIx++;
            Length++;
        }

        public void Append(char[] chars, int start, int len)
        {
            IncreaseCharPtr();

            var charsCopy = new char[len];
            Array.Copy(chars, start, charsCopy, 0, len);

            Chars[CharPtr] = charsCopy;
            CharIxs[CharPtr] = OverallIx;
            
            OverallIx++;
            Length += len;
        }

        public void WriteTo(TextWriter writer)
        {
            var strPtr = 0;
            var charPtr = 0;
            var singleCharPtr = 0;

            var charIxs = CharIxs ?? EmptyIxs;
            var strIxs = StringIxs ?? EmptyIxs;
            var singleCharIxs = SingleCharIxs ?? EmptyIxs;

            for (var ix = 0; ix < OverallIx; ix++)
            {
                if (singleCharIxs[singleCharPtr] == ix)
                {
                    var toWrite = SingleChars[singleCharPtr];
                    writer.Write(toWrite);
                    if (singleCharPtr + 1 != singleCharIxs.Length)
                    {
                        singleCharPtr++;
                    }
                    continue;
                }

                if (charIxs[charPtr] == ix)
                {
                    var toWrite = Chars[charPtr];
                    writer.Write(toWrite);
                    if (charPtr + 1 != charIxs.Length)
                    {
                        charPtr++;
                    }
                    continue;
                }

                if (strIxs[strPtr] == ix)
                {
                    var toWrite = Strings[strPtr];
                    writer.Write(toWrite);
                    if (strPtr + 1 != strIxs.Length)
                    {
                        strPtr++;
                    }
                    continue;
                }

                throw new Exception("Shouldn't be possible");
            }
        }

        unsafe void Copy(char* into, char[] val)
        {
            fixed (char* fixedVal = val)
            {
                var len = val.Length;
                for (var i = 0; i < len; i++)
                {
                    *into = val[i];
                    into++;
                }
            }
        }

        unsafe void Copy(char* into, string val)
        {
            fixed (char* fixedVal = val)
            {
                var len = val.Length;
                for (var i = 0; i < len; i++)
                {
                    *into = val[i];
                    into++;
                }
            }
        }

        public unsafe string StaticToString()
        {
            var ret = stackalloc char[Length];

            var strPtr = 0;
            var charPtr = 0;
            var singleCharPtr = 0;

            var charIxs = CharIxs ?? EmptyIxs;
            var strIxs = StringIxs ?? EmptyIxs;
            var singleCharIxs = SingleCharIxs ?? EmptyIxs;

            var retPtr = ret;

            for (var ix = 0; ix < OverallIx; ix++)
            {
                if (singleCharIxs[singleCharPtr] == ix)
                {
                    var toWrite = SingleChars[singleCharPtr];
                    *retPtr = toWrite;
                    retPtr++;
                    if (singleCharPtr + 1 != singleCharIxs.Length)
                    {
                        singleCharPtr++;
                    }
                    continue;
                }

                if (charIxs[charPtr] == ix)
                {
                    var toWrite = Chars[charPtr];
                    Copy(retPtr, toWrite);
                    retPtr += toWrite.Length;
                    if (charPtr + 1 != charIxs.Length)
                    {
                        charPtr++;
                    }
                    continue;
                }

                if (strIxs[strPtr] == ix)
                {
                    var toWrite = Strings[strPtr];
                    Copy(retPtr, toWrite);
                    retPtr += toWrite.Length;
                    if (strPtr + 1 != strIxs.Length)
                    {
                        strPtr++;
                    }
                    continue;
                }

                throw new Exception("Shouldn't be possible");
            }

            return new string(ret, 0, Length);
        }

        public override string ToString()
        {
            return StaticToString();
        }

        public void Clear()
        {
            OverallIx = 0;
            Length = 0;
            CharPtr = -1;
            StringPtr = -1;
            SingleCharPtr = -1;
        }
    }
}
