# Atomic Repositories for CRUD (1.0.x)
If you haven't already read the [Atoms Data Model Classes](https://github.com/caleb-bender/atoms/blob/main/Documentation/1.0.x/AtomsDataModelClasses.md) documentation, please do so since it will make understanding the `IAtomicRepository<TModel>` interface much easier.
## Introduction
The `IAtomicRepository<TModel>` interface defines several methods for reading/writing to database entities:
```csharp
namespace CalebBender.Atoms.Repositories
{
	public interface IAtomicRepository<TModel> where TModel : class, new()
	{
		Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models);
		Task DeleteManyAsync(IEnumerable<TModel> models);
		Task<AtomicOption<TModel>> GetOneAsync(TModel model);
	}
}
```
As noted in the [Atoms Data Model Classes](https://github.com/caleb-bender/atoms/blob/main/Documentation/1.0.x/AtomsDataModelClasses.md) documentation,  the `UpdateManyAsync` method is missing and will be added in the next release or so. Each of these methods allow you to plug in any instance or set of instances of a data class model and perform CRUD operations that are atomic; that is, they either fully succeed or not at all. To create an instance of an `IAtomicRepository<TModel>`, one must always use an implementation of the `IAtomicRepositoryFactory<TModel>` interface.

## Creating a repository instance
As of version 1.0.x, the following `IAtomicRepositoryFactory<TModel>` implementations exist within the `CalebBender.Atoms.Repositories.Factories` namespace:
```
SqlServerAtomicRepositoryFactory<TModel>
```
Suppose you have an ASP.NET Core Web API project and you want to register an `IAtomicRepository<TModel>` using the .NET Dependency Injection Container. Since each `IAtomicRepository<TModel>` is a completely stateless object, it makes sense to register a singleton for each type of repository. As a result, the code in your `Program.cs` file might look something like this:
```csharp
using CalebBender.Atoms.Repositories.Factories;
using MyBlogProject.Models;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
// load the connection string from environment
string connectionString = Environment.GetEnvironmentVariable("SQL_SERVER_DB_CONNECTION");
// Add singleton for BlogPostAuthor
builder.Services.AddSingleton(
	() => new SqlServerAtomicRepositoryFactory<BlogPostAuthor>()
			.CreateRepository(connectionString)
);
// Add singleton for BlogUser
builder.Services.AddSingleton(
	() => new SqlServerAtomicRepositoryFactory<BlogUser>()
			.CreateRepository(connectionString)
);
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();
app.Run();
```
Here you can see that the process for creating a new `IAtomicRepository<TModel>` instance is rather trivial. Just load your connection string and then pass it to the `CreateRepository` method of your factory.

You may be wondering why a factory is utilized to obtain an `IAtomicRepository<TModel>`. There are three reasons for this design:

1. The `CreateRepository` method always returns the interface `IAtomicRepository<TModel>` as opposed to the underlying implementation class. This is good because it prevents tight-coupling, forcing code to only depend on the abstraction. As a corollary, suppose another database engine is implemented in Atoms in a later version, and thus has a corresponding factory `AnotherDatabaseRepositoryFactory<TModel>`. Because this factory also returns an `IAtomicRepository<TModel>` from its `CreateRepository` method, to change the underlying database used for CRUD would amount to zero code changes, apart from the dependency injection logic in the `Program.cs` file, which is trivial.
2. The `CreateRepository` validates that the connection string provided is valid, by attempting to quickly open and close a connection to the database. If this connection attempt fails, a `AtomsConnectionException` is thrown, allowing the programmer to catch issues with the connection immediately on initialization.
3. The `CreateRepository` method validates the `UniqueIdAttribute`s of the `TModel` generic data model class.
	* First, it confirms that it contains at least one public property with the `UniqueIdAttribute`. If there is not at least one public property annotated with the `UniqueIdAttribute`, then a `MissingUniqueIdAttributeException` is thrown.
	* Second, it validates that the public properties annotated as unique ids are compatible types (See [Introduction](https://github.com/caleb-bender/atoms/blob/main/Documentation/1.0.x/AtomsDataModelClasses.md#introduction) of the Atoms Data Model Class docs for a list of the supported unique id types). If a public property annotated with `UniqueIdAttribute` is incompatible, then a `PropertyTypeIsIncompatibleWithUniqueIdAttributeException` is thrown.
	* Moreover, it validates that if there are auto-generated unique id properties (See [Multiple properties used as unique identifier](https://github.com/caleb-bender/atoms/blob/main/Documentation/1.0.x/AtomsDataModelClasses.md#multiple-properties-used-as-unique-identifier) for an example), that the auto-generated properties are valid for the specific database. If the type of the property cannot be annotated with `[UniqueIdAttribute(AutoGenerated = true)]`, then an exception is thrown (For SQL Server, this exception is `AutoGeneratedUniqueIdMustBeAnIntegerTypeException`, since identity columns can only be integers). The method `CreateRepository` also validates that there are not too many auto-generated unique id properties. If there are too many public properties annotated with `[UniqueIdAttribute(AutoGenerated = true)]`, then a `TooManyAutoGeneratedUniqueIdsException` is thrown.

In short, the `CreateRepository` method attempts to do a thorough validation process on both the `TModel` data model class used as the generic type argument, as well as its usage of the `UniqueIdAttribute`. By doing this validation upfront whenever a new `IAtomicRepository<TModel>` instance is created, the probability for unexpected errors later on is vastly reduced. Simply put, if you factory's `CreateRepository` method executes with no exceptions thrown, you can be confident that the returned `IAtomicRepository<TModel>` is ready for use!
## Creating a single data model
To create a single data model using an `IAtomicRepository<TModel>` instance, call the `CreateOneAsync` extension method with a single data model class instance passed in. Here is a code example:
```csharp
using CalebBender.Atoms.Repositories;

IAtomicRepository<Employee> employeeRepo = new SqlServerRepositoryFactory<Employee>.CreateRepository("some connection string");
Employee employee = new Employee
{
	EmployeeId = Guid.NewGuid(),
	Salary = 120_000.00M
};
var createdEmployee = await employeeRepo.CreateOneAsync(employee);
```
In this case, a single employee is created in the database. Notice that the created employee is returned to the `createdEmployee` method. All create repository functions return the created model or models, since it is possible that the model type contains auto-generated ids that the database defines. Suppose this is our `Employee` class definition:
```csharp
public class Employee
{
	[UniqueId]
	public Guid EmployeeId { get; set; }
	[UniqueId(AutoGenerated = true)]
	public long LocationId { get; set; }
	public decimal Salary { get; set; }
}
```
Because the `LocationId` property value is generated by the database, we need to return the employee instance with the `LocationId` set, thus requiring the code to capture the return in `createdEmployee`. Even when a data model class does not contain any unique id properties that are auto-generated, it is still best practice to use the returned model or models, as it prevents unexpected behavior down the road from forgetting this nuance.
## Creating multiple data models in a batch
To create a series of data models, there are two ways to do this. The first way is passing an `IEnumerable<TModel>` to the `CreateManyAsync` method. Here is an example:
```csharp
using CalebBender.Atoms.Repositories;

IAtomicRepository<Employee> blogUserRepo = new SqlServerRepositoryFactory<BlogUser>.CreateRepository("some connection string");

var blogUsersToCreate = new List<BlogUser> { /* pretend there are blog users in here */ };
var createdBlogUsers = await blogUserRepo.CreateManyAsync(blogUsersToCreate);
// do something with the createdBlogUsers
```
Another way is to pass in a variable number of data model instances to the `CreateManyAsync` method. Here is an example of this:
```csharp
using CalebBender.Atoms.Repositories;

IAtomicRepository<BlogUser> blogUserRepo = new SqlServerRepositoryFactory<BlogUser>.CreateRepository("some connection string");

var blogUser1 = new BlogUser { /* set properties here */ };
var blogUser2 = new BlogUser { /* set properties here */ };
var blogUser3 = new BlogUser { /* set properties here */ };
var createdBlogUsers = await blogUserRepo.CreateManyAsync(blogUser1, blogUser2, blogUser3);
// do something with the createdBlogUsers
```
In both cases, a single atomic create command is generated by Atoms, such that either all data models passed in will be created, or not at all. In addition, this has the consequence of improving performance, as `CreateManyAsync` only sends the command once over the network to the database, as opposed to a network communication for every model.

If you recall from earlier, the `IAtomicRepository<TModel>` interface only contains the following definition for creating data model classes:
```csharp
Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models);
```
As it turns out, the other `CreateManyAsync` overload and `CreateOneAsync` methods are extension methods that call the `IAtomicRepository<TModel>.CreateManyAsync` method behind the scenes.

## Retrieving a single data model
You may expect the `IAtomicRepository<TModel>` to contain a definition for `GetManyAsync` and not just `GetOneAsync`, However, because an `IAtomicRepository<TModel>` is designed to be used with a data model class containing a unique id or ids, it should only ever read a distinct model from the database. To query multiple, see the [Atoms Query Template Documentation](https://github.com/caleb-bender/atoms/blob/main/Documentation/1.0.x/AtomsQueryTemplates.md).

To get a single data model, just pass in a partially defined instance with only the unique id public properties set:
```csharp
using CalebBender.Atoms.Repositories;

IAtomicRepository<Employee> employeeRepo = new SqlServerRepositoryFactory<Employee>.CreateRepository("some connection string");
// Use appropriate unique ids
Guid someGuid = Guid.NewGuid();
long someLong = 1L;

var employeeOption = await employeeRepo.GetOneAsync(
	new Employee {
		EmployeeId = someGuid,
		LocationId = someLong
	}
);
```
If you recall from the `GetOneAsync` definition, a `Task` containing an `AtomicOption<T>` is returned. `AtomicOption<T>` is a generic wrapper used by Atoms to make unexpected null reference errors virtually impossible. To unwrap an `AtomicOption<T>` to see if there is an underlying employee model instance, you would perform the following check:
```csharp
if (employeeOption is AtomicOption<Employee>.Exists employeeExists)
{
	var retrievedEmployee = employeeExists.Value;
}
else
{
	// handle non-existent retrieved employee
}
```
If you wanted to return early from a function whenever an employee was not retrieved, you could invert the above `if` statement:
```csharp
if (employeeOption is not AtomicOption<Employee>.Exists employeeExists) return;
var retrievedEmployee = employeeExists.Value;
```
You can also check the explicit case when it is empty with this `if` statement:
```csharp
if (employeeOption is AtomicOption<Employee>.Empty employeeEmpty)
{
	// handle empty case
}
```
Atoms opts for this approach whenever a value could be absent, since it is safer and less error prone than returning null or a nullable object.
## Deleting a single data model
To delete a single model, you can make use of the `DeleteOneAsync` extension method, by passing in a data model class instance with at least the unique id public properties set. Here is an example:
```csharp
using CalebBender.Atoms.Repositories;

IAtomicRepository<BlogPost> blogPostRepo = new SqlServerRepositoryFactory<BlogPost>.CreateRepository("some connection string");
// Assume the blog post with a PostId of 1 already exists
var blogPost = new BlogPost { PostId = 1L };
await blogPostRepo.DeleteOneAsync(blogPost);
```
## Deleting multiple data models in a batch
As with `CreateManyAsync`, there are two different ways to delete data models with `DeleteManyAsync`. The first is by passing in an `IEnumerable<TModel>` to `DeleteManyAsync`. Here is an example:
```csharp
using CalebBender.Atoms.Repositories;

IAtomicRepository<BlogPost> blogPostRepo = new SqlServerRepositoryFactory<BlogPost>.CreateRepository("some connection string");
// Assume the list contains some blog posts to delete
var blogPosts = new List<BlogPost> { /* insert blog post instances here */ };
await blogPostRepo.DeleteManyAsync(blogPosts);
```
Another way is to pass in a variable number of data model instances to the `DeleteManyAsync` method. Here is an example of this:
```csharp
using CalebBender.Atoms.Repositories;

IAtomicRepository<BlogPost> blogPostRepo = new SqlServerRepositoryFactory<BlogPost>.CreateRepository("some connection string");
var blogPost1 = new BlogPost { PostId = 1L };
var blogPost2 = new BlogPost { PostId = 2L };
var blogPost3 = new BlogPost { PostId = 3L };
await blogPostRepo.DeleteManyAsync(blogPost1, blogPost2, blogPost3);
```
In both cases, a single atomic delete command is generated by Atoms, such that either all data models passed in will be deleted, or none at all. In addition, this has the consequence of improving performance, as `DeleteManyAsync` only sends the command once over the network to the database, as opposed to a network communication for every model.

If you recall from earlier, the `IAtomicRepository<TModel>` interface only contains the following definition for deleting data model classes:
```csharp
Task DeleteManyAsync(IEnumerable<TModel> models);
```
As it turns out, the other `DeleteManyAsync` overload and `DeleteOneAsync` methods are extension methods that call the `IAtomicRepository<TModel>.DeleteManyAsync` method behind the scenes.
## `CreateOneAsync` and `CreateManyAsync` Exceptions

* `DbEntityNotFoundException`
* `DuplicateUniqueIdException`
* `EnumPropertyMappingFailedException`
* `ModelDbEntityMismatchException`
* `ModelPropertyTypeMismatchException`
* `StringPropertyValueExceedsMaxLengthException`

## `GetOneAsync` Exceptions

* `DbEntityNotFoundException`
* `ModelDbEntityMismatchException`
* `ModelPropertyTypeMismatchException`

## `DeleteOneAsync` and `DeleteManyAsync` Exceptions

* `DbEntityNotFoundException`
* `ModelDbEntityMismatchException`
* `ModelPropertyTypeMismatchException`

## Conclusion
In a nutshell, the `IAtomicRepository<TModel>` interface is meant to make CRUD operations seamless and simple. Once an `IAtomicRepository<TModel>` instance is constructed, any data model instance matching that repository's `TModel` generic type can be passed to the CRUD methods discussed above without extra boilerplate and setup code.
