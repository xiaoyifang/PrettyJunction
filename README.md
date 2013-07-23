PrettyJunction
==============

aim to replace junction in some way



how to use
================

```
Try `prettyjunction --help' for more information.
Options:
  -f, --config=CONFIG        the CONFIG
  -c, --clean=DIRECTORY      clean the DIRECTORY
  -h, --help                 show this message and exit
```


```prettyjunction --config=example.txt```

this tool is mainly used the config file to combine all the junction rules together.
and improve some function.

see the example.txt for further info.


##advantages:
compared to junction.exe ,this tool can be more stable,
1)if the junction point is already exist(and it was junction point),it will be replaced by the target
2)if error occured,red error message will be display.
