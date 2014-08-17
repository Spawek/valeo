using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
  public static class DBStatsCollector
    {
        private static StatsSqlClassesDataContext db = new StatsSqlClassesDataContext();

        public static void AddNewDataToDB(
            double? curr_speed, double? target_speed, double? speed_steering,
            double? curr_angle, double? target_angle, double? angle_steering,
            double? curr_brake, double? target_brake, double? brake_steering)
        {
            log log = new log();

            log.datetime = DateTime.Now;

            log.current_speed = curr_speed;
            log.target_speed = target_speed;
            log.speed_steering = speed_steering;

            log.current_angle = curr_angle;
            log.target_angle = target_angle;
            log.angle_steering = angle_steering;

            log.current_brake = curr_brake;
            log.target_brake = target_brake;
            log.brake_steering = brake_steering;

            db.logs.InsertOnSubmit(log);

            db.SubmitChanges();
        }
        
    }
}
