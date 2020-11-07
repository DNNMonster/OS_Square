# Project Title
OS_Square

## Getting Started
This is a payment provider plugin for Open Store. It will enable any 
DNN 9.4+ site running Open Store to accept CC payments into their  
<span>&copy;</span>Square account.  You must have a valid <span>&copy;</span>Square account and 
a [developers](https://developer.squareup.com/) api key for this provider to work.

### Installing
1. Install into DNN as a normal module.  Ensure that your DNN Open Store installation is using 
   at least v8.5.2 of the NBrightTemplateSys.
2. Go into Open-Store BO>Admin, the "OS_Square" option should be listed.
![OpenStore Back Office Admin Panel](assets/images/plugin_installed.png)


3. See <span>&copy;</span><span>&copy;</span>Square's developer portal for your Application ID & API Access Token.
4. Configure your Open Store Back Office plugin settings for the <span>&copy;</span>Square plugin with the credentials from step 3. 
![OpenStore Back Office Admin Panel](assets/images/settings.png)
5. The provider by default uses the first location returned from your account but if you have more 
	than one location you can optionally specify it by Name in the Location input.

The gateway should now be ready and your customers can purchase securely with the <span>&copy;</span>Square 
payment form.
![OpenStore Back Office Admin Panel](assets/images/cc_form.png)

### Dependencies

 * Square.Connect 2.25
 * System.ComponentModel.Annotations 4.2.1.0
 * System.ComponentModel.DataAnnotations 4.0.0.0
 * RestSharp 106.3.1.0

 * DotNetNuke.DependencyInjection 9.7.1.0
 * Microsoft.Extensions.DependencyInjection 2.1.1.0
 * Microsoft.Extensions.DependencyInjection.Abstractions 2.1.1.0


NOTE The installation includes a version of Square.Connect dll and 3 supporting dlls 
which the installer places in the bin directory.  The installation also installs the 
DotNetNuke.DependencyInjection & 2 supporting libraries.  There is no sql provider with 
this module install but it is still best to *back up both your db & file system as a precaution 
before installation.* The project also references DotNetNuke.DependencyInjection library 
in preparation for .net core support.

## History
This module is the evolution of an earlier version that worked with 
the NBStore system before it's name change to OpenStore. It was also depending 
on an earlier version of the Square.Connect library.  The v2 version of this 
plugin began when when the breaking changes from Square.Connect 2.25 were mitigated. 
First released publicly at v2.0.4-rc
 

 ### Development

 1. Install the module into your development enviroment.
 2. Clone the repo to your /DesktopModules/i502Club/ directory.
 2. Your development environment IIS server must bind your DNN site to localhost 
	otherwise the payment form & Square.Connect assembly will not work using the sandbox.  
 3. See <span>&copy;</span>Square's developer portal for your Application ID, API Access Token and test cc card information.
 4. Configure your settings for the <span>&copy;</span>Square plugin.  You will need an Application ID and API Access Token.
	The provider by default uses the first location returned from your account but if you have more 
	than one location you can optionally specify it by Name in the Location input.
 5. You should be able to compile and atstach the debugger at this point.

 ## Authors
 i502 Club(https://www.i502.club), [Reggae](https://www.youtube.com/watch?v=lEmLqH2gTd8)

 ## License
This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details

## Acknowledgments
* All the contributors to [DNN](https://github.com/dnnsoftware/Dnn.Platform) & [OpenStore]( https://github.com/openstore-ecommerce/OpenStore) 

 ## Contribute
 * Contributions are awesome.  You can create an issue or submit a pull request
 to help make the plugin work better.
