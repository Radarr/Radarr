
# How to Contribute

This is a personal project forked from Radarr. We're not actively seeking contributions at this time, but this guide documents the development process.

# Development

Logarr is written in C# (backend) and JS (frontend). The backend is built on .NET 8, while the frontend utilizes React.

## Tools required

- Visual Studio 2022 or higher is recommended (<https://www.visualstudio.com/vs/>). The community version is free and works (<https://www.visualstudio.com/downloads/>).

> VS 2022 V17.8 or higher is recommended as it includes the .NET 8 SDK
{.is-info}

- HTML/Javascript editor of choice (VS Code/Sublime Text/Webstorm/Atom/etc)
- [Git](https://git-scm.com/downloads)
- The [Node.js](https://nodejs.org/) runtime is required. The following versions are supported:
  - **20** (any minor or patch version within this)
{.grid-list}

> The Application will **NOT** run on older versions such as `18.x`, `16.x` or any version below 20.0! Due to a dependency issue, it will also not run on `21.x` and is untested on other verisons.
{.is-warning}

- [Yarn](https://yarnpkg.com/getting-started/install) is required to build the frontend
  - Yarn is included with **Node 20**+ by default. Enable it with `corepack enable`
  - For other Node versions, install it with `npm i -g corepack`

## Getting started

1. Clone the repository: `git clone https://github.com/Cody-k/logarr.git`
1. Install dependencies and build as described below

> Be sure to run lint `yarn lint --fix` on your code for any front end changes before committing.
For css changes `yarn stylelint-windows --fix` {.is-info}

### Building the frontend

- Navigate to the cloned directory
- Install the required Node Packages

     ```bash
     yarn install
     ```

- Start webpack to monitor your development environment for any changes that need post processing using:

     ```bash
     yarn start
     ```

### Building the Backend

The backend solution is most easily built and ran in Visual Studio or Rider, however if the only priority is working on the frontend UI it can be built easily from command line as well when the correct SDK is installed.

#### Visual Studio

> Ensure startup project is set to `Radarr.Console` and framework to `net8.0`
{.is-info}

1. First `Build` the solution in Visual Studio, this will ensure all projects are correctly built and dependencies restored
1. Next `Debug/Run` the project in Visual Studio to start Logarr
1. Open <http://localhost:7878>

#### Command line

1. Clean solution

```shell
dotnet clean src/Radarr.sln -c Debug
```

1. Restore and Build debug configuration for the correct platform (Posix or Windows)

```shell
dotnet msbuild -restore src/Radarr.sln -p:Configuration=Debug -p:Platform=Posix -t:PublishAllRids
```

1. Run the produced executable from `/_output`

## Contributing Code

- Make meaningful commits
- Add tests (unit/integration) for new features
- Commit with \*nix line endings for consistency
- Use 4 spaces instead of tabs
- Match existing code patterns and style

## Pull Requesting

- Only make pull requests to `develop`, never `master`
- Use meaningful feature branch names (what is being added/fixed)
- Each PR should contain related changes (one feature/bug fix per PR)

## Unit Testing

Logarr utilizes nunit for its unit, integration, and automation test suite.

### Running Tests

Tests can be run easily from within VS using the included nunit3testadapter nuget package or from the command line using the included bash script `test.sh`.

From VS simply navigate to Test Explorer and run or debug the tests you'd like to examine.

Tests can be run all at once or one at a time in VS.

From command line the `test.sh` script accepts 3 parameters

```bash
test.sh <PLATFORM> <TYPE> <COVERAGE>
```

### Writing Tests

While not always fun, we encourage writing unit tests for any backend code changes. This will ensure the change is functioning as you intended and that future changes dont break the expected behavior.

> We currently require 80% coverage on new code when submitting a PR
{.is-info}

If you have any questions about any of this, please let us know.

# Translation

Translation files are stored in the repo at `src/NzbDrone.Core/Localization`. The English translation, `en.json`, serves as the source for all other translations.

## Adding Translation Strings in Code

When adding a new string to either the UI or backend, a key must also be added to `src/NzbDrone.Core/Localization/en.json` along with the default value in English. This key may then be consumed as follows:

> PRs for translation of log messages will not be accepted
{.is-warning}

### Backend Strings

Backend strings may be added utilizing the Localization Service `GetLocalizedString` method

```dotnet
private readonly ILocalizationService _localizationService;

public IndexerCheck(ILocalizationService localizationService)
{
  _localizationService = localizationService;
}
        
var translated = _localizationService.GetLocalizedString("IndexerHealthCheckNoIndexers")
```

### Frontend Strings

New strings can be added to the frontend by importing the translate function and using a key specified from `en.json`

```js
import translate from 'Utilities/String/translate';

<div>
  {translate('UnableToAddANewIndexerPleaseTryAgain')}
</div>
```
