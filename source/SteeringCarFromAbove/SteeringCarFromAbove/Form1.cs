using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteeringCarFromAbove
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            TrackPlanner planner = new TrackPlanner(
            locationTolerance: 9.0d, angleTolerance: 19.0d,
            positionStep: 10.0d, angleStep: 20.0d,
            mapSizeX: 1000.0d, mapSizeY: 1000.0d);
            Map map = new Map(2, 2, 2);

            map.car = new PositionAndOrientation(_x: 100.0d, _y: 100.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 900.0d, _y: 900.0d, _angle: 90.0d);

            List<PositionAndOrientation> track = planner.PlanTrack(map);

            Application.Exit();
            InitializeComponent();
        }
    }
}
