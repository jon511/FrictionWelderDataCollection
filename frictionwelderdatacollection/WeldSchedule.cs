using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrictionWelderDataCollection
{
    public class WeldSchedule
    {
        private int _rpm;

        public int rpm
        {
            get { return _rpm; }
            set { _rpm = value; }
        }

        public string RPM
        {
            get { return "Weld Speed " + _rpm.ToString() + " rpm"; }
        }

        private int _scrubPressure;

        public int scrubPressure
        {
            get { return _scrubPressure; }
            set { _scrubPressure = value; }
        }

        public string ScrubPressure
        {
            get { return "Scrub Pressure " + _scrubPressure.ToString() + " psi"; }
        }

        private int _burnPressure;

        public int burnPressure
        {
            get { return _burnPressure; }
            set { _burnPressure = value; }
        }

        public string BurnPressure
        {
            get { return "Burn Pressure " + _burnPressure.ToString() + " psi"; }
        }

        private int _forgePressure;

        public int forgePressure
        {
            get { return _forgePressure; }
            set { _forgePressure = value; }
        }

        public string ForgePressure
        {
            get { return "Forge Pressure " + _forgePressure.ToString() + " psi"; }
        }

        private double _scrubTime;

        public double scrubTime
        {
            get { return _scrubTime; }
            set { _scrubTime = value; }
        }

        public string ScrubTime
        {
            get { return "Scrub Time " + _scrubTime.ToString() + " sec"; }
        }

        private double _burnTime;

        public double burnTime
        {
            get { return _burnTime; }
            set { _burnTime = value; }
        }

        public string BurnTime
        {
            get { return "Burn Time " + _burnTime.ToString() + " sec"; }
        }

        private double _forgeTime;

        public double forgeTime
        {
            get { return _forgeTime; }
            set { _forgeTime = value; }
        }

        public string ForgeTime
        {
            get { return "Forge Time " + _forgeTime.ToString() + " sec"; }
        }

        public bool compare(WeldSchedule ws)
        {
            if (ws.rpm != _rpm)
            {
                return false;
            }

            if (ws.scrubPressure != _scrubPressure)
            {
                return false;
            }

            if (ws.scrubTime != _scrubTime)
            {
                return false;
            }

            if (ws.burnPressure != _burnPressure)
            {
                return false;
            }

            if (ws.burnTime != _burnTime)
            {
                return false;

            }

            if (ws.forgePressure != _forgePressure)
            {
                return false;
            }

            if (ws.forgeTime != _forgeTime)
            {
                return false;
            }

            return true;
        }

    }
}
