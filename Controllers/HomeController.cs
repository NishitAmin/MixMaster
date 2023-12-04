using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mix_Master.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Ajax.Utilities;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Services.Protocols;
using OpenAI_API;
using OpenAI_API.Completions;

namespace Mix_Master.Controllers
{
    public class HomeController : Controller
    {
        userdbEntities db = new userdbEntities();
        private readonly MongoDbContext _dbContext;
        private readonly HttpClient _httpClient;

        public HomeController()
        {
            var connectionString = "mongodb+srv://mixmaster:mongopassword123@cluster0.iiwaevt.mongodb.net/?retryWrites=true&w=majority";
            _dbContext = new MongoDbContext(connectionString, "Bar");
            _httpClient = new HttpClient();
        }
        
        public ActionResult Index()
        {
            return View(db.TBLUserInfoes.ToList());
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Products()
        {
            return View();
        }

        public ActionResult Store()
        {
            return View();
        }

        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(TBLUserInfo tBLUserInfo)
        {
            if (db.TBLUserInfoes.Any(x => x.UsernameUs == tBLUserInfo.UsernameUs))
            {
                ViewBag.Notification = "Account already exists";
                return View();
            }
            else
            {
                try
                {
                    db.TBLUserInfoes.Add(tBLUserInfo);
                    db.SaveChanges();

                    Session["IdUsSS"] = tBLUserInfo.IdUs.ToString();
                    Session["UsernameSS"] = tBLUserInfo.UsernameUs.ToString();
                    return RedirectToAction("Index", "Home");
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }

            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(TBLUserInfo tBLUserInfo)
        {
            var checkLogin = db.TBLUserInfoes.Where(x => x.UsernameUs.Equals(tBLUserInfo.UsernameUs) && x.PasswordUs.Equals(tBLUserInfo.PasswordUs)).FirstOrDefault();
            if (checkLogin != null)
            {
                Session["IdUsSS"] = tBLUserInfo.IdUs.ToString();
                Session["UsernameSS"] = tBLUserInfo.UsernameUs.ToString();
                return RedirectToAction("Users", "Home");
            }
            else
            {
                ViewBag.Notification = "Wrong Username Or Password";
            }
            return View();
        }

        public ActionResult Users()
        {
            return View();
        }

        public ActionResult Shelf()
        {
            if (Session["UsernameSS"] != null)
            {
                string sessionUsername = Session["UsernameSS"].ToString();
                var drinks = _dbContext.UserDrinks.Find(drink => drink.Username == sessionUsername).ToList();
                var distinctDrinks = drinks.GroupBy(drink => drink.DrinkName).Select(group => group.First()).ToList();
                return View(distinctDrinks);
            }
            else
            {
                return RedirectToAction("Login"); // Or whatever your login action is
            }
        }

        public async Task<ActionResult> Assistant()
        {
            if (Session["UsernameSS"] != null)
            {
                string sessionUsername = Session["UsernameSS"].ToString();
                var drinks = _dbContext.UserDrinks.Find(drink => drink.Username == sessionUsername).ToList();
                var distinctDrinks = drinks.GroupBy(drink => drink.DrinkName).Select(group => group.First()).ToList();
                List<string> drinks_all = new List<string>();
                foreach (var drink in distinctDrinks)
                {
                    drinks_all.Add(drink.DrinkName);

                }
                string.Join(",", drinks_all.ToString());
                string prompt = "Please make me a cocktail. I have these " + string.Join(", ", drinks_all) + " alcohol drinks only with me. I have pop sodas and fruits etc. for the cocktail. Ensure the recipe is safe for consumption and creatively name the cocktail. Format your response as follows: 'New Drink Name - List of Ingredients'.";
                var openAIResponse = await GetOpenAIResponse(prompt);
                ViewBag.OpenAIResponse = openAIResponse;
                return View();
            }
            else
            {
                return RedirectToAction("Login"); 
            }
        }

        public ActionResult Favorite()
        {
            if (Session["UsernameSS"] != null)
            {
                string sessionUsername = Session["UsernameSS"].ToString();
                var favorites = _dbContext.Favorites.Find(fav => fav.Username == sessionUsername).ToList();
                var distinctFavorites = favorites.GroupBy(fav => fav.Recipe).Select(group => group.First()).ToList();
                return View(distinctFavorites);
            }
            else
            {
                return RedirectToAction("Login"); 
            }
        }

        public ActionResult AddFavorite(string recipe)
        {
            if (Session["UsernameSS"] != null && !string.IsNullOrEmpty(recipe))
            {
                string username = Session["UsernameSS"].ToString();
                _dbContext.InsertUserFav(username, recipe);
                return RedirectToAction("Favorite");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult AddDrink() {
            var drinks = _dbContext.Drinks.Find(drink => true).ToList();
            return View(drinks);
        }

        public ActionResult Shelf_Drink(string name)
        {
            var type = _dbContext.Drinks.Find(dt => dt.Name == name).FirstOrDefault();
            if (Session["UsernameSS"] != null)
            {
                var add_drink = new Shelf_Drinks
                {
                    Username = Session["UsernameSS"].ToString(),
                    DrinkName = name,
                    DrinkType = type.Type
                };

                _dbContext.UserDrinks.InsertOne(add_drink);
                return RedirectToAction("Shelf", "Home");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult DeleteDrink(string name)
        {
            if (Session["UsernameSS"] != null)
            {
                var username = Session["UsernameSS"].ToString();
                _dbContext.DeleteUserDrink(username, name);
                return RedirectToAction("Shelf", "Home");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        private async Task<string> GetOpenAIResponse(string prompt)
        {
            string OutPutResult = "";
            var openai = new OpenAIAPI("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"); // Replace with your actual API key
            CompletionRequest completionRequest = new CompletionRequest
            {
                Prompt = prompt,
                Model = OpenAI_API.Models.Model.DavinciText,
                MaxTokens = 500
            };

            try
            {
                var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

                if (completions?.Completions != null)
                {
                    foreach (var completion in completions.Completions)
                    {
                        OutPutResult += completion.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                OutPutResult = "Error fetching response from OpenAI";
            }

            return OutPutResult;
        }
    }
}