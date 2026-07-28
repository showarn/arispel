using System;
using System.Collections.Generic;

namespace ArisMonsterTrucks.Fishing
{
    public enum FishingState
    {
        Idle,
        Casting,
        WaitingForBite,
        FishBiting,
        ReelingIn,
        CatchReveal,
        ReturningToIdle,
        Paused
    }

    public sealed class FishingStateMachine
    {
        private static readonly Dictionary<FishingState, FishingState[]> Allowed =
            new()
            {
                { FishingState.Idle, new[] { FishingState.Casting, FishingState.Paused } },
                { FishingState.Casting, new[] { FishingState.WaitingForBite, FishingState.Paused } },
                {
                    FishingState.WaitingForBite,
                    new[]
                    {
                        FishingState.FishBiting,
                        FishingState.ReturningToIdle,
                        FishingState.Paused
                    }
                },
                {
                    FishingState.FishBiting,
                    new[]
                    {
                        FishingState.ReelingIn,
                        FishingState.ReturningToIdle,
                        FishingState.Paused
                    }
                },
                { FishingState.ReelingIn, new[] { FishingState.CatchReveal, FishingState.Paused } },
                {
                    FishingState.CatchReveal,
                    new[] { FishingState.ReturningToIdle, FishingState.Paused }
                },
                { FishingState.ReturningToIdle, new[] { FishingState.Idle, FishingState.Paused } },
                { FishingState.Paused, Array.Empty<FishingState>() }
            };

        private FishingState stateBeforePause = FishingState.Idle;

        public FishingState Current { get; private set; } = FishingState.Idle;

        public event Action<FishingState, FishingState> Changed;

        public bool CanTransitionTo(FishingState next)
        {
            if (Current == FishingState.Paused)
            {
                return next == stateBeforePause;
            }

            FishingState[] validStates = Allowed[Current];
            for (int index = 0; index < validStates.Length; index++)
            {
                if (validStates[index] == next)
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryTransition(FishingState next)
        {
            if (!CanTransitionTo(next))
            {
                return false;
            }

            FishingState previous = Current;
            Current = next;
            Changed?.Invoke(previous, next);
            return true;
        }

        public bool Pause()
        {
            if (Current == FishingState.Paused)
            {
                return false;
            }

            stateBeforePause = Current;
            FishingState previous = Current;
            Current = FishingState.Paused;
            Changed?.Invoke(previous, Current);
            return true;
        }

        public bool Resume()
        {
            if (Current != FishingState.Paused)
            {
                return false;
            }

            FishingState previous = Current;
            Current = stateBeforePause;
            Changed?.Invoke(previous, Current);
            return true;
        }

        public void Reset()
        {
            FishingState previous = Current;
            Current = FishingState.Idle;
            stateBeforePause = FishingState.Idle;
            if (previous != Current)
            {
                Changed?.Invoke(previous, Current);
            }
        }
    }
}
