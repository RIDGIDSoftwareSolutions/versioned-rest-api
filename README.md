# What is versioned-rest-api?
versioned-rest-api is a tiny library which provides a single attribute for handling versioning of your Web API via the URI. This is useful if you have a public-facing API and need to maintain versions for backward compatibility.

# Why would I version my API using the URI?
Versioning your API via the URI is actually a very simple and explicit approach to versioning. A URI like /api/v3/Chickens/ is clearly using version 3 of the API.

# How does it work?
When you add a NuGet reference to versioned-rest-api it will add a new appSetting called currentApiVersion, which will start off with a value of "1". This represents the most recent version of your API.
You can then annotate your Web API actions with the [ApiRoute] attribute to indicate that the action is versioned. See examples below:

# Examples
This annotation will make the action respond to any versioned request for this resource. Any API action that has never had a breaking change could use this attribute.

```
  [ApiRoute("Chickens/")]
  public IList<Chickens> GetAllChickens()
  {
    //query for and then return all chickens
  }
```

Assuming currentApiVersion in the config is set to "3", then the above action will be invoked for requests to:
```
  /api/v1/Chickens/
  --OR--
  /api/v2/Chickens/
  --OR--
  /api/v3/Chickens/
```
For check out [this code for more examples](https://github.com/RIDGIDSoftwareSolutions/versioned-rest-api/blob/master/VersionedRestApi.Examples/Controllers/ExamplesApiController.cs)
