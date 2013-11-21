using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMOController
{
    /// <summary>
    /// Maintains information about the world.
    /// </summary>
    public static class MmoWorld
    {
        public static ICollection<Realm> Realms;

        static MmoWorld()
        {
            Realms = MmoDatabase.List<Realm>();
        }
    }
}
