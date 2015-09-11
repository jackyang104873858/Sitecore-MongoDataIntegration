# Sitecore Import / DataProvider Hybrid

This is a POC of a hybrid integration approach for getting external data into Sitecore.

## More Information
Read the blog article, watch the video!

* [The Blog Article](http://www.techphoria414.com/Blog/2015/September/Black-Art-Revisited-Sitecore-DataProvider-Import-Hybrid)
* [The Video](https://youtu.be/ntVVOKtFn5k)

## Build
1. Setup a Sitecore instance and install Sitecore Powershell Extensions
2. Update deploy.targets before opening the Solution in Visual Studio -- this is used to resolve the path to needed references in the Sitecore /bin folder.
3. Open solution in Visual Studio.
4. Update the properties of the TDS project as needed. If you don't have TDS, you can manually import the serialized .item files.
    * In that case you will also need to manually deploy or publish build artifacts to your Sitecore install.
5. Create a MongoDB database on localhost called "ProductDataProvider," update the connection string in ProductDataProvider.config if needed.
6. If running the PowerShell scripts, be sure to update file paths and URLs.