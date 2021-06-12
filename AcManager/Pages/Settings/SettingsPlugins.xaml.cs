﻿using System.ComponentModel;
using AcManager.Tools.Managers.Plugins;
using AcTools.Utils.Helpers;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;

namespace AcManager.Pages.Settings {
    public partial class SettingsPlugins {
        public SettingsPlugins() {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        public class ViewModel : NotifyPropertyChanged {
            public ViewModel() {
                PluginsManager.Instance.UpdateIfObsolete().Ignore();

                View = new BetterListCollectionView(PluginsManager.Instance.List);
                View.SortDescriptions.Add(new SortDescription(nameof(PluginEntry.Name), ListSortDirection.Ascending));
            }

            public BetterListCollectionView View { get; }
        }
    }
}
