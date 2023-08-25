using ChromeCookieExtractor;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Cookie = System.Net.Cookie;

//default location is %LOCALAPPDATA%\Chromium\User Data
string profileDir = Path.Combine(Environment.CurrentDirectory, "Profile");

PrepareCookies(profileDir);

//Have to wait some time
await Task.Delay(3000);

ReadCookies(profileDir);

Console.ReadLine();
return;


void PrepareCookies(string profileDirectory)
{
    ChromeOptions options = new ChromeOptions() { };
    options.AddArgument($"user-data-dir={profileDirectory}");
    options.AddArgument("--log-level=3");
    
    ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService(options);
    chromeDriverService.SuppressInitialDiagnosticInformation = true;

    SeleniumManager.DriverPath(options);
    using (ChromeDriver driver = new ChromeDriver(chromeDriverService, options))
    {
        driver.Navigate().GoToUrl("https://stackoverflow.com");
        driver.Close();
    }
}

void ReadCookies(string profileDirectory)
{
    CookieExtractor retriever = new CookieExtractor();
    ICollection<Cookie> cookies = retriever.GetCookies(profileDirectory);
    foreach (Cookie cookie in cookies)
    {
        Console.WriteLine($"Domain={cookie.Domain}; {cookie.Name}={cookie.Value};");
    }
}

