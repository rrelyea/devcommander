# devcommander

Helps you work with repos with dev tools, consoles, etc...

- `git clone https://github.com/rrelyea/devcommander`
- `msbuild -r -p:Configuration=release -v:minimal`
- `DevCommander\bin\Release\netcoreapp3.1\DevCommander.exe`
   - click on settings button and add your root directories for repos similar to: {"RepoRoots":["c:\\repos", "c:\\users\rrelyea\\source\\repos"]}
     - at that point, a list of repos will be in the top half of the UI.
     - and a list of tools will be in the bottom. (it will show the nuget version included in the tool in VS, VSCode, SDKs)
     - the main idea...is to select a repo...and then double click a tool...and it will generally do the logical thing. 
     - press F5 to refresh the repos, or tools. 
   - settings also has
     - "ToolExes":["%ProgramFiles%\\Beyond Compare 4\\BCompare.exe"], (this adds a link to launch beyond compare with a diff of one repo to the other.)
     - "WebUrls":["URLGOESHERE"],
