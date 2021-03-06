﻿using System;
using System.Windows.Input;
using AiForms.Effects;
using AiForms.Effects.Droid;
using Android.App;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using APicker = Android.Widget.NumberPicker;

[assembly: ExportEffect(typeof(AddNumberPickerPlatform), nameof(AddNumberPicker))]
namespace AiForms.Effects.Droid
{
    public class AddNumberPickerPlatform : PlatformEffect
    {
        private AlertDialog _dialog;
        private Android.Views.View _view;
        private ICommand _command;
        private int _min;
        private int _max;
        private int _number;

        protected override void OnAttached()
        {
            _view = Control ?? Container;

            _view.Click += _view_Click;

            UpdateList();
            UpdateCommand();
        }

        void _view_Click(object sender, EventArgs e)
        {
            CreateDialog();
        }

        protected override void OnDetached()
        {
            var renderer = Container as IVisualElementRenderer;
            if (renderer?.Element != null) {
                _view.Click -= _view_Click;
            }
            if (_dialog != null) {
                _dialog.Dispose();
                _dialog = null;
            }
            _view = null;
            _command = null;
        }

        protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(e);

            if (e.PropertyName == AddNumberPicker.MaxProperty.PropertyName) {
                UpdateList();
            }
            else if (e.PropertyName == AddNumberPicker.MinProperty.PropertyName) {
                UpdateList();
            }
            else if (e.PropertyName == AddNumberPicker.NumberProperty.PropertyName) {
                UpdateNumber();
            }
            else if (e.PropertyName == AddNumberPicker.CommandProperty.PropertyName) {
                UpdateCommand();
            }
        }

        void CreateDialog()
        {
            if (_dialog != null) return;

            var picker = new APicker(Container.Context);
            picker.MinValue = _min;
            picker.MaxValue = _max;
            picker.Value = _number;

            using (var builder = new AlertDialog.Builder(Container.Context)) {

                builder.SetTitle(AddNumberPicker.GetTitle(Element));

                Android.Widget.FrameLayout parent = new Android.Widget.FrameLayout(Container.Context);
                parent.AddView(picker, new Android.Widget.FrameLayout.LayoutParams(
                        ViewGroup.LayoutParams.WrapContent,
                        ViewGroup.LayoutParams.WrapContent,
                       GravityFlags.Center));
                builder.SetView(parent);

                builder.SetNegativeButton(global::Android.Resource.String.Cancel, (o, args) => { });

                builder.SetPositiveButton(global::Android.Resource.String.Ok, (o, args) => {
                    AddNumberPicker.SetNumber(Element, picker.Value);
                    _command?.Execute(picker.Value);
                });

                _dialog = builder.Create();
            }

            _dialog.SetCanceledOnTouchOutside(true);

            _dialog.DismissEvent += (ss, ee) => {
                _dialog.Dispose();
                _dialog = null;
                picker.RemoveFromParent();
                picker.Dispose();
                picker = null;
            };

            _dialog.Show();
        }

        void UpdateList()
        {
            _min = AddNumberPicker.GetMin(Element);
            _max = AddNumberPicker.GetMax(Element);
            if (_min > _max) {
                throw new ArgumentOutOfRangeException(
                    AddNumberPicker.MaxProperty.PropertyName, "Min must not be larger than Max");
            }
            if (_min < 0) _min = 0;
            if (_max < 0) _max = 0;

            UpdateNumber();
        }
        void UpdateNumber()
        {
            _number = AddNumberPicker.GetNumber(Element);
            if (_number < _min || _number > _max) {
                _number = _min;
            }
        }

        void UpdateCommand()
        {
            _command = AddNumberPicker.GetCommand(Element);
        }
    }
}
