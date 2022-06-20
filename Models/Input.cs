using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise
{
    internal class Input : ICloneable
    {
        #region Properties
        /*
        *   NOTES ON INPUT VARIABLES: 
        *      UT, Local Time, and Longitude are used independently in the
        *      model and are not of equal importance for every situation.  
        *      For the most physically realistic calculation these three
        *      variables should be consistent (lst=sec/3600 + g_long/15).
        *      The Equation of Time departures from the above formula
        *      for apparent local time can be included if available but
        *      are of minor importance.
        *
        *      f107 and f107A values used to generate the model correspond
        *      to the 10.7 cm radio flux at the actual distance of the Earth
        *      from the Sun rather than the radio flux at 1 AU. The following
        *      site provides both classes of values:
        *      ftp://ftp.ngdc.noaa.gov/STP/SOLAR_DATA/SOLAR_RADIO/FLUX/
        *
        *      f107, f107A, and ap effects are neither large nor well
        *      established below 80 km and these parameters should be set to
        *      150., 150., and 4. respectively.
        */
        int year;           //Year (Not used)
        int day_of_year;   //Day of the year
        double seconds;     //Seconds in the day (UT - Universal Time)
        double altitude;    //Altitude (KM)
        double g_lat;       //Geodetic latitude
        double g_long;      //Geodetic longitude
        double lst;         //Local apparent solar time (hours) (view note above)
        double f107A;       //81 day average of F10.7 flux (center on doy)
        double f107;        //Daily F10.7 flux for previous day
        double ap;          //Magnetic index (daily)
        AP ap_array;
        #endregion

        #region Constructor
        public Input(int year, int doy, double sec, double alt, double g_lat, double g_long, double lst, double f107A, double f107, double ap)
        {
            this.year = year;
            this.day_of_year = doy;
            this.seconds = sec;
            this.altitude = alt;
            this.g_lat = g_lat;
            this.g_long = g_long;
            this.lst = lst;
            this.f107A = f107A;
            this.f107 = f107;
            this.ap = ap;
            this.ap_array = new AP();
        }

        public Input() { }
        #endregion

        #region Getters & Setters
        public int Year
        {
            get { return year; }
            set { year = value; }
        }

        public int DayOfYear
        {
            get { return day_of_year; }
            set { day_of_year = value; }
        }

        public double Seconds
        {
            get { return seconds; }
            set { seconds = value; }
        }

        public double Altitude
        {
            get { return altitude; }
            set { altitude = value; }
        }

        public double G_lat
        {
            get { return g_lat; }
            set { g_lat = value; }
        }

        public double G_long
        {
            get { return g_long; }
            set { g_long = value; }
        }

        public double Lst
        {
            get { return lst; }
            set { lst = value; }
        }

        public double F107A
        {
            get { return f107A; }
            set { f107A = value; }
        }

        public double F107
        {
            get { return f107; }
            set { f107 = value; }
        }

        public double Ap
        {
            get { return ap; }
            set { ap = value; }
        }

        public AP Ap_array
        {
            get { return ap_array; }
            set { ap_array = value; }
        }
        #endregion

        #region Cloning
        public Object Clone()
        {
            Input input = new Input();
            input.Altitude = this.altitude;
            input.DayOfYear = this.day_of_year;
            input.Seconds = this.seconds;
            input.Altitude = this.altitude;
            input.G_lat = this.g_lat;
            input.G_long = this.g_long;
            input.Lst = this.lst;
            input.F107A = this.f107A;
            input.F107 = this.f107;
            input.Ap = this.ap;
            input.Ap_array = this.ap_array;
            return input;
        }
        #endregion
    }
}
