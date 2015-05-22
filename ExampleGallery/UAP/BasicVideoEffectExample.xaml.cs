// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may
// not use these files except in compliance with the License. You may obtain
// a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations
// under the License.

using ExampleGallery.Effects;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExampleGallery
{
    public sealed partial class BasicVideoEffectExample : UserControl, ICustomThumbnailSource
    {
        IRandomAccessStream thumbnail;

        public BasicVideoEffectExample()
        {
            this.InitializeComponent();
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.mediaElement.Visibility = Visibility.Collapsed;
            this.progressInfo.Visibility = Visibility.Visible;
            this.progressRing.IsActive = true;
            this.progressText.Text = "Downloading video...";

            var thumbnailFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Logo.scale-100.png"));
            var thumbnail = RandomAccessStreamReference.CreateFromFile(thumbnailFile);

            var url = "http://video.ch9.ms/ch9/4597/8db5a656-b173-4897-b2aa-e2075fb24597/windows10recap.mp4";

            // TODO: replace TemporaryFolder and HttpClient with StorageFile.CreateStreamedFileFromUriAsync once TH:2458060 is fixed
            //var file = await StorageFile.CreateStreamedFileFromUriAsync(
            //    "windows10recap.mp4",
            //    new Uri(url),
            //    thumbnail);

            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("windows10recap.mp4", CreationCollisionOption.ReplaceExisting);

            using (var httpClient = new HttpClient())
            {
                byte[] videoData = await httpClient.GetByteArrayAsync(url);

                using (var writer = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await writer.WriteAsync(videoData.AsBuffer());
                }
            }

            this.progressText.Text = "Creating clip...";
            var clip = await MediaClip.CreateFromFileAsync(file);
            clip.VideoEffectDefinitions.Add(new VideoEffectDefinition(typeof(ExampleVideoEffect).FullName));

            var composition = new MediaComposition();
            composition.Clips.Add(clip);

            if (ThumbnailGenerator.IsDrawingThumbnail)
            {
                this.thumbnail = await composition.GetThumbnailAsync(TimeSpan.FromSeconds(10), 1280, 720, VideoFramePrecision.NearestFrame);
            }

            mediaElement.SetMediaStreamSource(composition.GenerateMediaStreamSource());
            mediaElement.IsLooping = true;
        
            this.mediaElement.Visibility = Visibility.Visible;
            this.progressInfo.Visibility = Visibility.Collapsed;
            this.progressRing.IsActive = false;
        }

        IRandomAccessStream ICustomThumbnailSource.Thumbnail
        {
            get { return thumbnail; }
        }
    }
}
