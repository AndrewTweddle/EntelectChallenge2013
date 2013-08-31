using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.BattleCity.Core
{
    public struct Instant
    {
        public int InstantId { get; private set; }

        private const int UnitSequenceBitMask = 0x000FFF;
        private const int UnitSequenceBitShift = 0;

        private const int PhaseTypeBitMask = 0x07000;
        private const int PhaseTypeBitShift = 12;

        private const int TickBitMask = 0x7FF8000;
        private const int TickBitShift = 15;

        public byte UnitSequence
        {
            get
            {
                return (byte)(InstantId & UnitSequenceBitMask);
            }
            set
            {
                InstantId = (InstantId & ~UnitSequenceBitMask) | value;
            }
        }

        public PhaseType PhaseType
        {
            get
            {
                return (PhaseType)((InstantId & PhaseTypeBitMask) >> PhaseTypeBitShift);
            }
            set
            {
                InstantId = (InstantId & ~PhaseTypeBitMask) | (((byte) value) << PhaseTypeBitShift);
            }
        }

        public int Tick
        {
            get
            {
                return (int)(InstantId & TickBitMask);
            }
            set
            {
                InstantId = (InstantId & ~TickBitMask) | value;
            }
        }

        public Phase Phase
        {
            get
            {
                return new Phase(Tick, PhaseType);
            }
        }

        public Instant(int tick, PhaseType phaseType, byte unitSequence): this()
        {
            InstantId
                = unitSequence
                | (((int) phaseType) << PhaseTypeBitShift)
                | (tick << TickBitShift);

            Debug.Assert(UnitSequence == unitSequence, "UnitSequence wrong");
            Debug.Assert(PhaseType == phaseType, "Phase type wrong");
            Debug.Assert(Tick == tick, "Tick wrong");
        }

        public static bool operator <(Instant instant1, Instant instant2)
        {
            return instant1.InstantId < instant2.InstantId;
        }

        public static bool operator <=(Instant instant1, Instant instant2)
        {
            return instant1.InstantId <= instant2.InstantId;
        }

        public static bool operator >(Instant instant1, Instant instant2)
        {
            return instant1.InstantId > instant2.InstantId;
        }

        public static bool operator >=(Instant instant1, Instant instant2)
        {
            return instant1.InstantId >= instant2.InstantId;
        }

        public static bool operator ==(Instant instant1, Instant instant2)
        {
            return instant1.InstantId == instant2.InstantId;
        }

        public static bool operator !=(Instant instant1, Instant instant2)
        {
            return instant1.InstantId != instant2.InstantId;
        }

        public override int GetHashCode()
        {
            return InstantId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Instant: tick {0}, phase type {1}, unit sequence {2}", Tick, PhaseType, UnitSequence);
        }

        public Instant AddTicks(int tickOffset)
        {
            return new Instant(this.Tick + tickOffset, this.PhaseType, this.UnitSequence);
        }
    }
}
