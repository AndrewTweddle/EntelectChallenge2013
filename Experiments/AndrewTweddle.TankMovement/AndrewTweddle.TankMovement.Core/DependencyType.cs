using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.TankMovement.Core
{
    public enum DependencyType: byte
    {
        None,
        OnNewLeadingEdge,
        OnUnchangedBody,
        OnOldTrailingEdge
    }
}
