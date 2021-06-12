using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AcManager.Tools.AcErrors;
using AcManager.Tools.AcManagersNew;
using AcManager.Tools.Data.GameSpecific;
using AcManager.Tools.Helpers;
using AcManager.Tools.Managers;
using AcManager.Tools.SemiGui;
using AcTools.DataFile;
using AcTools.Processes;
using AcTools.Utils.Helpers;
using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using JetBrains.Annotations;

namespace AcManager.Tools.Objects {
    public class SpecialEventObject : KunosEventObjectBase {
        public sealed class AiLevelEntry : Displayable, IWithId<int> {
            private static readonly string[] DisplayNames = {
                ToolsStrings.DifficultyLevel_Easy, ToolsStrings.DifficultyLevel_Medium,
                ToolsStrings.DifficultyLevel_Hard, ToolsStrings.DifficultyLevel_Alien
            };

            public AiLevelEntry(int index, int aiLevel) {
                Place = 4 - index;
                AiLevel = aiLevel;
                DisplayName = DisplayNames[index];
            }

            public int Place { get; }

            public int AiLevel { get; }

            private double _placeStat;

            public double PlaceStat {
                get => _placeStat;
                set => Apply(value, ref _placeStat);
            }

            public int Id => AiLevel;
        }

        private string _guid;

        [CanBeNull]
        public string Guid {
            get => _guid;
            set => Apply(value, ref _guid);
        }

        public SpecialEventObject(IFileAcManager manager, string id, bool enabled)
                : base(manager, id, enabled) { }

        private AiLevelEntry[] _aiLevels;

        public AiLevelEntry[] AiLevels {
            get => _aiLevels;
            set => Apply(value, ref _aiLevels);
        }

        private AiLevelEntry _selectedLevel;

        public AiLevelEntry SelectedLevel {
            get => _selectedLevel;
            set {
                if (Equals(value, _selectedLevel)) return;
                _selectedLevel = value;
                OnPropertyChanged();
                SpecialEventsManager.ProgressStorage.Set(KeySelectedLevel, value.AiLevel);
                AiLevel = value.AiLevel;
            }
        }

        private string KeyTakenPlace => $@"{Id}:TakenPlace";

        private string KeySelectedLevel => $@"{Id}:SelectedLevel";

        private string _displayDescription;

        public string DisplayDescription {
            get => _displayDescription;
            set => Apply(value, ref _displayDescription);
        }

        private void UpdateDescription() {
            DisplayDescription = new [] {
                string.Format(ToolsStrings.SpecialEvent_Description, CarObject?.DisplayName ?? CarId, TrackObject?.Name ?? TrackId),
                DisplayPlaceStats
            }.NonNull().JoinToString(@" ");
        }

        protected override void LoadObjects() {
            base.LoadObjects();
            UpdateDescription();
        }

        protected override void LoadData(IniFile ini) {
            base.LoadData(ini);
            Guid = ini["SPECIAL_EVENT"].GetNonEmpty("GUID");
        }

        protected override void LoadConditions(IniFile ini) {
            if (string.Equals(ini["CONDITION_0"].GetNonEmpty("TYPE"), @"AI", StringComparison.OrdinalIgnoreCase)) {
                var aiLevels = ini.GetSections("CONDITION").Take(4).Select((x, i) => new AiLevelEntry(i, x.GetInt("OBJECTIVE", 100))).Reverse().ToArray();
                if (aiLevels.Length != 4) {
                    AddError(AcErrorType.Data_KunosCareerConditions, $"NOT {aiLevels.Length}");
                    AiLevels = null;
                } else {
                    RemoveError(AcErrorType.Data_KunosCareerConditions);
                    AiLevels = aiLevels;
                }

                ConditionType = null;
                FirstPlaceTarget = SecondPlaceTarget = ThirdPlaceTarget = null;
            } else {
                AiLevels = null;
                base.LoadConditions(ini);
            }
        }

        protected override void TakenPlaceChanged() {
            SpecialEventsManager.ProgressStorage.Set(KeyTakenPlace, TakenPlace);
        }

        public override void LoadProgress() {
            TakenPlace = SpecialEventsManager.ProgressStorage.Get(KeyTakenPlace, 5);
            if (AiLevels != null) {
                SelectedLevel = AiLevels.GetByIdOrDefault(SpecialEventsManager.ProgressStorage.Get(KeySelectedLevel, 0)) ??
                        AiLevels.ArrayElementAtOrDefault(1);
            }
        }

        protected override IniFile ConvertConfig(IniFile ini) {
            ini = base.ConvertConfig(ini);

            if (SelectedLevel != null) {
                ini["RACE"].Set("AI_LEVEL", SelectedLevel.AiLevel);
            }

            foreach (var section in ini.GetSections("CONDITION")) {
                section.Set("TYPE", section.GetPossiblyEmpty("TYPE")?.ToLowerInvariant());
                section.Set("ACHIEVED", false);
            }

            return ini;
        }

        private double[] _placeStats;

        [CanBeNull]
        public double[] PlaceStats {
            get => _placeStats;
            set => Apply(value, ref _placeStats, () => {
                if (value?.Length == 4) {
                    for (var i = 0; i < 4; ++i) {
                        AiLevels[i].PlaceStat = value[3 - i];
                    }
                }

                OnPropertyChanged(nameof(DisplayPlaceStats));
                OnPropertyChanged(nameof(DisplayFirstPlaceStat));
                OnPropertyChanged(nameof(DisplaySecondPlaceStat));
                OnPropertyChanged(nameof(DisplayThirdPlaceStat));
                UpdateDescription();
            });
        }

        [CanBeNull]
        public string DisplayPlaceStats {
            get {
                if (PlaceStats != null) {
                    var postfix = TakenPlace == 1 ? "Congrats!" : TakenPlace == 2 ? "You’re almost there!" : "Good luck!";
                    if (PlaceStats.Length == 3) {
                        return $"Only {PlaceStats[2]:F1}% of all players got first place. {postfix}";
                    }
                    if (PlaceStats.Length == 4) {
                        return $"Only {PlaceStats[3]:F1}% of all players won this event at highest difficulty. {postfix}";
                    }
                }
                return null;
            }
        }

        public string DisplayFirstPlaceStat => PlaceStats?.Length == 3 ? $"{PlaceStats[2]:F1}% of all AC players got this place" : null;
        public string DisplaySecondPlaceStat => PlaceStats?.Length == 3 ? $"{PlaceStats[1]:F1}% of all AC players got this place" : null;
        public string DisplayThirdPlaceStat => PlaceStats?.Length == 3 ? $"{PlaceStats[0]:F1}% of all AC players got this place" : null;

        private static bool ShowStarterDoesNotFitMessage() {
            var dlg = new ModernDialog {
                Title = ToolsStrings.Common_Warning,
                Content = new ScrollViewer {
                    Content = new SelectableBbCodeBlock {
                        Text = string.Format(ToolsStrings.SpecialEvent_StarterWarning, SettingsHolder.Drive.SelectedStarterType.DisplayName),
                        Margin = new Thickness(0, 0, 0, 8)
                    },
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                },
                MinHeight = 0,
                MinWidth = 0,
                MaxHeight = 480,
                MaxWidth = 640
            };

            dlg.Buttons = new[] {
                dlg.YesButton,
                dlg.CreateCloseDialogButton(ToolsStrings.SpecialEvent_StarterWarning_Solution, false, false, MessageBoxResult.OK),
                dlg.NoButton
            };

            dlg.ShowDialog();

            switch (dlg.MessageBoxResult) {
                case MessageBoxResult.Yes:
                    return true;
                case MessageBoxResult.OK:
                    SettingsHolder.Drive.SelectedStarterType = SettingsHolder.Drive.StarterTypes.First();
                    return true;
                case MessageBoxResult.None:
                case MessageBoxResult.Cancel:
                case MessageBoxResult.No:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AsyncCommand<Game.AssistsProperties> _goCommand;

        // TODO: async command
        public AsyncCommand<Game.AssistsProperties> GoCommand => _goCommand ?? (_goCommand = new AsyncCommand<Game.AssistsProperties>(async o => {
            if (SettingsHolder.Drive.SelectedStarterType.Id == @"SSE" && !ShowStarterDoesNotFitMessage()) {
                return;
            }

            await GameWrapper.StartAsync(new Game.StartProperties {
                AdditionalPropertieses = {
                    ConditionType.HasValue ? new PlaceConditions {
                        Type = ConditionType.Value,
                        FirstPlaceTarget = FirstPlaceTarget,
                        SecondPlaceTarget = SecondPlaceTarget,
                        ThirdPlaceTarget = ThirdPlaceTarget
                    } : null,
                    new SpecialEventsManager.EventProperties { EventId = Id }
                },
                PreparedConfig = ConvertConfig(new IniFile(IniFilename)),
                AssistsProperties = o
            });
        }));
    }
}