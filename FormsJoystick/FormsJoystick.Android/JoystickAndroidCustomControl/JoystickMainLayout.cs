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
using Android.Graphics;
using Android.Util;

namespace FormsJoystick.Droid.JoystickAndroidCustomControl
{
    class JoystickMainLayout : LinearLayout, View.IOnTouchListener
    {
        public JoystickMainLayout(Context context) : base(context)
        {
            SetContent(context);
        }
        public JoystickMainLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetContent(context);
        }
        public JoystickMainLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            SetContent(context);
        }
        public JoystickMainLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            SetContent(context);
        }

        private readonly int _resolution = 200;

        private LinearLayout _myJoyStick;
        private Action<int, int, double, double> _updateValues;

        private float _xInView;
        private float _yInView;

        private float _originalX;
        private float _originalY;

        public int Xposition { get; private set; }
        public int Yposition { get; private set; }
        public double DistanceFromZero { get; private set; }
        public double Angle { get; private set; }

        private void SetContent(Context context)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            inflater.Inflate(Resource.Layout.joystickMainLayout, this);

            SetMinimumWidth(200);
            SetMinimumHeight(500);

            _myJoyStick = FindViewById<LinearLayout>(Resource.Id.stickview);

            SetGravity(GravityFlags.Center);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Up:
                    //return stick view to original position.
                    v.Layout((int)_originalX, (int)_originalY, (int)_originalX + v.Width, (int)_originalY + v.Height);
                    UpdateXandY((int)v.GetX(), (int)v.GetY());
                    break;
                case MotionEventActions.Down:
                    //Set initial X and Y position for the stick location.                    
                    _originalX = v.GetX();
                    _originalY = v.GetY();
                    //Set initial X and Y position for the user finger on the stick
                    ////since this event only triggers on the stick view, the finger position will always be inside the stick view.
                    _xInView = e.GetX();
                    _yInView = e.GetY();
                    break;
                case MotionEventActions.Move:
                    //get the "SquareLinearLayout" Left and Top position on screen 
                    var parent = v.Parent as View;
                    int[] parentCoordinates = new[] { 0, 0 };
                    parent.GetLocationOnScreen(parentCoordinates);

                    //Calculate the future position of the stick, as left,right,top,bottom
                    var newLeft = (int)(e.RawX - (_xInView)) - parentCoordinates[0];
                    var newRight = (int)(newLeft + v.Width);
                    var newTop = (int)(e.RawY - (_yInView)) - parentCoordinates[1];
                    var newBottom = (int)(newTop + v.Height);

                    //_originalX is the distance between left of "SquareLinearLayout" and left of "stick view", so it is might as well be the maximum distance the stick can move
                    int maxDistance = (int)_originalX;

                    //Calculate future position from left and top to X and Y.
                    int newViewX = newLeft - (int)_originalX;
                    int newViewY = (newTop - (int)_originalY) * -1;
                    double newDistanceFromZero = Math.Sqrt(Math.Pow(newViewX, 2) + Math.Pow(newViewY, 2));

                    //System.Diagnostics.Debug.WriteLine($"x:{newViewX}, y:{newViewY}, d:{newDistanceFromZero}, MAXd:{maxDistance}");

                    //check if calculated future position exceeds maximum distance
                    if (newDistanceFromZero > maxDistance)
                    {
                        //if exceeds then get set the position to the maximum distance with respect to "future angle".
                        //"future angle" is the angle of the imaginary line drawn from (x=0, y=0) to calculated future stick position on the x-axis

                        //get radians then get angle.
                        double radians = Math.Atan2(newViewX, newViewY);
                        double angle = radians * (180 / Math.PI);

                        //calculate the X and Y from maximum distance and "future angle".
                        newViewX = (int)(Math.Sin(angle * (Math.PI / 180)) * maxDistance);
                        //multiply Yposition by -1 to change sign to make y-axis positive on above x-axis and negative under x-axis
                        newViewY = (int)(Math.Cos(angle * (Math.PI / 180)) * maxDistance) * -1;

                        //ReCalculate the future position of the stick, as left,right,top,bottom
                        newLeft = (int)(newViewX + (int)_originalX);
                        newRight = (int)(newLeft + v.Width);
                        newTop = (int)(newViewY + (int)_originalX);
                        newBottom = (int)(newTop + v.Height);
                    }

                    //change position of "stick view"
                    v.Layout(newLeft, newTop, newRight, newBottom);

                    //update X and Y from the actual position of "stick view" relative to "SquareLinearLayout".
                    UpdateXandY((int)v.GetX(), (int)v.GetY());
                    break;
            }
            return true;
        }

        private void UpdateXandY(int x, int y)
        {
            //_originalX is the distance between left of "SquareLinearLayout" and left of "stick view", So double it will get total x-axis length
            //since the parent view is always square, then y-axis = x-axis.
            int totalAxisLength = (int)_originalX * 2;

            //Calculate X and Y with respect to resolution.
            //subtract (resolution/2) to make position of (x=0,y=0) on the center of control instead of top left (default behaviour of mobile positioning).
            Xposition = (x * _resolution / totalAxisLength) - (_resolution / 2);
            //multiply Yposition by -1 to change sign to make y-axis positive on above x-axis and negative under x-axis
            Yposition = ((y * _resolution / totalAxisLength) - (_resolution / 2)) * -1;

            DistanceFromZero = Math.Sqrt(Math.Pow(Xposition, 2) + Math.Pow(Yposition, 2));

            //get radians then get angle.
            double radians = Math.Atan2(Xposition, Yposition);
            Angle = radians * (180 / Math.PI);
            if (Angle < 0) Angle = Angle + 360;

            _updateValues?.Invoke(Xposition, Yposition, DistanceFromZero, Angle);
        }

        public void AddTouchListener(Action<int, int, double, double> updateValues)
        {
            _myJoyStick.SetOnTouchListener(this);
            _updateValues = updateValues;
        }

        public void RemoveTouchListener()
        {
            _myJoyStick.SetOnTouchListener(null);
            _updateValues = null;
        }
    }
}