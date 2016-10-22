using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.UI.Core;
using ReactiveUI;
using App2;

namespace App2
{
    public class ViewModel : ReactiveObject
    {            
        private readonly Barometer _barometer;
        //private readonly SynchronizationContext _syncContext;

        private static readonly Lazy<ViewModel> Lazy = new Lazy<ViewModel>(() => new ViewModel(Barometer.GetDefault()));

        //public static ViewModel Instance { get; } = new ViewModel();
        //if we have to we can fake some injection at this point..
        public static ViewModel Instance => Lazy.Value;

        public ViewModel(){ } //public default for XAML

        private ViewModel(Barometer barometer)
        {
            //_syncContext = SynchronizationContext.Current;

            _barometer = barometer;

            SetUpListenerAsync();

            //_barometer.ReadingChanged += _barometer_ReadingChanged;

            //Test = 2.0; //imagine this is meters per second. ...and the max is 4 up or down.
        }

        //private void _barometer_ReadingChanged(Barometer sender, BarometerReadingChangedEventArgs args)
        //{
        //    Pressure = args.Reading.StationPressureInHectopascals;
        //}

        //private double _pressure;

        //public double Pressure
        //{
        //    get { return _pressure; }
        //    set { this.RaiseAndSetIfChanged(ref _pressure, value); }
        //}

        //private double _test;

        //public double Test
        //{
        //    get { return _test; }
        //    set { this.RaiseAndSetIfChanged(ref _test, value); }
        //}


        private  ObservableAsPropertyHelper<double> _rateOfClimb;
        public double RateOfClimb => _rateOfClimb.Value;

        private async void SetUpListenerAsync()
        {
            //IScheduler iScheduler = new SynchronizationContextScheduler(SynchronizationContext.);


            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                var seq = Observable.FromEventPattern<TypedEventHandler<Barometer, BarometerReadingChangedEventArgs>, BarometerReadingChangedEventArgs>(h => _barometer.ReadingChanged += h, h => _barometer.ReadingChanged -= h, RxApp.TaskpoolScheduler);
                seq.DistinctUntilChanged()
                .Buffer(2, 1).Select(r =>
                {
                    UnitsNet.Length dHeight = r[1].EventArgs.Reading.Pressure().GetHeightFromPressure() -
                                              r[0].EventArgs.Reading.Pressure().GetHeightFromPressure();
                    TimeSpan dTime = r[1].EventArgs.Reading.Timestamp - r[0].EventArgs.Reading.Timestamp;

                    return UnitsNet.Speed.FromMetersPerSecond(dHeight.Meters/dTime.TotalSeconds).MetersPerSecond;
                }).ToProperty(this, x => x.RateOfClimb, result: out _rateOfClimb, scheduler: RxApp.MainThreadScheduler);

                //seq.Subscribe(p => { OnPressureChanged((Barometer)p.Sender, p.EventArgs); });
            });

        }



        

        //private double pressureChange;
        //public double PressureChange
        //{
        //    get { return pressureChange; }
        //    set
        //    {
        //        pressureChange = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public event PropertyChangedEventHandler PropertyChanged;

        ////[NotifyPropertyChangedInvocator]
        //protected async void OnPropertyChanged([CallerMemberName] String propertyName = null)
        //{
        //    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
        //    () =>
        //    {
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //    });
        //}        
    }
}


//[NotifyPropertyChangedInvocator]
//private async void OnPressureChanged(Barometer sender, BarometerReadingChangedEventArgs args)
//{
//    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
//        () =>
//        {
//            PressureChange = args.Reading.StationPressureInHectopascals;
//        });
//}

//private async void OnClimbRateChanged(double climbRate)
//{
//    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
//        () =>
//        {
//            PressureChange = climbRate;
//        });
//}

//private double pressure;
//public double Pressure
//{
//    get { return pressure; }
//    set
//    {
//        if (Math.Abs(pressure - value) > TOLERANCE)
//        {
//            pressure = value;
//            OnPropertyChanged();
//        }
//    }
//}