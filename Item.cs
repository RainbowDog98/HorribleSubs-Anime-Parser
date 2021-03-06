﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HorribleSubsXML_Parser
{
    class Anime
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime PubDate { get; set; }
        public bool IsInWatchList { get; set; }
        public bool IsReleasedToday { get; set; }
    }


    class WatchListItem : IEquatable<WatchListItem>, IComparable<WatchListItem>
    {
        public string Title { get; set; }
        public int LatestEpisode { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime ReleaseDay { get; set; }
        public string LatestEpisodeLink{ get; set; }

        public int CompareTo(WatchListItem other)
        {
            // Handle animes with no episodes in the downloaded list
            if (string.IsNullOrEmpty(other.LatestEpisodeLink))
                return -1;
            else if (string.IsNullOrEmpty(LatestEpisodeLink))
                return 1;
            
            // Handle found animes
            if (ReleaseDay.ToShortDateString() == other.ReleaseDay.ToShortDateString())
                return ReleaseDay.TimeOfDay.CompareTo(other.ReleaseDay.TimeOfDay);
            return ReleaseDay.Date.CompareTo(other.ReleaseDay.Date);
        }
        public bool Equals(WatchListItem other) => other.Title == Title;
    }
}
