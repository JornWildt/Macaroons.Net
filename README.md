# Macaroons are Better Than Cookies!

This library provides a C# implementation of macaroons[1], which are flexible
authorization tokens that work great in distributed systems. Like cookies,
macaroons are bearer tokens that enable applications to ascertain whether their
holders' actions are authorized. But macaroons are better than cookies!

This implementation is a port of the original C implementation from https://github.com/rescrv/libmacaroons.
It uses exactly the same serialization format, encryption and hashing algorithms and should as such be 
compatible with the C implementation.

See the documentation for a complete walk through of how to use macaroons.

## Articles, presentations and tools
* [Google Research publication](http://research.google.com/pubs/pub41892.html)
* [Techtalk by Úlfar Erlingsson (one of the authors)](https://air.mozilla.org/macaroons-cookies-with-contextual-caveats-for-decentralized-authorization-in-the-cloud/)
* [Macaroons playground](http://macaroons.io/)

## Other implementations of macaroons
* [libmacaroons (C)](https://github.com/rescrv/libmacaroons)
* [jmacaroons (Java)](https://github.com/nitram509/jmacaroons)
* [pymacaroons (Python)](https://github.com/ecordell/pymacaroons)
* [macaroon (Go)](https://github.com/rogpeppe/macaroon)

## Author
* Jørn Wildt
* E-mail: jw@fjeldgruppen.dk
* Twitter: @JornWildt

## Dependencies
Macaroons.net depends on the following libraries:

  * CuttingEdge.Condition
    * Source: http://conditions.codeplex.com/
    * License: MIT, http://conditions.codeplex.com/license
    
  * libsodium-net
    * Source: https://github.com/adamcaudill/libsodium-net
    * License: MIT, https://github.com/adamcaudill/libsodium-net/blob/master/LICENSE

## LICENSE
Macaroons.Net is distributed under the MIT License: http://www.opensource.org/licenses/MIT
A copy of this license is included in the file LICENSE.TXT

[1] http://research.google.com/pubs/pub41892.html
