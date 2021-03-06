﻿using System;
using System.Collections.Generic;
using System.Linq;

using BetterCms.Core.DataContracts;
using BetterCms.Core.DataContracts.Enums;
using BetterCms.Core.Models;
using BetterCms.Core.Security;

namespace BetterCms.Module.Root.Models
{
    /// <summary>
    /// A generic page entity.
    /// </summary>
    [Serializable]
    public class Page : EquatableEntity<Page>, IPage, IAccessSecuredObject
    {
        /// <summary>
        /// Gets or sets the page URL.
        /// </summary>
        /// <value>
        /// The page URL.
        /// </value>
        public virtual string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the lower trimmed page URL.
        /// </summary>
        /// <value>
        /// The lower trimmed page URL.
        /// </value>
        public virtual string PageUrlLowerTrimmed { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public virtual string Title { get; set; }

        /// <summary>
        /// Gets or sets the page status.
        /// </summary>
        /// <value>
        /// The page status.
        /// </value>
        public virtual PageStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the page published date.
        /// </summary>
        /// <value>
        /// The page published date.
        /// </value>
        public virtual DateTime? PublishedOn { get; set; }

        /// <summary>
        /// Gets a value indicating whether this page has SEO meta data.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this page has SEO; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasSEO
        {
            get
            {
                return !string.IsNullOrWhiteSpace(MetaTitle)
                    && !string.IsNullOrWhiteSpace(MetaKeywords)
                    && !string.IsNullOrWhiteSpace(MetaDescription);
            }
        }

        /// <summary>
        /// Gets or sets the page meta title.
        /// </summary>
        /// <value>
        /// The page meta title.
        /// </value>
        public virtual string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the page meta keywords.
        /// </summary>
        /// <value>
        /// The page meta keywords.
        /// </value>
        public virtual string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the page meta description.
        /// </summary>
        /// <value>
        /// The page meta description.
        /// </value>
        public virtual string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the page layout.
        /// </summary>
        /// <value>
        /// The page layout.
        /// </value>
        public virtual Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets the page contents.
        /// </summary>
        /// <value>
        /// The page contents.
        /// </value>
        public virtual IList<PageContent> PageContents { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public virtual IList<PageOption> Options { get; set; }

        /// <summary>
        /// Gets or sets the access rules.
        /// </summary>
        /// <value>
        /// The access rules.
        /// </value>
        public virtual IList<AccessRule> AccessRules { get; set; }

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        IList<IAccessRule> IAccessSecuredObject.AccessRules
        {
            get
            {
                if (AccessRules == null)
                {
                    return null;
                }

                return AccessRules.Cast<IAccessRule>().ToList();
            }           
        }

        public virtual void AddRule(IAccessRule accessRule)
        {
            if (AccessRules == null)
            {
                AccessRules = new List<AccessRule>();
            }

            AccessRules.Add((AccessRule)accessRule);
        }

        public virtual void RemoveRule(IAccessRule accessRule)
        {            
            AccessRules.Remove((AccessRule)accessRule);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}, Title={1}", base.ToString(), Title);
        }
    }
}