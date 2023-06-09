---
sidebar_position: 2
---

# Generating fake data

There is a ```Faker``` object available to generate fake data.

```
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK",
	"body": {
	  "id": "<#= Faker.Random.Guid() #>",
	  "fruit": "<#= Faker.PickRandom(new[] { "apple", "banana", "orange", "strawberry", "kiwi" }) #>",
	  "recentDate": <#= JsonConvert.SerializeObject(Faker.Date.Recent()) #>
	}
  }
}
```

The built-in fake data is generated via [Bogus](https://github.com/bchavez/Bogus).
The faker can also generate localized data using ```Accept-Language``` HTTP header. Defaults to ```en``` (english) fake data.