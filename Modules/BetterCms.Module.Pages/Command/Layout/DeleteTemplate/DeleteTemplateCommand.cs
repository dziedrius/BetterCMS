﻿using System.Linq;

using BetterCms.Core.Exceptions.Mvc;
using BetterCms.Core.Mvc.Commands;
using BetterCms.Module.Pages.Content.Resources;
using BetterCms.Module.Root.Mvc;

namespace BetterCms.Module.Pages.Command.Layout.DeleteTemplate
{
    public class DeleteTemplateCommand: CommandBase, ICommand<DeleteTemplateCommandRequest, bool>
    {
        public bool Execute(DeleteTemplateCommandRequest request)
        {
            var pagesInUsage = Repository
                .AsQueryable<Root.Models.Page>()
                .Any(p => p.Layout.Id == request.TemplateId);

            if (pagesInUsage)
            {
                throw new ValidationException(() => PagesGlobalization.DeleteTemplate_TemplateIsInUse_Message, 
                    string.Format("Failed to delete template {0}. Template is in use.", request.TemplateId));
            }

            Repository.Delete<Root.Models.Layout>(request.TemplateId, request.Version);
            UnitOfWork.Commit();
            return true;
        }
    }
}