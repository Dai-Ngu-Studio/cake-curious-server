﻿namespace Repository.Models.Users
{
    public class FollowingUser
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public bool? IsFollowedByCurrentUser { get; set; }
    }
}
