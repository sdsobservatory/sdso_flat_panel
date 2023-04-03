using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace ASCOM.SDSO
{
    [Guid("c51ca245-0877-47b9-8e9b-37d35e9791f3")]
    [ClassInterface(ClassInterfaceType.None)]
    public class CoverCalibrator : ICoverCalibratorV1
    {
        internal static readonly string driverID = "ASCOM.SDSO.CoverCalibrator";

        private static readonly string driverName = "SDSO Flat Panel";
        private static readonly string driverDescription = "SDSO Flat Panel";
        private static readonly Version driverVersion = new Version(1, 0);

        internal static readonly string comPortProfileName = "COM Port";
        internal static readonly string comPortDefault = "COM1";
        internal static readonly string traceStateProfileName = "Trace Level";
        internal static readonly string traceStateDefault = "false";

        internal static string comPort;

        private Serial serial;
        private CalibratorStatus calibratorStatus = CalibratorStatus.Unknown;
        private int brightness;
        
        internal TraceLogger tl;

        public CoverCalibrator()
        {
            tl = new TraceLogger("", "SDSO");
            serial = new Serial();

            ReadProfile();
        }

        #region Common properties and methods.

        public void SetupDialog()
        {
            ReadProfile();

            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile();
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }
        public void CommandBlind(string command, bool raw)
        {
            CheckConnected(nameof(CommandBlind));
            throw new MethodNotImplementedException(nameof(CommandBlind));
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected(nameof(CommandBool));
            throw new MethodNotImplementedException(nameof(CommandBool));
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected(nameof(CommandString));
            throw new MethodNotImplementedException(nameof(CommandString));
        }

        public void Dispose()
        {
            serial.Connected = false;
            serial.Dispose();
            serial = null;

            tl.Enabled = false;
            tl.Dispose();
            tl = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    LogMessage("Connected Set", "Connecting to port {0}", comPort);
                    serial.PortName = comPort;
                    serial.Speed = SerialSpeed.ps115200;
                    serial.Connected = true;
                    CalibratorState = CalibratorStatus.NotReady;

                    CalibratorOff();
                }
                else
                {
                    CalibratorOff();

                    LogMessage("Connected Set", "Disconnecting from port {0}", comPort);
                    serial.Connected = false;
                }
            }
        }

        public string Description
        {
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = GetType().Assembly.GetName().Version;
                string driverInfo = "Version: " + string.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }
        
        public string DriverVersion
        {
            get
            {
                string version = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", driverVersion.Major, driverVersion.Minor);
                tl.LogMessage("DriverVersion Get", version);
                return version;
            }
        }

        public short InterfaceVersion
        {
            get
            {
                LogMessage("InterfaceVersion Get", "1");
                return 1;
            }
        }

        public string Name
        {
            get
            {
                tl.LogMessage("Name Get", driverName);
                return driverName;
            }
        }

        #endregion

        #region ICoverCalibrator Implementation

        public CoverStatus CoverState
        {
            get
            {
                tl.LogMessage("CoverState Get", "Not present");
                return CoverStatus.NotPresent;
            }
        }

        public void OpenCover()
        {
            tl.LogMessage("OpenCover", "Not implemented");
            throw new MethodNotImplementedException("OpenCover");
        }

        public void CloseCover()
        {
            tl.LogMessage("CloseCover", "Not implemented");
            throw new MethodNotImplementedException("CloseCover");
        }

        public void HaltCover()
        {
            tl.LogMessage("HaltCover", "Not implemented");
            throw new MethodNotImplementedException("HaltCover");
        }

        public CalibratorStatus CalibratorState
        {
            get
            {
                if (!serial.Connected) return CalibratorStatus.Unknown;
                return calibratorStatus;
            }
            private set => calibratorStatus = value;
        }

        public int Brightness
        {
            get
            {
                tl.LogMessage("Brightness Get", $"{brightness}");
                return brightness;
            }
        }

        public int MaxBrightness
        {
            get
            {
                tl.LogMessage("MaxBrightness Get", "1000");
                return 1000;
            }
        }

        public void CalibratorOn(int Brightness)
        {
            CheckConnected(nameof(CalibratorOn));

            if (Brightness < 0)
                throw new InvalidValueException(nameof(CalibratorOn), $"{Brightness}", $"0-{MaxBrightness}");
            else if (Brightness > MaxBrightness)
                throw new InvalidValueException(nameof(CalibratorOn), $"{Brightness}", $"0-{MaxBrightness}");
            
            brightness = Brightness;
            CalibratorState = CalibratorStatus.NotReady;

            serial.TransmitBinary(Encoding.ASCII.GetBytes($"set {brightness}\n"));
            var _ = serial.ReceiveTerminatedBinary(new byte[] { 0x0a });
            serial.ClearBuffers();

            CalibratorState = CalibratorStatus.Ready;
        }

        public void CalibratorOff()
        {
            CheckConnected(nameof(CalibratorOff));

            brightness = 0;

            serial.TransmitBinary(Encoding.ASCII.GetBytes($"off\n"));
            var _ = serial.ReceiveTerminatedBinary(new byte[] { 0x0a });
            serial.ClearBuffers();

            CalibratorState = CalibratorStatus.Off;
        }

        #endregion

        #region Private properties and methods

        #region ASCOM Registration

        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "CoverCalibrator";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        private bool IsConnected
        {
            get
            {
                return serial.Connected;
            }
        }

        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
            }
        }

        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "CoverCalibrator";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                if (!string.IsNullOrWhiteSpace(comPort))
                    driverProfile.WriteValue(driverID, comPortProfileName, comPort);
            }
        }

        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
        #endregion
    }
}
