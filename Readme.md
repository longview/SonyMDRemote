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

## Basic Command Sequence
Commands to the MD start with 0x7E, length, and some fixed parameters. Termination is 0xFF, and the length includes headers and termination.

Commands from the MD follow the same format, but start with 0x6F.

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

I am hoping that when writing track names is implemented it might work better since I suspect the issue might be with how manually entered track names are stored internally confusing the serial interface routines. The behaviour noted below is consistent, some tracks read fine and others don't. All these were entered manually on the same recorder.

For example, here is the output for a track named "Leuchtturm / Nena" showing more or less in-spec behaviour (at least it could be decoded):

	2022-10-15T13:28:37Z: MD: Status dump no disc powered off track no. 15 Play TOC read not copy protected stereo audio digital input locked Analog
	2022-10-15T13:28:39Z: MD sent: 6F-18-05-47-20-4A-0F-4C-65-75-63-68-74-74-75-72-6D-20-2F-20-4E-65-6E-FF, ASCII: oG JLeuchtturm / Nen?
	2022-10-15T13:28:39Z: MD: Track 15 name part 1 is: Leuchtturm / Nen
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

The output data is fairly buggy, with the second line not conforming to the written spec at all, the start and terminator is correct but the payload appears to just be random ASCII data from the middle of the name. The first part of the title may have been output without proper framing bytes, if so they were lost.

Here is another for "Love Will Tear Us Apart / New Order":

	2022-10-15T13:42:40Z: MD: Status dump no disc powered off track no. 18 Play TOC read not copy protected stereo audio digital input locked Analog
	2022-10-15T13:42:44Z: MD sent: 6F-76-65-20-57-69-6C-6C-20-54-65-61-72-20-55-FF, ASCII: ove Will Tear U?
	2022-10-15T13:42:44Z: MD sent: 6F-18-05-47-20-4B-02-73-20-41-70-61-72-74-20-2F-20-4E-65-77-20-4F-72-FF, ASCII: oG Ks Apart / New Or?
	2022-10-15T13:42:44Z: MD: Track name part 2 is: s Apart / New Or
	2022-10-15T13:42:45Z: MD sent: 6F-18-05-47-20-4B-03-64-65-72-00-00-00-00-44-61-72-6B-6E-65-73-73-00-FF, ASCII: oG KderDarkness?
	2022-10-15T13:42:45Z: MD: Track name part 3 is: der
	2022-10-15T13:42:45Z: MD sent: 6F-07-05-47-20-4C-FF, ASCII: oG L?
	2022-10-15T13:42:45Z: MD: ALL NAME END

We can observe that the first undecoded message does in fact contain the entire contents this time (the first L is not printed due to a bug on my end). The second and third segments are correct.

## Will likely not be supported:
* Recording
* Track splitting/editing

## Documentation bugs
Here's what I got so far, seems like there will be lots to come!
I've mostly been using the Combined E11, 12, 52 document.

### General Comms
The recorder doesn't clear its output buffers, so many outputs involving text contain trailing garbage after the null termination. This is slightly irritating in C# but manageable.

### 7.16 TRACK NAME
The first packet is listed as:

	0x20, 0x4A, PacketNo, ASCII Data

The output appears to be :

	0x20, 0x4A, TrackNo, ASCII Data

The field descriptions and my observations suggest this is the case, at least when a specific track name is requested.

This command is very prone to bugs, see above.