using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine
{
    public class TimeBlock<T>
    {
        public Phase StartPhase { get; private set; }
        public Phase EndPhase { get; private set; }
        public int SlotCount
        {
            get
            {
                return EndPhase.PhaseId - StartPhase.PhaseId + 1;
            }
        }

        public T[] TimeSlots { get; private set; }

        public T this[Phase phase]
        {
            get
            {
                Debug.Assert(phase >= StartPhase && phase <= EndPhase, 
                    String.Format("{0} is not part of the element schedule between {1} and {2}", 
                    phase, StartPhase, EndPhase));
                return TimeSlots[phase.PhaseId - StartPhase.PhaseId];
            }
            set
            {
                Debug.Assert(phase >= StartPhase && phase <= EndPhase,
                    String.Format("{0} is not part of the element schedule between {1} and {2}",
                    phase, StartPhase, EndPhase));
                TimeSlots[phase.PhaseId - StartPhase.PhaseId] = value;
            }
        }

        protected TimeBlock()
        {
        }

        public TimeBlock(Phase startPhase, Phase endPhase)
            : this()
        {
            StartPhase = startPhase;
            EndPhase = endPhase;

            TimeSlots = new T[SlotCount];
        }

    }
}
