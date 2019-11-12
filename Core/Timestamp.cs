using System;

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
    }
}