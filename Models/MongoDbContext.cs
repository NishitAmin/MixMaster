﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mix_Master.Models
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Drink> Drinks => _database.GetCollection<Drink>("Drinks");

        public IMongoCollection<Shelf_Drinks> UserDrinks => _database.GetCollection<Shelf_Drinks>("Shelf");

        public void InsertUserDrink(string username, string drinkName)
        {
            var shelf_Drinks = new Shelf_Drinks
            {
                Username = username,
                DrinkName = drinkName
            };

            UserDrinks.InsertOne(shelf_Drinks);
        }
        public IMongoCollection<Favorite> Favorites => _database.GetCollection<Favorite>("Favorites");

        public void InsertUserFav(string username, string recipe)
        {
            var userFavorite = new Favorite
            {
                Username = username,
                Recipe= recipe
            };

            Favorites.InsertOne(userFavorite);
        }
    }
}