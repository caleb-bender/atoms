
# Atoms Mutation Template Documentation (2.0.x)
If you haven't already read the [Atoms Data Model Classes](https://github.com/caleb-bender/atoms/blob/main/Documentation/2.0.x/AtomsDataModelClasses.md) documentation, please do so since it will make understanding the `IAtomicMutationTemplate` interface and any related documentation much easier to grasp, at least as it pertains to enum parameters.
## Introduction
Atoms Mutation Templates are simpler than [Atoms Query Templates](https://github.com/caleb-bender/atoms/blob/main/Documentation/2.0.x/AtomsQueryTemplates.md), mainly because they are meant to only execute writes and they always return the same thing: a 32-bit integer representing the number of entries modified by the mutation. This is the definition of the `IAtomicMutationTemplate` interface:
```csharp
namespace CalebBender.Atoms.Templates.Mutation
{
	public interface IAtomicMutationTemplate
	{
		Task<int> MutateAsync(object? parameters = null);
	}
}
```
Like the `IAtomicQueryTemplate<T>` interface's `QueryLazy` and `QueryAsync` methods, an object containing parameters as C# properties can be passed into `MutateAsync` if the mutation requires them. Otherwise, they can be let empty.

Essentially, an implementation of `IAtomicMutationTemplate` contains a reusable template for writing to the database as many times as necessary. The only state it contains is immutable and set once on construction, any objects like a connection are open and closed within the scope of the `MutateAsync` method.
## Creating a mutation template using `SqlServerRawTemplateBuilder`
In addition to being able to create an `IAtomicQueryTemplate<T>` instance with `SqlServerRawTemplateBuilder`, the same can be done for an `IAtomicMutationTemplate`. Here is an example that constructs a mutation template that changes all blog posts with genre `Horror` to a new genre:
```csharp
var blogPostGenreMutationTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString("the connection string")
		.SetMutationText("UPDATE BlogPosts SET Genre = @NewGenre WHERE Genre = 'Horror'")
		.GetMutationTemplate();
```
And its as simple as that! Recall our definition of the `BlogPost` data model class:
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
Then, if we wanted to change all `Horror` blog posts to be the `Thriller` genre, the call to the mutation template we created above would look like this:
```csharp
var numberOfBlogPostsChanged =
	await blogPostGenreMutationTemplate.MutateAsync(new { NewGenre = BlogPost.BlogPostGenre.Thriller });
```
If we had another template that deleted blog posts based on the title's first letter, then the construction using a `SqlServerRawTemplateBuilder` would look something like this:
```csharp
var blogPostDeletionMutationTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString("the connection string")
		.SetMutationText("DELETE FROM BlogPosts WHERE Title LIKE @Title")
		.GetMutationTemplate();
```
And if we wanted to delete all blog posts with a title that starts with the letter 'A', then the call would look like this:
```csharp
var numberOfBlogPostsDeleted =
	await blogPostDeletionMutationTemplate.MutateAsync(new { Title = "A%" });
```
And that's it! Constructing and reusing an `IAtomicMutationTemplate` instance with the help of the `SqlServerRawTemplateBuilder` is fairly straight forward.
## `IEnumerable` parameters
Suppose we want to delete a set of blog posts using a list of titles. How would we do this using an `IAtomicMutationTemplate` obtained from a `SqlServerRawTemplateBuilder`? It's actually quite simple. Here is an example:
```csharp
var blogPostsDeleteByTitleTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString("the connection string")
		.SetMutationText("DELETE FROM BlogPosts WHERE Title IN @Titles") 
		.GetMutationTemplate();

// Delete the blog posts with specific titles
var Titles = new List<string> { "A Haunted Cove", "Journey to the Ethereal Garden", "The Man in the Shadows" };
var numberOfDeletedBlogPosts =
	await blogPostsDeleteByTitleTemplate.MutateAsync(new { Titles });
```
If you have any experience using SQL parameters, you will recall that SQL Server doesn't support parameters in this way, since a normal SQL parameter cannot be a collection or set of things. However, Atoms automatically expands `IEnumerable` parameters so that the text in our above example gets converted from
```
"DELETE FROM BlogPosts WHERE Title IN @Titles"
```
to
```
"DELETE FROM BlogPosts WHERE Title IN (@Titles0,@Titles1,@Titles2)"
```
Also, each actual parameter is resolved dynamically with the corresponding value in the `IEnumerable`. This allows one to easily delete or update by using an `IEnumerable` of a variable number of elements for the filtering criteria. One thing to note is that each `IEnumerable` parameter used must be one dimensional and contain elements of a type that can map to a database type. Other than that, this approach is pretty flexible and saves the programmer from a bunch of nasty SQL string concatenation logic....whew!
## Defining a custom exception handler using `SqlServerRawTemplateBuilder`
By default, if an exception occurs in the `MutateAsync` method, it will be thrown. However, when an exception handler is defined, the exception will instead be passed to this handler without being thrown. To define an exception handler on the returned mutation template, simply use the builder method `SetExceptionHandler`. First, we define the exception handler to call:
```csharp
// Define the exception handler
private void HandleException(Exception exception) {
	// handle the error
}
```
And then the builder code would look like this:
```csharp
var blogPostGenreMutationTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString("the connection string")
		.SetMutationText("UPDATE BlogPosts SET Genre = @NewGenre WHERE Genre = 'Horror'")
		// Called if an exception occurs when executing the mutation
		.SetExceptionHandler(HandleException)
		.GetMutationTemplate();
```
Now whenever an exception occurs in `MutateAsync`, then the exception handler `HandleException` will be invoked.

## Defining a custom `CancellationToken` using `SqlServerRawTemplateBuilder`
In C#, you can cancel an asynchronous operation by utilizing a `CancellationToken`. If a cancellation token is defined for the `IAtomicMutationTemplate` instance, then a `MutateAsync` call can be cancelled dynamically if it no longer is necessary. To utilize a custom cancellation token, you must call the builder method `SetCancellationToken` before obtaining the `IAtomicMutationTemplate`. Here is an example where we create a cancellation token source:
```csharp
var cancellationTokenSource = new CancellationTokenSource();
var cancellableBlogPostMutationTemplate =
	new SqlServerRawTemplateBuilder()
		.SetConnectionString("the connection string")
		.SetMutationText("UPDATE BlogPosts SET Genre = @NewGenre WHERE Genre = 'Horror'")
		.SetCancellationToken(cancellationTokenSource.Token)
		.GetMutationTemplate();
```
The reason we use the `Token` property of the `cancellationTokenSource`, as opposed to just a regular old `CancellationToken`, is so that we can dynamically control whether or not to cancel. Then, by calling `Cancel` on the `cancellationTokenSource`, a `TaskCanceledException` is thrown. Continuing on our previous example:
```csharp
try
{
	// cancel before mutating
	cancellationToken.Cancel();
	await cancellableBlogPostMutationTemplate.MutateAsync(new { NewGenre = BlogPost.BlogPostGenre.Thriller });
}
catch (TaskCanceledException exception)
{
	// handle caught exception
}
```

## Conclusion
Like query templates, mutation templates are meant to make your life as a programmer easier by allowing you to define a skeleton for a mutation/modification once, and then supply different parameters (if the template needs parameters) to easily change the behavior of the template without any additional code. As of 2.0.x, you can only use a `SqlServerRawTemplateBuilder` instance to obtain a `IAtomicMutationTemplate`, which the latter you can reuse as much to your heart's content.