# Usage

`BCCMSBuildLog --input <input binlog path> --output <output json path> --cloneRoot <clone root> --ownerRepo <owner/repository> --hash <commit hash> [--configuration <configuration file path>]`

## Arguments

| Argument | Required | Description |
| :--- | :--- | :--- |
| input | :white_check_mark: | Path to MSBuild binary log file |
| output | :white_check_mark: | Path to output checkrun json file |
| cloneRoot | :white_check_mark: | Path where build occurred |
| ownerRepo | :warning: | Owner and Repository name in `owner/repo` format |
| owner | :warning: | Owner |
| repo | :warning: | Repository name |
| hash | :white_check_mark: | Hash of the current commit |
| configuration | | Path to configuration file |