using System;
using System.Diagnostics.CodeAnalysis;

namespace BCC.Core.Model.CheckRunSubmission
{
    public class CheckRunImage: IEquatable<CheckRunImage>
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public CheckRunImage()
        {
        }

        public CheckRunImage(string alt, string imageUrl)
        {
            Alt = alt;
            ImageUrl = imageUrl;
        }

        public string Alt { get; set; }

        public string ImageUrl { get; set; }

        public string Caption { get; set; }
        public bool Equals(CheckRunImage other)
        {
            if (other == null)
                return false;

            return Alt == other.Alt &&
                   ImageUrl == other.ImageUrl &&
                   Caption == other.Caption;
        }
    }
}