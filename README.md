# KeePassHax

KeePassHax is a managed DLL that, when injected into the KeePass process, will extract all data that makes up the 
CompositeKey used to decrypt the password database. This data (along with the database) could be transmitted to some 
server running in the cloud to then be decrypted and abused in all kinds of fun ways.

Inspired by [KeeFarce](https://github.com/denandz/KeeFarce), but better ;)


## Building
Compile it with Visual Studio 2017 or higher ¯\\\_(ツ)\_/¯


## Usage
Get yourself a nice managed DLL injector (like [this one](https://www.codeproject.com/articles/607352/injecting-net-assemblies-into-unmanaged-processes)) 
and inject the DLL into the KeePass process. You do not need administrator permissions for this, so it can be ran from 
the context of any application.

You can see it in action in [this video](https://youtu.be/J663mUBIzE0).


## Disclaimer
You probably could have guessed this, but I don't take responsibility for what you do with this. Please don't use this 
to actually steal passwords. This is merely a proof-of-concept to remind people to not run untrusted programs.


## License
This code is licensed under the MIT license. I will always appreciate a link back to this repository :)
