# Usage
1. Add BuildCrossCheck service token as an environment variable named `BCC_TOKEN`
2. Install the nuget package `BCC-MSBuild`
2. Integrate in your build by adding the msbuild logger 
   `msbuild [Solution] -logger:packages\BCC-MSBuildLog.1.0.0\tools\net472\BCCMSBuildLog.dll`
   `msbuild [Solution] -logger:packages\BCC-MSBuildLog.1.0.0\tools\net472\BCCMSBuildLog.dll;configuration=bcc-config.json`
   
## Optional Arguments
| Argument          | Description                                                       |
| :---              | :---                                                              |
| **configuration** | Path to configuration file                                        |
| **token**         | BuildCrossCheck service token (if not using environment variable) |

## Supported CI Build Systems
On supported CI build systems, BuildCrossCheck will grab all required information about GitHub and your repo from the build environment.
- AppVeyor
- Cirlce
- Jenkins
- Travis

## Custom CI Arguments
On other CI build systems, the following arguments should be provided in the msbuild command
| Argument          | Description                                       |
| :---              | :---                                              |
| **cloneRoot**     | Path where build occurred                         |
| **owner**         | Owner                                             |
| **repo**          | Repository name                                   |
| **hash**          | Hash of the current commit                        |

## Configuration
The configuration file is a json document that allows customization of the Check Run output.

### Example

Here we are taking `CS0219` which is normally a warning and forcing it to be reported as an error. If used in combination with branch protection settings, this could be used to prevent a Pull Request from being merged.

```
{
    'rules': [
        {
            'code': 'CS0219',
            'reportAs': 'error'
        }
    ]
}
```

### Schema

#### CheckRunConfiguration <sub><sup>[src](https://github.com/justaprogrammer/BCC-MSBuildLog/blob/master/src/BCC.MSBuildLog/Model/CheckRunConfiguration.cs)</sup></sub>

- rules (array[LogAnalyzerRule]) - Array of rules
- name (string)

#### LogAnalyzerRule <sub><sup>[src](https://github.com/justaprogrammer/BCC-MSBuildLog/blob/master/src/BCC.MSBuildLog/Model/LogAnalyzerRule.cs)</sup></sub>

- code (string, required) - The MSBuild warning/error code to match against
- reportAs: asIs, ignore, notice, warning, error (enum, required)