using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core;
using System.Diagnostics;

namespace AndrewTweddle.BattleCity.AI.ScriptEngine
{
    public struct Instant
    {
        public uint InstantId { get; private set; }

        private const uint UnitSequenceBitMask = 0x0000FF;
        private const byte UnitSequenceBitShift = 0;

        private const uint TurnPhaseBitMask = 0x0F00;
        private const byte TurnPhaseBitShift = 8;

        private const uint TickBitMask = 0x0FFF000;
        private const byte TickBitShift = 12;

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
                InstantId = (InstantId & ~TurnPhaseBitMask) | (((uint) value) << TurnPhaseBitShift);
            }
        }

        public ushort Tick
        {
            get
            {
                return (ushort)(InstantId & TickBitMask);
            }
            set
            {
                InstantId = (InstantId & ~TickBitMask) | ((ushort) value);
            }
        }

        public Instant(ushort tick, TurnPhase phase, byte unitSequence): this()
        {
            InstantId
                = unitSequence
                | (((uint) phase) << TurnPhaseBitShift)
                | (uint) (tick << TickBitShift);

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
    }
}
