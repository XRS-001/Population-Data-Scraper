using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

class Program
{
    static async Task Main()
    {
        string pastPopulation = "https://www.worldometers.info/world-population/world-population-by-year/";
        string forecastedPopulation = "https://www.worldometers.info/world-population/world-population-projections/";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Pick which option you'd like.");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("View credits");
        Console.WriteLine("---------------");
        Console.WriteLine("Country Data:");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Option 1: Country population data.");
        Console.WriteLine("Option 2: Data of a specific country.");
        Console.WriteLine("Option 3: Largest cities of countries.");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Earth Data:");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Option 4: World population data (past).");
        Console.WriteLine("Option 5: World population data (forecasted).");
        Console.WriteLine("Option 6: Misc World data.");
        Console.WriteLine("Option 7: Continent Data");
        Console.ResetColor();
        string optionPicked = Console.ReadLine();

        while (!string.IsNullOrEmpty(optionPicked))
        {
            switch (optionPicked)
            {
                case var _ when optionPicked.Contains("credits") || optionPicked.Contains("Credits"):
                    await WriteCredits();
                    break;

                case var _ when optionPicked.Contains("1") || optionPicked.Contains("first") || optionPicked.Contains("First"):
                    await GetCountryPopulation();
                    break;

                case var _ when optionPicked.Contains("2") || optionPicked.Contains("second") || optionPicked.Contains("Second"):
                    await GetCountryData();
                    break;

                case var _ when optionPicked.Contains("3") || optionPicked.Contains("third") || optionPicked.Contains("Third"):
                    await GetCountryCities();
                    break;

                case var _ when optionPicked.Contains("4") || optionPicked.Contains("fourth") || optionPicked.Contains("Fourth"):
                    await GetWorldPopulation(pastPopulation, 1951, 2020);
                    break;

                case var _ when optionPicked.Contains("5") || optionPicked.Contains("fifth") || optionPicked.Contains("Fifth"):
                    await GetWorldPopulation(forecastedPopulation, 2020, 2100);
                    break;

                case var _ when optionPicked.Contains("6") || optionPicked.Contains("sixth") || optionPicked.Contains("Sixth"):
                    await GetWorldData();
                    break;

                case var _ when optionPicked.Contains("7") || optionPicked.Contains("seventh") || optionPicked.Contains("Seventh"):
                    await GetRegionData();
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
                    List<int> populationRows = new List<int>();
                    List<string> populationList = new List<string>();
                    List<string> landAreaList = new List<string>();
                    string currentCountryName = string.Empty;
                    string currentCountryAreaString = string.Empty;
                    double currentCountryArea = 0;
                    double currentCountryPopulation = 0;

                    foreach (var row in tableRows)
                    {
                        // Extract the values from the <td> elements in each row
                        string rank = row.SelectSingleNode(".//td[1]")?.InnerText;
                        string landArea = row.SelectSingleNode(".//td[7]")?.InnerText;

                        string country = row.SelectSingleNode(".//td[2]/a")?.InnerText;
                        string population = row.SelectSingleNode(".//td[3]")?.InnerText;
                        populationList.Add(population);
                        landAreaList.Add(landArea);
                        if (string.Equals(country, normalizedCountryPicked, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Rank: {rank}");
                            Console.WriteLine($"Country: {country}");
                            Console.WriteLine($"Population: {population}");
                            countryFound = true; // Set the flag to true since the country is found
                            
                            currentCountryArea = double.Parse(landArea);
                            currentCountryAreaString = landArea;
                            currentCountryName = country;
                            currentCountryPopulation = double.Parse(population);
                        }
                    }
                    if (!countryFound) // If the flag is still false, the country was not found
                    {
                        Console.WriteLine("Country not found.");
                    }
                    List<double> doubleList = populationList.Select(x => double.Parse(x)).ToList();
                    List<double> areaListDouble = landAreaList.Select(x => double.Parse(x)).ToList();
                    areaListDouble = areaListDouble.OrderByDescending(num => num).ToList();
                    double[] areaArray = areaListDouble.ToArray();
                    int landRank = Array.IndexOf(areaArray, currentCountryArea) + 1;
                    double totalArea = Count(areaListDouble);
                    string formattedTotalArea = totalArea.ToString("N0") + "Km\u00B2";
                    double median = Median(doubleList);
                    double average = Average(doubleList);
                    double totalPopulation = Count(doubleList); 
                    string formattedTotalPopulation = totalPopulation.ToString("N0");
                    string formattedMedian = median.ToString("N0");
                    string formattedAverage = average.ToString("N0");
                    double decimalOfArea = currentCountryArea / totalArea;
                    decimalOfArea *= 100;
                    string percentageOfArea = decimalOfArea.ToString("0.00") + "%";
                    double decimalOfPopulation = currentCountryPopulation / totalPopulation;
                    string leastPopulatedCountry = populationList[populationList.Count - 1];
                    string mostPopulatedCountry = populationList[0];
                    decimalOfPopulation *= 100;
                    string percentageOfPopulation = decimalOfPopulation.ToString("0.00") + "%";
                    if(countryFound)
                    {
                        Console.WriteLine($"{currentCountryName} makes up {percentageOfPopulation} of the world population");
                        Console.WriteLine("Land Data:");
                        Console.ResetColor();
                        Console.WriteLine($"Rank(area): {landRank}");
                        Console.WriteLine($"Land area: {currentCountryAreaString}Km\u00B2");
                        Console.WriteLine($"{currentCountryName} makes up {percentageOfArea} of the total world land area");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Some Other Data (click T to show or Enter to move on):");
                        Console.ResetColor();
                    }
                    bool tPressed = false;
                    while (true)
                    {
                        // Read the key that was pressed
                        ConsoleKeyInfo keyInfo = Console.ReadKey();

                        // Check if the pressed key is 'A'
                        if (keyInfo.Key == ConsoleKey.T && !tPressed)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.WriteLine($"Median population of all countries: {formattedMedian}");
                            Console.WriteLine($"Average population of all countries: {formattedAverage}");
                            Console.WriteLine($"Least populated country: {tableRows[tableRows.Count - 1].SelectSingleNode(".//td[2]/a")?.InnerText}: {leastPopulatedCountry}");
                            Console.WriteLine($"Most populated country: {tableRows[0].SelectSingleNode(".//td[2]/a")?.InnerText}: {mostPopulatedCountry}");
                            Console.WriteLine($"Total world population: {formattedTotalPopulation}");
                            Console.WriteLine($"Total world land area: {formattedTotalArea}");
                            tPressed = true;
                            break;
                        }
                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured: {ex.Message}");
                }
            }
        }
    }
    static async Task GetCountryCities()
    {
        using (var client = new HttpClient())
        {
            bool shouldContinue = true;
            while (shouldContinue)
            {
                string url = GetUrlCountry();
                if (string.IsNullOrWhiteSpace(url))
                {
                    shouldContinue = false; 
                    continue;
                }
                var response = await client.GetAsync(url);   

                var html = await response.Content.ReadAsStringAsync();

                // Create a new HtmlDocument instance
                HtmlDocument htmlDoc = new HtmlDocument();

                // Load the HTML content from the Worldometer website
                htmlDoc.LoadHtml(html);

                try
                {
                    // Find the table body element
                    var tableBody = htmlDoc.DocumentNode.SelectSingleNode("//tbody");

                    if (tableBody != null)
                    {
                        // Find all table rows within the table body
                        var tableRows = tableBody.SelectNodes(".//tr");

                        foreach (var row in tableRows)
                        {
                            string rank = row.SelectSingleNode(".//td[1]")?.InnerText;
                            string city = row.SelectSingleNode(".//td[2]")?.InnerText;
                            string population = row.SelectSingleNode(".//td[3]")?.InnerText;
                            Console.WriteLine($"{rank}. {city} population: {population}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Country not found.");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error occured: {ex.Message}");
                }
            }
        }
    }
    static async Task GetRegionData()
    {
        using (var client = new HttpClient())
        {
            bool shouldContinue = true;
            while (shouldContinue)
            {
                string url = GetUrlRegion();
                if (string.IsNullOrWhiteSpace(url))
                {
                    shouldContinue = false; 
                    continue;
                }
                var response = await client.GetAsync(url);   

                var html = await response.Content.ReadAsStringAsync();

                // Create a new HtmlDocument instance
                HtmlDocument htmlDoc = new HtmlDocument();

                // Load the HTML content from the Worldometer website
                htmlDoc.LoadHtml(html);

                bool regionFound = false;
                try
                {
                    // Find the table body element
                    var tableBody = htmlDoc.DocumentNode.SelectSingleNode("//tbody");

                    if (tableBody != null)
                    {
                        // Find all table rows within the table body
                        var tableRows = tableBody.SelectNodes(".//tr");
                        double continentPopulation = 0;
                        double continentLandArea = 0;
                        List<object> countries = new List<object>();
                        foreach (var row in tableRows)
                        {
                            regionFound = true;
                            string rank = row.SelectSingleNode(".//td[1]")?.InnerText;
                            string country = row.SelectSingleNode(".//td[2]")?.InnerText;
                            double landArea = double.Parse(row.SelectSingleNode(".//td[7]")?.InnerText.Replace(",", string.Empty));
                            double population = double.Parse(row.SelectSingleNode(".//td[3]")?.InnerText.Replace(",", string.Empty));
                            double landAreaPerPerson = population / landArea;
                            countries.Add(rank);
                            countries.Add(country);
                            countries.Add(population.ToString("#,###"));
                            countries.Add($"{landArea}");
                            countries.Add($"{landAreaPerPerson:F1} Km²");
                            continentLandArea += landArea;
                            continentPopulation += population;

                            Console.WriteLine();
                            string continentLandAreaPerPerson = (continentPopulation / continentLandArea).ToString("#,###");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Continent Population:");
                            Console.ResetColor();
                            Console.WriteLine(continentPopulation.ToString("#,###"));
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Continent Land Area:");
                            Console.ResetColor();
                            Console.WriteLine(continentLandArea.ToString("#,###"));
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Continent Land Area per person:");
                            Console.ResetColor();
                            Console.WriteLine($"{continentLandAreaPerPerson}Km²");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Three most populated countries in this region:");
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        for (int i = 0; i < 15; i += 5)
                        {
                            string rank = countries[i].ToString();
                            string country = countries[i + 1].ToString();
                            string population = countries[i + 2].ToString();
                            string landArea = string.Format("{0:#,###}", Convert.ToDouble(countries[i + 3]));
                            string landAreaPerPerson = countries[i + 4].ToString();

                            Console.WriteLine($"{rank}. {country} {population}");
                            Console.WriteLine($"{country} land area: {landArea} Km²");
                            Console.WriteLine($"Land area per person: {landAreaPerPerson}");
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Continent not found.");
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error occured: {ex.Message}");
                }
            }
        }
    }
    static string GetUrlCountry()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("What country would you like to view the largest cities of(the US and UK are Acronymised)? (press enter to exit)");
        Console.ResetColor();
        string countryPicked = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(countryPicked))
        {
            return null;
        }
        else
        {
            countryPicked = countryPicked.ToLower();
            string normalizedCountryPicked = RemoveLeadingThe(countryPicked);
            return $"https://www.worldometers.info/demographics/{countryPicked}-demographics/";
        }
    }
    static string GetUrlRegion()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("What continent would you like the data of? (press enter to exit)");
        Console.ResetColor();
        string regionPicked = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(regionPicked))
        {
            return null;
        }
        else
        {
            regionPicked = regionPicked.ToLower();
            regionPicked = regionPicked.Replace(" ", "-");
            regionPicked = regionPicked.Replace("south-america", "latin-america-and-the-caribbean");
            regionPicked = regionPicked.Replace("north-america", "northern-america");
            return $"https://www.worldometers.info/population/countries-in-{regionPicked}-by-population/";
        }
    }
    static async Task GetWorldPopulation(string url, int minimumDate, int maximumDate)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDoc = new HtmlDocument();

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
                else
                {
                    dateFound = false;
                }

                try
                {
                    var tableBody = htmlDoc.DocumentNode.SelectSingleNode("//tbody");

                    var tableRows = tableBody.SelectNodes(".//tr");

                    foreach (var row in tableRows)
                    {
                        string tableDate = row.SelectSingleNode(".//td[1]")?.InnerText;
                        int parsedTableDate = int.Parse(tableDate);
                        string tablePopulation = row.SelectSingleNode(".//td[2]")?.InnerText;

                        if (date == parsedTableDate)
                        {
                            Console.WriteLine($"Date: {date}, population: {tablePopulation}");
                            Console.WriteLine();
                            dateFound = true;
                            break;
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
    static async Task GetCountryData()
    {
        bool shouldContinue = true;
        while (shouldContinue)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"What country would you like the data of? Press enter to exit.");
            Console.ResetColor();
            Console.WriteLine();
            string countryPicked = Console.ReadLine();
            string formattedCountryPicked = countryPicked.ToLower();
            string formattedCountryNoThe = RemoveLeadingThe(formattedCountryPicked);
            formattedCountryNoThe = formattedCountryNoThe.Replace(" ", "-");
            string capatilizedCountry = CapitalizeFirstLetter(countryPicked);
            string url = $"https://www.cia.gov/the-world-factbook/countries/{formattedCountryNoThe}/";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var html = await response.Content.ReadAsStringAsync();

                // Create a new HtmlDocument instance
                HtmlDocument htmlDoc = new HtmlDocument();

                // Load the HTML content from the Worldometer website
                htmlDoc.LoadHtml(html);

                bool dateFound = false;

                if (string.IsNullOrWhiteSpace(countryPicked))
                {
                    shouldContinue = false;
                    continue;
                }
                try
                {
                    string totalMedianAge;
                    string maleMedianAge;
                    string femaleMedianAge;
                    string totalPopulationLifeExpectancy;
                    string malePopulationLifeExpectancy;
                    string femalePopulationLifeExpectancy;
                    string urbanPopulation;
                    string urbanRateOfChange;
                    string[] data = {
                    totalMedianAge = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[8]/p/text()[1]")?.InnerText, 
                    maleMedianAge = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[8]/p/text()[2]")?.InnerText,
                    femaleMedianAge = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[8]/p/text()[3]")?.InnerText,
                    totalPopulationLifeExpectancy = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[20]/p/text()[1]")?.InnerText,
                    malePopulationLifeExpectancy = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[20]/p/text()[2]")?.InnerText,
                    femalePopulationLifeExpectancy = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[20]/p/text()[3]")?.InnerText,
                    urbanPopulation = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[14]/p/text()[1]")?.InnerText,
                    urbanRateOfChange = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[14]/p/text()[2]")?.InnerText
                    };

                    bool hasNullElement = data.Any(element => element == null);
                    if (hasNullElement)
                    {
                        data = data.Select(element => element ?? "Not Found").ToArray();
                    }
                    data = data.Select(element => RemoveTextInsideBrackets(element)).ToArray();
                    string firstItem = data[0];
                    bool allEqual = data.Skip(1).All(s => string.Equals(firstItem, s, StringComparison.InvariantCultureIgnoreCase));
                    if (!allEqual)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"{capatilizedCountry}:");
                        Console.WriteLine("Median Age:");
                        Console.WriteLine($"Total median age: {data[0]}");
                        Console.WriteLine($"Male median age: {data[1]}");
                        Console.WriteLine($"Female median age: {data[2]}");
                        Console.WriteLine();
                        Console.WriteLine("Life Expectancy:");
                        Console.WriteLine($"Total life expectancy: {data[3]}");
                        Console.WriteLine($"Male life expectancy: {data[4]}");
                        Console.WriteLine($"Female life expectancy: {data[5]}");
                        Console.WriteLine();
                        Console.WriteLine("Urbanization:");
                        Console.WriteLine($"Urban Population: {data[6]}");
                        Console.WriteLine($"Urban Rate of Change: {data[7]}");
                    }
                    else
                    {
                        Console.Write("Country not found.");
                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Country not found.");
                }
            }
        }
    }
static async Task WriteCredits()
{
    bool shouldContinue = true;
    while (shouldContinue)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("World population data was obtained from:");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("https://www.worldometers.info/world-population/");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Society data and economic data was obtained from:");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("https://www.cia.gov/the-world-factbook/");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press enter to exit");
        
        string line = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(line))
        {
            shouldContinue = false;
        }
    }
}

static async Task GetWorldData()
{
    Console.WriteLine("(Data is from 2020-2023)");
    bool shouldContinue = true;
    string optionPicked = "";
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Choose a metric to view (Press enter to exit).");
    Console.WriteLine("------------------------------------------------");
    Console.WriteLine("1: Population data");
    Console.WriteLine("2: Language data");
    Console.WriteLine("3: Religion data");
    Console.WriteLine("4: Age structure data");
    Console.WriteLine("5: Median age data");
    Console.WriteLine("6: Urbanization");
    Console.WriteLine("7: Sex ratio");
    Console.WriteLine("8: Life expectancy at birth");
    Console.WriteLine("9: Economy:");
    Console.WriteLine("10: Print all");
    Console.ResetColor();

    while (shouldContinue)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("What data would you like? (pick a number)");
        Console.ResetColor();
        Console.WriteLine();
        optionPicked = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(optionPicked))
        {
            shouldContinue = false;
            continue;
        }

        string url = $"https://www.cia.gov/the-world-factbook/countries/world/";
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            // Create a new HtmlDocument instance
            HtmlDocument htmlDoc = new HtmlDocument();

            // Load the HTML content from the Worldometer website
            htmlDoc.LoadHtml(html);

            switch (optionPicked)
            {
                case "1":
                    GetPopulationData(htmlDoc);
                    break;

                case "2":
                    GetLanguageData(htmlDoc);
                    break;

                case "3":
                    GetReligionData(htmlDoc);
                    break;

                case "4":
                    GetAgeStructureData(htmlDoc);
                    break;

                case "5":
                    GetMedianAgeData(htmlDoc);
                    break;

                case "6":
                    GetUrbanizationData(htmlDoc);
                    break;

                case "7":
                    GetSexRatioData(htmlDoc);
                    break;

                case "8":
                    GetLifeExpectancyData(htmlDoc);
                    break;

                case "9":
                    GetEconomyData(htmlDoc);
                    break;

                case "10":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("WorldData:");
                    Console.ForegroundColor =ConsoleColor.White;
                    Console.WriteLine("----------------");
                    Console.ResetColor();
                    GetPopulationData(htmlDoc);
                    GetLanguageData(htmlDoc);
                    GetReligionData(htmlDoc);
                    GetAgeStructureData(htmlDoc);
                    GetMedianAgeData(htmlDoc);
                    GetUrbanizationData(htmlDoc);
                    GetSexRatioData(htmlDoc);
                    GetLifeExpectancyData(htmlDoc);
                    GetEconomyData(htmlDoc);
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Invalid option. Please pick a number from the list.");
                    Console.ResetColor();
                    Console.WriteLine();
                    break;
            }
        }
    }
}

static void GetPopulationData(HtmlDocument htmlDoc)
{
    try
    {
        string totalPopulation = RemoveTextInsideBrackets(RemoveAnythingPastSpace(htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[1]/p/text()[1]").InnerText));
        string mostPopulatedCountry = RemoveTextInsideBrackets(RemoveAnythingPastSpace(htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[1]/p/text()[2]").InnerText));
        string leastPopulatedCountry = RemoveTextInsideBrackets(RemoveAnythingPastSpace(htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[1]/p/text()[3]").InnerText));
        string mostDenseCountry = RemoveTextInsideBrackets(RemoveAnythingPastSpace(htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[1]/p/text()[3]").InnerText));
        string leastDenseCountry = RemoveTextInsideBrackets(RemoveAnythingPastSpace(htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[1]/p/text()[5]").InnerText));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Population Data:");
        Console.ResetColor();
        Console.WriteLine($"Total population: {totalPopulation}");
        Console.WriteLine($"Most populated country (in millions): {mostPopulatedCountry}");
        Console.WriteLine($"Least populated country: {leastPopulatedCountry}");
        Console.WriteLine($"Most densely populated country: {mostDenseCountry}");
        Console.WriteLine($"Least densely populated country: {leastDenseCountry}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving population data.");
    }
}

static void GetLanguageData(HtmlDocument htmlDoc)
{
    try
    {
        string xpathLanguages = "//div/p/strong[contains(., 'most-spoken language:')]/following-sibling::text()[1]";
        string xpathFirstLanguages = "//div/p/strong[contains(., 'most-spoken first language:')]/following-sibling::text()[1]";

        string mostSpokenLanguages = htmlDoc.DocumentNode.SelectSingleNode(xpathLanguages)?.InnerText.Trim();
        string mostSpokenFirstLanguages = htmlDoc.DocumentNode.SelectSingleNode(xpathFirstLanguages)?.InnerText.Trim();

        string[] rankedLanguages = GetRankedList(mostSpokenLanguages);
        string[] rankedFirstLanguages = GetRankedList(mostSpokenFirstLanguages);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Most spoken languages:");
        Console.ResetColor();
        foreach (string language in rankedLanguages)
        {
            Console.WriteLine(language);
        }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Most spoken first languages:");
        Console.ResetColor();
        foreach (string language in rankedFirstLanguages)
        {
            Console.WriteLine(language);
        }
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving language data.");
    }
}

static void GetReligionData(HtmlDocument htmlDoc)
{
    try
    {
        string religions = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='people-and-society']/div[3]/p/text()")?.InnerText;
        string[] rankedReligions = GetRankedList(religions);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Religions:");
        Console.ResetColor();
        foreach (string religion in rankedReligions)
        {
            string formattedReligion = religion.Replace("&lt;", ">");
            Console.WriteLine(formattedReligion);
        }
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving religion data.");
    }
}
static void GetAgeStructureData(HtmlDocument htmlDoc)
{
    try
    {
        string zeroTo14Years = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[4]/p/text()[1]");
        string fifteenTo64Years = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[4]/p/text()[2]");
        string moreThan65Years = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[4]/p/text()[3]");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Age Structure:");
        Console.ResetColor();
        Console.WriteLine($"0-14 years: {zeroTo14Years}");
        Console.WriteLine($"15-64 years: {fifteenTo64Years}");
        Console.WriteLine($"65+ years: {moreThan65Years}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving age structure data.");
    }
}

static void GetMedianAgeData(HtmlDocument htmlDoc)
{
    try
    {
        string totalMedianAge = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[6]/p/text()[1]");
        string femaleMedianAge = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[6]/p/text()[3]");
        string maleMedianAge = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[6]/p/text()[2]");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Median Age:");
        Console.ResetColor();
        Console.WriteLine($"Total: {totalMedianAge}");
        Console.WriteLine($"Male age: {maleMedianAge}");
        Console.WriteLine($"Female age: {femaleMedianAge}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving median age data.");
    }
}

static void GetUrbanizationData(HtmlDocument htmlDoc)
{
    try
    {
        string urbanRateOfChange = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[11]/p/text()[2]");
        string urbanPopulation = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[11]/p/text()[1]");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Urbanization:");
        Console.ResetColor();
        Console.WriteLine($"Urban population: {urbanPopulation}");
        Console.WriteLine($"Rate of change: {urbanRateOfChange}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving urbanization data.");
    }
}

static void GetSexRatioData(HtmlDocument htmlDoc)
{
    try
    {
        string atBirth = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[13]/p/text()[1]");
        string zeroTo14Years = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[13]/p/text()[2]");
        string fifteenTo64Years = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[11]/p/text()[1]");
        string moreThan65Years = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[13]/p/text()[3]");
        string totalPopulation = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[13]/p/text()[5]");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Sex Ratio:");
        Console.ResetColor();
        Console.WriteLine($"Total population: {totalPopulation}");
        Console.WriteLine($"At birth: {atBirth}");
        Console.WriteLine($"0-14 years: {zeroTo14Years}");
        Console.WriteLine($"15-64 years: {fifteenTo64Years}");
        Console.WriteLine($"65+ years: {moreThan65Years}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving sex ratio data.");
    }
}

static void GetLifeExpectancyData(HtmlDocument htmlDoc)
{
    try
    {
        string male = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[16]/p/text()[2]");
        string female = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[16]/p/text()[3]");
        string totalPopulation = GetInnerTextByXPath(htmlDoc, "//*[@id='people-and-society']/div[16]/p/text()[1]");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Life Expectancy:");
        Console.ResetColor();
        Console.WriteLine($"Total population: {totalPopulation}");
        Console.WriteLine($"Male: {male}");
        Console.WriteLine($"Female: {female}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving life expectancy data.");
    }
}

static void GetEconomyData(HtmlDocument htmlDoc)
{
    try
    {
        string gdp = GetInnerTextByXPath(htmlDoc, "//*[@id='economy']/div[1]/p/text()[1]");
        string gdpGrowthRate = GetInnerTextByXPath(htmlDoc, "//*[@id='economy']/div[2]/p/text()[1]");
        string gdpPerCapita = GetInnerTextByXPath(htmlDoc, "//*[@id='economy']/div[3]/p/text()[1]");
        string inflationRate = GetInnerTextByXPath(htmlDoc, "//*[@id='economy']/div[5]/p/text()[1]");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Economy:");
        Console.ResetColor();
        Console.WriteLine($"GDP: {gdp}");
        Console.WriteLine($"GDP growth rate: {gdpGrowthRate}");
        Console.WriteLine($"GDP per capita: {gdpPerCapita}");
        Console.WriteLine($"Inflation rate: {inflationRate}");
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Error occurred while retrieving economy data.");
    }
}
static string GetInnerTextByXPath(HtmlDocument htmlDoc, string xpath)
{
    var node = htmlDoc.DocumentNode.SelectSingleNode(xpath);
    string innerText = node?.InnerText.Trim() ?? string.Empty;

    // Remove text between brackets
    innerText = Regex.Replace(innerText, @"\([^()]*\)", "");

    return innerText.Trim();
}
static string[] GetRankedList(string languages)
{
    if (string.IsNullOrWhiteSpace(languages))
        return new string[0];

    string[] languageArray = languages.Split(',');

    List<string> rankedLanguages = new List<string>();
    foreach (string language in languageArray)
    {
        string trimmedLanguage = language?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedLanguage))
        {
            string rank = (rankedLanguages.Count + 1).ToString();
            rankedLanguages.Add($"{rank}. {RemoveTextInsideBrackets(trimmedLanguage)}");
        }
    }
    return rankedLanguages.ToArray();
}

static string RemoveAnythingPastSpace(string input)
{
    if (string.IsNullOrEmpty(input))
        return input;

    int semicolonIndex = input.IndexOf(';');
    if (semicolonIndex != -1)
    {
        input = input.Substring(0, semicolonIndex);
    }

    return input;
}
    static string RemoveTextInsideBrackets(string input)
    {
        return Regex.Replace(input, @"\s*\([^()]*\)", "").Trim();
    }
    static double Median(List<double> numbers)
    {
        if (numbers.Count == 0)
            return 0;

        numbers = numbers.OrderBy(n => n).ToList();
        var halfIndex = numbers.Count() / 2;
        if (numbers.Count() % 2 == 0)
            return (numbers[halfIndex] + numbers[halfIndex - 1]) / 2.0;

        return numbers[halfIndex];
    }
    
    static double Average(List<double> numbers)
    {
        if (numbers.Count == 0)
            return 0;

        return numbers.Sum() / numbers.Count;
    }
    static double Count(List<double> numbers)
    {
        if (numbers.Count == 0)
            return 0;

            return numbers.Sum();
    }
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
}

