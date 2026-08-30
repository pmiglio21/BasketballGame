using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online
{
    //Can't inherit node2d or node 3d because once it gets passed over to another client via packet,
    //the debugger throws an error saying the player needs to be part of the scene tree (AddChild),
    //which we don't want to do just yet.
    public class TestOnlinePlayer
    {
        public string PlayerId;
    }
}
