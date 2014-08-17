using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSimulator
{
    //http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html <- can be usefull

    //http://www.carfolio.com/specifications/models/car/?car=140591 <- more spec but for different model

    // http://carsmind.com/specification.php?make=TOYOTA&model=Yaris - technical spec
    /*
        FOR NOW LET'S TAKE THIS ONE:
        
        Technical specification TOYOTA Yaris 1.3 2008Make:	TOYOTA
        Model:	Yaris 1.3
        Year:	2008
        Car category:	Small / Economy Cars
        Car engine position:	Front
        Car engine:	1298 ccm (78,80 cubic inches)
        Car engine type:	in-line, 4-cyl
        Car valves per cylinder:	4
        Car max power:	87.00 PS (63,68 kW or 85,57 HP) at 6000 Rev. per min.
        Car max torque:	122.00 Nm (12,34 kgf-m or 89,55 ft.lbs) at 4200 Rev. per min.
        Car bore stroke:	72.0 x 79.7 mm (2,79 x 3.1 inches)
        Car compression:	10.0:1
        Car top speed:	175.0 km/h (108,16 mph)
        Car fuel:	Gasoline, unleaded 95
        Car transmission:	Manual, 5-speed
        Car power per weight:	0.0888 PS/kg
        0 100 km h 0 62 mph:	12.1 seconds
        Car dr
        Car seats:	5
        Car passenger space:	2662 litres (699,49 gallons)
        Car doors:	3
        Car front tire:	175/65-R14
        Car rear tire:	175/65-R14
        Car chassis:	Hatchback
        Car co2 emissions:	165.0 g/km
        Car turn circle:	10 m (383,87 inches)
        Car weight:	984 kg (2158,45 pounds)
        Car towing weight:	906 kg (1987,41 pounds)
        Car total length:	3650 mm (142,98 inches)
        Car total width:	1670 mm (65,37 inches)
        Car total height:	1510 mm (59,10 inches)
        Car max weight with load:	1368 kg (3000,82 pounds)
        Car wheelbase:	2380 mm (93,23 inches)
        Car cooling:	Liquid
        Car front brakes type:	Ventilated disks
        Car rear brakes type:	Disks
        Car cargo space:	205 litres (53,73 gallons)
        Car lubrication:	Wet sump
        Car aerodynamic dragcoefisient:	1
        Car fuel with mixed drive:	5.8 litres/100 km (40,36 miles per gallon)
        Car fuel tank capacity:	45.0 litres (11,83 gallons)
    */

    /* 
        TORQUE AND POWER DIAGRAM:
        
        http://rototest-research.eu/popup/performancegraphs.php?ChartsID=582
      
        also avitable in .pdf file in projects dir 
     */


    /*
     * GEARS:
     * http://en.wikipedia.org/wiki/Toyota_C_transmission
     * 
     * z wiki:
     * 1st	    2nd	     3rd	4th	     5th    Reverse	 Final
     * 3.545	1.913	1.310	1.027	0.850	3.214	 3.550
     * 
     * odwrotnosc (1/x) tego z wiki:
     * 1 -> 0,28208744710860366713681241184767
     * 2 -> 0,52273915316257187663355985363304
     * 3 -> 0,76335877862595419847328244274809
     * 4 -> 0,97370983446932814021421616358325
     * 5 -> 1,1764705882352941176470588235294
     * 
     * Final =  Differential Ratio  = 1 / 3.550 = 0,28169014084507042253521126760563
     */


    public class ToyotaYaris : CarModel
    {
        private List<EnginePointStats> __ENGINE_STATS__ = new List<EnginePointStats>()     
        {
            new EnginePointStats(1493, 13500, 86.1),
            new EnginePointStats(2010, 20200, 96.1),
            new EnginePointStats(2508, 27000, 102.8),
            new EnginePointStats(3010, 33200, 105.2),
            new EnginePointStats(3508, 41400, 112.6),
            new EnginePointStats(4017, 47500, 112.8),
            new EnginePointStats(4209, 50200, 113.8),
            new EnginePointStats(4416, 52800, 114.3),
            new EnginePointStats(4616, 54800, 113.6),
            new EnginePointStats(5011, 55800, 106.3),
            new EnginePointStats(5512, 57500, 99.6),
            new EnginePointStats(5811, 57300, 94.2),
            new EnginePointStats(6006, 56200, 89.4),
            new EnginePointStats(6203, 56100, 86.4)
        };
        public override List<EnginePointStats> EngineStats { get { return __ENGINE_STATS__; } }
        private double[] __GEAR_TRANMISSIONS_RATIOS__ = new double[]{
                0.2820874, // gear 1
                0.5227392, // 2
                0.7633588, // 3
                0.9737099, // 4
                1.1764705  // 5
        };
        public override double[] GearTransmissionRatios { get { return __GEAR_TRANMISSIONS_RATIOS__; } }

        public override double DifferentialRatio { get { return 1.0 / 3.550; } }
        public override int MaxGear { get { return 5; } }
        public override double StaticEngineResistanceForces { get { return 10.0; } }
        public override double DynamicEngineResistancePerRPM { get { return 0.0009; } }
        public override double EngineMomentum { get { return 8.0; } } //TODO: its actually random value
        public override double Torque { get { return this.GetTorque(RPM); } }
        public override double Power { get { throw new NotImplementedException(); } } //NOTE: I think power is not needed to do anything in a car
        public override double WheelRadius { get { return 14.0 * 2.54 / 2 / 100 + 0.65 * 0.175; } } // = 0,29155m //in meters // wheel: 175/65-R14
        public override double MaxEngineRPM { get { return 7000.0; } }
        public override double Mass { get { return 984.0; } }
        public override double WheelMomentum { get { return 1.0 / 2.0 * WheelMass * WheelRadius * WheelRadius; } } // 1/2 M * R^2 for cylinder //TODO: calculate it better
        public override double WheelsNo { get { return 4.0; } }
        public override double WheelMass { get { return 6.5 + 6.5; } } //TODO: find true data //mass of wheel and tire
        public override double CarDragCoeffcient { get { return 0.29; } } //from: http://en.wikipedia.org/wiki/Automobile_drag_coefficient
        public override double Width { get { return 1.66; } } //from: http://en.wikipedia.org/wiki/Toyota_Vitz
        public override double Height { get { return 1.51; } }
        public override double TyrePresure { get { return 1.7; } } //TODO: CHECK IT!!! 
        public override double MaxBreakingForcePerWheel { get { return 10000.0; } }  //TODO: its complately random value, but seems legit (excluding sliding)
        public override int BrakingWheelsNo { get { return 2; } } //only front wheels are breaking
        public override int AcceleratingWheelsNo { get { return 2; } }
        public override bool IsGearBoxAutomatic { get { return true; } }
        public override double RpmToRaiseGearOnAutomaticGearbox { get { return 5300; } } //TODO: IMPORTANT: It is complately random value
        public override double RpmToLowerGearOnAutomaticGearbox { get { return 2000; } } //TODO: IMPORTANT: It is complately random value

        public ToyotaYaris()
        {
            //tarcie guma-asfalt bazujac na SLABYCH zrodlach z neta //TODO: find some real data
            StaticFrictionFactor = 0.9;
            KineticFrictionFactor = 0.6;
        }

        public override void Start()
        {
            RPM = 1000;
        }


    }
}
