using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace CarSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double FORM_UPDATE_INTERVAL_IN_MS = 25.0d;

        EngineSimulator sim = new EngineSimulator(new ToyotaYaris());
        Timer formUpdater = new Timer(FORM_UPDATE_INTERVAL_IN_MS);

        public MainWindow()
        {
            InitializeComponent();

            formUpdater.Elapsed += formUpdater_Elapsed;
            formUpdater.Start();
        }

        void formUpdater_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action<double>(x => this.slider_RPM.Value = x), sim.model.RPM); //update RMP slider
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_currRPM.Text = x.ToString("0.0")), sim.model.RPM);
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_speed.Text = x.ToString("0.00") + " km/h"), sim.model.SpeedInKilometersPerHour);
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_forwardForce.Text = x.ToString("0.0") + " N"), sim.model.ForwardForceOnWheelsFromEngine);
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_EngineResisntance_times_transmissionRate.Text = x.ToString("0.0") + " N"), sim.model.EngineResistanceForcesOnWheels);
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_trasmissionRate.Text = x.ToString("0.0000")), 1.0/sim.model.TransmissionRate);
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_aerodynamicResistance.Text = x.ToString("0.0") + " N"), sim.model.AerodynemicResistance);
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_rollingResistance.Text = x.ToString("0.0") + " N"), sim.model.RollingResistance);
            this.Dispatcher.Invoke(new Action<double>(x => this.TextBlock_distanceDone.Text = x.ToString("0.0") + " m"), sim.model.DistanceDoneInMeters);
            this.Dispatcher.Invoke(new Action<int>(x => this.slider_transmission.Value = x), sim.model.CurrGear);
            this.Dispatcher.Invoke(new Action<int>(x => this.TextBlock_currGear.Text = x.ToString()), sim.model.CurrGear);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            sim.model.Start();
        }

        private void slider_transmission_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int newGear = Convert.ToInt32(e.NewValue);

            sim.model.CurrGear = newGear;
            if (this.TextBlock_currGear != null) // fix for initialization order issues
            {
                this.TextBlock_currGear.Text = newGear.ToString();
            }
        }

        private void slider_externalForces_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sim.model.ExternalResistanceForces = e.NewValue;

            if (this.TextBlock_externalForces != null)
            {
                this.TextBlock_externalForces.Text = String.Format("{0}", e.NewValue.ToString("0.0"));
            }
        }

        private void slider_acceleration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sim.model.ThrottleOppeningLevel = e.NewValue / 100.0;

            if (this.TextBlock_acceleration != null)
            {
                this.TextBlock_acceleration.Text = String.Format("{0}", e.NewValue.ToString("0.0"));
            }
        }

        private void Slider_braking_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sim.model.BrakingLevel = e.NewValue / 100.0;

            if (this.TextBlock_braking != null)
            {
                this.TextBlock_braking.Text = String.Format("{0}", e.NewValue.ToString("0.0"));
            }
        }
    }
}
