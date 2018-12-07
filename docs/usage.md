# Usage

`BCCMSBuildLog --input <input binlog path> --output <output json path> --cloneRoot <clone root> --ownerRepo <owner/repository> --hash <commit hash> [--configuration <configuration file path>]`

## Arguments

| Argument | Required | Description |
| :--- | --- | :--- |
| **input** | :white_check_mark: | Path to MSBuild binary log file |
| **output** | :white_check_mark: | Path to output checkrun json file |
| **cloneRoot** | :white_check_mark: | Path where build occurred |
| **ownerRepo** | :warning: | Owner and Repository name in `owner/repo` format |
| **owner** | :warning: | Owner |
| **repo** | :warning: | Repository name |
| **hash** | :white_check_mark: | Hash of the current commit |
| **configuration** | | Path to configuration file |

**Note**: Owner and repo must be specified. Combined and seperate arguments are provided for ease of integration.

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

+ rules (array[LogAnalyzerRule]) - Array of rules
+ name (string) 

#### LogAnalyzerRule <sub><sup>[src](https://github.com/justaprogrammer/BCC-MSBuildLog/blob/master/src/BCC.MSBuildLog/Model/LogAnalyzerRule.cs)</sup></sub>

+ code (string, required) - The MSBuild warning/error code to match against
+ reportAs: asIs, ignore, notice, warning, error (enum, required)