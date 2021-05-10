FrebUI - A GUI for Failed Request Event Buffering

# Intro

FREB is frequently used when troubleshooting problems with IIS, the webserver of Microsoft.

Out of the box, there is a handy XML stylesheet (XSL) that you can use when analyzing a single FREB (*) file.

However, as soon as you have to analyse a few 100's of files it will for sure become time consuming and you will not be able to find the needle in the haystack.
You could search through the files as raw text but what you really need is a way to present the information in a grid like view.

# Overview

FrebUI is born for this reason and has already solved numerous difficult to diagnose production issues.

Among others, it gives gives you an option to sort the columns (sort by time-taken to see the longest running, etc), spot the slowest request quickly, lookup a specific error code and search through the list to find the request that you are interested in quickly.
The "Show the slowness" button is very helpful if you have a very large FREB file and trying to find where the maximum time is taken between stages.

(*) FREB is a feature of IIS and stands for Failed Request Event Buffering, later rebranded to Failed Request Event Trancing.

# Benefits
Provides an easy to use interface to sort the FREB files
