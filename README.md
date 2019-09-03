# DBSizeChecker
Simple config, must be stored as **settings.json** in app folder. 

```
{"Hosts":[{"ServerID":"LocalTest","Connection":"Host=127.0.0.1;Username=postgres;Password=123456;","DiskSpace":930.0}],
"Output":{"PathToCredentials":"client_id.json", "SheetName":"test"},
"RetryInterval":10}
```
**client_id.json** -- API Credentials, must be obtainet via Google Dev Console.
