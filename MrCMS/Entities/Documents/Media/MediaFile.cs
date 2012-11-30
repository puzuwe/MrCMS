﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using MrCMS.Models;
using MrCMS.Services;

namespace MrCMS.Entities.Documents.Media
{
    public class MediaFile : BaseEntity
    {

        public virtual string FileExtension { get; set; }
        public virtual string ContentType { get; set; }
        public virtual MediaCategory MediaCategory { get; set; }
        public virtual string FileLocation { get; set; }
        public virtual int ContentLength { get; set; }
        public virtual string FileName { get; set; }

        [DisplayName("Alt")]
        public virtual string Title { get; set; }
        [DisplayName("Title")]
        public virtual string Description { get; set; }

        //images only
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual Size Size { get { return Width > 0 && Height > 0 ? new Size(Width, Height) : Size.Empty; } }

        public static readonly List<string> ImageExtensions = new List<string> { ".jpg", ".jpeg", ".gif", ".png" };
        public virtual bool IsImage
        {
            get { return ImageExtensions.Any(s => s.Equals(FileExtension, StringComparison.InvariantCultureIgnoreCase)); }
        }

        public virtual IEnumerable<ImageSize> Sizes
        {
            get
            {
                yield return new ImageSize { Size = Size, ActualSize = Size, Name = "Original" };
                foreach (var imageSize in ImageProcessor.ImageSizes.Where(size => ImageProcessor.RequiresResize(Size, size.Size)))
                {
                    imageSize.ActualSize = ImageProcessor.CalculateDimensions(Size, imageSize.Size);
                    yield return imageSize;
                }
            }
        }
    }
}