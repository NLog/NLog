NLog

What is it? 
-----------
NLog is a .NET logging library designed with simplicity and flexibility in mind. 
With NLog you can process diagnostic messages emitted from any .NET language, augment
them with contextual information, format them according to your preference and send 
them to one or more targets. 

The API (application programming interface) is similar to log4net, and the configuration 
is very simple. NLog uses a routing table while log4net uses a logger hierarchy with 
attachable appenders. This makes NLog's configuration very easy to read and maintain. 

NLog is licensed under the terms of BSD license, which permits commercial use and the 
source code is available to anyone at no cost. Everyone is encouraged to test it and 
report feedback to the mailing list. 

NLog supports .NET, C/C++ and COM interop API so that all your application components 
including legacy modules written in C++/COM can send their messages through a common 
log routing engine. 

The .NET API is very fast at filtering messages, so that you can keep your logging 
instructions in code and let NLog filter them out at runtime. NLog can filter out as 
many as 150 million logging instructions per second on a single-CPU 1.6 GHz laptop. 
Add that to asynchronous processing and other wrappers and you'll get a very 
powerful and scalable logging tool. 

The Latest Version
------------------
Details of the latest version can be found on the NLog project web site
http://www.nlog-project.org/

Documentation
-------------
Documentation is available in HTML format on the 
project website (http://www.nlog-project.org/)

License
-------
NLog is licensed under the terms of BSD license which permits its usage
in open-source and proprietary software. See LICENSE.txt for details.
