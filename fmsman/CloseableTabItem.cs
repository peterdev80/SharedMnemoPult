using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using fmsman.Formats;

namespace fmsman
{
    public class CloseableTabItem : TabItem
    {
        static CloseableTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseableTabItem), new FrameworkPropertyMetadata(typeof(CloseableTabItem)));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly RoutedEvent CloseTabEvent =
            EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(CloseableTabItem));

        public event RoutedEventHandler CloseTab
        {
            add { AddHandler(CloseTabEvent, value); }
            remove { RemoveHandler(CloseTabEvent, value); }
        }

        public CloseableTabItem()
        {
            DragEnter += CloseableTabItem_DragEnter;
            Drop += CloseableTabItem_Drop;
            AllowDrop = true;
        }

        public static readonly DependencyProperty PinnedProperty = DependencyProperty.Register("Pinned", typeof(Boolean), typeof(CloseableTabItem));

        public bool Pinned 
        {
            get { return (bool)GetValue(PinnedProperty); }
            set { SetValue(PinnedProperty, value); }
        }

        void CloseableTabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("VarEntry"))
            {
                var vm = Tag as VariablesMap;

                if (vm != null)
                {
                    if (vm.IsCustom)
                    {
                        vm.VariableDragged(e.Data.GetData("VarEntry") as VarEntry);

                        IsSelected = true;
                    }
                }
            }

        }

        void CloseableTabItem_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("VarEntry") || sender == e.Source)
                e.Effects = DragDropEffects.None;
            else
                e.Effects = DragDropEffects.Copy;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var closeButton = GetTemplateChild("PART_Close") as Button;
            if (closeButton != null)
                closeButton.Click += closeButton_Click;

            var pin = GetTemplateChild("PART_pin") as Button;

            Debug.Assert(pin != null, "pin != null");

            pin.Click += (s, e) => Pinned = !Pinned;
        }

        void closeButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
        }
    }

}
