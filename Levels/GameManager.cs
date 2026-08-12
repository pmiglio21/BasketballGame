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
        public static List<TestBasketballPlayer> Players = new List<TestBasketballPlayer>();

        public override void _Ready()
        {
            
        }
    }
}
