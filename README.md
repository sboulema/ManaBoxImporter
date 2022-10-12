# ManaBox Importer
A simple console application to transform a [Magic the Gathering Arena](https://magic.wizards.com/en/mtgarena) collection export made with [mtga-tracker-daemon](https://github.com/frcaton/mtga-tracker-daemon) to a [ManaBox](https://www.manabox.app/) compatible import.

## Example
`dotnet run -- -c <path to json file>`

## Arguments
| Name             | Value  | Required | Description |
|------------------|--------|----------|-------------|
| -c, --collection | <path> | True     | Path to json file containing your MTGA collection |
| -s, --scryfall   | <path> | False    | Path to json file containing all cards from Scryfall, If not given ManaBox Importer will use the Scryfall API
| -l, --log        |        | False    | Enable writing errors to log file |

## Usages

### Method 1, prefered usage, bit harder than method 2 but faster
1. Create an export with mtga-tracker-daemon
2. Download the Scryfall 'Default Cards' json from: [Scryfall Bulk Data](https://scryfall.com/docs/api/bulk-data)
3. Transform the json file to a ManaBox compatible csv file
4. Import csv file into ManaBox
5. Profit!

### Method 2, simpler but slower
1. Create an export with mtga-tracker-daemon
2. Transform the json file to a ManaBox compatible csv file
3. Import csv file into ManaBox
4. Profit!