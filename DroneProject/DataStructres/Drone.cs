using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneProject.DataStructres
{
    public class Drone
    {

        public string id { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public double Price { get; set; }

        
        public Battery[] Batteries { get; set; }
        public Camera[] Cameras { get; set; }


        public Drone(string model, int year, double price, Battery[] batteries, Camera[] cameras)
        {
            Model = model;
            Year = year;
            Price = price;
            Batteries = batteries;
            Cameras = cameras;
        }
        public static List<Drone> ConvertFromStringToListOfObject(string DataAsStr)
        {
            if (string.IsNullOrEmpty(DataAsStr)) return new List<Drone>();
            return System.Text.Json.JsonSerializer.Deserialize<List<Drone>>(DataAsStr);
        }
        public override string ToString()
        {
            string result = "Drone Information:\n";
            result += "ID: " + (string.IsNullOrEmpty(id) ? "No ID" : id) + "\n";
            result += "Model: " + (string.IsNullOrEmpty(Model) ? "No model" : Model) + "\n";
            result += "Year: " + Year + "\n";
            result += "Price: " + Price + "\n";

            // Batteries
            if (Batteries == null || Batteries.Length == 0)
            {
                result += "No batteries in this drone.\n";
            }
            else
            {
                result += "Batteries:\n";
                for (int i = 0; i < Batteries.Length; i++)
                {
                    Battery battery = Batteries[i];
                    if (battery == null)
                    {
                        result += $"  Battery {i + 1}: NULL\n";
                    }
                    else
                    {
                        result += $"  Battery {i + 1}: Type={battery.Type}, Capacity={battery.Capacity}mAh, ChargeTime={battery.ChargeTime}h\n";
                    }
                }
            }

            // Cameras
            if (Cameras == null || Cameras.Length == 0)
            {
                result += "No cameras in this drone.\n";
            }
            else
            {
                result += "Cameras:\n";
                for (int i = 0; i < Cameras.Length; i++)
                {
                    Camera camera = Cameras[i];
                    if (camera == null)
                    {
                        result += $"  Camera {i + 1}: NULL\n";
                    }
                    else
                    {
                        result += $"  Camera {i + 1}: Resolution={(string.IsNullOrEmpty(camera.Resolution) ? "Unknown" : camera.Resolution)}, " +
                                  $"FrameRate={camera.FrameRate}fps, LensType={(string.IsNullOrEmpty(camera.LensType) ? "Unknown" : camera.LensType)}\n";
                    }
                }
            }

            return result;
        }



    }
    public class Battery
    {
        public string Type { get; set; }
        public double Capacity { get; set; } 
        public double ChargeTime { get; set; } 

        public Battery(string type, double capacity, double chargeTime)
        {
            Type = type;
            Capacity = capacity;
            ChargeTime = chargeTime;
        }
    }
    public class Camera
    {
        public string Resolution { get; set; }
        public double FrameRate { get; set; }
        public string LensType { get; set; }

        public Camera(string resolution, double frameRate, string lensType)
        {
            Resolution = resolution;
            FrameRate = frameRate;
            LensType = lensType;
        }
    }



}
