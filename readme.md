# Chrome Cookie Extractor

[Related SO question](https://stackoverflow.com/questions/71718371/decrypt-cookies-encrypted-value-from-chrome-chromium-80-in-c-sharp-issue-wi/72156951)

This project represents an example of chrome-like browser cookie decryption

1. It opens chrome browser window through Selenium
   1. Browser uses profile folder which is located in app folder
   2. Navigates to https://stackoverflow.com
   3. Closes the browser window 
2. App decrypts cookie out of profile folder


Example output:
```
Domain=.stackoverflow.com; OptanonAlertBoxClosed=;
Domain=stackoverflow.com; OptanonAlertBoxClosed=;
Domain=.stackoverflow.com; OptanonConsent=;
Domain=stackoverflow.com; OptanonConsent=;
Domain=.stackoverflow.com; prov=ee427636-d130-94f0-ecee-945692582bf3;
```

This example works only on windows machines, because Chrome for linux-like OS uses different cookie encryption algorithm.

> :warning: **It is possible to decrypt only cookies files what was created on same machine and same windows user as it runs this app**
>
> The key for cookies file is stored with `current user` protection scope
>
> `ProtectedData.Unprotect(key, null, DataProtectionScope.CurrentUser);`