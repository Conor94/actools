﻿using System;
using System.Threading.Tasks;
using System.Windows;
using AcManager.Tools.About;
using AcTools.Utils.Helpers;

namespace AcManager.Pages.About {
    public partial class PieceOfInformationBlock {
        public PieceOfInformationBlock() {
            InitializeComponent();
            Root.DataContext = this;

            /* TODO */
            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow != null) {
                WeakEventManager<Window, EventArgs>.AddHandler(mainWindow, nameof(mainWindow.Activated), Handler);
            }
        }

        public static readonly DependencyProperty PieceProperty = DependencyProperty.Register(nameof(Piece), typeof(PieceOfInformation),
                typeof(PieceOfInformationBlock), new PropertyMetadata(OnPieceChanged));

        public PieceOfInformation Piece {
            get => (PieceOfInformation)GetValue(PieceProperty);
            set => SetValue(PieceProperty, value);
        }

        private static void OnPieceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ((PieceOfInformationBlock)o).OnPieceChanged((PieceOfInformation)e.NewValue);
        }

        private void OnPieceChanged(PieceOfInformation newValue) {
            MarkAsRead(newValue).Ignore();
        }

        private void Handler(object sender, EventArgs eventArgs) {
            MarkAsRead(Piece).Ignore();
        }

        private async Task MarkAsRead(PieceOfInformation value) {
            if (value == null) return;
            await Task.Delay(1000);
            if (value != Piece) return;
            if (Application.Current?.MainWindow?.IsActive == true) {
                value.MarkAsRead();
            }
        }
    }
}
