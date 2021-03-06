﻿using System.Collections.Generic;
using System.Web.Mvc;

using BetterCms.Core.Security;

using BetterCms.Module.MediaManager.ViewModels;
using BetterCms.Module.Pages.Command.Layout.GetLayoutOptions;
using BetterCms.Module.Pages.Command.Page.AddNewPage;
using BetterCms.Module.Pages.Command.Page.ClonePage;
using BetterCms.Module.Pages.Command.Page.CreatePage;
using BetterCms.Module.Pages.Command.Page.DeletePage;
using BetterCms.Module.Pages.Command.Page.GetPageForCloning;
using BetterCms.Module.Pages.Command.Page.GetPageForDelete;
using BetterCms.Module.Pages.Command.Page.GetPageProperties;
using BetterCms.Module.Pages.Command.Page.GetPagesList;
using BetterCms.Module.Pages.Command.Page.SavePageProperties;
using BetterCms.Module.Pages.Command.Page.SavePagePublishStatus;
using BetterCms.Module.Pages.Content.Resources;
using BetterCms.Module.Pages.Services;
using BetterCms.Module.Pages.ViewModels.Filter;
using BetterCms.Module.Pages.ViewModels.Page;

using BetterCms.Module.Root;
using BetterCms.Module.Root.Models;
using BetterCms.Module.Root.Mvc;
using BetterCms.Module.Root.ViewModels.Security;

using Microsoft.Web.Mvc;

namespace BetterCms.Module.Pages.Controllers
{
    /// <summary>
    /// Controller for CMS pages: create / edit / delete pages.
    /// </summary>
    [BcmsAuthorize]
    [ActionLinkArea(PagesModuleDescriptor.PagesAreaName)]
    public class PageController : CmsControllerBase
    {
        /// <summary>
        /// The page service.
        /// </summary>
        private readonly IPageService pageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageController"/> class.
        /// </summary>
        /// <param name="pageService">The page service.</param>
        public PageController(IPageService pageService)
        {
            this.pageService = pageService;
        }

        /// <summary>
        /// Renders a page list for the site settings dialog.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// Rendered pages list.
        /// </returns>
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent, RootModuleConstants.UserRoles.PublishContent, RootModuleConstants.UserRoles.DeleteContent)]
        public ActionResult Pages(PagesFilter request)
        {
            request.SetDefaultPaging();
            var model = GetCommand<GetPagesListCommand>().ExecuteCommand(request);
            var success = model != null;

            var view = RenderView("Pages", model);
            var json = new
            {
                Tags = request.Tags,
                IncludeArchived = request.IncludeArchived
            };

            return ComboWireJson(success, view, json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Creates add new page modal dialog.
        /// </summary>
        /// <param name="parentPageUrl">The parent page URL.</param>
        /// <returns>
        /// ViewResult to render add new page modal dialog.
        /// </returns>
        [HttpGet]
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult AddNewPage(string parentPageUrl)
        {
            var request = new AddNewPageCommandRequest { ParentPageUrl = parentPageUrl };
            var model = GetCommand<AddNewPageCommand>().ExecuteCommand(request);
            var view = RenderView("AddNewPage", model);

            return ComboWireJson(true, view, model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Validates and creates new page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Json with result status and redirect url.</returns>
        [HttpPost]
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult AddNewPage(AddNewPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = GetCommand<CreatePageCommand>().ExecuteCommand(model);
                if (response != null)
                {
                    response.PageUrl = Http.GetAbsolutePath(response.PageUrl);
                    Messages.AddSuccess(PagesGlobalization.SavePage_CreatedSuccessfully_Message);
                    return Json(new WireJson { Success = true, Data = response });
                }
            }

            return Json(new WireJson(false));
        }

        /// <summary>
        /// Creates edit page properties modal dialog for given page.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <returns>
        /// ViewResult to render edit page properties modal dialog.
        /// </returns>
        [HttpGet]
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult EditPageProperties(string pageId)
        {
            var model = GetCommand<GetPagePropertiesCommand>().ExecuteCommand(pageId.ToGuidOrDefault());
            var success = model != null;

            var view = RenderView("EditPageProperties", model);
            var json = new
                           {
                               Tags = success ? model.Tags : null,
                               Image = success ? model.Image : new ImageSelectorViewModel(),
                               SecondaryImage = success ? model.SecondaryImage : new ImageSelectorViewModel(),
                               FeaturedImage = success ? model.FeaturedImage : new ImageSelectorViewModel(),
                               OptionValues = success ? model.OptionValues : null,
                               UserAccessList = success ? model.UserAccessList : new List<UserAccessViewModel>()
                           };

            return ComboWireJson(success, view, json, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Validates and saves page properties.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Json with result status and redirect url.</returns>
        [HttpPost]
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult EditPageProperties(EditPagePropertiesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = GetCommand<SavePagePropertiesCommand>().ExecuteCommand(model);
                if (response != null)
                {
                    response.PageUrl = Http.GetAbsolutePath(response.PageUrl);
                    return Json(new WireJson { Success = true, Data = response });
                }
            }

            return Json(new WireJson { Success = false });
        }

        /// <summary>
        /// Creates delete page confirmation dialog.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// ViewResult to render delete page confirmation modal dialog.
        /// </returns>
        [HttpGet]
        [BcmsAuthorize(RootModuleConstants.UserRoles.DeleteContent)]
        public ActionResult DeletePageConfirmation(string id)
        {
            var model = GetCommand<GetPageForDeleteCommand>().ExecuteCommand(id.ToGuidOrDefault());
            var view = RenderView("DeletePageConfirmation", model ?? new DeletePageViewModel());
            return ComboWireJson(model != null, view, model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes CMS page.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <returns>
        /// Json with delete result status.
        /// </returns>
        [HttpPost]
        [BcmsAuthorize(RootModuleConstants.UserRoles.DeleteContent)]
        public ActionResult DeletePage(DeletePageViewModel model)
        {
            if (GetCommand<DeletePageCommand>().ExecuteCommand(model))
            {
                Messages.AddSuccess(PagesGlobalization.DeletePage_DeletedSuccessfully_Message);
                return Json(new WireJson { Success = true });
            }

            return Json(new WireJson { Success = false });
        }

        /// <summary>
        /// Clones the page.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// Json result status.
        /// </returns>
        [HttpGet]
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult ClonePage(string id)
        {
            var model = GetCommand<GetPageForCloningCommand>().ExecuteCommand(id.ToGuidOrDefault());
            var view = RenderView("ClonePage", model ?? new ClonePageViewModel());
            return ComboWireJson(model != null, view, model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Clones the page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Json result status.
        /// </returns>
        [HttpPost]
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult ClonePage(ClonePageViewModel model)
        {
            model = GetCommand<ClonePageCommand>().ExecuteCommand(model);
            if (model != null)
            {
                Messages.AddSuccess(string.Format(PagesGlobalization.ClonePage_Dialog_Success, model.PageUrl));
                return Json(new WireJson { Success = true, Data = model });
            }

            return Json(new WireJson { Success = false });
        }

        /// <summary>
        /// Changes CMS page IsPublished status.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// Json with delete result status.
        /// </returns>
        [HttpPost]
        [BcmsAuthorize(RootModuleConstants.UserRoles.PublishContent)]
        public ActionResult ChangePublishStatus(SavePagePublishStatusRequest request)
        {
            var success = GetCommand<SavePagePublishStatusCommand>().ExecuteCommand(request);
            if (success)
            {
                var message = request.IsPublished 
                    ? PagesGlobalization.PublishPage_PagePublishedSuccessfully_Message
                    : PagesGlobalization.PublishPage_PageUnpublishedSuccessfully_Message;
                Messages.AddSuccess(message);
            }
            else
            {
                var message = request.IsPublished
                    ? PagesGlobalization.PublishPage_FailedToPublishPage_Message
                    : PagesGlobalization.PublishPage_FailedToUnpublishPage_Message;
                Messages.AddError(message);
            }

            return Json(new WireJson { Success = success });
        }

        /// <summary>
        /// Converts the string to slug.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="senderId">The sender id.</param>
        /// <param name="parentPageUrl">The parent page URL.</param>
        /// <returns>
        /// URL, created from text.
        /// </returns>
        public ActionResult ConvertStringToSlug(string text, string senderId, string parentPageUrl)
        {
            var slug = pageService.CreatePagePermalink(text, parentPageUrl);

            return Json(new { Text = text, Url = slug, SenderId = senderId }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Loads the layout options.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpGet]
        [BcmsAuthorize(RootModuleConstants.UserRoles.EditContent)]
        public ActionResult LoadLayoutOptions(string id)
        {
            var model = GetCommand<GetLayoutOptionsCommand>().ExecuteCommand(id.ToGuidOrDefault());
            return WireJson(model != null, model, JsonRequestBehavior.AllowGet);
        }
    }
}