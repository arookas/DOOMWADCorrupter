# DOOM WAD Corrupter

## Summary

Welcome to _DOOM WAD Corrupter_. Well, the gist of the program is to take a WAD — any WAD — you know and love and butcher it into an unintelligible mess of a file that you can then run in G/ZDOOM for your viewing pleasure. Well, only if the spawn wasn't screwed up or G/ZDOOM doesn't choke on PNAMES.

In this way, it is analogous to (and, in fact, inspired by) the Vinesauce ROM Corrupter. But, this is cooler, right?

### Corruption Basics

A corrupter will corrupt the data of all the applicable lumps in the WAD file by changing bytes across the lump. You may select at which byte in a lump’s data to begin the corruption, as well as at which byte the corruption shall end.

The corrupter will manipulate the byte at each multiple of a specified interval, or _increment_, between these starting and ending points. You may also determine how the corrupter manipulates the bytes.

## Usage

_DOOM WAD Corrupter_ is a command-line program, and all options are specified as command-line arguments. The only required arguments are the first two, which are the paths to the WAD file to corrupt and the file into which to save the corrupted WAD, respectively.

For example, to corrupt _DOOM.wad_ into _DOOM corrupted.wad_ in the directory of the program, the command line would be ___doomwadcorrupter.exe "DOOM.wad" "DOOM corrupted.wad"___.

All arguments after the input and output files will be interpreted as options that alter the default logic of the corrupter (trust me, the default values suck, anyway). Options are optional, and are not required to run the program. An option is denoted with a starting hyphen "-" and a case-insensitive name. An option may have arguments specified directly after the option name, separated by whitespace.

### Options

The following is a full list of options supported by _DOOM WAD Corrupter_:

|Options|Arguments|Description|
|--------|---------|-----------|
|___-start___|_\<offset>_|Specifies the byte at which the corruption begins. This byte is included in the corruption. The default value is a ranomd number between zero and fifty
|___-end___|_\<offset>_|Specifies the byte at which corruption ends. Omit this option to specify ending at the end of the lump's data (default).|
|___-inc___|_\<amount>_|Specifies the inveral at which to corrupt bytes within the starting and ending points of the lump's data. The default value is a random number between one and thirty-two.|
|___-mode___|_\<mode> [\<value>]_|Specifies exactly how to manipulate each byte qeued for corruption. The default value is a random mode with a random value or seed.
|___-skip___ <br> ___-only___|_\<filter> [...]_ <br> _\<filter> [...]_|Filters the lumps queued for corruption by skipping over or including only the ones matching the filter, respectively. See __Lump Filtering__ for more details.|
|___-zdoom___||Specifies that any G/ZDOOM lumps that are found in the WAD should not be corrupted, regardless of any filters. See __Lump Filtering__ for more details.|

Please note that all numeric arguments (including the ones noted below) are specified as a positive integer in decimal. To specify a number in hexadecimal, suffix the number with the hexadecimal specifier "h" (case insensitive).

## Corruption Modes

The ___-mode___ option specifies how the corrupter is to manipulate each of the bytes queued for corruption. There are lots of modes, each with their own unique properties and arguments. The name of the mode is not case sensitive. All the arguments may be specified in hexadecimal or decimal as noted above.

The modes are as follows:

|Mode|Description|
|----|-----------|
|___add___|Adds _\<value>_ to the byte.|
|___sub___|Subtracts _\<value>_ from the byte.|
|___mul___|Multiplies the byte by _\<value>_.|
|___div___|Divides the byte by _\<value>_.|
|___mod___|Replaces the byte with the modulo of the byte and _\<value>_.|
|___or___|Bitwise-ORs the byte with _\<value>_.|
|___xor___|Exclusive-ORs the byte with _\<value>_.|
|___and___|Bitwise-ANDs the byte with _\<value>_.|
|___not___|Bitwise-NOTs the byte. This mode does not take an argument.|
|___lsh___|Shifts the bits in the byte left by _\<value>_ bits.|
|___rhs___|Shifts the bits in the byte right by _\<value>_ bits.|
|___rol___|Rotates the bits in the byte left by _\<value>_ bits.|
|___ror___|Rotates the bits in the byte right by _\<value>_ bits.|
|___replace___|Replaces the byte with _\<value>_.|
|___random___|Replaces the byte with a random value using a PRNG algorithm initialized with the seed _\<value>_.|

### Auto complete

For the options ___-start___, ___-end___, ___-inc___, and ___-mode___, you may use auto complete for the values. This is indicated by an argument value of "??" (two question marks). Depending on the option, this is followed by a range of values (min and max, inclusive).

This auto-complete mode will fill in random values for each of the specified options according to the following table:

|Option|Arguments|Description|
|------|---------|-----------|
|___-start___|_?? \<min> \<max>_|Sets the starting byte to a random value between _\<min>_ (inclusive) and _\<max>_ (inclusive).|
|___-end___|_?? \<min> \<max>_|Sets the ending byte to a random value between _\<min>_ (inclusive) and _\<max>_ (inclusive).|
|___-inc___|_?? \<min> \<max>_|Sets the increment to a random value between _\<min>_ (inclusive) and _\<max>_ (inclusive).|
|___-mode___|_??_|Sets the mode to arandom mode and assigns it a random value or seed. This is the default.|

## Lump Filtering

By default, _DOOM WAD Corrupter_ corrupts all lumps in the WAD. To skip certain lumps, or to corrupt only certain lumps, you may use the ___-skip___ (exlusive) and ___-only___ (inclusive) options, respectively. These options are called _filter groups_. Lump filtering works using regular expressions and special keywords (see below). You many combine both kinds of filter groups on the same command line, as well as have any number of each.

Both of the filter groups allow you to specify more than _filter_ (i.e. lump name, regular expression, or special keyword). Each of these must successfully match for that particular filter group to match; on the other hand, only one filter group needs to successfully match for a lump to be selected for corruption.

#### Precedence

When inclusive and exclusive filter groups are intermixed on the same command line, the exclusive filter groups have precedence over the inclusive. This means that, even if an inclusive filter group successfully matches, the lump will still be skipped if even one exclusive filter group successfully matches.

### Keywords

Special types of lumps can be caught using special keywords. These keywords can be specified in place of a regular expression in a filter group and are not case sensitive.

#### Namespace

You can reference a specific lump's namespace by using one of the following keywords:

|Keyword|Function|
|-------|--------|
|___\<sprites>___|Matches all lumps in the _sprites_ namespace.|
|___\<patches>___|Matches all lumps in the _patches_ namespace.|
|___\<flats>___|Matches all lumps in the _flats_ namespace.|

#### Maps

Map lumps can be referenced in a similar way to namespaces using the following keywords:

|Keyword|Function|
|-------|--------|
|___\<maps>___|Matches lumps with the names "THINGS", "VERTEXES", "SECTORS", and "SSECTORS". Identical to _\<maps-t-v-s-ss>_ (see below).|
|___\<maps-all>___|Matches all map lumps.|
|___\<maps-...>___|Matches the specified map-based lumps. The lumps to match are specified as shorthand "lump codes" between the hyphen and the final angled bracket, each separated by a hyphen. The lump codes may be specified in any order.|

The available lump codes are as follows:

| Code |  Lump  | Code |  Lump  |
|:----:|:------:|:----:|:------:|
|**bm**|BLOCKMAP|**ld**|LINEDEFS|
|**n** | NODES  |**r** |REJECTS |
|**s** |SECTORS |**sd**|SIDEDEFS|
|**sg**|  SEGS  |**ss**|SSECTORS|
|**t** | THINGS |**v** |VERTEXES|

### G/ZDOOM Filtering

Another type of lump filtering is activated when the ___-zdoom___ option is specified on the command line. By default, this type of filtering is off. When activated, the corrupter skips over any G/ZDOOM lumps, based on their name, which don't corrupt nicely (mostly text-based).

The lumps that will be skipped are described below:

|  Name  |  Name  |  Name  |  Name  |
|:------:|:------:|:------:|:------:|
|ALTHUDCF|ANIMDEFS|CVARINFO|DECALDEF|
|DECORATE|DEHACKED|DEHSUPP | DMXGUS |
|FSGLOBAL|FONTDEFS|GAMEINFO| GLDEFS |
|KEYCONF |LANGUAGE|LOADACS |LOCKDEFS|
|MAPINFO |MENUDEF |MODELDEF|MUSINFO |
|PALVERS |S_SKIN* |SBARINFO|SCRIPTS |
|SECRETS |SNDINFO | SNDSEQ |TEAMINFO|
|TERRAIN |TEXCOLOR|TEXTURES|VOXELDEF|
| XHAIRS |X116R6GB|ZMAPINFO|ANIMATED|
|BEHAVIOR|GENMIDI |SNDCURVE|SWITCHES|

The result of this special filtering overrides that of all inclusive filter groups.

_\* The last two characters, if any, do not matter, and will match regardless of their value or existence._