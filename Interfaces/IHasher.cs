using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.Interfaces
{
    public interface IHasher
    {
        string GetHash(string str);
        bool Сompare(string hash, string str);
    }
}
