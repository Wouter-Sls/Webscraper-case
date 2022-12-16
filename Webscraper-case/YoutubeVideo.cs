using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webscraper_case
{
    public class YoutubeVideo
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public int Views { get; set; }
        public string ChannelName { get; set; }


        public YoutubeVideo(string url, string title, int views, string channelName)
        {
            this.Url = url;
            this.Title = title;
            this.Views = views;
            this.ChannelName = channelName;
        }
    }
}
