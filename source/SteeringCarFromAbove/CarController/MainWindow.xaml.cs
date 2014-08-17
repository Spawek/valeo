using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using Helpers;

namespace CarController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DefaultCarController Controller { get; private set; }

        private System.Timers.Timer mTimer = new System.Timers.Timer(TIMER_INTERVAL_IN_MS);
        private const int TIMER_INTERVAL_IN_MS = 25;

        private const int TARGET_BRAKE_SETTING_WHEN_MANUAL_BRAKING_ON = 100;
        private const int BRAKE_ACTIVATION_TIME_ON_SPACE_PRESSING_IN_MS = 500; //its much too much, but smaller values blinks at start
        private const double MAX_FORWARD_SPEED_WHEN_DRIVING_ON_GAMEPAD_IN_MPS = 7.5; 
        private const int MAX_BACKWARD_SPEED_WHEN_DRIVING_ON_GAMEPAD_IN_MPS = -10; //it should be < 0 !
        private const int MAX_WHEEL_ANGLE_VALEUE_WHEN_DRIVING_ON_GAMEPAD = 30;
        private const int MAX_WHEEL_ANGLE_CHANGE_PER_SEC_WHEN_DRIVING_ON_GAMEPAD = 10;
        private const int WHEEL_ANGLE_CHANGING_WITH_GAMEPAD_TIMER_INTERVAL_IN_MS = 50;
        private const double MIN_GAMEPAD_Y_TO_START_TURNING_WHEEL_IN_PERCENTS = 5.0;
        private const double SPEED_CHANGE_PER_UP_DOWN_ARR_CLICK = 2.5;
        private const double WHEEL_ANGLE_CHANGE_PER_LEFT_RIGHT_ARR_CLICK = 2;

        private System.Timers.Timer brakingTimer;
        private bool GamePadBrakingButtonPressed = false;

        private double gamePadCurrentTurningPerTick = 0.0;

        
        /// <summary>
        /// constructor which is initializing CarController on itself
        /// </summary>
        public MainWindow()
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
            Controller = new DefaultCarController();
            
            InitializeComponent();

            ExternalEventsHandlingInit();
            
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            //initialize timer
            mTimer.Elapsed += mTimer_Elapsed;
            mTimer.Start();
        }

        void mTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(
                new Action<TextBlock, TimeSpan>(
                    (textBlock, timeSpan) => 
                        textBlock.Text = timeSpan.ToString(@"mm\:ss\:ff")
                ), 
                textBlock_time, 
                Time.GetTimeFromProgramBeginnig()
            );
        }

        public MainWindow(DefaultCarController _controller)
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
            Controller = _controller;

            InitializeComponent();

            ExternalEventsHandlingInit();
            
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            //initialize timer
            mTimer.Interval = TIMER_INTERVAL_IN_MS;
            mTimer.Elapsed += mTimer_Elapsed;
            mTimer.Start();
        }

        private void ExternalEventsHandlingInit()
        {
            brakingTimer = new Timer();
            brakingTimer.Elapsed += new System.Timers.ElapsedEventHandler(brakingTimer_Elapsed);

            Controller.Model.evTargetSpeedChanged += new TargetSpeedChangedEventHandler(Model_evTargetSpeedChanged);
            Controller.Model.CarComunicator.evSpeedInfoReceived += new SpeedInfoReceivedEventHander(CarComunicator_evSpeedInfoReceived);

            Controller.Model.evTargetSteeringWheelAngleChanged += new TargetSteeringWheelAngleChangedEventHandler(Model_evTargetSteeringWheelAngleChanged);
            Controller.Model.CarComunicator.evSteeringWheelAngleInfoReceived += new SteeringWheelAngleInfoReceivedEventHandler(CarComunicator_evSteeringWheelAngleInfoReceived);
            Controller.Model.CarComunicator.evBrakePositionReceived += new BrakePositionReceivedEventHandler(CarComunicator_evBrakePositionReceived);
            Controller.Model.CarComunicator.evGearPositionReceived += CarComunicator_evGearPositionReceived;

            Controller.Model.SpeedRegulator.evNewSpeedSettingCalculated += new NewSpeedSettingCalculatedEventHandler(SpeedRegulator_evNewSpeedSettingCalculated); //this is also target for brake regulator
            Controller.Model.SteeringWheelAngleRegulator.evNewSteeringWheelSettingCalculated += new NewSteeringWheelSettingCalculatedEventHandler(SteeringWheelAngleRegulator_evNewSteeringWheelSettingCalculated);
            Controller.Model.BrakeRegulator.evNewBrakeSettingCalculated += new NewBrakeSettingCalculatedEventHandler(BrakeRegulator_evNewBrakeSettingCalculated);

            Controller.Model.evAlertBrake += new EventHandler(Model_evAlertBrake);
        }

        void wheelAngleChangingWithGamePadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Controller.ChangeTargetWheelAngle(gamePadCurrentTurningPerTick);
        }

        void brakingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!GamePadBrakingButtonPressed)
            {
                Controller.EndTargetBrakeSteeringOverriding();
            }
        }

        void gamePad_evNewGamePadButtonInfoAcquired(object sender, int buttonNo, bool pressed)
        {
            switch(buttonNo)
            {
                case 5:
                case 6:
                    if (pressed)
                    {
                        Controller.OverrideTargetBrakeSetting(TARGET_BRAKE_SETTING_WHEN_MANUAL_BRAKING_ON);
                        GamePadBrakingButtonPressed = true;
                    }
                    else
                    {
                        Controller.EndTargetBrakeSteeringOverriding();
                        GamePadBrakingButtonPressed = false;
                    }
                    break;
            }
        }

        void Model_evAlertBrake(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(
                new Action<TextBlock>((textBlock)
                    => textBlock.Text = "ALERT BRAKE ACTIVATED!!!!"),
                    textBlock_alertBrakeActivated
            );
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }


        private enum RideMode { Forward, Backward }
        private RideMode rideMode = RideMode.Forward;

        double LimitSpeedDueToCurrRideMode(double wantedSpeed)
        {
            if (rideMode == RideMode.Forward)
            {
                Limiter.Limit(ref wantedSpeed, 0.0d, double.MaxValue);
            }
            else
            {
                Limiter.Limit(ref wantedSpeed, double.MinValue, 0.0d);
            }

            return wantedSpeed;
        }

        private void HandleKeyDown(KeyEventArgs key)
        {
            double valToBeSet;

            switch (key.Key)
            {
                case Key.Up:
                case Key.W:
                    valToBeSet = Controller.Model.CarInfo.TargetSpeed + SPEED_CHANGE_PER_UP_DOWN_ARR_CLICK;
                    Limiter.Limit(ref valToBeSet, MAX_BACKWARD_SPEED_WHEN_DRIVING_ON_GAMEPAD_IN_MPS, MAX_FORWARD_SPEED_WHEN_DRIVING_ON_GAMEPAD_IN_MPS);
                    valToBeSet = LimitSpeedDueToCurrRideMode(valToBeSet);
                    Controller.SetTargetSpeed(valToBeSet);
                    break;

                case Key.Down:
                case Key.S:
                    valToBeSet = Controller.Model.CarInfo.TargetSpeed - SPEED_CHANGE_PER_UP_DOWN_ARR_CLICK;
                    Limiter.Limit(ref valToBeSet, MAX_BACKWARD_SPEED_WHEN_DRIVING_ON_GAMEPAD_IN_MPS, MAX_FORWARD_SPEED_WHEN_DRIVING_ON_GAMEPAD_IN_MPS);
                    valToBeSet = LimitSpeedDueToCurrRideMode(valToBeSet);
                    Controller.SetTargetSpeed(valToBeSet);
                    break;

                case Key.Left:
                case Key.A:
                    valToBeSet = Controller.Model.CarInfo.TargetWheelAngle - WHEEL_ANGLE_CHANGE_PER_LEFT_RIGHT_ARR_CLICK;
                    Limiter.Limit(ref valToBeSet, -1 * MAX_WHEEL_ANGLE_VALEUE_WHEN_DRIVING_ON_GAMEPAD, MAX_WHEEL_ANGLE_VALEUE_WHEN_DRIVING_ON_GAMEPAD);
                    Controller.SetTargetWheelAngle(valToBeSet);
                    break;

                case Key.Right:
                case Key.D:
                    valToBeSet = Controller.Model.CarInfo.TargetWheelAngle + WHEEL_ANGLE_CHANGE_PER_LEFT_RIGHT_ARR_CLICK;
                    Limiter.Limit(ref valToBeSet, -1 * MAX_WHEEL_ANGLE_VALEUE_WHEN_DRIVING_ON_GAMEPAD, MAX_WHEEL_ANGLE_VALEUE_WHEN_DRIVING_ON_GAMEPAD);
                    Controller.SetTargetWheelAngle(valToBeSet);
                    break;

                case Key.Escape:
                    Controller.AlertBrake();
                    break;

                case Key.Space:
                    Controller.OverrideTargetBrakeSetting(TARGET_BRAKE_SETTING_WHEN_MANUAL_BRAKING_ON);
                    brakingTimer.Interval = BRAKE_ACTIVATION_TIME_ON_SPACE_PRESSING_IN_MS;
                    brakingTimer.Start();
                    break;

                case Key.K: //K -> change road direction
                    if(rideMode == RideMode.Forward)
                    {
                        rideMode = RideMode.Backward;
                    }
                    else
                    {
                        rideMode = RideMode.Forward;
                    }
                    Controller.SetTargetSpeed(0.0);
                    break;

                case Key.Enter: //Enter -> set wheel to middles
                    Controller.SetTargetWheelAngle(0.0);
                    break;
            }
        }

        private class LabelData
        {
            public double newValue;
            public double setValue;
            public bool labelSetInvokeAwaiting;

            public LabelData()
            {
                newValue = Double.NegativeInfinity;
                setValue = Double.NegativeInfinity;
                labelSetInvokeAwaiting = false;
            }
        }

        /// <summary>
        /// this method updates label with new value,
        ///     you can use this method as much as you want (e.g. 10000 times per second) 
        ///     it wont lead to stackoverflow (standard invoke would lead to stack overflow (and did it!))
        ///     
        /// the reason this method has sense is events which are updating textblock values sometimes are 
        /// invoked very frequently (like few hundred times a second)
        /// </summary>
        /// <param name="textBlock"></param>
        /// <param name="valueToSet"></param>
        /// <param name="labelData"></param>
        private void UpdateTextBlock(TextBlock textBlock, double valueToSet, LabelData labelData)
        {
            labelData.newValue = valueToSet;
            if (labelData.newValue != labelData.setValue)
            {
                if (!labelData.labelSetInvokeAwaiting)
                {
                    labelData.labelSetInvokeAwaiting = true;
                    this.Dispatcher.Invoke(
                        new Action<TextBlock, double>((textBox, val) =>
                        {
                            textBox.Text = String.Format("{0:0.###}", val);
                            labelData.setValue = val;
                            labelData.labelSetInvokeAwaiting = false;
                        }),
                            textBlock,
                            labelData.newValue
                    );
                }
            }
        }

        private void UpdateProgressBar(ProgressBar progressBar, double valueToSet, LabelData labelData)
        {
            labelData.newValue = valueToSet;
            if (labelData.newValue != labelData.setValue)
            {
                if (!labelData.labelSetInvokeAwaiting)
                {
                    labelData.labelSetInvokeAwaiting = true;
                    this.Dispatcher.Invoke(
                        new Action<TextBlock, double>((textBox, val) =>
                        {
                            progressBar.Value = valueToSet;
                            labelData.setValue = val;
                            labelData.labelSetInvokeAwaiting = false;
                        }),
                            progressBar,
                            labelData.newValue
                    );
                }
            }
        }

        /* 
         * below actions are done in order to fix stack overflow error which was happening in here
         *      (when there was just invoke on event like commented bellow)
         *      
         * when there were a lot of invoked events on every event window thread saved an information (on stack)
         * that something has to be done (usually label changed), when machine had a lot of computing to do it
         * ended in stack overflow in here (in window...)
         */
        LabelData steeringWheelSettingLabelData = new LabelData();
        void SteeringWheelAngleRegulator_evNewSteeringWheelSettingCalculated(object sender, NewSteeringWheelSettingCalculateddEventArgs args)
        {
            UpdateTextBlock(textBlock_steeringAngle, args.getSteeringWheelAngleSetting(), steeringWheelSettingLabelData);
            
            //this.Dispatcher.Invoke(
            //    new Action<TextBlock, string>((textBox, val)
            //        => textBox.Text = val),
            //            textBlock_steeringAngle,
            //            String.Format("{0:0.###}", args.getSteeringWheelAngleSetting())
            //);
        }

        LabelData steeringSpeedLabelData = new LabelData();
        LabelData targetBrakeLabelData = new LabelData();
        void SpeedRegulator_evNewSpeedSettingCalculated(object sender, NewSpeedSettingCalculatedEventArgs args)
        {
            UpdateTextBlock(textBlock_steeringSpeed, args.getSpeedSetting(), steeringSpeedLabelData);

            //target for brake regulator
            double targetBrake = args.getSpeedSetting();
            if(targetBrake < 0)
            {
                targetBrake *= -1;
            }
            else
            {
                targetBrake = 0;
            }
            UpdateTextBlock(textBlock_targetBrake, targetBrake, targetBrakeLabelData);
        }

        LabelData currentAngleLabelData = new LabelData();
        void CarComunicator_evSteeringWheelAngleInfoReceived(object sender, SteeringWheelAngleInfoReceivedEventArgs args)
        {
            UpdateTextBlock(textBlock_currentAngle, args.GetAngle(), currentAngleLabelData);
        }

        LabelData targetAngleLabelData = new LabelData();
        void Model_evTargetSteeringWheelAngleChanged(object sender, TargetSteeringWheelAngleChangedEventArgs args)
        {
            UpdateTextBlock(textBlock_targetAngle, args.GetTargetWheelAngle(), targetBrakeLabelData);
            //UpdateProgressBar(progressBar_rightDir, args.GetTargetWheelAngle, 
        }

        LabelData currentSpeedLabelData = new LabelData();
        void CarComunicator_evSpeedInfoReceived(object sender, SpeedInfoReceivedEventArgs args)
        {
            UpdateTextBlock(textBlock_currentSpeed, args.GetSpeedInfo(), currentSpeedLabelData);
        }

        LabelData targetSpeedLabelData = new LabelData();
        void Model_evTargetSpeedChanged(object sender, TargetSpeedChangedEventArgs args)
        {
            UpdateTextBlock(textBlock_targetSpeed, args.GetTargetSpeed(), targetSpeedLabelData);
        }

        LabelData currentBrakeLabelData = new LabelData();
        void CarComunicator_evBrakePositionReceived(object sender, BrakePositionReceivedEventArgs args)
        {
            UpdateTextBlock(textBlock_currentBrake, args.GetPosition(), currentBrakeLabelData);
        }

        LabelData steeringBrakeLabelData = new LabelData();
        void BrakeRegulator_evNewBrakeSettingCalculated(object sender, NewBrakeSettingCalculatedEventArgs args)
        {
            UpdateTextBlock(textBlock_steeringBrake, args.GetBrakeSetting(), steeringBrakeLabelData);
        }

        LabelData currentGearLabelData = new LabelData();
        void CarComunicator_evGearPositionReceived(object sender, GearPositionReceivedEventArgs args)
        {
            this.Dispatcher.Invoke(
                new Action<Gear>((gear) =>
                {
                    textBlock_currentGear.Text = gear.ToString();
                }));
        }
    }
}
