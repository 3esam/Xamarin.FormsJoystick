using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace FormsJoystick.iOS.JoystickiOSCustomControl
{
    class JoystickUIView : UIView
    {
        private UIView SquareUIView;
        private UIView StickUIView;
        private readonly double stickUIViewSize = 40;

        private nfloat _xInView;
        private nfloat _yInView;

        private nfloat _originalX;
        private nfloat _originalY;

        private readonly int _resolution = 200;
        private Action<int, int, double, double> _updateValues;

        private bool _dragable;

        public int Xposition { get; private set; }
        public int Yposition { get; private set; }
        public double DistanceFromZero { get; private set; }
        public double Angle { get; private set; }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            var squareUIViewSize = rect.Height > rect.Width ? rect.Width : rect.Height;
            //Odd sized square makes some wrong calculations, still don't know why :/
            if (squareUIViewSize % 2 != 0) squareUIViewSize--;

            SquareUIView = new UIView();
            AddSubview(SquareUIView);
            SquareUIView.Frame = new CGRect(Frame.Width / 2 - squareUIViewSize / 2, Frame.Height / 2 - squareUIViewSize / 2, squareUIViewSize, squareUIViewSize);
            //SquareUIView.BackgroundColor = UIColor.White;

            var squareUIViewBackgroundImage = new UIImageView(UIImage.FromBundle("pad.png"));
            squareUIViewBackgroundImage.Frame = new CGRect(0, 0, squareUIViewSize, squareUIViewSize);
            SquareUIView.AddSubview(squareUIViewBackgroundImage);

            StickUIView = new UIView();
            SquareUIView.AddSubview(StickUIView);
            StickUIView.Frame = new CGRect(squareUIViewSize / 2 - stickUIViewSize / 2, squareUIViewSize / 2 - stickUIViewSize / 2, stickUIViewSize, stickUIViewSize);

            var stickUIViewBackgroundImage = new UIImageView(UIImage.FromBundle("stick.png"));
            stickUIViewBackgroundImage.Frame = new CGRect(0, 0, stickUIViewSize, stickUIViewSize);
            StickUIView.AddSubview(stickUIViewBackgroundImage);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            UITouch touch = (UITouch)evt.AllTouches.AnyObject;

            //get touch location relative to StickView
            CGPoint locationInStickView = touch.LocationInView(StickUIView);

            //return if touch location out of StickView bounds
            if (locationInStickView.X > stickUIViewSize || locationInStickView.X < 0 ||
                locationInStickView.Y > stickUIViewSize || locationInStickView.Y < 0)
            {
                _dragable = false;
                return;
            }

            _dragable = true;

            //Set initial X and Y position for the stick location.                    
            _originalX = StickUIView.Frame.Left;
            _originalY = StickUIView.Frame.Top;

            //Set initial X and Y position relative to self                        
            _xInView = locationInStickView.X;
            _yInView = locationInStickView.Y;
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            if (!_dragable)
                return;

            UITouch touch = (UITouch)evt.AllTouches.AnyObject;

            //Get touch position relative to SquareUIView            
            CGPoint touchLocation = touch.LocationInView(SquareUIView);

            //Calculate the future position of the stick
            nfloat newLeft = touchLocation.X - _xInView;
            nfloat newTop = touchLocation.Y - _yInView;

            //_originalX is the distance between left of "SquareLinearLayout" and left of "stick view", so it is might as well be the maximum distance the stick can move
            nfloat maxDistance = (int)_originalX;

            //Convert future position (screen positioning) to (axis positioning).
            nfloat newXAxis = newLeft - _originalX;
            nfloat newYAxis = ((newTop) - (_originalY)) * -1;
            double newDistanceFromZero = Math.Sqrt(Math.Pow(newXAxis, 2) + Math.Pow(newYAxis, 2));

            //System.Diagnostics.Debug.WriteLine($"x:{newXAxis}, y:{newYAxis}, d:{newDistanceFromZero}, MAXd:{maxDistance}");

            //check if calculated future position exceeds maximum distance
            if (newDistanceFromZero > maxDistance)
            {
                //if exceeds then get set the position to the maximum distance with respect to "future angle".
                //"future angle" is the angle of the imaginary line drawn from (x=0, y=0) to calculated future stick position on the x-axis

                //get radians then get angle.
                double radians = Math.Atan2(newXAxis, newYAxis);
                double angle = radians * (180 / Math.PI);

                //calculate the X and Y from maximum distance and "future angle".
                newXAxis = (int)(Math.Sin(angle * (Math.PI / 180)) * maxDistance);
                //Multiply by -1 to Convert future position back from (axis positioning) to (screen positioning).
                newYAxis = (int)((Math.Cos(angle * (Math.PI / 180)) * maxDistance) * -1);

                newLeft = (int)newXAxis + (int)_originalX;
                newTop = (int)(newYAxis + (int)_originalY);
            }

            StickUIView.Frame = new CGRect(newLeft, newTop, StickUIView.Frame.Width, StickUIView.Frame.Height);

            //update X and Y from the actual position of "stick view" relative to "SquareLinearLayout".
            UpdateXandY((int)StickUIView.Frame.Left, (int)StickUIView.Frame.Top);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            if (!_dragable)
                return;

            //return stick view to original position.
            StickUIView.Center = new CGPoint(SquareUIView.Frame.Width / 2, SquareUIView.Frame.Height / 2);
            UpdateXandY((int)StickUIView.Frame.Left, (int)StickUIView.Frame.Top);
            _dragable = false;
        }

        private void UpdateXandY(int x, int y)
        {
            //_originalX is the distance between left of "SquareLinearLayout" and left of "stick view", So double it will get total x-axis length
            //since the parent view is always square, then y-axis = x-axis.
            int totalAxisLength = (int)(_originalX * 2);

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

        public void AddUpdater(Action<int, int, double, double> updateValues)
        {
            _updateValues = updateValues;
        }

        public void RemoveUpdater()
        {
            _updateValues = null;
        }
    }
}
