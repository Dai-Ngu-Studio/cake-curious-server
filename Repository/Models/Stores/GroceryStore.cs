﻿namespace Repository.Models.Stores
{
    public class GroceryStore
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public double? Score { get; set; }
		public int? Key { get;set; }
    }
}
