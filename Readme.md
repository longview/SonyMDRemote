# SonyMDRemote
This is an attempt at making a C# application to talk to a Sony MDS-E12 MiniDisc recorder. If it works it should also work for the E11 and E52 models. 

The goal is to replace the older SonyMDRemote program, which doesn't really work at all on anything modern and is quite buggy even on older systems.

See the Docs folder for the protocol specifications if you want to help.
Currently targets .NET 4.7.2

## Hookup
You will need a female-female 9 pin D-Sub adapter cable (a null modem cable):
* Pin 2 to Pin 3
* Pin 3 to Pin 2
* Pin 5 to Pin 5

## Features implemented:
* Most MD-PC responses are decoded
* Reading disc name, track numbers etc. is supported
* Basic playback commands

## Planned:
* Polling state machine to read e.g. track number, status flags automatically
* Disc and Track Name editor (the main reason to make this in the first place)

## Not really working:
* Disc status and Power status appears to be incorrect for the E12
* Reading track names appears to be extremely buggy in the E12. 

I am hoping that when writing track names is implemented it might work better since I suspect the issue might be with how manually entered track names are stored internally confusing the serial interface routines.

For example, here is the output for a track named "Leuchtturm / Nena" showing more or less in-spec behaviour (at least it could be decoded):

	2022-10-15T13:28:37Z: MD: Status dump no disc powered off track no. 15 Play TOC read not copy protected stereo audio digital input locked Analog
	2022-10-15T13:28:39Z: MD sent: 6F-18-05-47-20-4A-0F-4C-65-75-63-68-74-74-75-72-6D-20-2F-20-4E-65-6E-FF, ASCII: oG JLeuchtturm / Nen?
	2022-10-15T13:28:39Z: MD: Track name part 15 is: Leuchtturm / Nen
	2022-10-15T13:28:39Z: MD sent: 6F-18-05-47-20-4B-02-61-00-00-65-72-20-53-63-68-69-6C-6C-69-6E-67-00-FF, ASCII: oG Kaer Schilling?
	2022-10-15T13:28:39Z: MD: Track name part 2 is: a
	2022-10-15T13:28:39Z: MD sent: 6F-07-05-47-20-4C-FF, ASCII: oG L?
	2022-10-15T13:28:39Z: MD: ALL NAME END

And here is the next track, which should read "99 Luftballons / Nena"

	2022-10-15T13:31:34Z: MD: Status dump no disc powered off track no. 16 Play TOC read not copy protected stereo audio digital input locked Analog
	2022-10-15T13:31:35Z: MD sent: 6F-6E-73-20-2F-FF, ASCII: ons /?
	2022-10-15T13:31:36Z: MD sent: 6F-18-05-47-20-4B-02-20-4E-65-6E-61-00-00-63-68-69-6C-6C-69-6E-67-00-FF, ASCII: oG K Nenachilling?
	2022-10-15T13:31:36Z: MD: Track name part 2 is:  Nena
	2022-10-15T13:31:36Z: MD sent: 6F-07-05-47-20-4C-FF, ASCII: oG L?
	2022-10-15T13:31:36Z: MD: ALL NAME END

The output data is fairly buggy, with the second line not conforming to the written spec at all, the start and terminator is correct but the payload appears to just be random ASCII data from the middle of the name.


## Will likely not be supported:
* Recording
* Track splitting/editing

## Documentation bugs
Here's what I got so far, seems like there will be lots to come!
I've mostly been using the Combined E11, 12, 52 document.

### 7.16 TRACK NAME
The first packet is listed as:

	0x20, 0x4A, PacketNo, ASCII Data

The output appears to be :

	0x20, 0x4A, TrackNo, ASCII Data

The field descriptions and my observations suggest this is the case, at least when a specific track name is requested.