Applecart
=========

Applecart is a test automation tool for use with native and hybrid iOS applications. It uses the webdriver JSON  wire protocol to drive Apple's UIAutomation. Applecart is based on [Dan Cuellar's](http://github.com/penguinho) work on iOS Auto.

Applecart uses the [Bottle micro web-framework](http://www.bottlepy.org), and has the goal of working with all off the shelf Selenium client libraries.

There are two big benefits to testing with AppleCart:

1: Applecart uses Apple's UIAutomation library under the hood to perform the automation, which means you do not have to recompile your app or modify in any way to be able to test automate it.

2: In the near-future, you'll be able to write your test in *any* language, using the Selenium WebDriver API and language-specific client libraries. In this example, we're using Python. Otherwise, using UIAutomation API would require writing tests in JavaScript, and only running the tests through the Instruments application. With Applecart, you can test your native iOS app with any language, and with your preferred dev tools.

Quick Start
-----------

To get started, clone the repo: `git clone git://github.com/saucelabs/applecart`.

Next, change into the 'applecart' directory, and install dependencies: `pip install -r requirements.txt`.


Contributing
------------

Mailing List
-----------
