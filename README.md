# jhernandez-hpe
Apps GD Costa Rica – Challenge

# About the developer.
Jose Hernandez Delgado

# Dependecies:

# Runtime ( frontend )
HTML5
CSS3
Bootstrap
Razor MVC

# Runtime (backend)
Azure SQL Server
JSON and Ajax
ASP.NET MVC
Entity Framework Code First
C#
Nuget
Azure SQL Server

# Deploying-Building
Azure

# About the project

Let’s take a quick look at some of the client side technologies and the server side technologies that we use to build this application.
So starting on the client side, we built things up using an html5 base with the help of ASP.NET Razor for the creation of our modern user interface. Now, we didn’t really start from scratch on html5 though, what we used is a bootstrap theme downloaded from the Internet and modified it and the CSS3 that it provided so that it could suit to our needs, this helps the application to look great on any device used (tablets, cellphones and computers).
Then, on the server side, we are using Ajax calls from the client to the server and communicating for example sending JSON data, ASP.NET MVC is actually what we used to serve up that data, this because of the great advantages that it provides that makes very simple to expose JSON data to the clients using basically a JSON serializer to convert our C# objects into JSON data and vice-versa.
The database interaction is all done through Entity Framework Code First, it provides a great way to write our CRUD operations with a bit of help of LINQ Language Integrated Query to actually query the database and write that type of code in almost all scenarios. The actual data access classes are following something called a repository pattern, that basically has classes that are almost identical to your database objects and they contain the crud operations in them. All those classes, the MVC code and everything on the server side are written using C#.
We used a lot of Microsoft Azure. We used its Azure SQL Server to store our data, and the tools it provides to deploy our site to the Azure servers directly from Visual Studio 2015.
Finally, we used Nuget to get Entity Framework Code First in our project to handle the integration between classes and database objects.
