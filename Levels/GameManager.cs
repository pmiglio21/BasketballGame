using Entities;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//autoload
namespace Levels
{
    public partial class GameManager : Node
    {
        //Players will exist here and in the tree separately. We have to keep the two lists in sync
        public static List<TestBasketballPlayer> Players = new List<TestBasketballPlayer>();

        public override void _Ready()
        {
            
        }
    }
}
