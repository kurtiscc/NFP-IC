
using System;
using System.Threading;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using Windows.Services.Maps;
using Newtonsoft.Json;
using System.Net;
using Windows.Web.Http;
using Windows.Networking.Proximity;

using System.IO;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;



namespace NFP_IC.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    //struct is used for creating an object to be converted to json in the function sendDataToEndpoint
    public struct PostAddressSet
    {
        public string lat;
        public string lon;
        public string addr;
        public string userid;
        public string purpose;
        public string timestamp;

    }
    public sealed partial class ReadExistingSampleView : Page
    {

        private CancellationTokenSource _cts = null;
        private String Stringlat, Stringlng, addressString;
        private string tagUID;

        // The list of records 
        private List<NdefRecord> recordList;

        // The subscription ID from ProximityDevice
        private long subscriptionId;

        // ProximityDevice instance
        private ProximityDevice device;

        // The log content which will be filled with the details of each
        // detected NFC tag
        private string logText;

        //private readonly CoreDispatcher _dispatcher;

        public ReadExistingSampleView()
        {
            this.InitializeComponent();
            recordList = new List<NdefRecord>();
            device = ProximityDevice.GetDefault();
            device.DeviceArrived += DeviceArrived;
            device.DeviceDeparted += DeviceDeparted;
            SubscribeForMessage();
        }




        private async void DeviceDeparted(ProximityDevice sender)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                logText = logText;// + "\nLost at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "\n";
                AppText.Text = "\n" + tagUID;
            });
        }

        /// <summary>
        /// Gets called when a NFC device is detected.
        /// </summary>
        /// <param name="sender">ProximityDevice instance</param>
        private async void DeviceArrived(ProximityDevice sender)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //logText = "\nDetected at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
                logText = "Boo";
            });
        }

        /// <summary>
        /// Gets called when a message is received. Updates the UI by adding
        /// the details into the log and scrolling the log if necessary.
        /// Note that subscription for messages needs to be redone.
        /// </summary>
        /// <param name="sender">ProximityDevice instance</param>
        /// <param name="message">A message to be handled.</param>
        private async void MessageReceived(ProximityDevice sender, ProximityMessage message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (device != null)
                {
                    device.StopSubscribingForMessage(subscriptionId);
                }

                logText = ParseNDEF(message);
                //ScrollViewer.UpdateLayout();
                //ScrollViewer.ScrollToVerticalOffset(0); 
                SubscribeForMessage();
            });
        }

        /// <summary>
        /// Parses the details from the given message. The output is a string
        /// that can be appended into the log.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>The parsed details as a string.</returns>
        private string ParseNDEF(ProximityMessage message)
        {
            var output = "";

            using (var buf = DataReader.FromBuffer(message.Data))
            {
                NdefRecordUtility.ReadNdefRecord(buf, recordList);

                for (int i = 0, recordNumber = 0, spRecordNumber = 0; i < recordList.Count; i++)
                {
                    NdefRecord record = recordList.ElementAt(i);

                    if (!record.IsSpRecord)
                    {
                        if (System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength) == "Sp")
                        {
                            //output = output + "\n --End of Record No." + recordNumber; spRecordNumber = 0;
                            continue;
                        }
                        else
                        {
                            recordNumber++;
                            //output = output + "\n --Record No." + recordNumber; 
                        }
                    }
                    else
                    {
                        if (spRecordNumber == 0)
                        {
                            recordNumber++;
                            //output = output + "\n --Record No." + recordNumber; 
                        }

                        spRecordNumber++;
                        //output = output + "\n Sp sub-record No." + spRecordNumber; 
                    }

                    //output = output + "\n MB:" + ((record.Mb) ? "1;" : "0;");
                    //output = output + " ME:" + ((record.Me) ? "1;" : "0;");
                    //output = output + " CF:" + ((record.Cf) ? "1;" : "0;");
                    //output = output + " SR:" + ((record.Sr) ? "1;" : "0;");
                    //output = output + " IL:" + ((record.Il) ? "1;" : "0;");

                    string typeName = NdefRecordUtility.GetTypeNameFormat(record);

                    if (record.TypeLength > 0)
                    {
                        //output = output + "\n Type: " + typeName + ":"
                        //  + System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength); 
                    }

                    if ((record.Il) && (record.IdLength > 0))
                    {
                        output = output + "\n Id:"
                            + System.Text.Encoding.UTF8.GetString(record.Id, 0, record.IdLength);
                    }

                    if ((record.PayloadLength > 0) && (record.Payload != null))
                    {
                        if ((record.Tnf == 0x01)
                            && (System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength) == "U"))
                        {
                            NdefUriRtd uri = new NdefUriRtd();
                            NdefRecordUtility.ReadUriRtd(record, uri);
                            //output = output + "\n Uri: " + uri.GetFullUri();
                        }
                        else if ((record.Tnf == 0x01)
                            && (System.Text.Encoding.UTF8.GetString(record.Type, 0, record.TypeLength) == "T"))
                        {
                            NdefTextRtd text = new NdefTextRtd();
                            NdefRecordUtility.ReadTextRtd(record, text);
                            //output = output + "\n Language: " + text.Language;
                            //output = output + "\n Encoding: " + text.Encoding;
                            output = output + "\n TagUID: " + text.Text;
                            tagUID = text.Text;
                            Debug.WriteLine(tagUID);

                        }
                        else
                        {
                            if (record.Tnf == 0x01)
                            {
                                // output = output + "\n Payload:"
                                //   + System.Text.Encoding.UTF8.GetString(record.Payload, 0, record.Payload.Length);
                            }
                        }
                    }

                    if (!record.IsSpRecord)
                    {
                        //output = output + "\n --End of Record No." + recordNumber;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Subscribes for NDEF messages. This ensures that we get notified
        /// about the NFC events.
        /// </summary>
        private void SubscribeForMessage()
        {
            if (device != null)
            {
                recordList.Clear();
                subscriptionId = device.SubscribeForMessage("NDEF", MessageReceived);
            }
        }










        async private void Scan_Button_Clicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                // Request permission to access location
                var accessStatus = await Geolocator.RequestAccessAsync();

                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:

                        // Get cancellation token
                        _cts = new CancellationTokenSource();
                        CancellationToken token = _cts.Token;

                        // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                        Geolocator geolocator = new Geolocator { };

                        // Carry out the operation
                        Geoposition pos = await geolocator.GetGeopositionAsync().AsTask(token);
                        UpdateLocationData(pos);
                        Debug.WriteLine("You got a pos to add to UpdateLocationData");
                        break;

                    case GeolocationAccessStatus.Denied:
                        UpdateLocationData(null);
                        Debug.WriteLine("Gelocation Denied");
                        break;

                    case GeolocationAccessStatus.Unspecified:
                        UpdateLocationData(null);
                        Debug.WriteLine("Geolocation Unspecified");
                        break;
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {

            }
            finally
            {
                _cts = null;
            }
        }

        async private void UpdateLocationData(Geoposition position)
        {
            if (position == null)
            {
                Stringlat = "";
                Stringlng = "";
                Debug.WriteLine("Position is null");
            }
            else
            {
                Stringlat = position.Coordinate.Point.Position.Latitude.ToString();
                Stringlng = position.Coordinate.Point.Position.Longitude.ToString();
                Debug.WriteLine("Good Job. You did it");
                Debug.WriteLine(Stringlat);
                Debug.WriteLine(Stringlng);

                // The location to reverse geocode.
                BasicGeoposition location = new BasicGeoposition();
                Double lat;
                Double lng;
                lat = Convert.ToDouble(Stringlat);
                lng = Convert.ToDouble(Stringlng);
                location.Latitude = lat;
                location.Longitude = lng;
                Geopoint pointToReverseGeocode = new Geopoint(location);

                // Reverse geocode the specified geographic location.
                MapLocationFinderResult result =
                      await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode);

                // If the query returns results, display the name of the town
                // contained in the address of the first result.
                if (result.Status == MapLocationFinderStatus.Success)
                {
                    addressString = result.Locations[0].Address.StreetNumber + " " + result.Locations[0].Address.Street + " " + result.Locations[0].Address.Town
                        + " " + result.Locations[0].Address.Region + " " + result.Locations[0].Address.PostCode;
                    Debug.WriteLine(addressString);
                    sendDataToEndpoint(addressString, Stringlat, Stringlng);
                }

            }
        }

        private async void sendDataToEndpoint(String address, String lat, String lng)
        {
            //Creating JSON
            string tagID = "F2DDBA18000104E0";
            PostAddressSet tempSet = new PostAddressSet();
            tempSet.addr = address;
            tempSet.lat = lat;
            tempSet.lon = lng;
            tempSet.purpose = "Testing";
            tempSet.userid = "admin";
            tempSet.timestamp = getCurrentTimeStamp();
            string json = JsonConvert.SerializeObject(tempSet);

            //POSTing
            string serverAddress = "http://nfp-project.azurewebsites.net/addtrackingstep/" + tagID;
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod("POST"), new Uri(serverAddress));
            msg.Content = new HttpStringContent(json);
            msg.Content.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await httpClient.SendRequestAsync(msg).AsTask();
        }
        private string getCurrentTimeStamp()
        {
            var utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            var formattedOffset = utcOffset.ToString("hhmm");
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff'-'" + formattedOffset);
            return timestamp;
        }
    }
}
