
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
        public string purpose;
        public string timestamp;

    }
    public sealed partial class ReadExistingSampleView : Page
    {

        private CancellationTokenSource _cts = null;
        private String Stringlat, Stringlng, addressString;

        public ReadExistingSampleView()
        {
            this.InitializeComponent();
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
                        Geolocator geolocator = new Geolocator {};

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
                    addressString = result.Locations[0].Address.StreetNumber +  " " + result.Locations[0].Address.Street + " " + result.Locations[0].Address.Town
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
            tempSet.timestamp = getCurrentTimeStamp();
            string json = JsonConvert.SerializeObject(tempSet);

            //POSTing
            string serverAddress = "http://nfp-project.azurewebsites.net/addtrackingstep/"+tagID;
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
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff'-'"+ formattedOffset);
            return timestamp;
        }
    }
}
