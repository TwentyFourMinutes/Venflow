# Contributing to Venflow
First and foremost, I want to say, thank you! Maintaining this project is not an easy task and any help, no matter the scope, is appreciated. However, to keep things well organized there are a few steps you should go through before creating an issue or a pull-request.
No matter how you are planning on contributing to Venflow, always ensure that your behaviour and the ones of others align with the [Code of Conduct](https://github.com/TwentyFourMinutes/Venflow/blob/dev/CODE_OF_CONDUCT.md).
## Creating an issue
1. You should make sure, that the issue you are facing is or was not discussed in an existent issue or discussion.
2. Make sure, that you include _all_ the necessary information to reproduce this issue.
## Creating a pull-requests
1.  You should make sure that the contribution you want to make is not already discussed in an existent pull request, issue, or discussion.
2. If you would like to contribute to Venflow, first identify the scale of what you would like to contribute. If it is small (grammar/spelling or a bug fix) feel free to start working on a fix. However, if you are submitting a feature or substantial code contribution, please discuss it beforehand with other contributors if your pull request is appropriate.

3. You should read the *Writing Code* section and make sure your code contributions stick as tightly to it as possible.
## Writing Code
**Do**
- Write code comments and documentation in English.
- Primarily write _fast_ code, that is easy to read and understand, even though it may require you to write more. If it is not possible to make it easily readable and would sacrifice performance, be sure to add comments to your code explaining what is happening.
- Stick to the Coding Conventions mentioned below and the ones [provided by Microsoft](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).
- Stick to the Naming Conventions provided by Microsoft.
  - [Capitalization Conventions](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions)
  - [General Naming Conventions](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions)
  - [Names of Classes, Structs, and Interfaces](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces)
  - [Names of Type Members](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members)
  - [Write XML Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/) for all publicly accessible types and members. 
  - [Naming Parameters](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-parameters)
- Write Unit Tests if the new code will not be covered by existing ones.
- Cache Reflection results in the same method of class if they are going to be used frequently.
- Question existing Coding and Naming Conventions and break them, if reasonable.

**Do not**
- Push code which reformats the whole project.
- Push code which contains breaking changes, without them being approved.
- Write unconcise and vage commit messages.
## Coding and Naming Conventions
Our conventions differ from the ones provided by Microsoft in some ways, here you will find the ones which actually differ.

**I. Types**

**II. Fields**
- a. All field names should be prefixed with “_”.

**III. Properties**

**VI. Methods**

**V. Misc**
- a. Avoid the use of Collections which do not have an indexer, if one with an indexer could be used instead.
- b. Avoid the use of foreach loops in places where a for loop could be used instead.
