//Quelle https://www.nuget.org/packages/OpenHardwareMonitorLib / https://github.com/HardwareMonitor/OpenHardwareMonitor
//Liefert u.a. diese Werte:
//  Hardware: AMD Ryzen 7 7800X3D
//  Auslastung
//  	Sensor: Total, value: 6,13
//      Sensor: Core Max, value: 25

//  Aggregiert: Temperatur, Taktfrequenz, Auslastung
//      Sensor: Cores (Average), value: 3353
//      Sensor: Cores (Average Effective), value: 352

//  unklar, ob Temperatur oder Leistung
//      Sensor: Package, value: 23,163925

//  Temperatur
//      Sensor: Core (Tctl / Tdie), value: 30
//      Sensor: CCD1 (Tdie), value: 22,750002

//  Hardware: Virtual Memory
//      Sensor: Used, value: 10,544212
//      Sensor: Available, value: 23,745098
//      Sensor: Processes, value: 182
//      Sensor: Threads, value: 3128
//      Sensor: Handles, value: 86788

//  Hardware: Physical Memory
//      Sensor: Used, value: 5,6003876
//      Sensor: Available, value: 25,563923
//      Sensor: Total, value: 31,16431
//      Sensor: Memory, value: 17,970512




using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Hardware.Cpu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Nodes;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyHwMon1
{
    /// <summary>
    /// A comment
    /// </summary>

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }


    public sealed partial class MainWindow : Window
    {
        HashSet<string> sensornames = [
                "Total",
//unklar, was da angezeigt wird:                "Core Max",
                "Package",//unklar, ob Temperatur oder, wahrscheinlicher, Leistung
                "Cores (Average)",
                "Cores (Average Effective)",
                "Core (Tctl/Tdie)",
                "CCD1 (Tdie)"
                    ];

        private string sSettingsFile;
        private string sSensorsFile;


        // Die Liste, die an das UI gebunden ist
        public ObservableCollection<string> SensorValues { get; } = new();
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            // Event-Handler registrieren
            this.AppWindow.Changed += AppWindow_Changed;

            this.AppWindow.Resize(new SizeInt32(260, 220));


            // Initial 14 Texte generieren
            UpdateData();

            // Timer für den Minutentakt einrichten
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateData();
            _timer.Start();

            // Vollständiger Pfad (z.B. C:\Pfad\MeineApp.exe)

            //Der Null-Coalescing-Operator (??) stellt sicher, dass fullPath niemals null ist.
            string fullProcessPath = Environment.ProcessPath ?? string.Empty; 

            // Null-Conditional-Operator(?.) sichert ab:
            //Wenn MainModule null ist, wird der restliche Teil der Kette übersprungen und dank ?? der Ersatzwert (leerer String) zugewiesen. 
            string fullPath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;


            // Nur der Name mit Endung (z.B. MeineApp.exe)
            string exeName = Path.GetFileName(fullPath);

            // Nur der Name ohne Endung (z.B. MeineApp)
            string exeNameOnly = Path.GetFileNameWithoutExtension(fullPath);

            // Pfad: C:\Users\<Name>\AppData\Local\<App>\window_pos.json
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), exeNameOnly);
            Directory.CreateDirectory(folder); // Ordner erstellen, falls nicht vorhanden
            sSettingsFile = Path.Combine(folder, $"{exeNameOnly}_settings.json");
            sSensorsFile = Path.Combine(folder, $"{exeNameOnly}_sensors.json");

            //einmal alle Sensoren wegschreiben
            WriteAllSensorNames();

            // Initial füllen
            UpdateData();
        }

        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = false,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsBatteryEnabled = false,
            IsStorageEnabled = false
        };

        Computer computerAll = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsBatteryEnabled = true,
            IsStorageEnabled = true
        };

        private void WriteAllSensorNames()
        {
            try
            {
                var jSensors = new JsonObject();
                int iSensor = 1;

                computerAll.Open(false);
                computerAll.Accept(new UpdateVisitor());
                foreach (IHardware hardware in computerAll.Hardware)
                {
                    System.Diagnostics.Debug.WriteLine("Hardware: {0}", hardware.Name);
                    foreach (IHardware subhardware in hardware.SubHardware)
                    {
                        System.Diagnostics.Debug.WriteLine("\tSubhardware: {0}", subhardware.Name);
                        foreach (ISensor sensor in subhardware.Sensors)
                        {
                            System.Diagnostics.Debug.WriteLine("\t\tSub-Sensor: {0}, value: {1}", sensor.Name, sensor.Value);
                            jSensors.Add($"{iSensor++}--{sensor.Name}", sensor.Value);
                        }
                    }

                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        System.Diagnostics.Debug.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                        jSensors.Add($"{iSensor++}--{sensor.Name}", sensor.Value);
                    }
                }

                computerAll.Close();

                var jDoc = new JsonObject();
                jDoc.Add("CpuSensors", jSensors);
                File.WriteAllText(sSensorsFile, jDoc.ToString());
            }
            catch (Exception ex)
            {
                // Allgemeiner Fehlerauffang
                System.Diagnostics.Debug.WriteLine($"Unerwarteter Fehler: {ex.Message}");
            }
        }

        private void UpdateData()
        {
            SensorValues.Clear();
            var now = DateTime.Now.ToString("HH:mm:ss");

            int iTctl = 0;
            int iTdie = 0;
            double dLeistung = 0.0;
            double dAuslastung = 0.0;
            double dAggr = 0;
            double dAggrEff = 0;



            computer.Open(false);
            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware)
            {
                Console.WriteLine("Hardware: {0}", hardware.Name);
                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    Console.WriteLine("\tSubhardware: {0}", subhardware.Name);
                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        if (sensornames.Contains(sensor.Name))
                            Console.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                    }
                }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    string sSensor = sensor.Name.Trim();
                    if (sensornames.Contains(sSensor) == true)
                        //Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                        if(sSensor.Contains("(Tdie"))
                        {
                            iTdie = (int)(sensor.Value ?? -99); //  ?? -99 verhindert die Warnung warning CS8629: Ein Werttyp, der NULL zulässt, kann NULL sein.
                            SensorValues.Add($"Tdie:\t\t{iTdie}°C");
                        }
                        else if (sSensor.Contains("(Tctl"))
                        {
                            iTctl = (int)(sensor.Value ?? -99);
                            SensorValues.Add($"Tctl/Tdie:\t{iTctl}°C");
                        }
                        else if (sSensor.Contains("Total"))
                        {
                            dAuslastung = Math.Round((double)(sensor.Value ?? -99.0), 1);
                            SensorValues.Add($"Auslastung:\t{dAuslastung}%");

                        }
                        else if (sSensor.Contains("age)")) //Cores (Average)
                        {
                            dAggr = Math.Round((double)(sensor.Value ?? -99.0), 2);
                            SensorValues.Add($"Cores aggr:\t{dAggr}");
                        }
                        else if (sSensor.Contains("age Eff"))   //Cores (Average Effective)
                        {
                            dAggrEff = Math.Round((double)(sensor.Value ?? -99.0), 2);
                            SensorValues.Add($"Cores aggr eff: {dAggrEff}");
                        }
                        else if (sSensor.Contains("Package"))
                        {
                            dLeistung = Math.Round((double)(sensor.Value ?? -99.0), 1);
                            SensorValues.Add($"Leistung?:\t{dLeistung}W");
                        }

                        else
                        {
                            SensorValues.Add($"{sensor.Name}: {sensor.Value}");
                        }
                }
            }

            computer.Close();

            WriteDb(dAuslastung, dLeistung, dAggr, dAggrEff, iTctl, iTdie);
        }

        private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
        {
            // Prüfen, ob die Position verschoben wurde
            if (args.DidPositionChange)
            {
                // Prüfen, ob das Fenster minimiert ist
                if (sender.Presenter is OverlappedPresenter overlapped &&
                    overlapped.State == OverlappedPresenterState.Minimized)
                {
                    // Position ignorieren, da das Fenster "nach -32000 verschoben" wurde
                    return;
                }

                var newPosition = sender.Position;

                System.Diagnostics.Debug.WriteLine($"Fenster verschoben nach: X={newPosition.X}, Y={newPosition.Y}");

                var jPos = new JsonObject();
                jPos.Add("PosX", sender.Position.X.ToString());
                jPos.Add("PosY", sender.Position.Y.ToString());
                var jDoc = new JsonObject();
                jDoc.Add("MainWindow", jPos);
                File.WriteAllText(sSettingsFile, jDoc.ToString());
            }
        }
    }
}
