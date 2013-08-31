using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.BattleCity.Core
{
    public struct Phase
    {
        public int PhaseId { get; private set; }

        private const int PhaseTypeBitMask = 0x07;
        private const int PhaseTypeBitShift = 0;

        private const int TickBitMask = 0xFFF8;
        private const int TickBitShift = 3;

        public PhaseType PhaseType
        {
            get
            {
                return (PhaseType)((PhaseId & PhaseTypeBitMask) >> PhaseTypeBitShift);
            }
            set
            {
                PhaseId = (PhaseId & ~PhaseTypeBitMask) | (((byte)value) << PhaseTypeBitShift);
            }
        }

        public int Tick
        {
            get
            {
                return (int)(PhaseId & TickBitMask);
            }
            set
            {
                PhaseId = (PhaseId & ~TickBitMask) | value;
            }
        }

        public Phase(int tick, PhaseType phaseType)
            : this()
        {
            PhaseId
                = (int)phaseType
                | (tick << TickBitShift);

            Debug.Assert(PhaseType == phaseType, "Phase type wrong");
            Debug.Assert(Tick == tick, "Tick wrong");
        }

        public static bool operator <(Phase phase1, Phase phase2)
        {
            return phase1.PhaseId < phase2.PhaseId;
        }

        public static bool operator <=(Phase phase1, Phase phase2)
        {
            return phase1.PhaseId <= phase2.PhaseId;
        }

        public static bool operator >(Phase phase1, Phase phase2)
        {
            return phase1.PhaseId > phase2.PhaseId;
        }

        public static bool operator >=(Phase phase1, Phase phase2)
        {
            return phase1.PhaseId >= phase2.PhaseId;
        }

        public static bool operator ==(Phase phase1, Phase phase2)
        {
            return phase1.PhaseId == phase2.PhaseId;
        }

        public static bool operator !=(Phase phase1, Phase phase2)
        {
            return phase1.PhaseId != phase2.PhaseId;
        }

        public override int GetHashCode()
        {
            return PhaseId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Phase: tick {0}, phase type {1}", Tick, PhaseType);
        }

        public Phase AddTicks(int tickOffset)
        {
            return new Phase(this.Tick + tickOffset, this.PhaseType);
        }
    }
}
