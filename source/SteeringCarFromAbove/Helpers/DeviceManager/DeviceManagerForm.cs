using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Helpers
{
    public partial class DeviceManagerForm : Form
    {
        private const int UPDATER_TIMER_INTERVAL_IN_MS = 100;

        //private Jurek jurek;

        private DeviceManager deviceManager;
        private System.Windows.Forms.Timer updaterTimer;

        public DeviceManagerForm(DeviceManager devManager)
        {
            deviceManager = devManager;

            InitializeComponent();
            //jurek = new Jurek();

            dataGridView1.Columns.Add("device name", "device name");
            dataGridView1.Columns.Add("overall state", "overall state");
            dataGridView1.Columns.Add("initialization state", "initialization state");

            updaterTimer = new System.Windows.Forms.Timer();
            updaterTimer.Interval = UPDATER_TIMER_INTERVAL_IN_MS;
            updaterTimer.Tick += updaterTimer_Tick;
            updaterTimer.Start();

            JurekGreeting();
            //jurek.StartListening(hearedCommand);

            deviceManager.evDeviceManagerOverallStateHasChanged += deviceManager_evDeviceStateHasChanged;
        }

        void deviceManager_evDeviceStateHasChanged(object sender, DeviceStateHasChangedEventArgs args)
        {
            if (args.GetDeviceState() == DeviceOverallState.Error)
            {
                UpdateButtonsStateInCaseOfDeviceError();
            }
            else if (args.GetDeviceState() == DeviceOverallState.OK && //its work arroud for bug causing wrong button enabling when initialized button was clicked in overall error state
                deviceManager.devicesList[0].initializationState == DeviceInitializationState.Initialized || //devices are initialized or initializing
                deviceManager.devicesList[0].initializationState == DeviceInitializationState.Initializing)
            {
                EnableStartSensorsButtonAndDisableInitButton();
            }

            ChangeOverallStateLabel(args.GetDeviceState().ToString());
        }

        //TODO: make it in setter or something like that - not in metod
        private void EnableStartSensorsButtonAndDisableInitButton()
        {
            if (label1.InvokeRequired)
            { //if this is not form thread
                label1.Invoke((MethodInvoker)delegate { EnableStartSensorsButtonAndDisableInitButton(); });
            }
            else
            { //if this is form thead
                button_StartSensors.Enabled = true;
                button_initialize.Enabled = false;
            }
        }

        private void ChangeOverallStateLabel(string newState)
        {
            if (label1.InvokeRequired)
            { //if this is not form thread
                label1.Invoke((MethodInvoker)delegate { ChangeOverallStateLabel(newState); });
            }
            else
            { //if this is form thead
                label_deviceManagerOverallState.Text = newState;
            }

        }

        /// <summary>
        /// disable buttons in form thread
        /// </summary>
        private void UpdateButtonsStateInCaseOfDeviceError()
        {
            if (label1.InvokeRequired)
            { //if this is not form thread
                label1.Invoke((MethodInvoker)delegate { UpdateButtonsStateInCaseOfDeviceError(); });
            }
            else
            { //if this is form thead
                button_StartSensors.Enabled = false;
                button_StartEffectors.Enabled = false;
                button_initialize.Enabled = true;
            }
        }

        private void JurekGreeting()
        {
            Random rnd = new Random();
            string[] greet = new string[]{
                "Cześć.", "Witaj.", "Dzień dobry.", "Dzień dobry."
            };

            string[] me = new string[]{
                "Jurek, kierowca. Do usług", "mówi Twój najlepszy kierowca Jurek!",
                "to ja, Jurek, kierowca.", "Kierowca Jurek do usług.", "Kierowca Jurek do usług."
            };

            //jurek.Say(greet[rnd.Next(0, greet.Length-1)]);
            //jurek.Say(me[rnd.Next(0, me.Length-1)]);
        }

        bool started = false;
        bool prepare = false;
        void hearedCommand(string cmd, float confidence)
        {
            if (confidence >= 0.6)
            {
                if (started == false && cmd == "start")
                {
                    button_initialize_Click(this, new EventArgs());
                    Thread.Sleep(3500);
                    button_StartSensors_Click(this, new EventArgs());
                    Thread.Sleep(1000);
                    button_StartEffectors_Click(this, new EventArgs());
                    started = true;
                }
                else if (prepare == false && cmd == "prepare")
                {
                    //button_StartSensors_Click(this, new EventArgs());
                    started = true;
                }
                else if (cmd == "stop")
                {
                    button_EmergencyStop_Click(this, new EventArgs());
                }
                else if (cmd == "yoorek")
                {
                    Random rnd = new Random();
                    string[] answer = new string[]{
                        "Słucham?", "no?", "Słucham Cię panie?"
                        ,"Tak?","Przed wyruszeniem w drogę należy zebrać drużynę!"
                        ,"Ha ha ha", "Co robi grabaż? Częstochowa. Ha ha ha he he"
                        ,"Jakiś problem?"
                        ,"Co robi blondynka pod drzewem?Czeka na autograf Kory. hehehe"
                        , "Co robi pojazd autonomiczny na torze wyścigowym? Jeździ haha"
                    };

                    //jurek.Say(answer[rnd.Next(0, answer.Length -1)]);
                }
            }
        }

        void updaterTimer_Tick(object sender, EventArgs e)
        {
            UpdateForm();
        }

        delegate void UpdateFormDelegate();
        private void UpdateForm() //TODO: in some future make it event based (not timer based)
        {
            if (label1.InvokeRequired)
            {
                // this is worker thread
                UpdateFormDelegate del = new UpdateFormDelegate(UpdateForm);
                label1.Invoke(del, new object[] {});
            }
            else
            {
                // this is UI thread
                label_DeviceManagerAction.Text = deviceManager.currentActionName;

                try
                {
                    for (int i = 0; i < deviceManager.devicesList.Count; i++)
                    {
                        if (dataGridView1.Rows.Count < i + 1)
                        {
                            dataGridView1.Rows.Add();
                        }

                        //update device name
                        if ((string)dataGridView1.Rows[i].Cells[0].Value != deviceManager.devicesList[i].ToString())
                        {
                            dataGridView1.Rows[i].Cells[0].Value = deviceManager.devicesList[i].ToString();
                        }

                        //update overall device state
                        if ((string)dataGridView1.Rows[i].Cells[1].Value != deviceManager.devicesList[i].overallState.ToString())
                        {
                            dataGridView1.Rows[i].Cells[1].Value = deviceManager.devicesList[i].overallState.ToString();
                        }

                        //update initialization device state
                        if ((string)dataGridView1.Rows[i].Cells[2].Value != deviceManager.devicesList[i].initializationState.ToString())
                        {
                            dataGridView1.Rows[i].Cells[2].Value = deviceManager.devicesList[i].initializationState.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // was crashing on program exit //TODO: catch only that one exception (not important - its only window)
                }
            }
        }

        private void UpdateButtonsVisibility()
        {
            deviceManager.evDeviceManagerOverallStateHasChanged += delegate { };
        }

        private void button_initialize_Click(object sender, EventArgs e)
        {
            if (button_initialize.Enabled) //voice recognition can "click" button even if it is disabled
            {
                //jurek.Say("Inicjalizacja sterowników.");
                deviceManager.Initialize();

                if (deviceManager.overallState != DeviceOverallState.Error)
                {
                    button_StartSensors.Enabled = true;
                    button_initialize.Enabled = false;
                }
            }
        }

        private void button_StartSensors_Click(object sender, EventArgs e)
        {
            if (button_StartSensors.Enabled)
            {
                //jurek.Say("Uruchamiam sensory.");
                deviceManager.StartSensors();

                button_StartSensors.Enabled = false;
                button_StartEffectors.Enabled = true;

            }
        }

        private void button_StartEffectors_Click(object sender, EventArgs e)
        {
            if (button_StartEffectors.Enabled)
            {
                //jurek.Say("Uruchamiam efektory.");
                deviceManager.StartEffectors();
                //jurek.Say("Pojazd gotowy do jazdy.");

                button_StartEffectors.Enabled = false;
                button_PauseEffectors.Enabled = true;
                button_EmergencyStop.Enabled = true;
            }
        }

        private void button_PauseEffectors_Click(object sender, EventArgs e)
        {
            if (button_PauseEffectors.Enabled)
            {
                //jurek.Say("Pauzuje efektory.");
                deviceManager.PauseEffectors();

                button_PauseEffectors.Enabled = false;
                button_StartEffectors.Enabled = true;
            }
        }

        private void button_EmergencyStop_Click(object sender, EventArgs e)
        {
            if (button_EmergencyStop.Enabled)
            {
                deviceManager.EmergencyStop();
                //jurek.Say("Ojej! Zatrzymać! Przycisk bezpieczeństwa użyty.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button_initialize_Click(this, new EventArgs());
            Thread.Sleep(3500);
            button_StartSensors_Click(this, new EventArgs());
            Thread.Sleep(1000);
            button_StartEffectors_Click(this, new EventArgs());
            started = true;
        }

    }
}
