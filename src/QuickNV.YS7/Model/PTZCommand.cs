using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.YS7.Model
{
    public enum PTZCommand
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        LeftUp = 4,
        LeftDown = 5,
        RightUp = 6,
        RightDown = 7,
        ZoomIn = 8,
        ZoomOut = 9,
        FocusFar = 10,
        FocusNear = 11,
        Auto = 16
    }
}
