using System;
using System.Collections.Generic;

namespace music163diff
{
    class ResponseJson
    {
        public Result result { get; set; }
        public int code { get; set; }
    }

    struct Result
    {
        public string name { get; set; }
        public string coverImgUrl { get; set; }
        public List<Track> tracks { get; set; }
    }

    struct Track : IEquatable<Track>
    {
        public string name { get; set; }
        public long id { get; set; }
        public Album album { get; set; }
        public List<Artist> artists { get; set; }

        public bool Equals(Track other)
        {
            return id.Equals(other.id);
        }
    }

    struct Album : IEquatable<Album>
    {
        public long id { get; set; }
        public string name { get; set; }

        public bool Equals(Album other)
        {
            if (id == 0)
            {
                return name.Equals(other.name);
            }
            return id.Equals(other.id);
        }
    }

    struct Artist : IEquatable<Artist>
    {
        public long id { get; set; }
        public string name { get; set; }

        public bool Equals(Artist other)
        {
            if (id == 0)
            {
                return name.Equals(other.name);
            }
            return id.Equals(other.id);
        }
    }
}
