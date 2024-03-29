﻿using BusinessObject;
using Microsoft.EntityFrameworkCore;

namespace Repository.Interfaces
{
    public class LikeRepository : ILikeRepository
    {
        public async Task<bool> IsRecipeLikedByUser(string userId, Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Likes.AnyAsync(x => x.UserId == userId && x.RecipeId == recipeId);
        }

        public async Task Add(string userId, Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            await db.Likes.AddAsync(new Like
            {
                UserId = userId,
                RecipeId = recipeId,
                CreatedDate = DateTime.Now,
            });
            await db.SaveChangesAsync();
        }

        public async Task Remove(string userId, Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            var like = await db.Likes.FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == recipeId);
            db.Likes.Remove(like!);
            await db.SaveChangesAsync();
        }
    }
}
