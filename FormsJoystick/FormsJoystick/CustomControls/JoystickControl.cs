using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace FormsJoystick.CustomControls
{
    class JoystickControl : View
    {
        public static readonly BindableProperty XpositionProperty =
            BindableProperty.Create(
                propertyName: "Xposition",
                returnType: typeof(int),
                declaringType: typeof(int),
                defaultValue: 0
            );

        public int Xposition
        {
            get { return (int)GetValue(XpositionProperty); }
            set { SetValue(XpositionProperty, value); }
        }

        public static readonly BindableProperty YpositionProperty =
            BindableProperty.Create(
                propertyName: "Yposition",
                returnType: typeof(int),
                declaringType: typeof(int),
                defaultValue: 0
            );

        public int Yposition
        {
            get { return (int)GetValue(YpositionProperty); }
            set { SetValue(YpositionProperty, value); }
        }

        public static readonly BindableProperty DistanceProperty =
            BindableProperty.Create(
                propertyName: "Distance",
                returnType: typeof(double),
                declaringType: typeof(double),
                defaultValue: 0.0
            );

        public double Distance
        {
            get { return (double)GetValue(DistanceProperty); }
            set { SetValue(DistanceProperty, value); }
        }

        public static readonly BindableProperty AngleProperty =
            BindableProperty.Create(
                propertyName: "Angle",
                returnType: typeof(double),
                declaringType: typeof(double),
                defaultValue: 0.0
            );

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }
    }
}
