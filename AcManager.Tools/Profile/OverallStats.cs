using System;
using System.Linq;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using StringBasedFilter;

namespace AcManager.Tools.Profile {
    public partial class PlayerStatsManager {
        /// <summary>
        /// Velocity by Y axis.
        /// </summary>
        public static float OptionTyreWearScale = 10f;

        public class FilteredStats : OverallStats {
            private static IFilter<SessionStats> _filter;

            public FilteredStats(string filter) : base(new Storage()) {
                _filter = Filter.Create(new SessionStatsTester(), filter);
            }

            public override void Extend(SessionStats session) {
                if (_filter.Test(session)) {
                    base.Extend(session);
                }
            }
        }

        public class OverallStats : NotifyPropertyChanged {
            private double _maxDistancePerCar;
            private string _maxDistancePerCarCarId;
            private double _maxDistancePerTrack;
            private string _maxDistancePerTrackTrackId;
            private double _maxSpeed;
            private string _maxSpeedCarId;
            private double _longestAirborne;
            private string _longestAirborneCarId;
            private double _longestWheelie;
            private string _longestWheelieCarId;
            private double _longestTwoWheels;
            private string _longestTwoWheelsCarId;
            private double _distance;
            private double _fuelBurnt;
            private TimeSpan _time;
            private int _goneOffroadTimes;
            private double _totalAirborne;
            private double _totalWheelie;
            private double _totalTwoWheels;

            /// <summary>
            /// Don’t forget to set Storage later if needed.
            /// </summary>
            [JsonConstructor]
            public OverallStats() { }

            public OverallStats(IStorage storage) {
                Storage = storage;
            }

            [JsonIgnore]
            public IStorage Storage { get; set; }

            [JsonProperty]
            public int Version => 2;

            /* extremums */
            [JsonProperty]
            public double MaxDistancePerCar {
                get => _maxDistancePerCar;
                internal set {
                    if (value.Equals(_maxDistancePerCar)) return;
                    _maxDistancePerCar = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MaxDistancePerCarKm));
                    OnPropertyChanged(nameof(MaxDistancePerCarSame));
                }
            }

            [JsonProperty]
            public string MaxDistancePerCarCarId {
                get => _maxDistancePerCarCarId;
                internal set => Apply(value, ref _maxDistancePerCarCarId);
            }

            [JsonProperty]
            public double MaxDistancePerTrack {
                get => _maxDistancePerTrack;
                internal set {
                    if (value.Equals(_maxDistancePerTrack)) return;
                    _maxDistancePerTrack = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MaxDistancePerTrackKm));
                    OnPropertyChanged(nameof(MaxDistancePerTrackSame));
                }
            }

            [JsonProperty]
            public string MaxDistancePerTrackTrackId {
                get => _maxDistancePerTrackTrackId;
                internal set => Apply(value, ref _maxDistancePerTrackTrackId);
            }

            [JsonProperty]
            public double MaxSpeed {
                get => _maxSpeed;
                internal set {
                    if (value.Equals(_maxSpeed)) return;
                    _maxSpeed = value;
                    OnPropertyChanged();
                }
            }

            [JsonProperty]
            public string MaxSpeedCarId {
                get => _maxSpeedCarId;
                internal set => Apply(value, ref _maxSpeedCarId);
            }

            [JsonProperty]
            public double LongestAirborne {
                get => _longestAirborne;
                internal set {
                    if (value.Equals(_longestAirborne)) return;
                    _longestAirborne = value;
                    OnPropertyChanged();
                }
            }

            [JsonProperty]
            public string LongestAirborneCarId {
                get => _longestAirborneCarId;
                internal set => Apply(value, ref _longestAirborneCarId);
            }

            [JsonProperty]
            public double LongestWheelie {
                get => _longestWheelie;
                internal set {
                    if (value.Equals(_longestWheelie)) return;
                    _longestWheelie = value;
                    OnPropertyChanged();
                }
            }

            [JsonProperty]
            public string LongestWheelieCarId {
                get => _longestWheelieCarId;
                internal set => Apply(value, ref _longestWheelieCarId);
            }

            [JsonProperty]
            public double LongestTwoWheels {
                get => _longestTwoWheels;
                internal set {
                    if (value.Equals(_longestTwoWheels)) return;
                    _longestTwoWheels = value;
                    OnPropertyChanged();
                }
            }

            [JsonProperty]
            public string LongestTwoWheelsCarId {
                get => _longestTwoWheelsCarId;
                internal set => Apply(value, ref _longestTwoWheelsCarId);
            }

            /* summary */
            /// <summary>
            /// Meters.
            /// </summary>
            [JsonProperty]
            public double Distance {
                get => _distance;
                internal set {
                    if (value.Equals(_distance)) return;
                    _distance = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AverageSpeed));
                    OnPropertyChanged(nameof(FuelConsumption));
                    OnPropertyChanged(nameof(DistanceKm));
                    OnPropertyChanged(nameof(MaxDistancePerCarSame));
                    OnPropertyChanged(nameof(MaxDistancePerTrackSame));
                }
            }

            /// <summary>
            /// Liters.
            /// </summary>
            [JsonProperty]
            public double FuelBurnt {
                get => _fuelBurnt;
                internal set {
                    if (value.Equals(_fuelBurnt)) return;
                    _fuelBurnt = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FuelConsumption));
                }
            }

            [JsonProperty]
            public TimeSpan Time {
                get => _time;
                internal set {
                    if (value.Equals(_time)) return;
                    _time = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AverageSpeed));
                }
            }

            private int _sessionsCount;

            [JsonProperty]
            public int SessionsCount {
                get => _sessionsCount;
                set => Apply(value, ref _sessionsCount);
            }

            [JsonProperty]
            public int GoneOffroadTimes {
                get => _goneOffroadTimes;
                internal set => Apply(value, ref _goneOffroadTimes);
            }

            [JsonProperty]
            public double TotalAirborne {
                get => _totalAirborne;
                internal set {
                    if (value.Equals(_totalAirborne)) return;
                    _totalAirborne = value;
                    OnPropertyChanged();
                }
            }

            [JsonProperty]
            public double TotalWheelie {
                get => _totalWheelie;
                internal set {
                    if (value.Equals(_totalWheelie)) return;
                    _totalWheelie = value;
                    OnPropertyChanged();
                }
            }

            [JsonProperty]
            public double TotalTwoWheels {
                get => _totalTwoWheels;
                internal set {
                    if (value.Equals(_totalTwoWheels)) return;
                    _totalTwoWheels = value;
                    OnPropertyChanged();
                }
            }

            private double _totalTyreWear;

            [JsonProperty]
            public double TotalTyreWear {
                get => _totalTyreWear;
                set {
                    if (Equals(value, _totalTyreWear)) return;
                    _totalTyreWear = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalTyreWearRounded));
                }
            }

            private int _totalCrashes;

            [JsonProperty]
            public int TotalCrashes {
                get => _totalCrashes;
                set => Apply(value, ref _totalCrashes);
            }

            /* calculated on-fly */
            [JsonIgnore]
            public double DistanceKm => Distance / 1e3;

            [JsonIgnore]
            public bool MaxDistancePerCarSame => Math.Abs(Distance - MaxDistancePerCar) < 0.1;

            [JsonIgnore]
            public bool MaxDistancePerTrackSame => Math.Abs(Distance - MaxDistancePerTrack) < 0.1;

            [JsonIgnore]
            public double MaxDistancePerCarKm => MaxDistancePerCar / 1e3;

            [JsonIgnore]
            public double MaxDistancePerTrackKm => MaxDistancePerTrack / 1e3;

            [JsonIgnore]
            public double AverageSpeed => Equals(Time.TotalHours, 0d) ? 0d : DistanceKm / Time.TotalHours;

            [JsonIgnore]
            public double FuelConsumption => Equals(DistanceKm, 0d) ? 0d : 1e2 * FuelBurnt / DistanceKm;

            [JsonIgnore]
            public double TotalTyreWearRounded => Math.Floor(TotalTyreWear * OptionTyreWearScale);

            private void UpdateMaxDistancePerCar([NotNull] SessionStats session) {
                var drivenDistance = Storage.Get(KeyDistancePerCarPrefix + session.CarId, 0d) + session.Distance;
                Storage.Set(KeyDistancePerCarPrefix + session.CarId, drivenDistance);

                if (session.CarId == MaxDistancePerCarCarId) {
                    MaxDistancePerCar += session.Distance;
                } else if (drivenDistance > MaxDistancePerCar) {
                    MaxDistancePerCar = drivenDistance;
                    MaxDistancePerCarCarId = session.CarId;
                }
            }

            private void UpdateMaxDistancePerTrack([NotNull] SessionStats session) {
                var drivenDistance = Storage.Get(KeyDistancePerTrackPrefix + session.TrackId, 0d) + session.Distance;
                Storage.Set(KeyDistancePerTrackPrefix + session.TrackId, drivenDistance);

                if (session.TrackId == MaxDistancePerTrackTrackId) {
                    MaxDistancePerTrack += session.Distance;
                } else if (drivenDistance > MaxDistancePerTrack) {
                    MaxDistancePerTrack = drivenDistance;
                    MaxDistancePerTrackTrackId = session.TrackId;
                }
            }

            public virtual void Extend([NotNull] SessionStats session) {
                /* per car/track */
                if (Storage == null) {
                    Storage = new Storage();
                }

                if (double.IsNaN(session.Distance)) {
                    Logging.Warning("Session distance is not a number! Ignoring it");
                    return;
                }

                UpdateMaxDistancePerCar(session);
                UpdateMaxDistancePerTrack(session);

                /* max speed per car */
                if (session.MaxSpeed > Storage.Get(KeyMaxSpeedPerCarPrefix + session.CarId, 0d)) {
                    Storage.Set(KeyMaxSpeedPerCarPrefix + session.CarId, session.MaxSpeed);
                }

                /* max speed per track */
                if (session.MaxSpeed > Storage.Get(KeyMaxSpeedPerTrackPrefix + session.TrackId, 0d)) {
                    Storage.Set(KeyMaxSpeedPerTrackPrefix + session.TrackId, session.MaxSpeed);
                }

                /* extremums */
                if (session.MaxSpeed > MaxSpeed) {
                    MaxSpeed = session.MaxSpeed;
                    MaxSpeedCarId = session.CarId;
                }

                if (session.LongestAirborne > LongestAirborne) {
                    LongestAirborne = session.LongestAirborne;
                    LongestAirborneCarId = session.CarId;
                }

                if (session.LongestWheelie > LongestWheelie) {
                    LongestWheelie = session.LongestWheelie;
                    LongestWheelieCarId = session.CarId;
                }

                if (session.LongestTwoWheels > LongestTwoWheels) {
                    LongestTwoWheels = session.LongestTwoWheels;
                    LongestTwoWheelsCarId = session.CarId;
                }

                /* summary */
                Distance += session.Distance;
                FuelBurnt += session.FuelBurnt;
                Time += session.Time;
                GoneOffroadTimes += session.GoneOffroad;
                TotalAirborne += session.TotalAirborne;
                TotalWheelie += session.TotalWheelie;
                TotalTwoWheels += session.TotalTwoWheels;
                TotalTyreWear += session.TotalTyreWear;
                TotalCrashes += session.TotalCrashes;
                SessionsCount++;
            }

            public void CopyFrom(OverallStats updated) {
                Storage = updated.Storage;
                foreach (var p in typeof(OverallStats).GetProperties().Where(p => p.CanWrite)) {
                    p.SetValue(this, p.GetValue(updated, null), null);
                }
            }

            public void Reset() {
                Distance = 0;
                MaxDistancePerCar = 0;
                MaxDistancePerCarCarId = null;
                MaxDistancePerTrack = 0;
                MaxDistancePerTrackTrackId = null;
                MaxSpeed = 0;
                MaxSpeedCarId = null;
                LongestAirborne = 0;
                LongestAirborneCarId = null;
                LongestWheelie = 0;
                LongestWheelieCarId = null;
                LongestTwoWheels = 0;
                LongestTwoWheelsCarId = null;
                FuelBurnt = 0;
                Time = TimeSpan.Zero;
                GoneOffroadTimes = 0;
                TotalAirborne = 0;
                TotalWheelie = 0;
                TotalTwoWheels = 0;
                TotalTyreWear = 0;
                TotalCrashes = 0;
                SessionsCount = 0;
                Storage.Clear();
            }
        }
    }
}