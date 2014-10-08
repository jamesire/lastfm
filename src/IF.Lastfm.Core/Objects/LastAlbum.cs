﻿using System;
using System.Collections.Generic;
using System.Linq;
using IF.Lastfm.Core.Api.Helpers;
using Newtonsoft.Json.Linq;

namespace IF.Lastfm.Core.Objects
{
    public class LastAlbum : ILastFmObject
    {
        #region Properties

        public string Name { get; set; }
        public IEnumerable<LastTrack> Tracks { get; set; }
        
        public string ArtistName { get; set; }
        public string ArtistId { get; set; }
        
        public DateTime ReleaseDateUtc { get; set; }

        public int ListenerCount { get; set; }
        public int TotalPlayCount { get; set; }

        public string Mbid { get; set; }

        public IEnumerable<Tag> TopTags { get; set; }

        public Uri Url { get; set; }

        public LastImageSet Images { get; set; }
        
        #endregion

        /// <summary>
        /// TODO datetime parsing
        /// TODO images
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        internal static LastAlbum ParseJToken(JToken token)
        {
            var a = new LastAlbum();

            var artistToken = token["artist"];
            switch (artistToken.Type)
            {
                case JTokenType.String:
                    a.ArtistName = token.Value<string>("artist");
                    a.ArtistId = token.Value<string>("id");
                    break;
                case JTokenType.Object:
                    a.ArtistName = artistToken.Value<string>("name");
                    a.ArtistId = artistToken.Value<string>("mbid");
                    break;
            }

            var tracksToken = token.SelectToken("tracks");
            if (tracksToken != null)
            {
                var trackToken = tracksToken.SelectToken("track");
                if (trackToken != null)
                    a.Tracks = tracksToken.Children().Select(t => LastTrack.ParseJToken(t, a.Name));
            }

            var tagsToken = token.SelectToken("toptags");
            if (tagsToken != null)
            {
                var tagToken = tagsToken.SelectToken("tag");
                a.TopTags = tagToken.Children().Select(Tag.ParseJToken);
            }
    
            a.ListenerCount = token.Value<int>("listeners");
            a.Mbid = token.Value<string>("mbid");
            a.Name = token.Value<string>("name");
            a.TotalPlayCount = token.Value<int>("playcount");

            var images = token.SelectToken("image");
            if (images != null)
            {
                var imageCollection = LastImageSet.ParseJToken(images);
                a.Images = imageCollection;
            }
            
            a.Url = new Uri(token.Value<string>("url"), UriKind.Absolute);

            return a;
        }

        internal static string GetNameFromJToken(JToken albumToken)
        {
            var name = albumToken.Value<string>("name");

            if (string.IsNullOrEmpty(name))
            {
                name = albumToken.Value<string>("#text");
            }

            return name;
        }

        public static PageResponse<LastAlbum> ParsePageJToken(JToken albumsToken, JToken attrToken)
        {
            var pageresponse = PageResponse<LastAlbum>.CreateSuccessResponse();
            pageresponse.AddPageInfoFromJToken(attrToken);

            var albums = new List<LastAlbum>();
            if (pageresponse.TotalItems > 0)
            {
                if (pageresponse.Page == pageresponse.TotalPages
                    && pageresponse.TotalItems % pageresponse.PageSize == 1)
                    // array notation isn't used on the api
                    albums.Add(ParseJToken(albumsToken));
                else
                    albums.AddRange(albumsToken.Children().Select(ParseJToken));
            }
            pageresponse.Content = albums;

            return pageresponse;
        }
    }
}
