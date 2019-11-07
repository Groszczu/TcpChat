using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TCPServer.Models;

namespace TCPServer.Services
{
    public class ClientIdsRepository : IClientIdsRepository
    {
        private const int NumberOfChancesToGenerate = 20;
        private const int NumberOfDigits = 5;
        private const string Digits = "0123456789";
        
        private readonly List<int> _usedIds = new List<int>();

        private int GenerateNextId()
        {
            var digits = new char[NumberOfDigits];
            var randomGenerator = new Random();
            var numberOfDigits = NumberOfDigits;
            while (--numberOfDigits > 0)
            {
                digits[numberOfDigits] = Digits[randomGenerator.Next(Digits.Length)];
            }

            digits[numberOfDigits] = Digits[randomGenerator.Next(1, Digits.Length)];

            var str = new string(digits);
            var generatedId = int.Parse(str);

            return generatedId;
        }

        private bool IsUsed(int id)
        {
            return _usedIds.Contains(id);
        }

        public int NewClientId()
        {
            var generated = GenerateNextId();
            var attempt = NumberOfChancesToGenerate;
            while (IsUsed(generated) && --attempt > 0)
            {
                generated = GenerateNextId();
            }
            
            if (IsUsed(generated))
                throw new DataException("Generation of new ClientId failed");

            _usedIds.Add(generated);
            return generated;
        }
    }
}