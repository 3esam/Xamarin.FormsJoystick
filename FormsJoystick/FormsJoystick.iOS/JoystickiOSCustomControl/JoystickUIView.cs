using CoreGraphics;
using Foundation;
using System;
using System.Diagnostics;
using UIKit;

namespace FormsJoystick.iOS.JoystickiOSCustomControl
{
    class JoystickUIView : UIView
    {
        private UIView SquareUIView;
        private UIView StickUIView;
        private readonly double stickUIViewSize = 40;
        private double SquareUIViewSize = 80;
        private UITouch _thisTouch;

        private nfloat _xInView;
        private nfloat _yInView;

        private nfloat _originalX;
        private nfloat _originalY;

        private readonly int _resolution = 200;
        private Action<int, int, double, double> _updateValues;

        private bool _dragable;
        private string _name;

        public int Xposition { get; private set; }
        public int Yposition { get; private set; }
        public double DistanceFromZero { get; private set; }
        public double Angle { get; private set; }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            _name = Guid.NewGuid().ToString().Substring(0, 5); //helps with debugging
            //Debug.Write($"NAME =-= {_name}");

            var squareUiViewSize = rect.Height > rect.Width ? rect.Width : rect.Height;
            SquareUIViewSize = squareUiViewSize;
            //Odd sized square makes some wrong calculations, still don't know why :/
            if (squareUiViewSize % 2 != 0) squareUiViewSize--;

            SquareUIView = new UIView();
            AddSubview(SquareUIView);
            SquareUIView.Frame = new CGRect(Frame.Width / 2 - squareUiViewSize / 2, Frame.Height / 2 - squareUiViewSize / 2, squareUiViewSize, squareUiViewSize);
            SquareUIView.MultipleTouchEnabled = true;
            //SquareUIView.BackgroundColor = UIColor.White;

            var squareUiViewBackgroundImage = new UIImageView(UIImage.FromBundle("pad.png"));
            squareUiViewBackgroundImage.Frame = new CGRect(0, 0, squareUiViewSize, squareUiViewSize);
            SquareUIView.AddSubview(squareUiViewBackgroundImage);

            StickUIView = new UIView();
            SquareUIView.AddSubview(StickUIView);
            StickUIView.Frame = new CGRect(squareUiViewSize / 2 - stickUIViewSize / 2, squareUiViewSize / 2 - stickUIViewSize / 2, stickUIViewSize, stickUIViewSize);

            var stickUiViewBackgroundImage = new UIImageView(UIImage.FromBundle("stick.png"));
            stickUiViewBackgroundImage.Frame = new CGRect(0, 0, stickUIViewSize, stickUIViewSize);
            StickUIView.AddSubview(stickUiViewBackgroundImage);

            //Set initial X and Y position for the stick location.                    
            _originalX = StickUIView.Frame.Left;
            _originalY = StickUIView.Frame.Top;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {

            base.TouchesBegan(touches, evt);

            //Debug.Write($"TB NAME =-= {_name}");
            bool foundTouch = false;

            CGPoint locationInSquareView;

            if (evt?.AllTouches == null || evt.AllTouches.Count == 0)
            {
                Debug.Write("All touches was 0 Returning");
                return;
            }

            if (evt.AllTouches.Count == 1)
            {
                _thisTouch = (UITouch)evt.AllTouches.AnyObject;
            }
            else
            {
                //Find this touch.
                //int cnt = -1;
                foreach (var o in evt.AllTouches)
                {
                    //cnt++;
                    var t = (UITouch)o;
                    //Change by Amit - Using SquareUIView to give a little more "room" to find the right one using smaller controls
                    locationInSquareView = t.LocationInView(SquareUIView);
                    if (locationInSquareView.X > SquareUIViewSize || locationInSquareView.X < 0 || locationInSquareView.Y > SquareUIViewSize || locationInSquareView.Y < 0)
                    {
                        //Debug.Write($"this is not my touch: {cnt} ({locationInStickView.X},{locationInStickView.Y})");
                        continue;
                    }

                    //Debug.Write($"FOUND my touch: {cnt} ({locationInStickView.X},{locationInStickView.Y})");
                    _thisTouch = t; //this is my touch!
                    foundTouch = true;
                    break;
                }

                if (!foundTouch)
                {
                    //Debug.Write("Could not find this touch... returning");
                    return;
                }
            }

            //get touch location relative to SquareView
            //This is an extra check, but for some reason sometimes it does fall into this... Not sure why
            locationInSquareView = _thisTouch.LocationInView(SquareUIView);
            if (locationInSquareView.X > SquareUIViewSize || locationInSquareView.X < 0 || locationInSquareView.Y > SquareUIViewSize || locationInSquareView.Y < 0)
            {
                //Debug.Write($"Even after finding - location not in square, return! ({locationInStickView.X},{locationInStickView.Y})");
                return;
            }

            _dragable = true;

            //Set initial X and Y position relative to self                        
            var locationInStickView = _thisTouch.LocationInView(StickUIView);
            _xInView = locationInStickView.X;
            _yInView = locationInStickView.Y;
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            if (!_dragable)
                return;

            if (_thisTouch == null)
            {
                //Debug.Write("_thisTouch was null no drag for you!");
                return;
            }

            //Get touch position relative to SquareUIView            
            CGPoint touchLocation = _thisTouch.LocationInView(SquareUIView);

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
            _thisTouch = null;


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