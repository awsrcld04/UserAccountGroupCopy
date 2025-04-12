
# UserAccountGroupCopy

DESCRIPTION: 
- Copies group membership between 2 user accounts

> NOTES: "v1.0" was completed in 2011. 

## Requirements:

Operating System Requirements:
- Windows Server 2003 or higher (32-bit)
- Windows Server 2008 or higher (32-bit)

Additional software requirements:
Microsoft .NET Framework v3.5

Additional requirements:
Administrative access is required to perform operations by UserAccountGroupCopy


## Operation and Configuration:

Command-line parameters:
- run (Required parameter)
- ref:[UserName] (specify reference user)
- dest:[UserName] (specify destination user)

Examples:
UserAccountGroupCopy -run -ref:User1 -dest:User2
