# Contributing to InstantAPIs

Thank you for taking the time to consider contributing to our project.

The following is a set of guidelines for contributing to the project.  These are mostly guidelines, not rules, and can be changed in the future.  Please submit your suggestions with a pull-request to this document.

1. [Code of Conduct](#code-of-conduct)
1. [What should I know before I get started?](#what-should-i-know-before-i-get-started?)
    1. [Project Folder Structure](#project-folder-structure)
    1. [Design Decisions](#design-decisions)
1. [How can I contribute?](#how-can-i-contribute?)
    1. [Work on an Issue in the project](#work-on-an-issue-in-the-project)
    1. [Report a Bug](#report-a-bug)
    1. [Write documentation](#write-documentation)


## Code of Conduct

We have adopted a [code of conduct](https://github.com/csharpfritz/InstantAPIs/blob/main/CODE-OF-CONDUCT.md) from the Contributor Covenant.  Contributors to this project are expected to adhere to this code.  Please report unwanted behavior to [jeff@jeffreyfritz.com](mailto:jeff@jeffreyfritz.com)

## What should I know before I get started?

This project is currently a proof-of-concept library that generates Minimal API endpoints for an Entity Framework context.  You should be
familiar with C# 10, .NET 6, ASP.NET Core, and Entity Framework Core.  Reflection and Source Generators are a plus as this project will use those 
.NET features to generate HTTP APIs.

### Project Folder Structure

The folders are a basic structure which will change as needed to support the project as it grows.  The folders are configured as follows:

```

  Fritz.InstantAPIs               The project code.
  WorkingApi                      The project to prototype and manually test the functionality being developed.

```

### Design Decisions

Design for this project is ultimately decided by the project lead, [Jeff Fritz](https://github.com/csharpfritz).  The following project tenets are adhered to when making decisions:

1. This is a library to help make the simple API endpoints that every project needs.
1. This library is not intended to generate more complex API endpoints.
1. This toolset should help users to deliver APIs with .NET on any and all ASP.NET Core supported platforms

## How can I contribute?

We are always looking for help on this project. There are several ways that you can help:

#### Tool suggestions for contributing

1. [Visual Studio](https://visualstudio.microsoft.com/) (Windows)
2. [Visual Studio Code](https://visualstudio.microsoft.com/) (Windows, Linux, Mac)
3. [Visual Studio For Mac](https://visualstudio.microsoft.com/)
4. Any text editor (Windows, Linux, Mac)
5. Any Web browser.

#### Work on an Issue in the project

1. [Work on an Issue](https://github.com/csharpfritz/InstantAPIs/issues) Choose an Issue that you are interested in working on follow the instruction provided in the link below. We thank you in advance for your contributions.

#### Report a Bug

1. [Report a Bug](https://github.com/csharpfritz/InstantAPIs/issues) with the details of a bug that you have found.  Be sure to tag it as a `Bug` so that we can triage and track it.

#### Write documentation

We are always looking for help to add content to the project.

#### Recources

[cmjchrisjones Blog: Contributing To Someone else's git repository](https://cmjchrisjones.dev/posts/contributing-to-someone-elses-git-repository/)
