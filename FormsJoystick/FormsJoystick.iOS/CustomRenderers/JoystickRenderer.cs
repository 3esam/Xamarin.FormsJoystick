using FormsJoystick.CustomControls;
using FormsJoystick.iOS.CustomRenderers;
using FormsJoystick.iOS.JoystickiOSCustomControl;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(JoystickControl), typeof(JoystickRenderer))]
namespace FormsJoystick.iOS.CustomRenderers
{
    class JoystickRenderer : ViewRenderer<JoystickControl, JoystickUIView>
    {
        private JoystickUIView _joystickUIView;

        protected override void OnElementChanged(ElementChangedEventArgs<JoystickControl> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                // Instantiate the native control and assign it to the Control property with
                // the SetNativeControl method
                _joystickUIView = new JoystickUIView();
                SetNativeControl(_joystickUIView);
            }

            if (e.OldElement != null)
            {
                // Unsubscribe from event handlers and cleanup any resources
                _joystickUIView.RemoveUpdater();
            }

            if (e.NewElement != null)
            {
                // Configure the control and subscribe to event handlers
                _joystickUIView.AddUpdater((xposition, yposition, distance, angle) =>
                {
                    Element.Xposition = xposition;
                    Element.Yposition = yposition;
                    Element.Distance = distance;
                    Element.Angle = angle;
                });
            }
        }
    }
}
