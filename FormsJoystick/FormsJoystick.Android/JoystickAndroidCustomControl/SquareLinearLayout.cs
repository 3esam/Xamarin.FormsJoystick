using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace FormsJoystick.Droid.JoystickAndroidCustomControl
{
    [Register("com.esamsherif.formsjoystick.FormsJoystick.Droid.JoystickAndroidCustomControl.SquareLinearLayout")]
    class SquareLinearLayout : LinearLayout
    {
        public SquareLinearLayout(Context context)
            : base(context) { }
        public SquareLinearLayout(Context context, IAttributeSet attrs)
            : base(context, attrs) { }
        public SquareLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr) { }
        public SquareLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes) { }
        public SquareLinearLayout(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            int width = MeasureSpec.GetSize(widthMeasureSpec);
            int height = MeasureSpec.GetSize(heightMeasureSpec);
            int size = width > height ? height : width;
            SetMeasuredDimension(size, size);
        }
    }
}