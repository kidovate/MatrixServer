using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixAPI.Interfaces;

namespace MMOController
{
    public interface IMMOCluster : IRMIInterface
    {
        string TestString(int value);
    }
}
