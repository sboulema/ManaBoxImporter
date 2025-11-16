# ManaBox Importer
A console application to transform a [Magic the Gathering Arena](https://magic.wizards.com/en/mtgarena) collection export made with:
- [MTG Tracker Daemon](https://github.com/frcaton/mtga-tracker-daemon)
- [MTG Arena Pro Tracker](https://mtgarena.pro/mtga-pro-tracker/)

to a [ManaBox](https://www.manabox.app/) compatible import.

## Example
`dotnet run -- -p 6842`

## Arguments
| Name           | Value    | Required | Description |
|----------------|----------|----------|-------------|
| -p, --port     | <number> | False    | Port number on which MTGA Tracker Daemon is running |
| -s, --scryfall | <path>   | False    | Path to json file containing all cards from Scryfall |
| -o, --output   | <path>   | False    | Path to the folder where the collection and log files will be exported |

## Usage

### Method 1 (MTGA Tracker Daemon)
Easiest option to get started

#### Prerequisites
1. Install the [MTGA Tracker Daemon](https://github.com/frcaton/mtga-tracker-daemon)
2. Start MTG Arena
3. Start MTGA Tracker Daemon as administrator

#### Exporting the collection
1. Run ManaBox Importer with the port number used when starting MTGA Tracker Daemon
2. Collection will be exported as csv file to the given output folder or a temp folder
3. Import csv file into ManaBox
4. Happy deck building!

### Method 2 (MTGA Arena Pro Tracker)
Alternative option if you don't want to use MTGA Tracker Daemon

#### Prerequisites
1. Install the [MTG Arena Pro Tracker](https://mtgarena.pro/mtga-pro-tracker/)
2. Start MTG Arena
3. [Enable detailed logging in MTG Arena](https://draftsim.com/enable-detailed-logging-in-mtg-arena/)
4. Start MTG Arena Pro Tracker
5. Wait for the tracker to export data to the Player log
6. Download the Scryfall 'Default Cards' json from: [Scryfall Bulk Data](https://scryfall.com/docs/api/bulk-data)

#### Exporting collection
1. Run ManaBox Importer with the path to the Scryfall json file
2. Collection will be exported as csv file to the given output folder or a temp folder
3. Import csv file into ManaBox
4. Happy deck building!

## Links
- [Scryfall Bulk Data API](https://api.scryfall.com/bulk-data)
- [17Lands Public Data](https://www.17lands.com/public_datasets)

## Disclaimer
This application is not produced, endorsed, supported, or affiliated with [SkillDevs](https://www.skilldevs.com/).