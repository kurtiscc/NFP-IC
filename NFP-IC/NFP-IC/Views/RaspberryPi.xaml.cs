using System;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.Foundation;

namespace NFP_IC.Views
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RaspberryPi : Page
    {
        private void Scan_Button_Clicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            startTheMagic();
        }


        async private void startTheMagic()
        {
            //var nfcReader = SerialDevice.GetDeviceSelector("COM3");
            //var devices = await DeviceInformation.FindAllAsync(nfcReader);
            
            //if (devices.Any())
            //{
            //    var deviceId = devices.First().Id;
            //    this.device = await SerialDevice.FromIdAsync(deviceId);

            //    if (this.device != null)
            //    {
            //        this.device.BaudRate = 115200;
            //        this.device.StopBits = SerialStopBitCount.One;
            //        this.device.DataBits = 8;
            //        this.device.Parity = SerialParity.None;
            //        this.device.Handshake = SerialHandshake.None;

            //        this.reader = new DataReader(this.device.InputStream);
            //    }
            //}

            SerialPort mySerialPort = new SerialPort("COM3");
            mySerialPort.BaudRate = 115200;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.Open();

            //var deviceSelector = SerialDevice.GetDeviceSelectorFromUsbVidPid(0x2341, 0x0043);
            //SerialDevice.FromIdAsync

            while (true)
            {

                string re1 = "((?:[a-z][a-z]+))";
                string txt = getTagID(mySerialPort);
                Regex r = new Regex(re1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = r.Match(txt);
                if (!m.Success)
                {
                    Console.WriteLine(txt);
                }

            }
        }

        public static string getTagID(SerialPort mySerialPort)
        {


            string theString = "";
            string[] strArray;

            //Write each of these commands to the scanner and then sleep
            mySerialPort.Write("010A0003041000200000");
            System.Threading.Thread.Sleep(100); // need to change to   await Task.Delay(TimeSpan.FromSeconds(100)); and function needs to be async and return a task or void

            mySerialPort.Write("010C00030410002101000000");
            System.Threading.Thread.Sleep(100);

            mySerialPort.Write("0109000304F0000000");
            System.Threading.Thread.Sleep(100);

            mySerialPort.Write("0109000304F1FF0000");
            System.Threading.Thread.Sleep(100);

            //Write this command to get the tagID
            mySerialPort.Write("010B000304142401000000");
            System.Threading.Thread.Sleep(100);

            //Receive all the input and output of the scanner
            //Including the tagID
            //Turn output into array for easier access to the tagID
            theString = getString(mySerialPort);
            strArray = theString.Split('\r');

            //Remove the uneeded characters
            //Take remaining output and get just the tagID
            char[] tocut = { '[', ']', '\n' };
            theString = strArray[strArray.Length - 2].Trim(tocut);
            strArray = theString.Split(',');

            //Return the tagID
            return strArray[0];
        }
        public static string getString(SerialPort mySerialPort)
        {
            string myString = "";
            int bytes = mySerialPort.BytesToRead;
            //create a byte array to hold the awaiting data
            byte[] serialBuffer = new byte[bytes];
            //read the data and store it
            mySerialPort.Read(serialBuffer, 0, bytes);
            //display the data to the user
            myString = System.Text.Encoding.Default.GetString(serialBuffer);
            return myString;
        }
    }
}
