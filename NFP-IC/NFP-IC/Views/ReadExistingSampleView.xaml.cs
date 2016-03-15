
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

namespace NFP_IC.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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

        private void sendDataToEndpoint(String address, String lat, String lng)
        {
            // This one is for you Dmitry
        }
    }
}
