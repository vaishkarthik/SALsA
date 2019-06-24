#
# Setup the developer environment
# **This script is not run on build machines**
#
# Use this script to setup tools which make local development better
# For example, Import all the PowerShell scripts in the InPath folder
#

# Setup PowerShell shortcuts
function root() { Push-Location "$PSScriptRoot" }
function src() { Push-Location "$PSScriptRoot\src" }

Get-ChildItem -Path "$PSScriptRoot\.config\PsModules" -Filter "*.psm1" | ForEach-Object { Import-Module -Force $_.FullName }
Restore-Packages