How to build your fork in the cloud
---
Steps to set up [AppVeyor](https://ci.appveyor.com)/[Travis](https://travis-ci.org/)/[CodeCov](https://codecov.io/) for your own fork.

**AppVeyor**:

1. Login with your Github account to https://ci.appveyor.com
2. Choose "projects"
3. Select your fork and press "+" button
4. Done. All config is in appveyor.yml already

**Travis**:

1. Login with your Github account to https://travis-ci.org/
2. Select your fork
3. Push and wait

**CodeCov**: (AppVeyor needed)

1. Login with your Github account to https://codecov.io/
2. Press "+  Add new repository to Codecov" button
3. Select your fork
4. Wait for a build on AppVeyor. All the config is already in appveyor.yml. The first report can take some minutes after the first build.
