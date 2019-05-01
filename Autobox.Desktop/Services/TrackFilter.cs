﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autobox.Core.Data;
using Autobox.Core.Services;

namespace Autobox.Desktop.Services
{
    // ##### TrackExtension
    // Add MatchFilter extension to check wheter a track match the given tags or not
    public static class TrackExtension
    {
        public static bool MatchFilter(this Track track, HashSet<string> toExclude, HashSet<string> toInclude)
        {
            if (toExclude != null && track.Tags.Intersect(toExclude).Any())
            {
                return false;
            }
            if (toInclude != null && toInclude.Count > 0 && !track.Tags.Intersect(toInclude).Any())
            {
                return false;
            }
            return true;
        }
    }

    // ### TrackFilter
    // Filter tracks from the given library with a set of tag to include / exclude
    public class TrackFilter : ITrackFilter
    {
        public TrackFilter(ITrackLibrary library)
        {
            Library = library;
        }

        public void UpdateFilter(HashSet<string> toExclude, HashSet<string> toInclude)
        {
            List<Track> filtered = Library.TrackList.Values.Where(track => track.MatchFilter(toExclude, toInclude)).ToList();
            _FilteredTrackList.SetRange(filtered);
        }

        public void AddFilteredTrack(Track track)
        {
            _FilteredTrackList.Add(track);
        }

        // ##### ObservableTrackCollection
        // Container over the ObservableCollection to provide SetRange method
        private class ObservableTrackCollection : ObservableCollection<Track>
        {
            public void SetRange(List<Track> tracks)
            {
                CheckReentrancy();
                Items.Clear();
                foreach (Track track in tracks)
                {
                    Items.Add(track);
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        // ##### Properties
        public ObservableCollection<Track> FilteredTrackList => _FilteredTrackList;
        public IObservable<Track> SelectedTrack { get; }
        // ##### Attributes
        private readonly ITrackLibrary Library;
        private ObservableTrackCollection _FilteredTrackList = new ObservableTrackCollection();
    }
}