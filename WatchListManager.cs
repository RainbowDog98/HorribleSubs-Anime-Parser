﻿using System;
using System.Collections.Generic;
using System.IO;

namespace HorribleSubsXML_Parser
{
    class WatchListManager
    {
        private List<WatchListItem> WatchList { get; set; }
        private readonly string FileName = "animeListing.txt";
        private readonly string Seperator = new string('-', Console.WindowWidth - 1);

        public int WatchListCount { get { return WatchList.Count; } }

        public WatchListManager()
        {
            WatchList = new List<WatchListItem>();
            // Read a watchlist file
            ReadWatchListFile();
        }

        public void AddToWatchList(string[] animes, List<Anime> animeList)
        {
            foreach (var item in animes)
            {
                if (!int.TryParse(item, out int index) || index >= animeList.Count || index < 0)
                {
                    Program.DisplayError($"ERROR: INVALID INDEX: {item} PROVIDED, SO IT WON'T BE ADDED");
                    continue;    
                }
                
                string title = animeList[index].Title;

                for (int i = title.Length - 1; i != 0; i--)
                {
                    if (title[i] == '-')
                    {
                        var animeToBeAdded = new WatchListItem()
                        {
                            Title = title.Remove(i),
                            LatestEpisode = 0,
                            IsDownloaded = false,
                            ReleaseDay = animeList[index].PubDate
                        };

                        // Check if it already exists in the watchlist
                        if(WatchList.Contains(animeToBeAdded))
                        {
                            Program.DisplayError($"Anime \"{animeToBeAdded.Title}\" already exists in the watchlist");
                            break;
                        }    

                        // Add a new watchlist value
                        WatchList.Add(animeToBeAdded);
                        break;
                    }
                }
            }
            // Rechecks the whole list for containing animes
            foreach (var anime in animeList)
            {
                anime.IsInWatchList = ContainsInWatchList(anime);
            }

            WatchList.Sort();
        }
        public void DisplayWatchList()
        {
            static string todayPrint()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                return "Today";
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            if (WatchList.Count == 0)
            {
                Console.WriteLine("There is no anime in here :(");
                Console.ResetColor();

                return;
            }
            Console.WriteLine(Seperator);

            for (int i = 0; i < WatchList.Count; i++)
            {
                var cachedDate = WatchList[i].ReleaseDay.AddDays(7);

                if (string.IsNullOrEmpty(WatchList[i].LatestEpisodeLink))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("{0,2}| {1,-80}| Last episode: {2,-5}| No episodes found", 
                        i, 
                        WatchList[i].Title, 
                        WatchList[i].LatestEpisode
                        );
                }
                else if(!WatchList[i].IsDownloaded) // Is NOT downloaded
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("{0,2}| {1,-80}| Latest Episode: {2,-3}| Next episode release date: {3} {4}:{5}",
                        i,
                        WatchList[i].Title,
                        WatchList[i].LatestEpisode,
                        cachedDate.Date == DateTime.Now.Date ? "Today" : cachedDate.ToShortDateString(),
                        WatchList[i].ReleaseDay.Hour,
                        WatchList[i].ReleaseDay.Minute < 30 ? "00" : "30"
                        );
                }
                else
                    Console.WriteLine("{0,2}| {1,-80}| Latest Episode: {2,-3}| Next episode release date: {3} {4}:{5}",
                        i,
                        WatchList[i].Title,
                        WatchList[i].LatestEpisode,
                        cachedDate.Date == DateTime.Now.Date ? todayPrint() : cachedDate.ToShortDateString(),
                        WatchList[i].ReleaseDay.Hour,
                        WatchList[i].ReleaseDay.Minute < 30 ? "00" : "30"
                        );
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(Seperator);
            }
            Console.ResetColor();
        }
        public bool ContainsInWatchList(Anime anime)
        {
            foreach (var item in WatchList)
            {
                if (anime.Title.Contains(item.Title))
                {
                    int episodeNumber = GetAnimesEpisodeNumber(anime.Title);
                    if(episodeNumber > item.LatestEpisode)
                    {
                        // Newer version of the episode was found
                        item.LatestEpisodeLink = anime.Link;
                        item.LatestEpisode = episodeNumber;
                        item.IsDownloaded = false;
                        item.ReleaseDay = anime.PubDate;
                    }
                    else if (episodeNumber == item.LatestEpisode)
                    {
                        item.LatestEpisodeLink = anime.Link;
                    }
                    return true;
                }
            }
            return false;
        }
        public void WriteWatchListFile()
        {
            if (WatchList.Count == 0)
                return;

            using var fs = new StreamWriter(FileName,false);
            foreach (var item in WatchList)
            {
                fs.WriteLine($"{item.Title};{item.LatestEpisode};{item.IsDownloaded};{item.ReleaseDay}");
            }
        }
        public void RemoveMultipleEntriesFromWatchList(string[] indexes, List<Anime> animeList)
        {
            List<WatchListItem> tmp = new List<WatchListItem>(indexes.Length);

            foreach (var item in indexes)
            {
                if (int.TryParse(item, out int index) && index < WatchList.Count)
                {
                    tmp.Add(new WatchListItem()
                    {
                       Title = WatchList[index].Title,
                       LatestEpisode = WatchList[index].LatestEpisode,
                       IsDownloaded = WatchList[index].IsDownloaded
                    });        
                }
                else
                {
                    Program.DisplayError($"ERROR: INDEX {item} DOES NOT EXIST, STOPPING THE REMOVAL");
                    return;
                }
            }

            foreach (var item in tmp)
            {
                WatchList.Remove(new WatchListItem() 
                { 
                    Title = item.Title, 
                    LatestEpisode = item.LatestEpisode,
                    IsDownloaded = item.IsDownloaded 
                });
            }
            foreach (var anime in animeList)
            {
                anime.IsInWatchList = ContainsInWatchList(anime);
            }
        }
        public void SetAnimeAsDownloadedByAnime(Anime anime)
        {
            foreach (var item in WatchList)
            {
                if (anime.Title.Contains(item.Title))
                {
                    int episodeNumber = GetAnimesEpisodeNumber(anime.Title);
                    if(episodeNumber == item.LatestEpisode)
                    {
                        item.IsDownloaded = true;
                        item.ReleaseDay = anime.PubDate;
                        return;
                    }
                }
            }
        }
        public void SetAnimeAsDownloadedByWatchListIndex(int index) => WatchList[index].IsDownloaded = true;
        public string GetWatchListItemLink(int index) => WatchList[index].LatestEpisodeLink;
        public void SortWatchList() => WatchList.Sort();

        private int GetAnimesEpisodeNumber(string AnimeName)
        {
            for (int i = AnimeName.Length - 1; i != 0; i--)
            {
                if (AnimeName[i] == '-')
                {
                    var newSplit = AnimeName.Substring(i).Split(' ');// FORMAT: [-][episodeNumber][othershit]...
                    if (int.TryParse(newSplit[1], out int number))
                        return number;
                    else
                        return -1;
                }
            }
            return -1;
        }
        private void ReadWatchListFile()
        {
            if (!File.Exists(FileName))
                return;

            using var fs = new StreamReader(FileName);
            while (!fs.EndOfStream)
            {
                //Format title;latest episode;is downloaded
                var line = fs.ReadLine().Split(';');
                if(line.Length != 4)
                {
                    Program.DisplayError("Error happened while reading the watchlist file");
                    return;
                }    
                WatchList.Add(new WatchListItem()
                {
                    Title = line[0],
                    LatestEpisode = int.Parse(line[1]),
                    IsDownloaded = bool.Parse(line[2]),
                    ReleaseDay = DateTime.Parse(line[3])
                });
            }
        }
    }
}
