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

        public static List<FishingAction> Create(FishingSettings settings)
        {
            return Enumerable.Range(0, settings.ActionCount).Select(index =>
            {
                var targetTime = settings.Duration / (settings.ActionCount + 1) * (index + 1);
                var startTime = targetTime - settings.Buffer;
                var endTime = targetTime + settings.Buffer;
                var normalisedStartTime = startTime / settings.Duration;
                var normalisedEndTime = endTime / settings.Duration;

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
