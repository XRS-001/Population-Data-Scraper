using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;

class Program
{
    static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string[] words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            char[] charArray = words[i].ToCharArray();
            if (charArray.Length > 0)
            {
                charArray[0] = char.ToUpper(charArray[0]);
                words[i] = new string(charArray);
            }
        }

        return string.Join(" ", words);
    }

    static string RemoveLeadingThe(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string[] words = input.Split(' ');
        if (words.Length > 0 && words[0].Equals("the", StringComparison.OrdinalIgnoreCase))
        {
            words[0] = string.Empty;
        }

        return string.Join(" ", words).Trim();
    }

    static async Task Main()
    {
        string pastPopulation = "https://www.worldometers.info/world-population/world-population-by-year/";
        string forecastedPopulation = "https://www.worldometers.info/world-population/world-population-projections/";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Pick which option you'd like.");
        Console.WriteLine("Option 1: Country population data.");
        Console.WriteLine("Option 2: World population data (past).");
        Console.WriteLine("Option 3: World population data (forecasted).");
        Console.ResetColor();
        string optionPicked = Console.ReadLine();

        while (!string.IsNullOrEmpty(optionPicked))
        {
            switch (optionPicked)
            {
                case var _ when optionPicked.Contains("1") || optionPicked.Contains("first") || optionPicked.Contains("First"):
                    await GetCountryPopulation();
                    break;

                case var _ when optionPicked.Contains("2") || optionPicked.Contains("second") || optionPicked.Contains("Second"):
                    await GetWorldPopulation(pastPopulation, 1951, 2020);
                    break;

                case var _ when optionPicked.Contains("3") || optionPicked.Contains("third") || optionPicked.Contains("Third"):
                    await GetWorldPopulation(forecastedPopulation, 2020, 2100);
                    break;

                default:
                    Console.WriteLine("Invalid option");
                    break;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Pick which option you'd like, or press Enter to exit.");
            Console.ResetColor();
            optionPicked = Console.ReadLine();
        }
    }

    static async Task GetCountryPopulation()
    {
        string url = "https://www.worldometers.info/world-population/population-by-country/";

        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            // Create a new HtmlDocument instance
            HtmlDocument htmlDoc = new HtmlDocument();

            // Load the HTML content from the Worldometer website
            htmlDoc.LoadHtml(html);

            bool shouldContinue = true;

            while (shouldContinue)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("What country would you like the population data of? (Data is from 2020) Press enter to exit.");
                Console.ResetColor();
                Console.WriteLine();
                string countryPicked = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(countryPicked))
                {
                    shouldContinue = false;
                    continue;
                }

                string capitalizedCountryPicked = CapitalizeFirstLetter(countryPicked);
                string normalizedCountryPicked = RemoveLeadingThe(capitalizedCountryPicked);
                bool countryFound = false; // Flag to track if the country is found

                try
                {
                    // Find the table body element
                    var tableBody = htmlDoc.DocumentNode.SelectSingleNode("//tbody");

                    // Find all table rows within the table body
                    var tableRows = tableBody.SelectNodes(".//tr");

                    foreach (var row in tableRows)
                    {
                        // Extract the values from the <td> elements in each row
                        string rank = row.SelectSingleNode(".//td[1]")?.InnerText;
                        string country = row.SelectSingleNode(".//td[2]/a")?.InnerText;
                        string population = row.SelectSingleNode(".//td[3]")?.InnerText;

                        if (string.Equals(country, normalizedCountryPicked, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Rank: {rank}");
                            Console.WriteLine($"Country: {country}");
                            Console.WriteLine($"Population: {population}");
                            Console.WriteLine();
                            countryFound = true; // Set the flag to true since the country is found
                            break; // Exit the loop since the country is found
                        }
                    }

                    if (!countryFound) // If the flag is still false, the country was not found
                    {
                        Console.WriteLine("Country not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred: " + ex.Message);
                }
            }
        }
    }

    static async Task GetWorldPopulation(string url, int minimumDate, int maximumDate)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            // Create a new HtmlDocument instance
            HtmlDocument htmlDoc = new HtmlDocument();

            // Load the HTML content from the Worldometer website
            htmlDoc.LoadHtml(html);

            bool dateFound = false;
            bool shouldContinue = true;

            while (shouldContinue)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"What date would you like the population data of? ({minimumDate} - {maximumDate}) Press enter to exit.");
                Console.ResetColor();
                Console.WriteLine();
                string datePicked = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(datePicked))
                {
                    shouldContinue = false;
                    continue;
                }

                int date = int.Parse(datePicked);

                if (date > minimumDate && date < maximumDate)
                {
                    dateFound = true;
                }

                try
                {
                    // Find the table body element
                    var tableBody = htmlDoc.DocumentNode.SelectSingleNode("//tbody");

                    // Find all table rows within the table body
                    var tableRows = tableBody.SelectNodes(".//tr");

                    foreach (var row in tableRows)
                    {
                        // Extract the values from the <td> elements in each row
                        string tableDate = row.SelectSingleNode(".//td[1]")?.InnerText;
                        int parsedTableDate = int.Parse(tableDate);
                        string tablePopulation = row.SelectSingleNode(".//td[2]")?.InnerText;

                        if (date == parsedTableDate)
                        {
                            Console.WriteLine($"Date: {date}, population: {tablePopulation}");
                            Console.WriteLine();
                            dateFound = true; // Set the flag to true since the date is found
                            break; // Exit the loop since the date is found
                        }
                    }

                    if (!dateFound)
                    {
                        Console.WriteLine("Invalid date");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred: " + ex.Message);
                }
            }
        }
    }
}
