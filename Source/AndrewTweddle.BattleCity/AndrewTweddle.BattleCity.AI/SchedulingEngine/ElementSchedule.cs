using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.States;
using AndrewTweddle.BattleCity.AI.ScriptEngine;
using System.Diagnostics;
using AndrewTweddle.BattleCity.Core;
using AndrewTweddle.BattleCity.Core.Collections;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine
{
    public class ElementSchedule<TSchedule, TTimeSlot>: Schedule
        where TSchedule : ElementSchedule<TSchedule, TTimeSlot>
        where TTimeSlot : ElementTimeSlot<TTimeSlot>
    {
        /*
        public Phase StartPhase { get; private set; }
        public Phase EndPhase { get; private set; }

        CircularBuffer<TimeBlock<TTimeSlot>> timeSlotBuffer;

        public TTimeSlot this[Phase phase]
        {
            get
            {
                Debug.Assert(phase >= StartPhase && phase <= EndPhase, 
                    String.Format("{0} is not part of the element schedule between {1} and {2}", 
                    phase, StartPhase, EndPhase));
                return timeSlotBuffer[phase.PhaseId - StartPhase.PhaseId];
            }
            set
            {
                Debug.Assert(phase >= StartPhase && phase <= EndPhase,
                    String.Format("{0} is not part of the element schedule between {1} and {2}",
                    phase, StartPhase, EndPhase));
                Debug.Assert(value.Instant.Phase == phase, 
                    "An element is being placed into the wrong time slot");
                TimeSlots[phase.PhaseId - StartPhase.PhaseId] = value;
            }
        }

        protected ElementSchedule()
        {
        }

        public ElementSchedule(Phase startPhase, Phase endPhase)
            : this()
        {
            StartPhase = startPhase;
            EndPhase = endPhase;

            TimeSlots = new TTimeSlot[SlotCount];
        }
         */
    }
}
