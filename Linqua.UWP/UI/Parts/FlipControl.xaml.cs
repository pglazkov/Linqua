﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Framework;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linqua.UI
{
    public sealed partial class FlipControl : UserControl
    {
        public FlipControl()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsFlipped)
            {
                // Always show the front side first so that user is not confused by what he/she sees on the back side after some
                // time has passed since he/she flipped the card.
                SetValue(IsFlippedProperty, false);
            }
        }

        #region FrontSideContent DP

        public object FrontSideContent
        {
            get { return (object)GetValue(FrontSideContentProperty); }
            set { SetValue(FrontSideContentProperty, value); }
        }

        public static readonly DependencyProperty FrontSideContentProperty =
            DependencyProperty.Register("FrontSideContent", typeof(object), typeof(FlipControl), new PropertyMetadata(null));

        #endregion

        #region BackSideContent DP

        public object BackSideContent
        {
            get { return (object)GetValue(BackSideContentProperty); }
            set { SetValue(BackSideContentProperty, value); }
        }

        public static readonly DependencyProperty BackSideContentProperty =
            DependencyProperty.Register("BackSideContent", typeof(object), typeof(FlipControl), new PropertyMetadata(null));

        #endregion

        #region FrontSideContentTemplate DP

        public DataTemplate FrontSideContentTemplate
        {
            get { return (DataTemplate)GetValue(FrontSideContentTemplateProperty); }
            set { SetValue(FrontSideContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty FrontSideContentTemplateProperty =
            DependencyProperty.Register("FrontSideContentTemplate", typeof(DataTemplate), typeof(FlipControl), new PropertyMetadata(null));

        #endregion

        #region BackSideContentTemplate DP

        public DataTemplate BackSideContentTemplate
        {
            get { return (DataTemplate)GetValue(BackSideContentTemplateProperty); }
            set { SetValue(BackSideContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty BackSideContentTemplateProperty =
            DependencyProperty.Register("BackSideContentTemplate", typeof(DataTemplate), typeof(FlipControl), new PropertyMetadata(null));

        #endregion

        #region IsFlipped DP

        public bool IsFlipped
        {
            get { return (bool)GetValue(IsFlippedProperty); }
            set { SetValue(IsFlippedProperty, value); }
        }

        public static readonly DependencyProperty IsFlippedProperty =
            DependencyProperty.Register("IsFlipped", typeof(bool), typeof(FlipControl), new PropertyMetadata(false, OnIsFlippedChanged));

        private static void OnIsFlippedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var this_ = (FlipControl)d;
            this_.OnIsFlippedChanged(e);
        }

        private void OnIsFlippedChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isFlipped = (bool)e.NewValue;

            SetIsFlipped(isFlipped);
        }

        #endregion

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            VisualStateManager.GoToState(this, "InitialState", false);
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var isButton = e.OriginalSource is ButtonBase ||
                           (e.OriginalSource is FrameworkElement && (e.OriginalSource as FrameworkElement).GetFirstAncestorOfType<ButtonBase>() != null);

            if (isButton)
            {
                return;
            }

            SetValue(IsFlippedProperty, !IsFlipped);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerPressedState", true);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerReleasedState", true);
        }

        private void SetIsFlipped(bool isFlipped)
        {
            if (isFlipped)
            {
                VisualStateManager.GoToState(this, "FlipCardFrontState", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "FlipCardBackState", true);
            }
        }
    }
}