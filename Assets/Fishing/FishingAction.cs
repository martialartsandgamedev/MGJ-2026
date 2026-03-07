using System;
using System.Collections.Generic;
using System.Linq;

namespace Controllers
{
    [Serializable]
    public record FishingAction
    {
        public enum AttemptState
        {
            Upcoming,
            Missed,
            Failed,
            Success
        }

        public int Index { get; set; }
        public AttemptState Attempt { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }

        public static List<FishingAction> Create(FishingSpotContext context)
        {
            return Enumerable.Range(0, context.ActionCount).Select(index =>
            {
                var targetTime = context.Duration / (context.ActionCount + 1) * (index + 1);
                var startTime = targetTime - context.Buffer;
                var endTime = targetTime + context.Buffer;
                var normalisedStartTime = startTime / context.Duration;
                var normalisedEndTime = endTime / context.Duration;

                return new FishingAction
                {
                    Index = index,
                    StartTime = normalisedStartTime,
                    EndTime = normalisedEndTime,
                    Attempt = AttemptState.Upcoming,
                };
            }).ToList();
        }
    }
}
