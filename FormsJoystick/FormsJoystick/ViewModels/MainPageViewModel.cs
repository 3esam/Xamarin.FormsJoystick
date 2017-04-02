using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FormsJoystick.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(nameof(Title)); }
        }

        private int _joystickXposition;
        public int JoystickXposition
        {
            get { return _joystickXposition; }
            set { _joystickXposition = value; NotifyPropertyChanged(nameof(JoystickXposition)); }
        }

        private int _joystickYposition;
        public int JoystickYposition
        {
            get { return _joystickYposition; }
            set { _joystickYposition = value; NotifyPropertyChanged(nameof(JoystickYposition)); }
        }

        private int _joystickDistance;
        public int JoystickDistance
        {
            get { return _joystickDistance; }
            set { _joystickDistance = value; NotifyPropertyChanged(nameof(JoystickDistance)); }
        }

        private int _joystickAngle;        

        public int JoystickAngle
        {
            get { return _joystickAngle; }
            set { _joystickAngle = value; NotifyPropertyChanged(nameof(JoystickAngle)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
