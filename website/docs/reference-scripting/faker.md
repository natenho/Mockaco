# Faker

## Basic usage

A `Faker` facade object is available to generate fake data within a script.

```
{
  "request": {
    "method": "GET"
  },
  "response": {
    "status": "OK",
    "headers": {
      "Content-Type": "application/json"
    },
    "body": {
      "name": "<#= Faker.Name.FullName() #>",
      "company": "<#= Faker.Company.CompanyName() #>",
      "city": "<#= Faker.Address.City() #>"
    }
  }
}
```

```shell
$ curl http://localhost:5000
{
  "name": "Mollie Beahan",
  "company": "Ziemann, Anderson and Durgan",
  "city": "Ritchiemouth"
}
```

##  Generating a list of items

This example creates a list of 10 items:

```
{
    "request": {
        "method": "GET",
        "route": "/names"
    },
    "response": {
        "body": <#=
class Person
{
    public int ID { get; set; }
    public string Name { get; set; }
}

var count =  10;
var people = new Person[count];

for(var i = 0; i < count; i++ ) {
    people[i] = new Person { ID = i + 1, Name = new Faker().Person.FullName };
}

return JsonConvert.SerializeObject(people);
#>
    }
}
```

## Localization

To generate localized data, use the `Accept-Language` HTTP header when sending a request to Mockaco. Defaults to `en` (english) fake data.

```
{
  "request": {
	"method": "GET"
  },
  "response": {
	"status": "OK",
    "headers": {
      "Content-Type": "application/json"
    },
	"body": {
	  "name": "<#= Faker.FullName() #>",
	  "company": "<#= Faker.Company.CompanyName() #>",
      "city": "<#= Faker.Address.City() #>"
	}
  }
}
```

```shell
$ curl -X GET "http://localhost:5000" -H "Accept-Language: ru"
{
  "name": "Екатерина Мельникова",
  "company": "Гусев - Никонов",
  "city": "Тула"
}
```

```shell
$ curl -X GET "http://localhost:5000" -H "Accept-Language: pt-BR"
{
  "name": "Maitê Albuquerque",
  "company": "Costa S.A.",
  "city": "Santo André"
}
```

## Using Bogus extensions

To use [Bogus API Extension Methods](https://github.com/bchavez/Bogus?tab=readme-ov-file#api-extension-methods), consider the following example, using the `Bogus.Extensions.Brazil` namespace to generate brazilian CPF numbers:

```
{
  "request": {
    "method": "GET"
  },
  "response": {
    "status": "OK",
    "body": <#= Faker.Person.Cpf() #>
  }
}
```

Use the [Imports option](/docs/configuration/#imports) to import the Bogus extension methods on Mockaco startup:

```shell
$ mockaco --urls=http://+:5000 --Mockaco:Imports:0="Bogus.Extensions.Brazil"
```

Then call the mock endpoint:

```shell
$ curl -i "http://localhost:5000"
HTTP/1.1 200 OK
Content-Type: application/json
Date: Sun, 10 Mar 2024 23:44:50 GMT
Server: Kestrel
Transfer-Encoding: chunked

"422.244.459-62"
```
