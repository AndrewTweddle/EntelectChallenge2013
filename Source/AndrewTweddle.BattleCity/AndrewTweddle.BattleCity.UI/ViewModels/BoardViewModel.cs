using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.UI.ViewModels
{
    public class BoardViewModel : BaseViewModel
    {
        public BitMatrix Walls { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }
    }
}
