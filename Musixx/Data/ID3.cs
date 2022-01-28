// ID3

using Musixx.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Musixx.Data
{
    public class ID3
    {
        public static int? GetID3v2HeaderLength(byte[] bytes)
        {
            if (bytes.Length < 3 || Encoding.UTF8.GetString(bytes, 0, 3) != "ID3")
                return null;

            return 7 + BitConverter.ToInt32(bytes.Skip(6).Take(4).Reverse().ToArray(), 0);
        }

        public static async void ExtractTagsFromBuffer(IBuffer buffer, IMusic music)
        {
            using(var stream = new MemoryStream(buffer.ToArray()))
            {
                TagLib.File file = null;
                try
                {
                    file = TagLib.File.Create(new StreamFileAbstraction(music.File.Name, stream, stream));
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine("!! ID3  - TagLib.File.Create Exception !! : " + ex1.Message);
                }

                Tag tag = null;

                try
                {
                    tag = file.GetTag(TagTypes.Id3v2);
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine("!! ID3 - GetTag Exception !! : " + ex1.Message);
                }


                if (tag != null)
                {
                    if (!String.IsNullOrWhiteSpace(tag.Title))
                    music.Title = tag.Title;

                    if (!String.IsNullOrWhiteSpace(tag.FirstPerformer))
                        music.Artist = tag.FirstPerformer;

                    if (!String.IsNullOrWhiteSpace(tag.Album))
                        music.Album = tag.Album;

                    if (tag.Pictures.Length > 0)
                    {
                        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                        var item = await localFolder.TryGetItemAsync(music.File.Id + ".jpg");
                        if (item != null)
                            music.CoverUri = new Uri(item.Path);
                        else
                        {
                            StorageFile storageFile = await localFolder.CreateFileAsync(music.File.Id + ".jpg");
                            await FileIO.WriteBytesAsync(storageFile, tag.Pictures[0].Data.Data);
                            music.CoverUri = new Uri(storageFile.Path);
                        }

                        Debug.WriteLine(music.CoverUri);
                    }
                }

                int bitrate = 125;

                try
                {
                    bitrate = file.Properties.AudioBitrate * 125;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Bitrate attr  Exception : " + ex.Message);
                }

                if (bitrate == 0)
                {
                    bitrate = 125;
                }
                music.Duration = TimeSpan.FromSeconds(music.File.Size / bitrate);
            }

        }
    }
}
