#20|20 Identity

### Installing User Secrets Manager
The secret manager can be installed globally with:
```
dnu commands install Microsoft.Framework.SecretManager
```

### Adding Project-Specific Secrets
The functional tests depend on access to actual cloud storage provider accounts.  Providing these configuration values can be done through environment variables or the user secret store. Here is how to add settings:
```
cd test\TwentyTwenty.Storage.Azure.Test
user-secret set ConnectionString "myreallylongconnectionstring"
```
Secrets can be listed with:
```
cd test\TwentyTwenty.Storage.Azure.Test
user-secret list
```