﻿using Avalonia.Controls.Platform;
using Avalonia.Platform;
using Avalonia.X11.Interop;
using Gtk;
using System;
using ViewModels;

namespace LinuxControl
{
    public class LinuxView : Window
    {
        private const string vmKey = "VM";
        ClickCounterViewModel _vm = new ClickCounterViewModel();
        Gtk.Label _label = new Gtk.Label();

        public IntPtr Xid { get; }

        Gtk.Button _button;

        public IntPtr ButtonHandle => _button.Handle;

        public LinuxView() : base(WindowType.Toplevel) 
        {
            Gtk.Grid grid = new Gtk.Grid();



            grid.Visible = true;
            this.Add(grid);

            VBox box = new VBox()
            {
                Halign = Align.Center,
                Valign = Align.Center
            };

            box.Visible = true;

            grid.Add(box);

            _vm.PropertyChanged += _vm_PropertyChanged;

            _label.Data[vmKey] = _vm;
            _label.Text = _vm.NumberClicksStr;
            _label.Hexpand = true;
            _label.Halign = Align.Center;
            _label.Vexpand = true;
            _label.Valign = Align.Center;
            _label.Visible = true;
            _label.Margin = 20;

            box.Add(_label);

            _button = new Gtk.Button("ClickMe!");
            _button.Hexpand = true;
            _button.Halign = Align.Center;
            _button.Vexpand = true;
            _button.Valign = Align.Center;
            _button.Visible = true;

            box.Add(_button);

            _button.Clicked += Button_Clicked;

            this.Show();

            Xid = GtkApi.gdk_x11_window_get_xid(_button.Window.Handle);
        }

        private void _vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _label.Text = _vm.NumberClicksStr;
        }

        private void Button_Clicked(object? sender, EventArgs e)
        {
            _vm.IncreaseNumberClicks();
        }

        class ControlWrapper : INativeControlHostDestroyableControlHandle
        {
            private readonly IntPtr _windowHandle;

            public IntPtr Handle { get; }

            public ControlWrapper(LinuxView linuxView)
            {
                _windowHandle = linuxView.Handle;
                Handle = linuxView.Xid;
            }

            public string? HandleDescriptor => "XID";

            public void Destroy()
            {
                GtkInteropHelper.RunOnGlibThread(() =>
                {
                    GtkApi.gtk_widget_destroy(_windowHandle);
                    return 0;
                }).Wait();
            }
        }

        public static IPlatformHandle Create()
        {
            return GtkInteropHelper.RunOnGlibThread(() =>
            {
                LinuxView linuxView = new LinuxView();

                return new ControlWrapper(linuxView);
            }).Result;
        }
    }
}
