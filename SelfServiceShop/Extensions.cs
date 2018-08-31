using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using StardewValley;

namespace SelfServiceShop
{
    static class Extensions
    {
        public static NPC Find(this NetCollection<NPC> npcs, string name)
        {
            foreach (NPC npc in npcs)
            {
                if (npc.Name == name)
                    return npc;
            }

            return null;
        }

        public static GameLocation Find(this IList<GameLocation> locations, string name)
        {
            foreach (GameLocation location in locations)
            {
                if (location.Name == name)
                    return location;
            }
            return null;
        }
    }
}
