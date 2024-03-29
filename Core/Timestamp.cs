﻿using System;

namespace Core
{
    public class Timestamp
    {
        public int Value { get; }
        public Timestamp(DateTime date)
        {
            Value = CalculateTimestamp(date);
        }

        public Timestamp(int unixTimestamp)
        {
            Value = unixTimestamp;
        }
        
        private static int CalculateTimestamp(DateTime date)
        {
            return (int) date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Timestamp timestamp && timestamp.Value == Value;
        }

        protected bool Equals(Timestamp other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}