# Macaroons are Better Than Cookies!

This library provides a C# implementation of [macaroons](http://research.google.com/pubs/pub41892.html), 
which are flexible authorization tokens that work great in distributed systems. Like cookies,
macaroons are bearer tokens that enable applications to ascertain whether their
holders' actions are authorized. But macaroons are better than cookies!

This implementation is a port of the original C implementation from https://github.com/rescrv/libmacaroons.
It uses exactly the same serialization format, encryption and hashing algorithms and should as such be 
compatible with the C implementation.

## Why Macaroons?

(*This text is from the original C implementation by Robert Escriva*)

Macaroons are great for authorization because they're similar enough to cookies
to be immediately usable by developers, but they include several features not
present in cookies or other token-based authorization schemes.  In particular:

* Delegation with Contextual Caveats (i.e., confinement of the usage context):  
  Macaroons support delegation.  Give your macaroon to another user, and they 
  can act on your behalf, with the same authority.  Cookies permit delegation 
  as well, but the remaining features of macaroons make it much more safe and
  practical to pass around macaroons than cookies.  In particular, macaroons
  can limit when, where, and by whom the delegated authority can be exercised
  (e.g., within one minute, from a machine that holds a certain key, or by a 
  certain logged-in user), by using attenuation and third-party caveats.

* Attenuation:  Macaroons enable users to add caveats to the macaroon that
  attenuate how, when, and where it may be used.  Unlike cookies, macaroons
  permit their holder to attenuate them before delegating.  Whereas cookies and
  authorization tokens enable an application to get access to all of your data
  and to perform actions on your behalf with your full privileges, macaroons
  enable you to restrict what they can do. Those questionable startups that
  "just want the address book, we swear it," become a whole lot more secure
  when the target application supports macaroons, because macaroons enable you
  to add caveats that restrict what the application can do.

* Proof-Carrying:  Macaroons are efficient, because they carry their own proof
  of authorization---cryptographically secured, of course.  A macaroon's
  caveats are constructed using chained HMAC functions, which makes it really
  easy to add a caveat, but impossible to remove a caveat.  When you attenuate
  a macaroon and give it to another application, there is no way to strip the
  caveats from the macaroon.  It's easy for the entity that created a macaroon
  to verify the embedded proof, but others cannot.

* Third-Party Caveats:  Macaroons allow caveats to specify predicates that are
  enforced by third parties.  A macaroon with a third-party caveat will only be
  authorized when the third party certifies that the caveat is satisfied.  This
  enables loosely coupled distributed systems to work together to authorize
  requests.  For example, a data store can provide macaroons that are
  authorized if and only if the application's authentication service says that
  the user is authenticated.  The user obtains a proof that it is
  authenticated from the authentication service, and presents this proof
  alongside the original macaroon to the storage service.  The storage service
  can verify that the user is indeed authenticated, without knowing anything
  about the authentication service's implementation---in a standard
  implementation, the storage service can authorize the request without even
  communicating with the authentication service.

* Simple Verification:  Macaroons eliminate complexity in the authorization
  code of your application.  Instead of hand-coding complex conditionals in
  each routine that deals with authorization, and hoping that this logic is
  globally consistent, you construct a general verifier for macaroons.  This
  verifier knows how to soundly check the proofs embedded within macaroons to
  see if they do indeed authorize access.

* Decoupled Authorization Logic:  Macaroons separate the policy of your
  application (who can access what, when), from the mechanism (the code that
  actually upholds this policy).  Because of the way the verifier is
  constructed, it is agnostic to the actual underlying policies it is
  enforcing.  It simply observes the policy (in the form of an embedded proof)
  and certifies that the proof is correct.  The policy itself is specified when
  macaroons are created, attenuated, and shared.  You can easily audit this
  code within your application, and ensure that it is upheld everywhere.


## Documentation

See the file [WALKTHROUGH.TXT](https://github.com/JornWildt/Macaroons.Net/blob/master/WALKTHROUGH.TXT) for
a complete guide to using Macaroons.Net.

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
* E-mail: jw@elfisk.dk
* Twitter: @JornWildt

## Credits
Thanks to Robert Escriva for making the C implementation public.

Thanks to [cBrain](http://cbrain.com/) for donating a few working hours for this implementation.

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

