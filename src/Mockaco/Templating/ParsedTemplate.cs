// 
// Template.cs
//  
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Mono.TextTemplating
{
    public interface ISegment
    {
        Location StartLocation { get; }
        Location EndLocation { get; set; }
        Location TagStartLocation { get; set; }
    }

    public class TemplateSegment : ISegment
    {
        public TemplateSegment(SegmentType type, string text, Location start)
        {
            Type = type;
            StartLocation = start;
            Text = text;
        }

        public SegmentType Type { get; }
        public string Text { get; }
        public Location TagStartLocation { get; set; }
        public Location StartLocation { get; }
        public Location EndLocation { get; set; }
    }

    public class Directive : ISegment
    {
        public Directive(string name, Location start)
        {
            Name = name;
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            StartLocation = start;
        }

        public string Name { get; }
        public Dictionary<string, string> Attributes { get; }
        public Location TagStartLocation { get; set; }
        public Location StartLocation { get; }
        public Location EndLocation { get; set; }

        public string Extract(string key)
        {
            if (!Attributes.TryGetValue(key, out var value))
                return null;
            Attributes.Remove(key);
            return value;
        }
    }

    public enum SegmentType
    {
        Block,
        Expression,
        Content,
        Helper
    }

    public struct Location : IEquatable<Location>
    {
        public Location(string fileName, int line, int column)
            : this()
        {
            FileName = fileName;
            Column = column;
            Line = line;
        }

        public int Line { get; }
        public int Column { get; }
        public string FileName { get; }

        public static Location Empty => new Location(null, -1, -1);

        public Location AddLine() => new Location(FileName, Line + 1, 1);

        public Location AddCol() => AddCols(1);

        public Location AddCols(int number) => new Location(FileName, Line, Column + number);

        public override string ToString() => $"[{FileName} ({Line},{Column})]";

        public bool Equals(Location other)
        {
            return other.Line == Line && other.Column == Column && other.FileName == FileName;
        }
    }
}