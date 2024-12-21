using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;


class Program
{
    public class Book
    {
        public string Title { get; set; }
        public bool IsBestSeller { get; set; }
    }

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        try
        {
            List<Book> books = new List<Book>();
            // Запуск браузера
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false // Встановіть true для фонової роботи
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                // Перейти на Amazon
                await page.GotoAsync("https://www.amazon.com/");

                // Встановити фільтр Books
                await page.Locator("//select[@id='searchDropdownBox']").SelectOptionAsync("Books");

                // Ввести пошукове слово "Java" та виконати пошук
                await page.Locator("//input[@id='twotabsearchtextbox']").FillAsync("Java");
                await page.Locator("//input[@id='twotabsearchtextbox']").PressAsync("Enter");
                //div[@role='listitem'][2]//span[@class='a-badge-text'][text()]

                // Зачекати, поки з'являться результати
                await page.WaitForSelectorAsync("(//div[@role='listitem'])[16]");
                Thread.Sleep(3000);
                //Записати назві книг та наявність позначки бестселлер
                int counter = await page.Locator("//div[@role='listitem']").CountAsync();
                //Console.WriteLine(counter);
                for (int i = 1; i <= counter; i++) 
                {
                    string title = await page.Locator($"//div[@role='listitem'][{i}]//div[@data-cy='title-recipe']//h2").InnerTextAsync();
                    string BestSellerBage = await page.Locator($"//div[@role='listitem'][{i}]//div[@class='a-section a-spacing-none aok-relative puis-status-badge-container s-list-status-badge-container']").InnerTextAsync();
                    books.Add(new Book {
                        Title = title, 
                        IsBestSeller = !String.IsNullOrEmpty(BestSellerBage) 
                    });
                    
                }
                
                // Перевірити, чи є книга "Head First Java, 2nd edition"
                await page.GotoAsync("https://www.amazon.com/Head-First-Java-Kathy-Sierra/dp/0596009208?dchild=1&keywords=Java&qid=1610356790&s=books&sr=1-2");
                //await page.GotoAsync("https://a.co/d/d8l1u2H");
                await page.WaitForSelectorAsync("//span[@id='productTitle']");
                Thread.Sleep(3000);
                string targetBook = await page.Locator("//span[@id='productTitle']").InnerTextAsync();
                //string targetBook = "Head First Java";
                bool containsTargetBook = books.Exists(book => book.Title.Contains(targetBook.Trim()));

                Console.WriteLine(containsTargetBook
                    ? $"Книга '{targetBook}' знайдена у списку."
                    : $"Книга '{targetBook}' НЕ знайдена у списку.");

                // Вивести всі знайдені книги
                foreach (var book in books)
                {
                    Console.Write($"Назва книг: {book.Title} \nЧи є вона бестселером: ");
                    Console.Write(book.IsBestSeller ? "Бестселер\n" : "Не є Бесселером\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час взаємодії із сторінкою: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка під час запуску браузера: {ex.Message}");
        }
    }
}


