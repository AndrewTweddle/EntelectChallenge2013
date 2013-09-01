using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.AI.ScriptEngine;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine
{
    public class TankTimeSlot: ElementTimeSlot<TankTimeSlot>
    {
        // TODO: Add triggers on setters to inform TankScriptSlot of the change.
        // BUT HOW?
        // Either need references to parent objects, or update through methods that provide the extra information
        // Prefer the latter to avoid memory leaks (e.g. events could be used, but that's even worse for memory leaks!)
        // Also there is a risk of cascading effects, which can be bad for performance, so be careful how you do this...

        public bool CanFire { get; set; }
        public TankAction TankActionToTake { get; set; }
        public Instant InstantWhenBulletIsAvailableAgain { get; set; }

        public int TicksUntilBulletIsAvailableAgain
        {
            get
            {
                return InstantWhenBulletIsAvailableAgain.Tick - Instant.Tick;
            }
        }

        // TODO: Must this be a specific script slot, or something more generic?
        // TODO: Must there be multiple of these? 
        // Can more than 1 script be in play simultaneously?
        public TankScriptSlot ScriptSlot { get; set; }
    }
}
