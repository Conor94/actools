﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AcManager.Tools.AcObjectsNew;

namespace AcManager.Controls {
    public class AcObjectHeaderSection : Control {
        static AcObjectHeaderSection() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AcObjectHeaderSection), new FrameworkPropertyMetadata(typeof(AcObjectHeaderSection)));
        }

        public static readonly DependencyProperty AcObjectProperty = DependencyProperty.Register(nameof(AcObject), typeof(AcCommonObject),
                typeof(AcObjectHeaderSection));

        public AcCommonObject AcObject {
            get { return (AcCommonObject)GetValue(AcObjectProperty); }
            set { SetValue(AcObjectProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(nameof(ShowIcon), typeof(bool),
                typeof(AcObjectHeaderSection));

        public bool ShowIcon {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(BitmapSource),
                typeof(AcObjectHeaderSection));

        public BitmapSource Icon {
            get { return (BitmapSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private UIElement _iconImage;

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_iconImage != null) {
                _iconImage.MouseDown -= IconImage_MouseDown;
            }

            _iconImage = GetTemplateChild("PART_IconImage") as UIElement;

            if (_iconImage != null) {
                _iconImage.MouseDown += IconImage_MouseDown;
            }
        }

        private void IconImage_MouseDown(object sender, MouseButtonEventArgs e) {
            IconMouseDown?.Invoke(sender, e);
        }

        public event MouseButtonEventHandler IconMouseDown;
    }
}
