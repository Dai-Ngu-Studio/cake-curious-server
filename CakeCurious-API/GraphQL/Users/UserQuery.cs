using BusinessObject;

namespace CakeCurious_API.GraphQL.Users
{
    public class UserQuery
    {
        [UseDbContext(typeof(CakeCuriousDbContext))]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<User> Users([ScopedService] CakeCuriousDbContext context) => context.Users;
    }
}
