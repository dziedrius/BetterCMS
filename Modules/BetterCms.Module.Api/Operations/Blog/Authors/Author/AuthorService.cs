﻿using System;
using System.Linq;

using BetterCms.Core.DataAccess;
using BetterCms.Core.DataAccess.DataContext;

using ServiceStack.ServiceInterface;

namespace BetterCms.Module.Api.Operations.Blog.Authors.Author
{
    public class AuthorService : Service, IAuthorService
    {
        private readonly IRepository repository;

        public AuthorService(IRepository repository)
        {
            this.repository = repository;
        }

        public GetAuthorResponse Get(GetAuthorRequest request)
        {
            var query = repository.AsQueryable<Module.Blog.Models.Author>();
            
            query = query.Where(author => author.Id == request.AuthorId);
            
            var model = query
                .Select(author => new AuthorModel
                    {
                        Id = author.Id,
                        Version = author.Version,
                        CreatedBy = author.CreatedByUser,
                        CreatedOn = author.CreatedOn,
                        LastModifiedBy = author.ModifiedByUser,
                        LastModifiedOn = author.ModifiedOn,

                        Name = author.Name,

                        ImageId = author.Image != null && !author.Image.IsDeleted ? author.Image.Id : (Guid?)null,
                        ImageUrl = author.Image != null && !author.Image.IsDeleted ? author.Image.PublicUrl : (string)null,
                        ImageThumbnailUrl = author.Image != null && !author.Image.IsDeleted ? author.Image.PublicThumbnailUrl : (string)null,
                        ImageCaption = author.Image != null && !author.Image.IsDeleted ? author.Image.Caption : (string)null
                    })
                .FirstOne();

            return new GetAuthorResponse
                       {
                           Data = model
                       };
        }
    }
}