﻿using System;

using BetterCms.Module.MediaManager.Models;

namespace BetterCms.Module.MediaManager.ViewModels.MediaManager
{
    [Serializable]
    public class MediaFileViewModel : MediaViewModel
    {
        public virtual long Size { get; set; }

        public virtual string FileExtension { get; set; }

        public virtual string PublicUrl { get; set; }

        public virtual bool IsProcessing { get; set; }
        
        public virtual bool IsFailed { get; set; }

        public MediaFileViewModel()
        {
            Type = MediaType.File;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}, Size: {1}, FileExtension: {2}, PublicUrl: {3}, IsProcessing: {4}, IsFailed: {5}", base.ToString(), Size, FileExtension, PublicUrl, IsProcessing, IsFailed);
        }
    }
}