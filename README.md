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

Author
Jørn Wildt
E-mail: jw@fjeldgruppen.dk
Twitter: JornWildt


[1] http://research.google.com/pubs/pub41892.html
