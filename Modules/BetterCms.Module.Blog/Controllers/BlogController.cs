﻿using System;
using System.Web.Mvc;

using BetterCms.Core.DataContracts.Enums;
using BetterCms.Core.Security;
using BetterCms.Module.Blog.Commands.GetBlogPost;
using BetterCms.Module.Blog.Commands.GetBlogPostList;
using BetterCms.Module.Blog.Commands.SaveBlogPost;
using BetterCms.Module.Blog.Content.Resources;
using BetterCms.Module.Blog.Services;
using BetterCms.Module.Blog.ViewModels.Blog;
using BetterCms.Module.Blog.ViewModels.Filter;
using BetterCms.Module.Root;
using BetterCms.Module.Root.Mvc;

using Microsoft.Web.Mvc;

namespace BetterCms.Module.Blog.Controllers
{
    /// <summary>
    /// Blogs management.
    /// </summary>
    [BcmsAuthorize]
    [ActionLinkArea(BlogModuleDescriptor.BlogAreaName)]
    public class BlogController : CmsControllerBase
    {
        /// <summary>
        /// The blog service.
        /// </summary>
        private readonly IBlogService blogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogController"/> class.
        /// </summary>
        /// <param name="blogService">The blog service.</param>
        public BlogController(IBlogService blogService)
        {
            this.blogService = blogService;
        }

        /// <summary>
        /// List with blog posts for Site Settings.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Blog post list html.</returns>
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent, RootModuleConstants.UserRoles.PublishContent, RootModuleConstants.UserRoles.DeleteContent)]
        public ActionResult Index(BlogsFilter request)
        {
            request.SetDefaultPaging();
            var model = GetCommand<GetBlogPostListCommand>().ExecuteCommand(request);
            var success = model != null;

            var view = RenderView("Index", model);
            var json = new
            {
                Tags = request.Tags,
                IncludeArchived = request.IncludeArchived
            };

            return ComboWireJson(success, view, json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Creates the blog post.
        /// </summary>
        /// <param name="parentPageUrl">The parent page URL.</param>
        /// <returns>Json result.</returns>
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        [HttpGet]
        public ActionResult CreateBlogPost(string parentPageUrl)
        {
            var model = GetCommand<GetBlogPostCommand>().ExecuteCommand(Guid.Empty);
            var view = RenderView("EditBlogPost", model);
            var success = false;
            if (model != null)
            {
                model.BlogUrl = parentPageUrl;
                success = true;
            }

            return ComboWireJson(success, view, model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Edits the blog post.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Json result.</returns>
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent, RootModuleConstants.UserRoles.PublishContent)]
        [HttpGet]
        public ActionResult EditBlogPost(string id)
        {
            var model = GetCommand<GetBlogPostCommand>().ExecuteCommand(id.ToGuidOrDefault());
            var view = RenderView("EditBlogPost", model);
            var success = model != null;
           
            return ComboWireJson(success, view, model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Saves the blog post.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Json result.</returns>
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent, RootModuleConstants.UserRoles.PublishContent)]
        [HttpPost]
        public ActionResult SaveBlogPost(BlogPostViewModel model)
        {
            var response = GetCommand<SaveBlogPostCommand>().ExecuteCommand(model);
            if (response != null)
            {
                if (model.DesirableStatus != ContentStatus.Preview && model.Id.HasDefaultValue())
                {
                    Messages.AddSuccess(BlogGlobalization.CreatePost_CreatedSuccessfully_Message);
                }
            }

            return WireJson(response != null, response);
        }

        /// <summary>
        /// Converts the string to slug.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="senderId">The sender id.</param>
        /// <returns>Json result.</returns>
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult ConvertStringToSlug(string text, string senderId)
        {
            var slug = blogService.CreateBlogPermalink(text);

            return Json(new { Text = text, Url = slug, SenderId = senderId }, JsonRequestBehavior.AllowGet);
        }
    }
}
