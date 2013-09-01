using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core;

namespace AndrewTweddle.BattleCity.AI.SchedulingEngine
{
    public class TimeBlockSequence<T>
        where T: class
    {
        private const int DEFAULT_BUFFER_CAPACITY = 10;

        private CircularBuffer<TimeBlock<T>> timeBlockBuffer;

        public Phase StartPhase { get; private set; }
        public Phase EndPhase { get; private set; }

        public T this[Phase phase]
        {
            get
            {
                TimeBlock<T> timeBlock = GetTimeBlockContainingPhase(phase);
                if (timeBlock == null)
                {
                    return null;
                }
                return timeBlock[phase];
            }
            set
            {
                TimeBlock<T> timeBlock = GetTimeBlockContainingPhase(phase);
                if (timeBlock == null)
                {
                    throw new InvalidOperationException(
                        String.Format(
                            "A new item cannot be inserted into the time sequence at {0} as the last phase ends at {1}",
                            phase, EndPhase));
                }
                timeBlock[phase] = value;
            }
        }

        public TimeBlockSequence(Phase startPhase, Phase endPhase, int bufferCapacity = DEFAULT_BUFFER_CAPACITY)
        {
            StartPhase = startPhase;
            EndPhase = endPhase;
            TimeBlock<T> initialTimeBlock = new TimeBlock<T>(startPhase, endPhase);

            timeBlockBuffer = new CircularBuffer<TimeBlock<T>>(bufferCapacity);
            timeBlockBuffer.Append(initialTimeBlock);
        }

        public void AppendTimeBlock(TimeBlock<T> timeBlockToAppend)
        {
            EndPhase = timeBlockToAppend.EndPhase;
            timeBlockBuffer.Append(timeBlockToAppend);
        }

        public void RemoveTimeBlocksBefore(Phase minEndPhase)
        {
            while (timeBlockBuffer.Size > 1)
            {
                TimeBlock<T> firstTimeBlock = timeBlockBuffer[0];
                if (firstTimeBlock.EndPhase < minEndPhase)
                {
                    timeBlockBuffer.RemoveFirst();
                }
            }
            if (timeBlockBuffer.Size > 0)
            {
                StartPhase = timeBlockBuffer[0].StartPhase;
            }
        }

        public TimeBlock<T> GetTimeBlockContainingPhase(Phase phase)
        {
            for (int i = 0; i < timeBlockBuffer.Size; i++)
            {
                TimeBlock<T> timeBlock = timeBlockBuffer[i];
                if (timeBlock.StartPhase <= phase && phase <= timeBlock.EndPhase)
                {
                    return timeBlock;
                }
            }
            return null;
        }
    }
}
