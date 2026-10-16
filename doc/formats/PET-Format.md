**P.F. Magic’s** .pet **File Format (**v.3**)**  
Reverse engineered and documented by Reflet@Yabiko  
*Last updated: March 10, 2023*

**Changelog**  
*Feb 14, 2023 – updated LoadInfo section*

*Mar 10, 2023 – added custom visual for trick layout, misc tweaks*

**Introduction**  
Greetings\! I’m proud to say that as of 5/16/22, I’ve completely figured out the .pet file format and the data structures contained within. This is the product of many, many hours of work, and it is my hope that with this document, I can inspire the next set of would-be programmers and modders in the Petz community to take up the craft and create things that can be used, shared, and built upon for years to come.

It should be noted that while the *structure* of the data is completely documented here, along with the data types of each field, the purpose of a select few values/structures is yet unknown. Hopefully, we as a community can figure these out together\!

**Some notes about formatting**  
I've done my best to name \+ group data according to their technical names in the game's code. In cases where the game doesn't provide an obvious label, I came up with an arbitrary name.

Values are listed in order of appearance and classified according to their data types. This is extremely important for mapping out the file correctly. Integers will denote the number of bits they contain; for example, a 32-bit (4-byte) integer will be labeled as Int-32. Arrays of values will be marked with their data type along with the number of elements contained inside square brackets – for example, Char\[17\] – just like you would do in many common programming languages.

Hexadecimal numbers will be prefixed with '0x'. If you are reading this for the purpose of navigating and/or modifying pet files, it's assumed that you are already familiar with the concept of endianness. Pet files are encoded in little-endian format, and any hexadecimal numbers shown here will reflect their value as opposed to their byte order.

**Redistribution**

Anyone is free to use this information as they see fit. Please just credit me and/or link to this document\!

Without further ado, let us get into the format.

---

**Load Info**  
This is a data structure common to most game objects. All descriptions here will be in the context of a pet.

Structure

* **Basic Information**  
* Int-16  
* **Session ID**

* This is the game's way of differentiating various entities in the currently running game session.

* Valid numbers for pets are 3000 – 3999\.

* If the ID is outside this range, the pet will be loaded, but will not show up in the Pick-A-Pet menu.

* If there is a clash between two or more pets, the conflicting session IDs will be reassigned based on the order in which the pets are loaded (alphabetically).

* Char\[256\]  
* **SName**

* The pet's name.

* The Windows API enforces a 260-character limit for entire file paths. Since the pet's name is dictated by its file name, and this equivalency is checked at runtime and corrected if needed, it's not possible to overflow this.

* If the pet’s name is changed, and the new name is at least two bytes shorter, the previously used bytes beyond the new 0x00 terminator will be left as-is. For example, if a pet’s name was “Oscar” but later changed to “Oz”, the ‘-ar’ part of Oscar will still be visible in a hex editor.

* In Petz II files, any space not taken up by the name will be filled with garbage from RAM at the time the pet was created. This is left intact even after importing the pet into a later game.

* Char\[256\]  
* **BName**

* The pet's breed.

* Although a real breed name cannot legitimately overflow this due to the limitations described above, it can be overflowed through use of a hex editor. If byte 256 is not 0x00, the game will crash when trying to open any menu that displays the breed name. 

* In Petz II files, any space not taken up by the breed will be filled with garbage from RAM at the time the pet was created. This is left intact even after importing the pet into a later game.

* Char\[260\]  
* **DName**

* The pet's breedfile path (relative to Petz.exe).

* Although a real file path cannot legitimately overflow this due to the limitations described above, it can be overflowed through use of a hex editor. If byte 276 (leaving the bounds of this buffer and intruding upon the checksum section) is not 0x00, the game will declare the file corrupt.

* In Petz II files, any space not taken up by the file path will be filled with garbage from RAM at the time the pet was created. This is left intact even after importing the pet into a later game.

* Int-16  
* **Padding**

* As these two bytes are never written to directly, any nonzero values are simply garbage from RAM at the time the pet was created.

* Int-32  
* **DIndex**

* If this is nonzero, the game will declare the breedfile missing, regardless of where the file path actually terminated.  
* Int-32  
* **Unknown**

* Appears to always be 0xFFFFFFFF

* Appears to never be accessed

* Int-32  
* **Species**

* 0 \= Catz

* 1 \= Dogz

* Int-32  
* **Load Flags**

* If bit 1 is set *and* bits 4 and/or 5 are *not* set, the game will set the icon based on the species value.

* Otherwise, the game will check the breedfile for a custom icon. If none is provided, it will defer to the species.

* If bits 10 or 11 are set, the Petz II import dialog will be triggered upon bringing the pet out. It should be noted that real Petz II files don’t have either of those bits set.

* Int-32  
* **Game version**

* 0x000004D3 \= Petz II

* 0xACB00002 \= Petz 3/4

* 0xACB00003 \= Petz 5

* Int-32  
* **File checksum**

* Tested when a pet is being loaded. If it’s incorrect, the game will throw a “data corruption” error and move on to the next file.

* It is a combination of two values: the sum of the header’s bytes (“upper checksum”) and the sum of the rest of the file’s bytes (“lower checksum”). Both sums are truncated from 32-bit to 16-bit and placed side by side.

* The algorithm starts by zeroing out all 4 bytes. Then, the lower checksum is calculated and written. The upper checksum is calculated second, and it includes the 2 bytes of the lower checksum in its total. The bitmap section is not included in either calculation.  
* Byte\[16\]  
* **Instance ID**

* The pet’s unique ID, in the form of a globally unique identifier (GUID). This is used primarily in the Association Matrix and the Ancestry Info to distinguish one pet from another.

* If there is a GUID clash between two pets in the same game, the newer file will have its GUID reassigned.

**Petz Info**  
These are miscellaneous, pet-specific data structures and variables.

Structure

* **First PFM**  
* Char\[17\]  
* **ASCII** “p.f.magicpetzIII”

* Includes 0x00 terminator

* **Modifiers & Thresholds**  
* Int-8  
* **New day flag**

* Altered at runtime based on when the game was last started. The reason for saving it to the file is unknown.

* If 1, the game has not been started today, and this pet will have its Energy set to 100 and Fullness set to 50 upon coming out.

* If 0, the game has already been started today, and the pet must be checked to see when it was last saved. If roughly 30 minutes have elapsed since the last save, the pet will have its Energy set to 100 and Fullness set to 50 upon coming out. 

* Int-32  
* **Treat training modifier**

* Determines how many times the pet will fall on its face before attempting a real trick. Appears to only affect the first time a pet is introduced to the concept of treat training.

* Will decrement every time a trick is called for until it hits 0, at which point the pet will start attempting tricks.

* Int-32  
* **Unknown**

* Appears to always be 10

* Appears to never be accessed

* Int-32  
* **Unknown**

* Appears to always be 10

* Appears to never be accessed

* Int-32  
* **Neglect modifier (active)**

* Appears to always be 1

* Controls the amount a pet’s neglect will increase when disciplined.

* Int-32  
* **Neglect modifier (passive)**

* Appears to always be 10

* Controls the amount a pet’s neglect will decrease when it is brought out for the first time that day, or increase if it is not played with.

* Int-32  
* **Neglect threshold**

* If the pet’s Neglect is greater than or equal to this number, the pet will begin to act visibly neglected.

* Int-32  
* **Unknown**

* Appears to always be 101

* Appears to never be accessed

* Int-32  
* **Runaway threshold**

* If the pet’s Neglect is greater than or equal to this number, the pet will display as “Runaway” in the Pick-A-Pet menu.

* Int-8  
* **Unknown**

* Appears to always be 0

* Appears to never be accessed

* **Event Timestamps**  
* Int-32  
* **Time last played with**

* Unix timestamp

* Updated constantly while the pet is out. Update triggers are things like petting, picking up/setting down a toy, double-clicking to call, etc.

* Int-32  
* **Time of last game start**

* Unix timestamp

* The time that Petz was last started.

* Int-32  
* **Time last fed**

* Unix timestamp

* The last time the pet ate something. While the pet is eating, this value is constantly updated. Drinking does not affect it.

* Int-32  
* **Time of last game start (duplicate)**

* Unix timestamp

* The time that Petz was last started. Updated alongside the other “time of last game start” value.

* Int-32  
* **Unknown**

* Appears to always be 0

* Appears to never be accessed

* Int-32  
* **Time last played with (duplicate)**

* Unix timestamp

* Updated constantly while the pet is out, alongside the other “time last played with” value.

* Int-32  
* **Time last saved**

* Unix timestamp

* The time the pet file was last saved. Update triggers include putting the pet away, closing the game, or the pet becoming pregnant.

* Int-32  
* **Random seed**

* The pet's personal seed value. This is generated via MSVCRT.dll’s rand() function when a pet is created.

* In AC pets, this controls individual variation. In bred pets, it’s unknown if it does anything.

* **Trick Knowledge**  
* Int-32  
* **Plan ID**

* IDs vary slightly between games, and are not corrected when moving a pet between game versions. This will cause a pet to behave strangely if called to do a trick outside of the game in which it was created.  
* Int-32  
* **Associated action**

* 0xFFFFFFFF \= not set

* Actions are defined in the dog and cat script (SCP) files, located in "\[species\] \[version\] Rez.dll", and may be overridden or added to by individual breedfiles.  
* Int-32  
* **Direction**

* Controls which direction the pet will go

* Most prominent in tricks such as the sideways roll  
* Int-32  
* **Angle**

* Controls the angle at which the pet will do the trick

* Most prominent in tricks such as the sideways roll  
* Int-32  
* **Association**

* 10 is the base value; can go up to 100

* Increased or decreased depending on whether you reward the pet or not.

* Dropping the treat will increase this value, even if you take it away before the pet eats it. Putting the treat away or gesturing for another trick will decrease this value.

* If the value is at 10 but needs to decrease, the game will either assign a new trick or do nothing.

There are 120 trick slots in total, divided across 8 flavors.

     Slot  	Flavor      	Associated treat (in vanilla game)

   1 – 15  	0 | Chicken 	Chicken  
  16 – 30  	1 | Beef    	Beef, Bone  
  31 – 45  	2 | Fish    	Fish, Olive  
  46 – 60  	3 | Turkey  
  61 – 75  	4 | Milk  
  76 – 90  	5 | Sweets  	Popcorn, Cotton Candy, Luvz  
 91 – 105  	6 | Catnip  
106 – 120  	7 | Cheese  	Cheese

The structure of each flavor is as follows:

* “Toy”  
* Trick 1  
* Trick 2  
* Trick 3

* “Up”  
* Trick 4  
* Trick 5  
* Trick 6

* “Down”  
* Trick 7  
* Trick 8  
* Trick 9

* “Left”  
* Trick 10  
* Trick 11  
* Trick 12

* “Right”  
* Trick 13  
* Trick 14  
* Trick 15

Alternatively, this can be visualized as a three-dimensional array, with the dimensions being flavor, gesture, and trick slot.

![][image1]

If a trick has invalid parameters, the pet will appear confused and start whining when asked to perform it. When given a non-trick action for “toy” tricks, the pet will jump on the toy and fall over before performing the action.

* **Biorhythms**  
* Int-32  
* **Energy**

* Int-32  
* **Fullness**

* Int-32  
* **Fatness**

* Int-32  
* **Sickness**

* Int-32  
* **Catnipped**

* Int-32  
* **Fleas**

* Int-32  
* **Horniness**

* Int-32  
* **Neglect**

* Int-32  
* **Age**

* **File Creation Time**  
* Int-32  
* **QueryPerformanceCounter value**

* High-resolution timestamp; different from Unix.

* Only written to on first save. Never changes afterwards.

* Possibly a debug leftover.

* **Saved Color Info**  
* Int-8  
* **Unknown**

* Appears to always be 1

* Int-8  
* **Unknown**

* Appears to always be 0

* Int-32  
* **Current eyelid color**

* LNZ color number

* Reset to \[256 Eyelid Color\] value upon hitting “Restore Original Colors”.

* Int-32  
* **Number of ball paint flags**

* 512

* Byte\[512\]  
* **Ball paint flags**

* 0 \= not painted

* 1 \= painted

* One byte for each ball (including all possible addballz)

* Int-32  
* **Total number of groups**

* 15

* Int\[15\]  
* **Group colors**

* LNZ color number

* Will be 0xFFFFFFFF if not painted

* Int-32  
* **Number of balls**

* 512

* Int\[512\]  
* **Current ball colors**

* LNZ color number

* Includes all possible addballz

* Will be 248 if ball does not exist

* Int-32  
* **Padding**

* Appears to always be 0

**LNZ Info**  
Defines a pet’s graphical appearance, and keeps track of what clothing is being worn, if any.

Structure

* **Second PFM**  
* Char\[17\]  
* **ASCII** “p.f.magicpetzIII”  
* Includes 0x00 terminator

* **Linez**  
* Char\[??\]  
* **Linez code**

* One big, long C-style string

* Includes 0x00 terminator

* Int-32  
* **Magic number**

* 0x03BBCCAA

* Int-32  
* **Seed (duplicate)**

* The pet's personal seed value (again)

* Byte\[196\]  
* **Padding**

* All bytes are 0x00

* **Clothing Information**  
* Int-32  
* **Ordinal**

* Number of this clothing slot.

* Char\[64\]  
* **Clothing file path relative to Petz.exe**

* Although a real file cannot legitimately overflow this due to the limitations described above, it can be overflowed through use of a hex editor.

* Char\[128\]  
* **Clothing resource file path relative to Petz.exe**

* Although a real file cannot legitimately overflow this due to the limitations described above, it can be overflowed through use of a hex editor.

This structure is repeated for every clothing slot. If the pet is not wearing something in a particular slot, all bytes will be 0x00. The ordinal is not affected by the presence or absence of clothing in preceding slots.

The clothing slots are slightly different in Petz 5 versus Petz 3/4. They are as follows:

Petz 3/4                                           Petz 5  
Ordinal  	Clothing type       	Ordinal  	Clothing type

  	1  	Shirt                     	1  	Shirt  
  	2  	Pant                      	2  	Pant  
  	3  	Sock\_FrontL               	3  	Sock\_FrontL  
  	4  	Sock\_FrontR               	4  	Sock\_FrontR  
  	5  	Sock\_BackL                	5  	Sock\_BackL  
  	6  	Sock\_BackR                	6  	Sock\_BackR  
  	7  	Tail                      	7  	Tail  
  	8  	Hat                       	8  	Hat  
  	9  	Hat2                      	9  	EarringL  
 	10  	EarringL                 	10  	EarringR  
 	11  	EarringR                 	11  	NoseThing  
 	12  	NoseThing                	12  	Glasses  
 	13  	NoseThing2  
 	14  	Glasses

* **Breedfile Information**  
* Int-32  
* **Number of required breedfiles**

* If 0, the following data is **not** present.

* Int-16  
* **Breed ID**

* ID number of the breed

* Int-32  
* **Length of file path**

* Does not include 0x00 terminator

* Char\[??\]  
* **Breedfile path (relative to Petz.exe)**

* Length determined by previous value

* No 0x00 terminator

This structure is repeated for every breedfile, except for the number of required breedfiles, which only appears once.

**Veterinary History**  
Although this section is present and constantly updated in every pet file, it does not appear to serve any purpose other than recordkeeping. It is most likely a vestige of Petz’s development.

Structure

* **Third PFM**  
* Char\[17\]  
* **ASCII** “p.f.magicpetzIII”

* Includes 0x00 terminator

* **Statuses**  
* Int-8  
* Gender  
* 0 \= Male  
* 1 \= Female

* Int-8  
* Spay/Neuter status  
* 0 \= Not spayed/neutered  
* 1 \= Spayed/Neutered

* Int-8  
* Dependent status  
* 0 \= Independent  
* 1 \= Dependent

* Int-8   
* Pregnancy status  
* 0 \= Not pregnant  
* 1 \= Pregnant

* **Number of Veterinary Info Sections**  
* Int-32  
* **Number of sections present**

* Between 0 and 8

The following sections, called “Veterinary Info” sections, are each only present if they contain one or more records.

* **PLAY**  
* Int-32  
* **0x504C4159**

* Numerical representation of ASCII “PLAY”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamps

* Recorded every time PetSprite::GotPlayedWith is called

* Between 1 and 30 records depending on previous value

* Int-32  
* **Number of neglect levels stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of neglect levels**

* Between 1 and 30 records depending on previous value

* Value is the difference between 100 and the pet’s neglect biorhythm at the time of recording

* **BFED**  
* Int-32  
* **0x44454642**

* Numerical representation of ASCII “BFED”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamps

* Recorded every time PlanEatDrinkFromBowl::Execute is called

* Between 1 and 30 records depending on previous value

* Int-32  
* **Number of fatness levels stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of fatness levels**

* Between 1 and 30 records depending on previous value

* Value is the pet’s fatness biorhythm at the time of recording

* **CATN**  
* Int-32  
* **0x4341544E**

* Numerical representation of ASCII “CATN”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamps

* Recorded every time PetSprite::HandleCatnipped is called

* Between 1 and 30 records depending on previous value 

* Int-32   
* **Number of catnipped levels stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of catnipped levels**

* Between 1 and 30 records depending on previous value

* Value is the pet’s catnipped biorhythm at the time of recording

* **DISC**  
* Int-32  
* **0x44495343**

* Numerical representation of ASCII “DISC”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamps

* Recorded every time PetSprite::HandleDisciplined is called

* Between 1 and 30 records depending on previous value

* Int-32  
* **Number of naughtiness levels stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of naughtiness levels**

* Between 1 and 30 records depending on previous value

* Value is the pet’s naughtiness trait at the time of recording

* **MATE**  
* Int-32  
* **0x4D415445**

* Numerical representation of ASCII “MATE”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]   
* **Array of timestamps** 

* Unix timestamps

* Recorded every time PetSprite::Mate is called

* Between 1 and 30 records depending on previous value

* Int-32  
* **Number of records stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of records**

* Between 1 and 30 records depending on previous value 

* Record is always 1

* **MFLE**  
* Int-32  
* **0x4D464C45**

* Numerical representation of ASCII “MFLE”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamps

* Recorded every time PetSprite::EventMedicine is called, if the Flea Spray was the trigger

* Between 1 and 30 records depending on previous value

* Int-32  
* **Number of fleas levels stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of fleas levels**

* Between 1 and 30 records depending on previous value

* Value is the pet’s fleas biorhythm at the time of recording

* **MSCK**  
* Int-32  
* **0x4D53434B**

* Numerical representation of ASCII “MSCK”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamps

* Recorded every time PetSprite::EventMedicine is called, if the Medicine Bottle was the trigger

* Between 1 and 30 records depending on previous value

* Int-32  
* **Number of sickness levels stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of sickness levels**

* Between 1 and 30 records depending on previous value 

* Value is the pet’s sickness biorhythm at the time of recording

* **TREA**  
* Int-32  
* **0x54524541**

* Numerical representation of ASCII “TREA”

* Int-32  
* **Max number of records**

* Always 30

* Int-32  
* **Number of timestamps stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamps

* Recorded every(?) time PlanEatCapturedSprite::Execute is called

* Between 1 and 30 records depending on previous value

* Int-32  
* **Number of fatness levels stored**

* Between 1 and 30

* Int-32\[??\]  
* **Array of fatness levels**

* Between 1 and 30 records depending on previous value

* Value is the pet’s fatness biorhythm at the time of recording

* **Biorhythm Records**  
* Int-32  
* Number of biorhythm records  
* Between 0 and 10  
* If 0, the following data is **not** present. 

* Int-32  
* **Energy**

* Int-32  
* **Fullness**

* Int-32  
* **Fatness**

* Int-32  
* **Sickness**

* Int-32  
* **Catnipped**

* Int-32  
* **Fleas**

* Int-32  
* **Horniness**

* Int-32  
* **Neglect**

* Int-32  
* **Age**

* Int-32  
* **Time of recording**  
* Unix timestamp  
* If the game is closed while the pet is out, a new entry will be recorded.  
* Only one entry is recorded per day.

This structure is repeated up to 10 times, save for the number of records, which only appears once.

**Genome**  
This section contains a pet’s genetics in the form of multiple ‘chromosome’ data structures. There are three types of chromosomes, and each has two copies which may have the same or different data. The Sprite chromosome describes a pet’s affinities for different adjectives, Behavior describes a pet’s disposition, and Looks describes a pet’s physique.

Structure  
Chromosomes hold their data within substructures known as ‘alleles’. Before we get into the layout of each chromosome, we must first talk about the structure of an allele:

* **Allele**  
* Int-32  
* **Center**

* Sprite: The pet’s preferred value for this adjective. If 0xFFFFFFFF, the allele is ignored, and not used in any calculations.

* Behavior: The pet’s trait level

* Looks: Either a breed ID or integer value

* Int-32  
* **Range**

* The maximum possible deviation from the Center.

* The Center is, naturally, the midpoint of this range. Therefore, the maximum deviation is Range / 2 on either side of the Center value.

* In the Looks chromosome, this is either a pet’s seed value or unused.

* Int-8  
* **Bitmask flag**

* 0 \= ‘Center’ is an integer

* 1 \= 'Center' is a bitmask

* Int-32  
* **Weight**

* Percent value

* Appears to always be 100

* Effectively dictates how important this trait is when calculating a pet's affinity for something.

* Int-8  
* **Weight type**

* Appears to always be 1 in the Sprite and Looks chromosomes, except for the ‘Discipline’ allele, where it is 0\.

* In the Behavior chromosome, it appears to always be 0\.

* Another possible value is 2, according to the code.

* Int-8  
* **Combine type**

* Appears to always be 0

* Unknown effect

* Int-32  
* **Offset**

* ONLY PRESENT IN **SPRITE/GOAL DESCRIPTOR** ALLELES

* Appears to always be 0

* Unknown effect

* Int-32  
* **Centering rate**

* ONLY PRESENT IN **SPRITE/GOAL DESCRIPTOR** ALLELES 

* Appears to always be 60

* Unknown effect

Now, the structure of each type of chromosome:

* **Sprite Chromosome**  
* Int-32  
* **Number of alleles**

* 41

* Allele  
* **Type**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Chrz**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Toyz**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Prop**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Part**

* Appears to always be 0xFFFFFFFF

* Allele  
* **3D**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Color**

* Always a bitmask. The flavor is encoded as 1 \<\< color \#.

* Multiple bits can be set at once, giving a pet multiple favorites.

* Allele  
* **Flavor**

* Always a bitmask. The flavor is encoded as 1 \<\< flavor \#.

* Multiple bits can be set at once, giving a pet multiple favorites.

* Allele  
* **Size**

* Allele  
* **Mass**

* Allele  
* **Friction**

* Allele  
* **Tasty**

* Allele  
* **Edible**

* Allele  
* **Fatty**

* Allele  
* **Liquid**

* Allele  
* **Drug**

* Allele  
* **Aphrodisiac**

* Allele  
* **Discipline**

* Allele  
* **Chew**

* Allele  
* **Tug**

* Allele  
* **Density**

* Allele  
* **Thickness**

* Allele  
* **Soft**

* Allele  
* **Fuzzy**

* Allele  
* **Round**

* Allele  
* **Bounce**

* Allele  
* **Swatty**

* Allele  
* **Pretty**

* Allele  
* **Vain**

* Allele  
* **Paint**

* Allele  
* **Groom**

* Allele  
* **BadNoisy**

* Allele  
* **NiceNoisy**

* Allele  
* **Flies**

* Allele  
* **Rideable**

* Allele  
* **Mouselike**

* Allele  
* **Unknown 1**

* Possibly unused

* Allele  
* **Unknown 2**

* Possibly unused

* Allele  
* **Unknown 3**

* Possibly unused

* Allele  
* **Unknown 4**

* Possibly unused

* **Behavior Chromosome**  
* Int-32  
* **Number of alleles**

* 22

* Allele  
* **Liveliness**

* Allele  
* **Playfulness**

* Allele  
* **Independence**

* Allele  
* **Confidence**

* Allele  
* **Naughtiness**

* Allele  
* **Acrobaticness**

* Allele  
* **Patience**

* Allele  
* **Kindness**

* Allele  
* **Nurturing**

* Allele  
* **Finickiness**

* Allele  
* **Intelligence**

* Allele  
* **Messiness**

* Allele  
* **Quirkiness**

* Allele  
* **Insanity**

* Allele  
* **Constitution**

* Allele  
* **Metabolism**

* Allele  
* **Dogginess**

* Allele  
* **LoveDestiny**

* Allele  
* **Fertility**

* Allele  
* **LoveLoyalty**

* Allele  
* **Libido**

* Allele  
* **OffspringSex**

In the Looks Chromosome, the Allele substructure is the same, but **the Center and Range integers are more accurately described as the Value and Seed** in this context.

* **Looks Chromosome**  
* Int-32  
* **Number of alleles**

* 25

* Allele  
* **Unknown**

* Integer

* Appears to always be 0xFFFFFFFF

* Allele  
* **Unknown**

* Breed ID \+ Seed

* Allele  
* **Default Scale**

* Integer

* Allele  
* **Ears**

* Breed ID \+ Seed

* Allele  
* **Head**

* Breed ID \+ Seed

* Allele  
* **Whiskers**

* Breed ID \+ Seed

* Allele  
* **Feet**

* Breed ID \+ Seed

* Allele  
* **Legs**

* Breed ID \+ Seed

* Allele  
* **Tail**

* Breed ID \+ Seed

* Allele  
* **Body**

* Breed ID \+ Seed

* Allele  
* **Coat**

* Breed ID \+ Seed

* Allele  
* **Tongue**

* Breed ID \+ Seed

* Allele  
* **Eye Color**

* Integer

* Allele  
* **Lid Color**

* Integer

* Allele  
* **Coat Color 1**

* Integer

* Allele  
* **Coat Color 2**

* Integer

* Allele  
* **Coat Color 3**

* Integer

* Allele  
* **Coat Color 4**

* Integer

* Allele  
* **Coat Color 5**

* Integer

* Allele  
* **Marking Factor 2**

* Integer

* Yes, 2 first

* Allele  
* **Marking Factor 1**

* Integer

* Yes, 1 second

* Allele  
* **Marking 1**

* Breed ID \+ Seed

* Now 1 first

* Allele  
* **Marking 2**

* Breed ID \+ Seed

* Now 2 second

* Allele  
* **Leg Extension**

* Integer

* Allele  
* **Body Extension**

* Integer

The genome is laid out in the following manner:

* **Sprite 1**  
* 41 alleles

* **Behavior 1**  
* 22 alleles

* **Looks 1**  
* 25 alleles

* **Sprite 2**  
* 41 alleles

* **Behavior 2**  
* 22 alleles

* **Looks 2**  
* 25 alleles

* **Padding (Petz 5 only)**  
* Int-32  
* Padding  
* Appears to always be 0

**Unborn Child Data**  
If the pet is pregnant, some information about the child-to-be is stored within the mother’s file before the offspring is delivered.

Structure

* **Child Data**  
* LNZ Info  
* **See above documentation.**

* No preceding "p.f.magicpetzIII"

* Ancestry Info  
* **See below documentation.**

* Note that both the pet name length and owner name length are 0, so there are no accompanying char arrays for those fields.

* Both the adoption date and birth date fields are also 0, as the child hasn’t been born yet.

* Int-8  
* **Unknown**

* Appears to always be 0

* Int-32  
* **Conception date**

* Unix timestamp

* The child will not be born until approximately two days have elapsed since this date.

* So-called 'stuck pregnancies' are caused by this date erroneously being very far in the future.

* PetzA's instant birth feature only works on conception dates in the past; therefore, even with instant birth, a future conception date will cause the pet to remain pregnant.

* Int-8  
* **Unknown**

* Appears to always be 0

**Behavior Descriptors**  
These structures actively affect a pet's preferences and behavior – their "personality", if you will.

Structure

* **Fourth PFM**  
* Char\[17\]  
* **ASCII** “p.f.magicpetzIII”

* Includes 0x00 terminator

* **Sprite Descriptor**  
* Int-32  
* **Number of alleles**

* Always 41

* Allele  
* **Type**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Chrz**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Toyz**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Prop**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Part**

* Appears to always be 0xFFFFFFFF

* Allele  
* **3D**

* Appears to always be 0xFFFFFFFF

* Allele  
* **Color**

* Always a bitmask. The flavor is encoded as 1 \<\< color \#.

* Multiple bits can be set at once, giving a pet multiple favorites.

* Allele  
* **Flavor**

* Always a bitmask. The flavor is encoded as 1 \<\< flavor \#.

* Multiple bits can be set at once, giving a pet multiple favorites.

* Allele  
* **Size**

* Allele  
* **Mass**

* Allele  
* **Friction**

* Allele  
* **Tasty**

* Allele  
* **Edible**

* Allele  
* **Fatty**

* Allele  
* **Liquid**

* Allele  
* **Drug**

* Allele  
* **Aphrodisiac**

* Allele  
* **Discipline**

* Allele  
* **Chew**

* Allele  
* **Tug**

* Allele  
* **Density**

* Allele  
* **Thickness**

* Allele  
* **Soft**

* Allele  
* **Fuzzy**

* Allele  
* **Round**

* Allele  
* **Bounce**

* Allele  
* **Swatty**

* Allele  
* **Pretty**

* Allele  
* **Vain**

* Allele  
* **Paint**

* Allele  
* **Groom**

* Allele  
* **BadNoisy**

* Allele  
* **NiceNoisy**

* Allele  
* **Flies**

* Allele  
* **Rideable**

* Allele  
* **Mouselike**

* Allele  
* **Unknown 1**

* Possibly unused

* Allele  
* **Unknown 2**

* Possibly unused

* Allele  
* **Unknown 3**

* Possibly unused

* Allele  
* **Unknown 4**

* Possibly unused

 

* **Goal Descriptor**  
* Int-32  
* **Number of alleles**

* Always 22

* Allele  
* **Liveliness**

* Allele  
* **Playfulness**

* Allele  
* **Independence**

* Allele  
* **Confidence**

* Allele  
* **Naughtiness**

* Allele  
* **Acrobaticness**

* Allele  
* **Patience**

* Allele  
* **Kindness**

* Allele  
* **Nurturing**

* Allele  
* **Finickiness**

* Allele  
* **Intelligence**

* Allele  
* **Messiness**

* Allele  
* **Quirkiness**

* Allele  
* **Insanity**

* Allele  
* **Constitution**

* Allele  
* **Metabolism**

* Allele  
* **Dogginess**

* Allele  
* **LoveDestiny**

* Allele  
* **Fertility**

* Allele  
* **LoveLoyalty**

* Allele  
* **Libido**

* Allele  
* **OffspringSex**

**Association Matrix**   
Records of encountered pets and toys are kept here. 

Structure   
The Association Matrix can best be described as a collection of key-value pairs. There are three main data substructures used in the Association Matrix. Before we talk about the layout of the section, we must first talk about the layout of each substructure:

* **Sprite Key**  
* Int-32  
* **Number of timestamps**

* Unknown limit

* Int-32\[??\]  
* **Timestamp indexes**

* Further into the Association Matrix, there is a list of timestamps. This number is an index to a specific timestamp in the list.

* Byte\[16\]  
* **Globally Unique Identifier (GUID)**

* If this key refers to a pet, this will be said pet’s GUID.

* Otherwise, all bytes are 0x00.

* Int-16  
* **Sprite ID**

* Breed ID, toy ID, etc.

* **Goal Key**  
* Int-32  
* **Number of timestamps**

* Unknown limit

* Int-32\[??\]  
* **Timestamp indexes**

* Further into the Association Matrix, there is a list of timestamps. This number is an index to a specific timestamp in the list.

* Int-16  
* **Unknown ID**

* IDs appear to start at 20,000.

* **Affinity Descriptor**  
* Byte\[16\]  
* **Globally Unique Identifier (GUID)**

* A pet’s GUID.

* Int-32  
* **Unknown**

* Unknown  
* Int-32  
* **Unknown**

* Unknown

* Int-32  
* **Unknown**

* Unknown

* Int-32  
* **Unknown**

* Unknown

* Int-8  
* **Unknown**

* Unknown

Now, the section itself:

* **Keys**  
* Int-32  
* **Number of sprite keys**

* Unknown limit

* Sprite Key\[??\]  
* **Array of sprite keys**

* One after the other, with no breaks in between.

* Int-32  
* **Number of goal keys**

* Unknown limit

* Goal Key\[??\]  
* **Array of goal keys**

* One after the other, with no breaks in between.

* **Values**  
* Int-32  
* **Number of integers**

* Unknown limit

* Int-32\[??\]  
* **Array of unknown integers**

* Unknown purpose

* There appears to be one integer per Sprite Key timestamp.  
* Int-32  
* **Number of timestamps**

* Unknown limit

* Int-32\[??\]  
* **Array of timestamps**

* Unix timestamp

* **Affinity Descriptors**  
* Int-32  
* **Number of affinity descriptors**

* Unknown limit

* Affinity Descriptor\[??\]  
* **Array of affinity descriptors**

* Unknown purpose 

* Presumably, every pet that appears in a sprite key will have a corresponding affinity descriptor.

**Ancestry Info**  
The pet's family tree.

Structure

* **Fifth PFM**  
* Byte\[15\]  
* **ASCII** “PfMaGiCpEtZIII”

* Includes 0x00 terminator

* **Pet Information**  
* Int-32  
* **Unknown**

* Unknown

* Byte\[16\]  
* **Globally Unique Identifier (GUID)**

* The pet’s GUID

* Int-32  
* **Pet name length**

* The length of the pet’s name in bytes

* Char\[??\]  
* **Pet name**

* Length determined by previous value

* No 0x00 terminator

* Int-32  
* **Breed name length**

* The length of the pet’s breed name in bytes

* Char\[??\]  
* **Breed name**

* Length determined by previous value

* No 0x00 terminator

* Int-32  
* **Owner name length**

* The length of the pet’s owner’s name in bytes

* Char\[??\]  
* **Owner name**

* Length determined by previous value

* No 0x00 terminator

* Int-32  
* **Adoption date**

* Unix timestamp

* 0 if pet is multi-generation

* Int-32  
* **Birth date**

* Unix timestamp

* 0 if pet is first-generation

* Int-32  
* **Number of generations**

* The total number of generations this pet has

* Int-8  
* **Outline color**

* The color of the pet’s outline in the family tree

* 0x03 \= pink

* 0x01 \= blue

* Int-8  
* **Next pet flag**

* 0 or 1

* Dictates whether there’s a subsequent entry or not

From here is the pet’s ancestor data (if the pet is multi-generation), starting with the mother. Each entry is sequential, but the overall structure can be visualized as follows:

3rd generation mother

* 2nd generation mother’s mother  
* 1st generation mother’s mother’s mother  
* 1st generation mother’s mother’s father  
* 2nd generation mother’s father  
* 1st generation mother’s father’s mother  
* 1st generation mother’s father’s father

3rd generation father

* 2nd generation father’s mother  
* 1st generation father’s mother’s mother  
* 1st generation father’s mother’s father  
* 2nd generation father’s father  
* 1st generation father’s father’s mother  
* 1st generation father’s father’s father

The structure of each entry is the same as the above.

* Int-8  
* **Padding**

* Appears to always be 0

**Text Info**  
The pet's owner.

Structure

* **Owner**  
* Int-32  
* **Owner name length**

* The length of the pet’s owner’s name in bytes

* Char\[??\]  
* **Owner name**

* Length determined by previous value

* No 0x00 terminator

**Comment Info**  
The comment in the pet’s profile.   
Structure 

* **Comment**  
* Int-32  
* **Number of characters**

* The length of the comment in bytes

* Char\[??\]  
* **Comment**

* Length determined by previous value

* No 0x00 terminator

* Int-32  
* **Comment timestamp**

* Unix timestamp

* Time the comment was last updated

* Int-32  
* **Extra info flag**

* 0 or 1

* Dictates whether there’s a subsequent Comment Info structure or not

**Dependent Child Data**  
Information about the pet’s dependent.

Structure

* **Dependent Flag**  
* Int-8  
* **Dependent flag**

* 0 or 1

* Dictates whether the pet has a dependent child or not

If the Dependent Flag is nonzero, then the dependent’s file will appear here. It’s a fully formed pet file, minus the Load Info and Bitmap sections, which are not present.

**Unknown**  
Unknown.

Structure

* **Unknown**  
* Int-32  
* **Unknown integer**

* Used in ComputeMatchmakerID

* Valid numbers are 0 \- 1,000,000

* If outside that range, it will be modulo'd by 1,000,000 (effectively treating it as if it were between 0 and 999,999)

* Int-32  
* **Unknown timestamp**

* Unix timestamp

* Used in ModifyCourtshipStage (0x485D18)

* Int-32  
* **Unknown timestamp**

* Unix timestamp

* Used in ModifyCourtshipStage (0x48595D)

* Int-8  
* **Unknown**

* Unknown

* Int-32  
* **Number of unknown 24-byte data structures**

* Unknown

* Byte\[??\]  
* **Unknown 24-byte data structures**

* Unknown

* Int-32  
* **Number of unknown 24-byte data structures**

* Unknown

* Byte\[??\]  
* **Unknown 24-byte data structures**

* Unknown

* Int-32  
* **Number of unknown 24-byte data structures**

* Unknown

* Byte\[??\]  
* **Unknown 24-byte data structures**

* Unknown

**Bitmap**  
The pet's thumbnail image.

Structure

* **Final PFM**  
* Byte\[15\]  
* **ASCII** “PFMAGICPETZIII”

* Includes 0x00 terminator

* Uppercase, no periods

* **Bitmap Header**  
* Int-32  
* **Total bitmap size in bytes**

* Does not include itself in the total

* Int-32  
* **Start of pixel data**

* 40

* Relative to this value; therefore, the actual offset is 44 from the start of the bitmap header.

* Int-32  
* **Image width**

* 108

* Int-32  
* **Image height**

* 80

* Int-16  
* **Number of planes**

* 1

* Int-16  
* **Bit depth**

* 8 in Petz 3/4

* 24 in Petz 5

* Int-32  
* **Compression type**

* 0 (none)

* Int-32  
* **Size of pixel data (in bytes)**

* 8,640 in Petz 3/4

* 25,920 in Petz 5

* Int-32  
* **Horizontal resolution (pixels per meter)**

* 0 (unused)

* Int-32  
* **Vertical resolution (pixels per meter)**

* 0 (unused)

* Int-32  
* **Number of colors**

* 0 (unused)

* Int-32  
* **Number of important colors**

* 0 (unused)

* **Pixel Data**  
* Pixel\[8640\]  
* **1 or 3 bytes per pixel (depending on bit depth)**

* 8640 (108 x 80\) pixels total

* Byte\[4000\]  
* **Padding**

* All bytes are 0x00

If the pet has a dependent, the dependent’s bitmap will appear here. The structure is the same as above.

[image1]: <data:image/png;base64,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>