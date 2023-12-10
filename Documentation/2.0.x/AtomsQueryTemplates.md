# Atoms Query Template Documentation (2.0.x)
If you haven't already read the [Atoms Data Model Classes](https://github.com/caleb-bender/atoms/blob/main/Documentation/2.0.x/AtomsDataModelClasses.md) documentation, please do so since it will make understanding the `IAtomicQueryTemplate<T>` interface and any related documentation much easier to grasp, at least as it pertains to data model classes.
## Introduction
If you have already read through the [Atomic Repositories for CRUD](https://github.com/caleb-bender/atoms/blob/main/Documentation/2.0.x/AtomsRepositoriesForCRUD.md) documentation, you will recall that the `IAtomicRepository<TModel>` interface only contains a `GetOneAsync` method, used to obtain a distinct data model from the database. What if you want to query multiple data models, or a subset of a data model's properties, or a single scalar? Moreover, what if you have specific filtering criteria? This is where the `IAtomicQueryTemplate<T>` comes in. Its definition is very simple:
```csharp
namespace CalebBender.Atoms.Templates.Query
{
	public interface IAtomicQueryTemplate<T>
	{
		IAsyncEnumerable<T> QueryLazy(object? parameters = null);
	}
}
```
It also has the following extension method defined in the same namespace as the interface:
```csharp
public static async Task<IEnumerable<T>> QueryAsync<T>(this IAtomicQueryTemplate<T> template, object? parameters = null);
```
Essentially, an implementation of a `IAtomicQueryTemplate` defines a query template that can be reused over and over with different parameters (if parameters are required). The only state a query template contains is a connection string, query command text, a reference to an exception handler, and a `CancellationToken`. All of these members are defined once and immutable thereafter. Any resources like a connection object are used and disposed of within the local scope of the returned `IAsyncEnumerable<T>` from the `QueryLazy` method, or the local scope of the `QueryAsync` method. As of version 2.0.x, query templates can only be created by the use of the class `SqlServerRawTemplateBuilder` which implements the interface `IRawTemplateBuilder`. Let's create a query template using this builder!
## Creating a scalar query template using `SqlServerRawTemplateBuilder`
Suppose we want to query the title of each blog post, filtering by the genre and the title's first letter. Recall our `BlogPost` data model class definition:
```csharp
public class BlogPost
{
	public enum BlogPostGenre
	{
		Horror,
		Thriller,
		Scifi,
		Fantasy
	}
	[UniqueId]
	public long PostId { get; set; }
	public BlogPostGenre Genre { get; set; }
	[MaxLength(255)]
	public string Title { get; set; } = "";
	public string Content { get; set; } = "";
	public List<BlogComment>? BlogComments { get; set; }
}
```
We could obtain an `IAtomicQueryTemplate<string>` by using these series of calls on our `SqlServerRawTemplateBuilder` instance:
```csharp
var connectionString = ""; // get the SQL Server connection string to use
var blogPostTitleQueryTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString(connectionString)
		.SetQueryText("SELECT Title FROM BlogPosts WHERE Genre = @Genre AND Title LIKE @Title")
		.GetQueryTemplate<string>();
```
Now we can reuse this template as many times as we want, passing in the relevant parameters each time as an anonymous object to either `QueryLazy` or `QueryAsync`. The `blogPostTitleQueryTemplate` is a scalar query template, since we are querying a single column. But what if we want a query template that queries multiple columns?
## Creating a tuple query template using `SqlServerRawTemplateBuilder`
Sometimes we want to retrieve multiple properties and not just one. Suppose we wanted to query the post id, title, and genre of each blog post. We could do so with this chain of calls:
```csharp
var blogPostIdTitleAndGenreQueryTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString(connectionString)
		.SetQueryText("SELECT PostId, Genre, Title FROM BlogPosts")
		.GetQueryTemplate<(long, BlogPost.BlogPostGenre, string)>();
```
This is known as a tuple query template, since we are capturing the queried properties in a `ValueTuple` instance. In C#, the maximum number of values a `ValueTuple` can hold is eight. Any more than that and nested tuples are required. Atoms only supports the use of `ValueTuples` and does not support using nested tuples or the `Tuple` type with query templates. If you need to query more than eight properties, then you should probably be using a data model class anyway.
## Creating a data model query template using `SqlServerRawTemplateBuilder`
Suppose we want to create a template to query all blog posts that are of a specific genre. This code would do the trick:
```csharp
var blogPostsWithGenreQueryTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString(connectionString)
		.SetQueryText("SELECT * FROM BlogPosts WHERE Genre = @Genre")
		.GetQueryTemplate<BlogPost>();
```
In this case, we are querying entire `BlogPost` objects based on whether they possess the `@Genre` parameter value or not. Thus, `blogPostsWithGenreQueryTemplate` would be considered a data model query template.
## Defining a custom exception handler using `SqlServerRawTemplateBuilder`
By default, if an exception occurs in either the `QueryLazy` or `QueryAsync` methods, it will be thrown. However, when an exception handler is defined, the exception will instead be passed to this handler without being thrown. To define an exception handler on the returned query, simply use the builder method `SetExceptionHandler`. If we wanted to add an exception handler to the above builder code, we would first define the method:
```csharp
// Define the exception handler
private void HandleBlogPostQueryException(Exception exception) {
	// handle the error
}
```
And then the builder code would look like this:
```csharp
var blogPostsWithGenreQueryTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString(connectionString)
		.SetQueryText("SELECT * FROM BlogPosts WHERE Genre = @Genre")
		// called every time exception occurs in query
		.SetExceptionHandler(HandleBlogPostQueryException)
		.GetQueryTemplate<BlogPost>();
```
Now whenever an exception occurs in either `QueryLazy` or `QueryAsync`, then the exception handler `HandleBlogPostQueryException` will be invoked.
## Defining a custom `CancellationToken` using `SqlServerRawTemplateBuilder`
In C#, you can cancel an asynchronous operation by utilizing a `CancellationToken`. Since results are queried asynchronously by the `IAtomicQueryTemplate<T>`, this approach can be used to cancel the query early if necessary. To utilize a custom cancellation token, you must call the builder method `SetCancellationToken` before obtaining the `IAtomicQueryTemplate<T>`. Here is an example where we create a cancellation token source:
```csharp
var cancellationTokenSource = new CancellationTokenSource();
var cancellableBlogPostQueryTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString(connectionString)
		.SetQueryText("SELECT * FROM BlogPosts")
		.SetCancellationToken(cancellationTokenSource.Token)
		.GetQueryTemplate<BlogPost>();
```
The reason we use the `Token` property of the `cancellationTokenSource`, as opposed to just a regular old `CancellationToken`, is so that we can dynamically control whether or not to cancel. Then, by calling `Cancel` on the `cancellationTokenSource`, a `TaskCanceledException` is thrown. Continuing on our previous example:
```csharp
try
{
	var lazyBlogPosts = cancellableBlogPostQueryTemplate.QueryLazy();
	// cancel before lazy loading
	cancellationToken.Cancel();
	await foreach (var blogPost in lazyBlogPosts)
	{
		// iteration is cancelled and TaskCanceledException is thrown
	}
}
catch (TaskCanceledException exception)
{
	// handle caught exception
}
```
This case is rather contrived, but you can imagine more complex scenarios, like a user cancelling a long running query, which behind the scenes would cancel the lazy loading using the above approach.
## Lazy loading results using `QueryLazy`
Lazy loading is most optimal when a large amount of data needs to be operated on, but the data is so large that storing it all in memory is not feasible, whether because of limited memory space or performance considerations. When using an `IAtomicQueryTemplate<T>`, you may lazy load results using `QueryLazy`, which returns an `IAsyncEnumerable<T>` that keeps track of the state of the iteration separate from the query template. In this way, the state of any lazy load is not stored in the `IAtomicQueryTemplate<T>` instance, but in a separate returned object. This is a cleaner approach, as it means you do not need to worry about the state of the query template before calling `QueryLazy`. Like implementations of `IAtomicRepository<TModel>`, implementations of  `IAtomicQueryTemplate<T>` contain immutable state that is set once on initialization and is read-only thereafter. Thus, the state of the query template remains constant throughout its lifetime.

You already saw an example of the `QueryLazy` method above, but here is a less verbose case:
```csharp
var blogPostsQueryTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString(connectionString)
		.SetQueryText("SELECT * FROM BlogPosts")
		.GetQueryTemplate<BlogPost>();

var lazyBlogPosts = blogPostsQueryTemplate.QueryLazy();
await foreach (var blogPost in lazyBlogPosts)
{
	// do something with each returned blog post
}
```
We need to use an `await foreach` loop since each blog post is retrieved asynchronously. Note that no loading takes place until the `await foreach` loop is reached, and then each result is read one at a time thereafter, until either there are no more results or until a cancellation occurs.
## Loading all results into memory using `QueryAsync`
When the number of results you are querying is not very large, it may be more efficient and sensible to just load all the results upfront. This is where the `QueryAsync` extension method comes in. While the read it performs is asynchronous, when the method completes all results are loaded as opposed to solely the very next one with `QueryLazy`. Here is the above example modified to load all blog posts at once:
```csharp
var blogPostsQueryTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString(connectionString)
		.SetQueryText("SELECT * FROM BlogPosts")
		.GetQueryTemplate<BlogPost>();

var blogPosts = await blogPostsQueryTemplate.QueryAsync();
foreach (var blogPost in blogPosts)
{
	// Do something with each blog post
}
```
Since `QueryAsync` returns an `IEnumerable<T>` when awaited, a regular `foreach` loop can be used to iterate over results. You can also utilize the various methods of the `IEnumerable<T>` like `Where`, `Select`, `SelectMany`, `ForEach`, and so on. Simply put, using `QueryAsync` makes sense when the queried data is not large and you need better control over the data.
## Passing parameters using anonymous objects
So far we haven't discussed the most powerful part of query templates: the ability to reuse a predefined query skeleton with different parameters. Suppose you have a class called `BlogPostSearchService`, used to search different blog posts on an internet blog. Let's say it is a creative story writing blog and you want users to be able to filter by a specific genre. A minimalistic implementation may look like this:
```csharp
using CalebBender.Atoms.Templates.Query;
using CalebBender.Atoms.Templates.Builder;

public class BlogPostSearchService
{
	private readonly IAtomicQueryTemplate<BlogPost> blogPostWithGenreQueryTemplate;

	public BlogPostSearchService(IRawTemplateBuilder blogPostWithGenreQueryTemplateBuilder, string connectionString)
	{
		blogPostWithGenreQueryTemplate =
			blogPostWithGenreQueryTemplateBuilder
				.SetConnectionString(connectionString)
				.SetQueryText("SELECT * FROM BlogPosts WHERE Genre = @Genre")
				.GetQueryTemplate<BlogPost>();
	}

	public async Task<IEnumerable<BlogPost>> QueryByGenreAsync(BlogPost.BlogPostGenre genre)
	{
		var blogPostsWithGenre = await blogPostWithGenreQueryTemplate.QueryAsync(new { Genre = genre });
		return blogPostsWithGenre;
	}
}
```
Now you can imagine different calls to the `QueryByGenreAsync` method, by just plugging in a different genre with no extra code nor work:
```csharp
var blogPostSearchService =
	new BlogPostSearchService(
		new SqlServerRawTemplateBuilder(),
		"some connection string"
	);

var horrorBlogPosts =
await blogPostSearchService.QueryByGenre(BlogPostGenre.Horror);

var scifiBlogPosts =
await blogPostSearchService.QueryByGenre(BlogPostGenre.Scifi);

var fantasyBlogPosts =
await blogPostSearchService.QueryByGenre(BlogPostGenre.Fantasy);

// etc...
```
## Conclusion
Query templates are meant to make your life as a programmer easier by allowing you to define a skeleton for a query once, and then supply different parameters (if the template needs parameters) to easily change the behavior of the template without any additional code. As of 2.0.x, you can only use a `SqlServerRawTemplateBuilder` instance to obtain a `IAtomicQueryTemplate<T>`, which the latter you can reuse as much to your heart's content.
