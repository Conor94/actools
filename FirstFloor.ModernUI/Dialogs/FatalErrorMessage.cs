﻿using System;
using System.Windows;
using System.Windows.Input;
using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Windows.Controls;

namespace FirstFloor.ModernUI.Dialogs {
    public class FatalErrorMessage : ModernDialog {
        public static string ErrorHeader { get; set; } = "ɿ(｡･ɜ･)ɾ";

        internal FatalErrorMessage() {
            DefaultStyleKey = typeof(FatalErrorMessage);
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string),
                typeof(FatalErrorMessage));

        public string Message {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty StackTraceProperty = DependencyProperty.Register(nameof(StackTrace), typeof(string),
                typeof(FatalErrorMessage));

        public string StackTrace {
            get => (string)GetValue(StackTraceProperty);
            set => SetValue(StackTraceProperty, value);
        }

        private ICommand _copyCommand;

        public ICommand CopyCommand => _copyCommand ?? (_copyCommand = new DelegateCommand(() => ClipboardHelper.SetText(StackTrace)));

        private ICommand _restartCommand;

        public ICommand RestartCommand => _restartCommand ?? (_restartCommand = new DelegateCommand(() => _restartHelper?.Restart()));

        private ICommand _exitCommand;

        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new DelegateCommand(() => {
            var app = Application.Current;
            if (app == null) {
                Environment.Exit(0);
            } else {
                app.Shutdown();
            }
        }));

        private static IAppRestartHelper _restartHelper;

        public static void Register(IAppRestartHelper helper) {
            _restartHelper = helper;
        }

        public interface IAppRestartHelper {
            void Restart();
        }
    }
}