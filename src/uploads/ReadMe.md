# AVEVA.Connect.Devops - acdo

## Getting Started

Acdo can either be used as a global system dependency or a package dependency for another project.

### Setup Azure Artifacts Feed Access

To access the package you will need to add credentials into the global .npmrc file.  This can be done by installing vsts-npm-auth globally, then running the command 'vsts-npm-auth .npmrc'.  Alternatively, this command can be added as a script in your project package.json.

The feed registry must also be added to the global and/or project specific .npmrc files.  This can be found by clicking the "Connect to feed" button in the @aveva/connect-devops-cli package page (under artefacts).

More details can be found here: https://docs.microsoft.com/en-gb/azure/devops/artifacts/npm/npmrc

If your npm ci or npm i fails with 401 unauthorized you will need to run vsts-npm-auth again.

### Install

#### Global

```bash
npm install -g @aveva/connect-devops-cli
```

#### Dev Dependency

```bash
npm install --save-dev @aveva/connect-devops-cli
```

### Initialize Credentials

Before being able to run any commands against Connect API's you will need to initialize your credentials. See `acdo init -h`. You will need to be a connect administrator at least in the target environment.

Your credentials will expire in 6 months from the time of initialization.

## Commands

### solutiondefinition-deploy

Creates a solution definition and any capability definitions defined within it. Use YAML or JSON to define the configuration and provide this via --config

```yaml
name: AVEVA Connect Information Standards Management
desc: Governance of corporate, project and asset level Information Standards
icon: 'C:\views\ISM_Logo.png'
capabilities:
  - name: Information Standards Management
    desc: Provides the ISM application
    provider:
      url: 'http://provider.url.com/ISM'
      scope: 'cap_ism'
    icon: 'C:\views\ISM_Logo.png'
  - name: Information Standards Export
    desc: Provides a route for exporting class libraries from ISM for use in other packages
    provider:
      url: 'http://provider.url.com/Foo'
      scope: 'cap_foo'
    icon: 'C:\views\ISM_Logo.png'
```

Both solution definition and capability definitions have an optional ```contexts``` parameter which allows the solution provider to specify the context types the solution and capabilities are valid on. For example some capabilities may only make sense at an account level, for example Licensing, and this can be specified by specifying
```yaml
contexts:
  - account
```
as part of the solution definition and capability definition. Allowed values are ```asset``` and ```account```. If this is not specified it will default to
```yaml
contexts:
  - account
```

The solution definition may have an optional ```transferCapability``` parameter which allows the solution provider to specify the capability that provides data transfer support for solutions. This parameter should indicate the name of one of the defined capabilities, e.g.
```yaml
transferCapability: Information Standards Export
```

The solution definition may have optional ```findOutMore``` and ```demoUrl``` parameters which allows the solution provider to specify urls,  to more information about the product.
```yaml
findOutMore: https://url-to-more-information
demoUrl: https://url-to-demo-video
```

For clarity in the context of the full example this would be
```yaml
name: AVEVA Connect Information Standards Management
desc: Governance of corporate, project and asset level Information Standards
icon: 'C:\views\ISM_Logo.png'
contexts:
  - account
  - asset
transferCapability: Information Standards Export
findOutMore: https://url-to-more-information
demoUrl: https://url-to-demo-video
capabilities:
  - name: Information Standards Management
    desc: Provides the ISM application
    provider:
      url: 'http://provider.url.com/ISM'
      scope: 'cap_ism'
    icon: 'C:\views\ISM_Logo.png'
    contexts:
      - account
      - asset
  - name: Information Standards Export
    desc: Provides a route for exporting class libraries from ISM for use in other packages
    provider:
      url: 'http://provider.url.com/Foo'
      scope: 'cap_foo'
    icon: 'C:\views\ISM_Logo.png'
    contexts:
      - account
      - asset
```

Use the boolean flag ```disableConnectProvisioning``` if Connect should not call the provider's POST tenancy endpoint when enabling a solution.

### Working behind a proxy (e.g.: zScaler)
The cli relies on the npm package `global-agent` for proxy injection. You must set the environment variable `GLOBAL_AGENT_HTTP_PROXY` with your proxy name. The following is an example of how to set the zScaler proxy:
````
[PowerShell]
SET GLOBAL_AGENT_HTTP_PROXY=http://gateway.zscaler.net:9480
[Bash]
GLOBAL_AGENT_HTTP_PROXY=http://gateway.zscaler.net:9480
````

## Version history
*4.7.1* (22 Oct 2019)
Bug fix - Delete Capability Definition API URL path fixed.

*4.7.0* (18 Oct 2019)
Add or remove capability definition in a solution definition.

*4.6.1* (15 Oct 2019)
Altered parameter -f to -u for the `sd-delete`command.

*4.6.0* (08 Oct 2019)
Added new `sd-delete`command to delete solution definition.

*4.4.4* (03 Sep 2019)
Added boolean flag disableConnectProvisioning for solution definition.

*4.0.1* (09 Jul 2019)
Added optional properties catalogueCategory and demoUrl

*4.0.0* (26 Oct 2018)
Solution definition terms of service deployment now requires a region and version.

*3.1.2* (11 Oct 2018)
Add Resource Manager specific scope

*3.1.0* (5 Oct 2018)
Deploy resource types and service types to a randomly named instance of a resource manager

*3.0.0* (26 Sep 2018)
Added ServiceTypeDeploy commands back

*2.1.0* (13 Aug 2018)
Added displayName parameter for solution definition and capability definition

*2.0.0* (30 July 2018)
Moved over to use /am/accounts api from /platform/accounts
Remove ServiceCleanUp and ServiceTypeDeploy commands

*1.0.15* (20 July 2018)
Small fix so that Jwt token types are used as requested when using --useDb switch

## Contributing
Submit bugfix and feature pull requests to master.
Remember to update the acdo.js file when adding or removing commands.
Update the package version in package.json otherwise the npm publishing will fail.
