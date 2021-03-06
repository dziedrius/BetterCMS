﻿using System;
using System.Linq;

using BetterCms.Core.DataAccess.DataContext;
using BetterCms.Core.DataContracts.Enums;
using BetterCms.Core.Mvc.Commands;
using BetterCms.Module.Pages.Helpers;
using BetterCms.Module.Pages.Models;
using BetterCms.Module.Pages.ViewModels.Content;
using BetterCms.Module.Root;
using BetterCms.Module.Root.Models;
using BetterCms.Module.Root.Mvc;
using BetterCms.Module.Root.Services;

namespace BetterCms.Module.Pages.Command.Content.SavePageHtmlContent
{
    public class SavePageHtmlContentCommand : CommandBase, ICommand<PageContentViewModel, SavePageHtmlContentResponse>
    {
        private readonly IContentService contentService;

        public SavePageHtmlContentCommand(IContentService contentService)
        {
            this.contentService = contentService;
        }

        public SavePageHtmlContentResponse Execute(PageContentViewModel request)
        {
            if (request.DesirableStatus == ContentStatus.Published)
            {
                DemandAccess(RootModuleConstants.UserRoles.PublishContent);
            }

            if (request.Id == default(Guid) || request.DesirableStatus != ContentStatus.Published)
            {
                DemandAccess(RootModuleConstants.UserRoles.EditContent);
            }

            UnitOfWork.BeginTransaction();

            PageContent pageContent;            
            if (!request.Id.HasDefaultValue())
            {
                pageContent = Repository.AsQueryable<PageContent>().Where(f => f.Id == request.Id && !f.IsDeleted).FirstOne();
            }
            else
            {              
                pageContent = new PageContent();
                pageContent.Order = contentService.GetPageContentNextOrderNumber(request.PageId);
            }

            pageContent.Page = Repository.AsProxy<Root.Models.Page>(request.PageId);
            pageContent.Region = Repository.AsProxy<Region>(request.RegionId);

            var contentToSave = new HtmlContent
                {
                    Id = request.ContentId,
                    ActivationDate = request.LiveFrom,
                    ExpirationDate = TimeHelper.FormatEndDate(request.LiveTo),
                    Name = request.ContentName,
                    Html = request.PageContent ?? string.Empty,
                    UseCustomCss = request.EnabledCustomCss,
                    CustomCss = request.CustomCss,
                    UseCustomJs = request.EanbledCustomJs,
                    CustomJs = request.CustomJs,
                    EditInSourceMode = request.EditInSourceMode
                };

            // Preserve content if user is not authorized to change it.
            if (!SecurityService.IsAuthorized(RootModuleConstants.UserRoles.EditContent) && request.Id != default(Guid))
            {
                var originalContent = Repository.First<HtmlContent>(request.ContentId);
                var contentToPublish = (HtmlContent)(originalContent.History != null
                    ? originalContent.History.FirstOrDefault(c => c.Status == ContentStatus.Draft) ?? originalContent
                    : originalContent);

                contentToSave.Name = contentToPublish.Name;
                contentToSave.Html = contentToPublish.Html;
                contentToSave.UseCustomCss = contentToPublish.UseCustomCss;
                contentToSave.CustomCss = contentToPublish.CustomCss;
                contentToSave.UseCustomJs = contentToPublish.UseCustomJs;
                contentToSave.CustomJs = contentToPublish.CustomJs;
                contentToSave.EditInSourceMode = contentToPublish.EditInSourceMode;
            }

            pageContent.Content = contentService.SaveContentWithStatusUpdate(
                contentToSave,
                request.DesirableStatus);
            
            Repository.Save(pageContent);
            UnitOfWork.Commit();

            // Notify.
            if (request.DesirableStatus != ContentStatus.Preview)
            {
                Events.PageEvents.Instance.OnPageContentInserted(pageContent);
            }

            return new SavePageHtmlContentResponse {
                                                       PageContentId = pageContent.Id,
                                                       ContentId = pageContent.Content.Id,
                                                       RegionId = pageContent.Region.Id,
                                                       PageId = pageContent.Page.Id
                                                   };
        }
    }
}