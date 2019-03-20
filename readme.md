# Slipe-CLI

Slipe-CLI is the command line interface for the [Slipe-MTA Framework](https://github.com/mta-slipe/Slipe-Core) 

## Getting Started

### prerequisites
```
.NET Core 3.0
```

## Installation
* download the .zip file with compiled binaries from the [latest release](https://github.com/mta-slipe/Slipe-CLI/releases). 
* unzip the binaries and place them somewhere on your system
* add the directory the binary files are in to your system PATH variable.

you can now use the `slipe` command anywhere on your system

## Usage
Slipe contains two types of commands, global commands and project dependent commands. Project dependent commands can only be executed in an existing Slipe project directory. Global commands can be executed anywhere on your system.

### Global commands
```sh
# Create a new Slipe project
slipe new {project-name}
```
### Project dependent commands
```sh
# Compile the slipe project to lua (this also generates the meta)
slipe compile 

# Indicate a visual studio project should be compiled to lua when running `slipe compile`
slipe add-project {project-name} [-server] [-client]

# Remove a visual studio project from compiling to lua
slipe remove-project {project-name} [-server] [-client]

# Adds a dll to be included durinAg C# compilation
slipe add-dll {dll-name}

# Removes a dll to no longer be included during C# compilation
slipe remove-dll {dll-name}

# Generates the meta.xml file for the project
slipe meta-generate
```

## Authors

* **Bob van Hooff** - [Nanobob](https://github.com/NanoBob)
* **Mathijs Sonnemans** - [DezZolation](https://github.com/DezZolation)

See also the list of [contributors](https://github.com/mta-slipe/Slipe-CLI/graphs/contributors) who participated in this project.

## License
[Apache 2.0 license](https://github.com/mta-slipe/Slipe-CLI/blob/master/LICENSE.MD).

## Communication

[Slipe Discord](https://discord.gg/sZ3GNPF)
