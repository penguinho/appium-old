#! /usr/bin/python

from sys import argv

f = open (argv[1],'w')
for arg in argv[2:]:
	f.write(arg)
f.close()

