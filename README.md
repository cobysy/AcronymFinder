
Acronym Finder
=========

Handy-dandy acronym finder for any text

Summary
----
`find_all_acronyms` finds basic and expanded acronyms

`find_acronyms` finds basic acronyms.  Basic acronyms are any word consisting of two or more capital letters.

`find_expanded_acronyms` finds expanded acronyms.  Expanded acronyms are strings of the form `"My Acronym Company (MAC)"`:  the expansion followed by the (basic) acronym in parentheses.

These functions all return a dict.  Keys are the acronym.  Each value is a list containing all expansions found.

Fork/port of https://github.com/mattschouten/acronym-finder
