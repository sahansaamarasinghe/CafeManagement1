﻿namespace WebApplication2.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; }

        public int Quantity { get; set; }

       // public decimal UnitPrice { get; set; }     
        public decimal TotalPrice { get; set; }    // Quantity * UnitPrice

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }

}
