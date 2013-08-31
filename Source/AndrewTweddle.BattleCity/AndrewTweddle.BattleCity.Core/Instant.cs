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

        private const int TurnPhaseBitMask = 0x07000;
        private const int TurnPhaseBitShift = 12;

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

        public TurnPhase Phase
        {
            get
            {
                return (TurnPhase)((InstantId & TurnPhaseBitMask) >> TurnPhaseBitShift);
            }
            set
            {
                InstantId = (InstantId & ~TurnPhaseBitMask) | (((byte) value) << TurnPhaseBitShift);
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

        public Instant(int tick, TurnPhase phase, byte unitSequence): this()
        {
            InstantId
                = unitSequence
                | (((int) phase) << TurnPhaseBitShift)
                | (tick << TickBitShift);

            Debug.Assert(UnitSequence == unitSequence, "UnitSequence wrong");
            Debug.Assert(Phase == phase, "Phase wrong");
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
            return string.Format("Instant: tick {0}, phase {1}, unit sequence {2}", Tick, Phase, UnitSequence);
        }

        public Instant AddTicks(int tickOffset)
        {
            return new Instant(this.Tick + tickOffset, this.Phase, this.UnitSequence);
        }
    }
}
