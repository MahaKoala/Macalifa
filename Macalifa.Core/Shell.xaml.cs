﻿/* 
	Macalifa. A music player made for Windows 10 store.
    Copyright (C) 2016  theweavrs (Abdullah Atta)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Macalifa.Core;
using Macalifa.Services;
using Macalifa.Common;
using Windows.Media;
using Macalifa.ViewModels;
using Windows.Storage;
using Macalifa.Models;
using Macalifa.Extensions;
using System.Threading.Tasks;

namespace Macalifa
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        static MacalifaPlayer Player => MacalifaPlayerService.Instance.Player;
        static ShellViewModel ShellVM => ShellViewService.Instance.ShellVM;

        LibraryViewService LibVM => LibraryViewService.Instance;
        List<Mediafile> OldFiles = new List<Mediafile>();
        SystemMediaTransportControls _smtc;
        public Shell()
        {
            this.InitializeComponent();
            
            ShellVM.TopMenuItems.Add(new SplitViewMenu.SimpleNavMenuItem
            {
                Label = "Library",
                DestinationPage = typeof(LibraryView),
                Symbol = Symbol.Library
            });
            ShellVM.BottomMenuItems.Add(new SplitViewMenu.SimpleNavMenuItem
            {
                Label = "Settings",
                //DestinationPage = typeof(Albums),
                Symbol = Symbol.Setting
            });
            ShellVM.PlaylistsItems.Add(new SplitViewMenu.SimpleNavMenuItem
            {
                Arguments = "Hello",
                Label = "Playlists",
                DestinationPage = typeof(PlaylistView),
                Symbol = Symbol.List
            });
            this.DataContext = ShellVM;

            _smtc = SystemMediaTransportControls.GetForCurrentView();
            _smtc.IsEnabled = true;
            _smtc.IsPlayEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.ButtonPressed += _smtc_ButtonPressed;

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            ShellVM.Play(e.Parameter);
            base.OnNavigatedTo(e);
        }
        private void Player_MediaStateChanged(object sender, Events.MediaStateChangedEventArgs e)
        {
            if (_smtc != null)
                switch (e.NewState)
                {
                    case PlayerState.Playing:
                        _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                        break;
                    case PlayerState.Paused:
                        _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                        break;
                    case PlayerState.Stopped:
                        _smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                        break;
                    default:
                        break;
                }
        }

        private async void _smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await Player.Play();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await Player.Pause();
                    });
                    break;
                default:
                    break;
            }
        }

        private void VolSliderThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var vm = this.DataContext as ShellViewModel;
            vm.DontUpdatePosition = true;
        }

        private void VolSliderThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var vm = this.DataContext as ShellViewModel;
            if (vm != null)
            {

                vm.CurrentPosition = positionSlider.Value;
                vm.DontUpdatePosition = false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Thumb volSliderThumb = FrameworkElementExtensions.FindChildOfType<Thumb>(positionSlider);
            volSliderThumb.DragCompleted += VolSliderThumb_DragCompleted;
            volSliderThumb.DragStarted += VolSliderThumb_DragStarted;

            Player.MediaStateChanged += Player_MediaStateChanged;
            
          
        }

        private async void positionSlider_Tapped(object sender, TappedRoutedEventArgs e)
        {         
            var vm = this.DataContext as ShellViewModel;
            vm.DontUpdatePosition = true;
            if (vm != null)
            {
                vm.CurrentPosition = positionSlider.Value;
            }
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(2));
            vm.DontUpdatePosition = false;

        }
        async void ShowMessage(string msg)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(msg);
            await dialog.ShowAsync();
        }

        private async void He_KeyUp(object sender, KeyRoutedEventArgs e)
        {
          
                await Task.Run(async () =>
{
    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
    {
        if (He.Text.Length > 0)
        {
           
            var list = LibVM.LibVM.OldItems.Where(w => w.Title.ToUpper().Contains(He.Text.ToUpper())).ToList();
            var col = new GroupedObservableCollection<string, Mediafile>(t => t.Title.Remove(1), list, a => a.Title.Remove(1));
            LibVM.LibVM.TracksCollection = null;
            LibVM.LibVM.TracksCollection = col;
            
        }     
        else
        {

            var col = new GroupedObservableCollection<string, Mediafile>(t => t.Title.Remove(1), LibVM.LibVM.OldItems, a => a.Title.Remove(1));
            LibVM.LibVM.TracksCollection = null;
            LibVM.LibVM.TracksCollection = col;

            GC.Collect();
        }
    });
});
           
        }
    }
}