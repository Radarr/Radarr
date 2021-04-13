using System;
using Nancy.Cookies;

namespace Radarr.Http.Authentication
{
    public class RadarrNancyCookie : NancyCookie
    {
        public RadarrNancyCookie(string name, string value)
            : base(name, value)
        {
        }

        public RadarrNancyCookie(string name, string value, DateTime expires)
            : base(name, value, expires)
        {
        }

        public RadarrNancyCookie(string name, string value, bool httpOnly)
            : base(name, value, httpOnly)
        {
        }

        public RadarrNancyCookie(string name, string value, bool httpOnly, bool secure)
            : base(name, value, httpOnly, secure)
        {
        }

        public RadarrNancyCookie(string name, string value, bool httpOnly, bool secure, DateTime? expires)
            : base(name, value, httpOnly, secure, expires)
        {
        }

        public override string ToString()
        {
            return base.ToString() + "; SameSite=Lax";
        }
    }
}
