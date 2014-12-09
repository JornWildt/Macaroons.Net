Macaroons are Better Than Cookies!
==================================

This library provides a C# implementation of macaroons[1], which are flexible
authorization tokens that work great in distributed systems. Like cookies,
macaroons are bearer tokens that enable applications to ascertain whether their
holders' actions are authorized. But macaroons are better than cookies!

This implementation is a port of the original C implementation from https://github.com/rescrv/libmacaroons.
It uses exactly the same serialization format, encryption and hashing algorithms and should as such be 
compatible with the C implementation.

License: MIT, see LICENSE file.

## Author
Jørn Wildt
E-mail: jw@fjeldgruppen.dk
Twitter: @JornWildt

## Credits
Macaroons.net depends on the following libraries:

  CuttingEdge.Condition
    Source: http://conditions.codeplex.com/
    License: MIT, http://conditions.codeplex.com/license
    
  libsodium-net
    Source: https://github.com/adamcaudill/libsodium-net
    License: MIT, https://github.com/adamcaudill/libsodium-net/blob/master/LICENSE

## LICENSE
Macaroons.Net is distributed under the MIT License: http://www.opensource.org/licenses/MIT
A copy of this license is included in the file LICENSE.TXT

[1] http://research.google.com/pubs/pub41892.html
