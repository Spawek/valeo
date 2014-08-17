using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CarSimulator
{
    class EngineSimulator
    {
        public CarModel model;

        const double SIMULATION_TIMER_INTERVAL_IN_MS = 10.0;
        Timer SimulationTimer = new Timer(SIMULATION_TIMER_INTERVAL_IN_MS);
        
        public EngineSimulator(CarModel _model)
        {
            model = _model;

            SimulationTimer.Elapsed += SimulationTimer_Elapsed;
            SimulationTimer.Start();
        }

        void SimulationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            model.CalculationsTick();
        }
    }
}
